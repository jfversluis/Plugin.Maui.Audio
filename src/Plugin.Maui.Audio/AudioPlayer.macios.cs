using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	readonly AVAudioPlayer player;
	bool isDisposed;

	public double Duration => player.Duration;

	public double CurrentPosition => player.CurrentTime;

	public double Volume
	{
		get => player.Volume;
		set => player.Volume = (float)Math.Clamp(value, 0, 1);
	}

	public double Balance
	{
		get => player.Pan;
		set => player.Pan = (float)Math.Clamp(value, -1, 1);
	}

	public double Speed
	{
		get => player.Rate;
		set
		{
			// Check if set speed is supported
			if (CanSetSpeed)
			{
				// Rate property supports values in the range of 0.5 for half-speed playback to 2.0 for double-speed playback.
				var speedValue = Math.Clamp((float)value, 0.5f, 2.0f);

				if (float.IsNaN(speedValue))
					speedValue = 1.0f;

				player.Rate = speedValue;
			}
			else
			{
				throw new NotSupportedException("Set playback speed is not supported!");
			}
		}
	}

	public double MinimumSpeed => 0.5;

	public double MaximumSpeed => 2;

	public bool CanSetSpeed => true;

	public bool IsPlaying => player.Playing;

	public bool Loop
	{
		get => player.NumberOfLoops != 0;
		set => player.NumberOfLoops = value ? -1 : 0;
	}

	public bool CanSeek => true;

	internal AudioPlayer(Stream audioStream)
	{
		var data = NSData.FromStream(audioStream)
			?? throw new FailedToLoadAudioException("Unable to convert audioStream to NSData.");
		player = AVAudioPlayer.FromData(data)
			?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from data.");

		PreparePlayer();
	}

	internal AudioPlayer(string fileName)
	{
		player = AVAudioPlayer.FromUrl(NSUrl.FromFilename(fileName))
			?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from url.");

		PreparePlayer();
	}
    public void Play()
    {
        if (player.Playing)
        {
            player.Pause();
        }
        else
        {
            player.Play();
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

			player.FinishedPlaying -= OnPlayerFinishedPlaying;
			player.Dispose();
		}

		isDisposed = true;
	}

	public void Pause() => player.Pause();

	public void Seek(double position) => player.CurrentTime = position;

	public void Stop()
	{
		player.Stop();
		Seek(0);
		PlaybackEnded?.Invoke(this, EventArgs.Empty);
	}

	bool PreparePlayer()
	{
		player.FinishedPlaying += OnPlayerFinishedPlaying;
		player.EnableRate = true;
		player.PrepareToPlay();

		return true;
	}

	void OnPlayerFinishedPlaying(object? sender, AVStatusEventArgs e)
	{
		PlaybackEnded?.Invoke(this, e);
	}
}