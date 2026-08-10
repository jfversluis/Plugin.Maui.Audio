using Windows.Media.Core;
using Windows.Media.Playback;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	bool isDisposed = false;
	readonly MediaPlayer player;
	AudioPlayerOptions? audioPlayerOptions;

	public double CurrentPosition => player.PlaybackSession.Position.TotalSeconds;

	public double Duration => player.PlaybackSession.NaturalDuration.TotalSeconds;

	public double Volume
	{
		get => player.Volume;
		set => SetVolume(value, Balance);
	}

	public double Balance
	{
		get => player.AudioBalance;
		set => SetVolume(Volume, value);
	}

	public double Speed
	{
		get => player.PlaybackSession.PlaybackRate;
		set => SetSpeedInternal(value);
	}

	protected void SetSpeedInternal(double speed)
	{
		player.PlaybackSession.PlaybackRate = Math.Clamp(speed, MinimumSpeed, MaximumSpeed);
	}

	public double MinimumSpeed => 0;

	public double MaximumSpeed => 8;

	public bool CanSetSpeed => true;

	public bool IsPlaying => player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing; //might need to expand

	public bool Loop
	{
		get => player.IsLoopingEnabled;
		set => player.IsLoopingEnabled = value;
	}

	public bool CanSeek => player.PlaybackSession.CanSeek;

	public static Windows.Storage.Streams.IRandomAccessStream ConvertToRandomAccessStream(MemoryStream memoryStream)
	{
		var randomAccessStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
		using (var outputStream = randomAccessStream.GetOutputStreamAt(0))
		{
			using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
			{
				dataWriter.WriteBytes(memoryStream.ToArray());
				dataWriter.StoreAsync().AsTask().GetAwaiter().GetResult();
			}
		}
		return randomAccessStream;
	}

	public AudioPlayer(AudioPlayerOptions audioPlayerOptions)
	{
		player = CreatePlayer();
		this.audioPlayerOptions = audioPlayerOptions;

		if (player is null)
		{
			throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
		}

		player.MediaFailed += OnError;
		player.MediaEnded += OnPlaybackEnded;
		Speed = 1.0;

		SetPreferredOutputDevice(audioPlayerOptions.PreferredOutputDeviceName);
	}

	void OnError(MediaPlayer sender, MediaPlayerFailedEventArgs e)
	{
		OnError(new MediaPlayerFailedEventArgsWrapper(e));
	}

	public void SetSource(Stream audioStream)
	{
		if (audioStream is System.IO.MemoryStream memoryStream)
		{
			var winStream = ConvertToRandomAccessStream(memoryStream);
			player.Source = MediaSource.CreateFromStream(winStream, string.Empty);
		}
		else
		{
			player.Source = MediaSource.CreateFromStream(audioStream?.AsRandomAccessStream(), string.Empty);
		}

		// Reapply preferred output device after source change for consistency
		SetPreferredOutputDevice(audioPlayerOptions?.PreferredOutputDeviceName);
	}

	public AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions)
	{
		player = CreatePlayer();
		this.audioPlayerOptions = audioPlayerOptions;

		if (player is null)
		{
			throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
		}

		if (audioStream is System.IO.MemoryStream memoryStream)
		{
			var winStream = ConvertToRandomAccessStream(memoryStream);
			player.Source = MediaSource.CreateFromStream(winStream, string.Empty);
		}
		else
		{
			player.Source = MediaSource.CreateFromStream(audioStream?.AsRandomAccessStream(), string.Empty);
		}


		player.MediaEnded += OnPlaybackEnded;
		Speed = 1.0;

		SetPreferredOutputDevice(audioPlayerOptions.PreferredOutputDeviceName);
	}

	public AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions)
	{
		player = CreatePlayer();
		this.audioPlayerOptions = audioPlayerOptions;

		if (player is null)
		{
			throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
		}

		player.Source = MediaSource.CreateFromUri(new Uri(fileName));
		player.MediaEnded += OnPlaybackEnded;
		Speed = 1.0;

		SetPreferredOutputDevice(audioPlayerOptions.PreferredOutputDeviceName);
	}

	void OnPlaybackEnded(MediaPlayer sender, object args)
	{
		PlaybackEnded?.Invoke(sender, EventArgs.Empty);
	}

	public void Play()
	{
		if (player.Source is null)
		{
			return;
		}

		if (player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
		{
			Pause();
			Seek(0);
		}

		player.Play();
	}

	public void Pause()
	{
		player.Pause();
	}

	public void Stop()
	{
		Pause();
		Seek(0);
		OnPlaybackEnded(player, EventArgs.Empty); //todo check for double invoke?
	}

	public void Seek(double position)
	{
		if (player.PlaybackSession is null)
		{
			return;
		}

		try
		{
			if (player.PlaybackSession.CanSeek)
			{
				player.PlaybackSession.Position = TimeSpan.FromSeconds(position);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

	void SetVolume(double volume, double balance)
	{
		if (isDisposed)
		{
			return;
		}

		player.Volume = Math.Clamp(volume, 0, 1);
		player.AudioBalance = Math.Clamp(balance, -1, 1);
	}

	MediaPlayer CreatePlayer()
	{
		return new MediaPlayer() { AutoPlay = false, IsLoopingEnabled = false };
	}

	void SetPreferredOutputDevice(string? preferredDeviceName)
	{
		if (string.IsNullOrWhiteSpace(preferredDeviceName))
		{
			return;
		}

		try
		{
			var devices = Windows.Devices.Enumeration.DeviceInformation
				.FindAllAsync(Windows.Media.Devices.MediaDevice.GetAudioRenderSelector())
				.AsTask()
				.GetAwaiter()
				.GetResult();

			var targetDevice = devices.FirstOrDefault(d =>
				d.Name.Contains(preferredDeviceName, StringComparison.OrdinalIgnoreCase));

			if (targetDevice is not null)
			{
				player.AudioDevice = targetDevice;
			}
			else
			{
				System.Diagnostics.Trace.TraceWarning($"Requested audio output device '{preferredDeviceName}' not found.");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Trace.TraceError($"Error setting preferred audio output device: {ex.Message}");
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
			Stop();

			player.MediaFailed -= OnError;
			player.MediaEnded -= OnPlaybackEnded;
			player.Dispose();
		}

		isDisposed = true;
	}
}

public class MediaPlayerFailedEventArgsWrapper : EventArgs
{
	public MediaPlayerFailedEventArgs Error { get; }

	public int ErrorCode { get; }
	public string ErrorMessage { get; }

	public MediaPlayerFailedEventArgsWrapper(MediaPlayerFailedEventArgs args)
	{
		Error = args ?? throw new ArgumentNullException(nameof(args));
		ErrorCode = (int)args.Error;
		ErrorMessage = args.Error.ToString();
	}
}

