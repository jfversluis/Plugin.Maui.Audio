using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	readonly AVAudioPlayer player;
	readonly AudioPlayerOptions audioPlayerOptions;
	bool isDisposed;

	public double CurrentPosition => player.CurrentTime;

	public double Duration => player.Duration;

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

	internal AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions)
	{
		var data = NSData.FromStream(audioStream)
		   ?? throw new FailedToLoadAudioException("Unable to convert audioStream to NSData.");
		player = AVAudioPlayer.FromData(data)
		   ?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from data.");

		this.audioPlayerOptions = audioPlayerOptions;

		PreparePlayer();
	}

	internal AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions)
	{
		player = AVAudioPlayer.FromUrl(NSUrl.FromFilename(fileName))
		   ?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from url.");

		this.audioPlayerOptions = audioPlayerOptions;

		PreparePlayer();
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

	public void Play()
	{
		if (player.Playing)
		{
			player.CurrentTime = 0;
		}
		else
		{
			player.Play();
		}
	}

	public void Seek(double position) => player.CurrentTime = position;

	public void Stop()
	{
		player.Stop();
		Seek(0);
		PlaybackEnded?.Invoke(this, EventArgs.Empty);
	}

	void InitAudioSession()
	{
		var audioSession = AVAudioSession.SharedInstance();

		var options = audioPlayerOptions;
		
		var error = audioSession.SetCategory(options.Category, options.Mode, options.CategoryOptions);
		if (error is not null)
		{
			throw new Exception(error.ToString());
		}

		error = audioSession.SetActive(true);
		if (error is not null)
		{
			throw new Exception(error.ToString());
		}
	}

	bool PreparePlayer()
	{
		InitAudioSession();
		
		player.FinishedPlaying += OnPlayerFinishedPlaying;
		player.PrepareToPlay();

		return true;
	}

	void OnPlayerFinishedPlaying(object? sender, AVStatusEventArgs e)
	{
		PlaybackEnded?.Invoke(this, e);
	}
}