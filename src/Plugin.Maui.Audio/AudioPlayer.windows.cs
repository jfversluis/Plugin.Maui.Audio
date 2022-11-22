using Windows.Media.Core;
using Windows.Media.Playback;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	bool isDisposed = false;
	readonly MediaPlayer player;

	public double Duration => player.PlaybackSession.NaturalDuration.TotalSeconds;

	public double CurrentPosition => player.PlaybackSession.Position.TotalSeconds;

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
		set
		{
			// Check if set speed is supported
			if (CanSetSpeed)
			{
				// Windows supports between 0 and 8, but will clamp automatically for us
				if (player.PlaybackSession.IsSupportedPlaybackRateRange(value, value))
				{
					player.PlaybackSession.PlaybackRate = value;
				}
			}
			else
			{
				throw new NotSupportedException("Set playback speed is not supported!");
			}
		}
	}

	public double MinimumSpeed => 0;

	public double MaximumSpeed => 8;

	public bool CanSetSpeed => true;

	public bool IsPlaying =>
		player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing; //might need to expand

	public bool Loop
	{
		get => player.IsLoopingEnabled;
		set => player.IsLoopingEnabled = value;
	}

	public bool CanSeek => player.PlaybackSession.CanSeek;

	public AudioPlayer(Stream audioStream)
	{
		player = CreatePlayer();

		if (player is null)
		{
			throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
		}

		player.Source = MediaSource.CreateFromStream(audioStream?.AsRandomAccessStream(), string.Empty);
		player.MediaEnded += OnPlaybackEnded;
	}

	public AudioPlayer(string fileName)
	{
		player = CreatePlayer();

		if (player is null)
		{
			throw new FailedToLoadAudioException($"Failed to create {nameof(MediaPlayer)} instance. Reason unknown.");
		}

		player.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/" + fileName));
		player.MediaEnded += OnPlaybackEnded;
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
		PlaybackEnded?.Invoke(this, EventArgs.Empty);
	}

	public void Seek(double position)
	{
		if (player.PlaybackSession is null)
		{
			return;
		}

		if (player.PlaybackSession.CanSeek)
		{
			player.PlaybackSession.Position = TimeSpan.FromSeconds(position);
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

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}

		if (disposing)
		{
			Stop();

			player.MediaEnded -= OnPlaybackEnded;
			player.Dispose();
		}

		isDisposed = true;
	}
}
