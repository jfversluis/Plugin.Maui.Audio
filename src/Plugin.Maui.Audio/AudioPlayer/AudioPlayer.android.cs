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
	Android.Media.AudioManager? audioManager;
	AudioFocusRequestClass? audioFocusRequest;
	AudioFocusChangeListener? audioFocusChangeListener;
	bool wasPlayingBeforeFocusLoss = false;
	double volumeBeforeDucking = 0;
	AudioPlayerOptions? audioPlayerOptions;
	AudioOutputDevice preferredOutputDevice = AudioOutputDevice.Default;

	const double DuckingVolumeMultiplier = 0.2;

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

			// reapply audio attributes and preferred device (Reset clears all configuration)
			ConfigureAudioAttributes();
			PrepareAudioSource();
			SetPreferredOutputDevice(preferredOutputDevice);

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
			// If not playing, leave player in Prepared state (Pause is invalid here)
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
		if (audioBytes is null && string.IsNullOrWhiteSpace(file))
		{
			throw new ArgumentException("audio source is not set");
		}

		if (audioBytes is not null && OperatingSystem.IsAndroidVersionAtLeast(23))
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
			if (file is null)
			{
				throw new FailedToLoadAudioException("Provided file was null");
			}

			AssetFileDescriptor afd = Android.App.Application.Context.Assets?.OpenFd(file)
				?? throw new FailedToLoadAudioException("Unable to create AssetFileDescriptor.");

			player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
		}

		player.Prepare();
	}

	internal AudioPlayer(AudioPlayerOptions audioPlayerOptions)
	{
		player = new MediaPlayer();
		this.audioPlayerOptions = audioPlayerOptions;

		// AudioManager is needed for both audio focus management and preferred output device
		audioManager = (Android.Media.AudioManager?)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);

		if (audioPlayerOptions.ManageAudioFocus)
		{
			audioFocusChangeListener = new AudioFocusChangeListener(this);
		}

		ConfigureAudioAttributes();

		player.Completion += OnPlaybackEnded;

		// Set preferred output device if specified (API 28+)
		SetPreferredOutputDevice(audioPlayerOptions.PreferredOutputDevice);
	}

	void ConfigureAudioAttributes()
	{
		if (audioPlayerOptions is null)
		{
			return;
		}

		if (OperatingSystem.IsAndroidVersionAtLeast(26))
		{
			var audioAttributes = new AudioAttributes.Builder()?
				.SetContentType(audioPlayerOptions.AudioContentType)?
				.SetUsage(audioPlayerOptions.AudioUsageKind)?
				.Build();

			if (audioAttributes is not null)
			{
				player.SetAudioAttributes(audioAttributes);

				// Build audio focus request for Android 26+ only if audio focus management is enabled
				if (audioPlayerOptions.ManageAudioFocus && audioManager is not null && audioFocusChangeListener is not null)
				{
					audioFocusRequest = new AudioFocusRequestClass.Builder(AudioFocus.Gain)?
						.SetAudioAttributes(audioAttributes)?
						.SetOnAudioFocusChangeListener(audioFocusChangeListener)?
						.Build();
				}
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
	}

	void SetPreferredOutputDevice(AudioOutputDevice preferredDevice)
	{
		// Store the preference so it can be reapplied after Reset()
		preferredOutputDevice = preferredDevice;

		// setPreferredDevice is only available on API 28 and above
		if (!OperatingSystem.IsAndroidVersionAtLeast(28))
		{
			return;
		}

		// If Default is specified, clear any preferred device
		if (preferredDevice == AudioOutputDevice.Default)
		{
			player.SetPreferredDevice(null);
			return;
		}

		try
		{
			if (audioManager is null)
			{
				System.Diagnostics.Trace.TraceWarning("AudioManager is not available.");
				return;
			}

			// Get all output audio devices (API 23+)
			var devices = audioManager.GetDevices(GetDevicesTargets.Outputs);

			if (devices is null || devices.Length == 0)
			{
				System.Diagnostics.Trace.TraceWarning("No output audio devices found.");
				return;
			}

			// Find the first device matching the preferred type
			var targetDeviceType = (AudioDeviceType)preferredDevice;
			var targetDevice = devices.FirstOrDefault(d => d.Type == targetDeviceType);

			if (targetDevice is not null)
			{
				player.SetPreferredDevice(targetDevice);
			}
			else
			{
				System.Diagnostics.Trace.TraceWarning($"Requested audio output device type {targetDeviceType} not found.");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Trace.TraceError($"Error setting preferred audio output device: {ex.Message}");
		}
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

		// reapply audio attributes and preferred device (Reset clears all configuration)
		ConfigureAudioAttributes();
		PrepareAudioSource();
		SetPreferredOutputDevice(preferredOutputDevice);
	}

	internal AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions)
	{
		player = new MediaPlayer();
		this.audioPlayerOptions = audioPlayerOptions;

		// AudioManager is needed for both audio focus management and preferred output device
		audioManager = (Android.Media.AudioManager?)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);

		if (audioPlayerOptions.ManageAudioFocus)
		{
			audioFocusChangeListener = new AudioFocusChangeListener(this);
		}

		ConfigureAudioAttributes();
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

		// Set preferred output device if specified (API 28+)
		SetPreferredOutputDevice(audioPlayerOptions.PreferredOutputDevice);
	}


	internal AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions)
	{
		player = new MediaPlayer();
		this.audioPlayerOptions = audioPlayerOptions;

		// AudioManager is needed for both audio focus management and preferred output device
		audioManager = (Android.Media.AudioManager?)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);

		if (audioPlayerOptions.ManageAudioFocus)
		{
			audioFocusChangeListener = new AudioFocusChangeListener(this);
		}

		ConfigureAudioAttributes();
		player.Completion += OnPlaybackEnded;
		player.Error += OnError;

		file = fileName;

		PrepareAudioSource();

		// Set preferred output device if specified (API 28+)
		SetPreferredOutputDevice(audioPlayerOptions.PreferredOutputDevice);
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

		// Request audio focus before playing
		if (!RequestAudioFocus())
		{
			System.Diagnostics.Trace.TraceWarning("Failed to request audio focus");
			// Continue playing even if focus request fails for backward compatibility
		}

		PlayInternal();
	}

	void PlayInternal()
	{
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

		// Abandon audio focus when stopping
		AbandonAudioFocus();

		Seek(0);
		
		OnPlaybackEnded(player, EventArgs.Empty);
	}

	public void Pause()
	{
		if (!IsPlaying)
		{
			return;
		}

		PauseInternal();

		// Abandon audio focus when pausing
		AbandonAudioFocus();
	}

	void PauseInternal()
	{
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

	bool RequestAudioFocus()
	{
		// Check if audio focus management is enabled
		if (audioPlayerOptions?.ManageAudioFocus != true || audioManager is null)
		{
			return false;
		}

		AudioFocusRequest result;

		if (OperatingSystem.IsAndroidVersionAtLeast(26) && audioFocusRequest is not null)
		{
			result = audioManager.RequestAudioFocus(audioFocusRequest);
		}
		else
		{
			// For API < 26, use deprecated method
#pragma warning disable CS0618 // Type or member is obsolete
			result = audioManager.RequestAudioFocus(
				audioFocusChangeListener,
				Android.Media.Stream.Music,
				AudioFocus.Gain);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		return result == AudioFocusRequest.Granted;
	}

	void AbandonAudioFocus()
	{
		// Check if audio focus management is enabled
		if (audioPlayerOptions?.ManageAudioFocus != true || audioManager is null)
		{
			return;
		}

		if (OperatingSystem.IsAndroidVersionAtLeast(26) && audioFocusRequest is not null)
		{
			audioManager.AbandonAudioFocusRequest(audioFocusRequest);
		}
		else
		{
			// For API < 26, use deprecated method
#pragma warning disable CS0618 // Type or member is obsolete
			audioManager.AbandonAudioFocus(audioFocusChangeListener);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}

	void HandleAudioFocusChange(AudioFocus focusChange)
	{
		switch (focusChange)
		{
			case AudioFocus.Loss:
				// Permanent loss of audio focus - stop playback
				// Reset state before Stop() to prevent incorrect state in any callbacks
				wasPlayingBeforeFocusLoss = false;
				volumeBeforeDucking = 0;
				if (IsPlaying)
				{
					Stop();
				}
				break;

			case AudioFocus.LossTransient:
				// Temporary loss of audio focus - pause playback
				if (IsPlaying)
				{
					wasPlayingBeforeFocusLoss = true;
					// Don't abandon audio focus here since we want to resume later
					PauseInternal();
				}
				break;

			case AudioFocus.LossTransientCanDuck:
				// Temporary loss of audio focus but can duck (lower volume)
				// Lower the volume but continue playing
				if (IsPlaying)
				{
					volumeBeforeDucking = Volume;
					Volume = volumeBeforeDucking * DuckingVolumeMultiplier;
				}
				break;

			case AudioFocus.Gain:
				// Regained audio focus
				if (wasPlayingBeforeFocusLoss)
				{
					// Resume playback if it was paused due to transient loss
					// Use PlayInternal() since we already have audio focus
					PlayInternal();
					wasPlayingBeforeFocusLoss = false;
				}
				// Restore volume if it was ducked
				if (volumeBeforeDucking > 0)
				{
					Volume = volumeBeforeDucking;
					volumeBeforeDucking = 0;
				}
				break;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}

		if (disposing)
		{
			AbandonAudioFocus();
			player.Completion -= OnPlaybackEnded;
			player.Error -= OnError;
			player.Reset();
			player.Release();
			player.Dispose();
			DeleteFile(cachePath);
			cachePath = string.Empty;
			stream?.Dispose();
			audioFocusRequest?.Dispose();
		}

		isDisposed = true;
	}

	/// <summary>
	/// Listens for audio focus changes from the Android system and delegates handling to the parent AudioPlayer.
	/// </summary>
	class AudioFocusChangeListener : Java.Lang.Object, Android.Media.AudioManager.IOnAudioFocusChangeListener
	{
		readonly AudioPlayer audioPlayer;

		public AudioFocusChangeListener(AudioPlayer player)
		{
			audioPlayer = player;
		}

		public void OnAudioFocusChange(AudioFocus focusChange)
		{
			audioPlayer.HandleAudioFocusChange(focusChange);
		}
	}
}
