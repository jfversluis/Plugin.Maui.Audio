using System.Runtime.Versioning;
using Android.Content.Res;
using Android.Media;
using Stream = System.IO.Stream;
using Uri = Android.Net.Uri;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	readonly MediaPlayer player; // For broader format support (FLAC, etc.), consider using ExoPlayer instead in future
	double volume = 0.5;
	double balance = 0;
	string cachePath = string.Empty;
	byte[]? audioBytes;
	MemoryStream? stream;
	bool isDisposed = false;
	AudioStopwatch stopwatch = new(TimeSpan.Zero, 1.0);

	public double Duration => player.Duration <= -1 ? -1 : player.Duration / 1000.0;

	public double CurrentPosition => stopwatch.ElapsedMilliseconds / 1000.0;

	public double Volume
	{
		get => volume;
		set => SetVolume(volume = value, Balance);
	}

	public double Balance
	{
		get => balance;
		set => SetVolume(Volume, balance = value);
	}

	bool isChangingSpeed = false;

	/// <summary>
	/// Internal state machine for isPlaying. This is needed because when Reset is called, the player is not playing anymore, but in Idle state IsPlaying is not available
	/// </summary>
	bool isPlaying = false;

	[Obsolete("Use Speed setter instead")]
	[SupportedOSPlatform("Android23.9")]
	public void SetSpeed(double sp)
	{
		SetSpeedInternal(sp);
	}

	protected void SetSpeedInternal(double sp)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			System.Diagnostics.Trace.TraceWarning("Setting speed is only supported on Android 23 and above.");
			return;
		}

		try
		{
			if (isChangingSpeed)
			{
				System.Diagnostics.Trace.TraceWarning("last speed update was not yet completed");
				return;
			}

			isChangingSpeed = true;

			// internal (android) speed value (float)
			internalSpeed = Math.Clamp((float)sp, minSpeed, maxSpeed);

			// shared speed value (double)
			speed = Math.Clamp(sp, MinimumSpeed, MaximumSpeed);

			// to prevent illegal exception when changing the speed on some devices,
			// we need to change the speed while the audio is reset, see: https://stackoverflow.com/questions/39442522/setplaybackparams-causes-illegalstateexception

			// store current state before resetting
			var previousPosition = stopwatch.ElapsedMilliseconds;
			var wasPlaying = isPlaying;

			// reset
			isPlaying = false;
			player.Reset();

			// after reset, we need to prepare the audio source again
			PrepareAudioSource();

			// now we are going to update the speed parameter of the actual audio player

			// allow defaults ensures that we can play the audio in case the pitch value is not set. When the audioplayer is paused, PlaybackParams may not be what we would like it to be
			var parms = (player.PlaybackParams.AllowDefaults() ?? new PlaybackParams().AllowDefaults())?.SetSpeed(internalSpeed) ?? throw new ArgumentException("speed value not supported");

			// this ensures that the audio will fail if the speed is not supported, otherwise android simply continues but does not play any audio
			// https://developer.android.com/reference/android/media/PlaybackParams
			parms.SetAudioFallbackMode((int)AudioFallbackMode.Fail);

			player.PlaybackParams = parms;

			// we need to update the stopwatch to reflect the new speed
			stopwatch = new AudioStopwatch(TimeSpan.FromMilliseconds(previousPosition), speed);

			// because we had to reset, we now restore the previous state of the audio player
			player.SeekTo((int)previousPosition);
			if (wasPlaying)
			{
				isPlaying = true;
				stopwatch.Start();
				player.Start();
			}
			else // Explicitly pause the player if it was not playing before
			{
				player.Pause();
			}
		}
		finally
		{
			isChangingSpeed = false;
		}
	}

	float internalSpeed = 1.0f;
	double speed = 1.0;

	public double Speed
	{
		get => speed;
		set => SetSpeedInternal(value);
	}
	const float minSpeed = 0;
	const float maxSpeed = 2.5f;

	public double MinimumSpeed => 0;

	public double MaximumSpeed => 2.5;

	public bool CanSetSpeed => Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M;

	public bool IsPlaying => isPlaying;

	public bool Loop
	{
		get => player.Looping;
		set => player.Looping = value;
	}

	public bool CanSeek => true;

	string? file;

	void PrepareAudioSource()
	{
		if (audioBytes == null && string.IsNullOrWhiteSpace(file))
		{
			throw new ArgumentException("audio source is not set");
		}

		if (audioBytes != null && OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			stream = new MemoryStream(audioBytes);
			var mediaSource = new StreamMediaDataSource(stream);
			player.SetDataSource(mediaSource);
		}
		else if (File.Exists(file))
		{
			try
			{
				player.SetDataSource(file);
			}
			catch
			{
				var context = Android.App.Application.Context;
				var encodedPath = Uri.Encode(file)
					?? throw new FailedToLoadAudioException("Unable to generate encoded path.");
				var uri = Uri.Parse(encodedPath)
					?? throw new FailedToLoadAudioException("Unable to parse encoded path.");

				player.SetDataSource(context, uri);
			}
		}
		else
		{
			AssetFileDescriptor afd = Android.App.Application.Context.Assets?.OpenFd(file)
				?? throw new FailedToLoadAudioException("Unable to create AssetFileDescriptor.");

			player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
		}

		player.Prepare();
	}

	internal AudioPlayer(AudioPlayerOptions audioPlayerOptions)
	{
		player = new MediaPlayer();

		if (OperatingSystem.IsAndroidVersionAtLeast(26))
		{
			var audioAttributes = new AudioAttributes.Builder()?
				.SetContentType(audioPlayerOptions.AudioContentType)?
				.SetUsage(audioPlayerOptions.AudioUsageKind)?
				.Build();

			if (audioAttributes is not null)
			{
				player.SetAudioAttributes(audioAttributes);
			}
		}
		else
		{
			Android.Media.Stream streamType = Android.Media.Stream.System;

			switch (audioPlayerOptions.AudioUsageKind)
			{
				case AudioUsageKind.Media:
					streamType = Android.Media.Stream.Music;
					break;
				case AudioUsageKind.Alarm:
					streamType = Android.Media.Stream.Alarm;
					break;
				case AudioUsageKind.Notification:
					streamType = Android.Media.Stream.Notification;
					break;
				case AudioUsageKind.VoiceCommunication:
					streamType = Android.Media.Stream.VoiceCall;
					break;
				case AudioUsageKind.Unknown:
					break;
			}

			player.SetAudioStreamType(streamType);
		}
			
		player.Completion += OnPlaybackEnded;
	}

	public void SetSource(Stream audioStream)
	{
		if (OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			using var memoryStream = new MemoryStream();
			audioStream.CopyTo(memoryStream);
			audioBytes = memoryStream.ToArray();
		}
		else
		{
			// we always store the audio in a file in cache, as the audio stream needs to be accessed again in case the speed is changed
			cachePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.wav");

			while (File.Exists(cachePath))
			{
				cachePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.wav");
			}

			var fileStream = File.Create(cachePath);
			audioStream.CopyTo(fileStream);
			fileStream.Close();

			file = cachePath;
		}

		player.Reset();

		PrepareAudioSource();
	}

	internal AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions)
	{
		player = new MediaPlayer();
		player.Completion += OnPlaybackEnded;


		if (OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			using var memoryStream = new MemoryStream();
			audioStream.CopyTo(memoryStream);
			audioBytes = memoryStream.ToArray();
		}
		else
		{
			// we always store the audio in a file in cache, as the audio stream needs to be accessed again in case the speed is changed
			cachePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.wav");

			while (File.Exists(cachePath))
			{
				cachePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.wav");
			}

			var fileStream = File.Create(cachePath);
			audioStream.CopyTo(fileStream);
			fileStream.Close();

			file = cachePath;
		}


		PrepareAudioSource();
	}


	internal AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions)
	{
		player = new MediaPlayer();
		player.Completion += OnPlaybackEnded;
		player.Error += OnError;

		file = fileName;

		PrepareAudioSource();
	}

	static void DeleteFile(string path)
	{
		if (string.IsNullOrWhiteSpace(path)) { return; }

		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch
		{
		}
	}

	public void Play()
	{
		if (IsPlaying)
		{
			player.Pause();
			Seek(0);
			stopwatch.Reset();
		}
		else if (CurrentPosition >= Duration)
		{
			Seek(0);
			stopwatch.Reset();
		}

		isPlaying = true;
		player.Start();
		stopwatch.Start();
	}

	public void Stop()
	{
		if (IsPlaying)
		{
			isPlaying = false;
			player.Pause();
		}

		Seek(0);
		
		OnPlaybackEnded(player, EventArgs.Empty);
	}

	public void Pause()
	{
		if (!IsPlaying)
		{
			return;
		}

		isPlaying = false;
		player.Pause();
		stopwatch.Stop();
	}

	public void Seek(double position)
	{
		player.SeekTo((int)(position * 1000D));
		stopwatch = new AudioStopwatch(TimeSpan.FromSeconds(position), Speed);
		if (IsPlaying)
		{
			stopwatch.Start();
		}
	}

	void SetVolume(double volume, double balance)
	{
		volume = Math.Clamp(volume, 0, 1);

		balance = Math.Clamp(balance, -1, 1);

		// Using the "constant power pan rule." See: http://www.rs-met.com/documents/tutorials/PanRules.pdf
		var left = Math.Cos((Math.PI * (balance + 1)) / 4) * volume;
		var right = Math.Sin((Math.PI * (balance + 1)) / 4) * volume;

		player.SetVolume((float)left, (float)right);
	}

	void OnPlaybackEnded(object? sender, EventArgs e)
	{
		isPlaying = player.IsPlaying;

		//this improves stability on older devices but has minor performance impact
		// We need to check whether the player is null or not as the user might have dipsosed it in an event handler to PlaybackEnded above.
		if (!OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			player.SeekTo(0);
			player.Stop();
			player.Prepare();
		}

		stopwatch.Reset();
		PlaybackEnded?.Invoke(this, e);
	}

	void OnError(object? sender, MediaPlayer.ErrorEventArgs e)
	{
		OnError(e);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}

		if (disposing)
		{
			player.Completion -= OnPlaybackEnded;
			player.Error -= OnError;
			player.Reset();
			player.Release();
			player.Dispose();
			DeleteFile(cachePath);
			cachePath = string.Empty;
			stream?.Dispose();
		}

		isDisposed = true;
	}
}
