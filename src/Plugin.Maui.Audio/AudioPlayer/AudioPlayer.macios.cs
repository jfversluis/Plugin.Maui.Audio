using System.Diagnostics;
using AudioToolbox;
using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	AVAudioPlayer player;
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
		get => player?.Rate ?? 0;
		set => SetSpeedInternal(value);
	}

	[Obsolete("Use Speed setter instead")]
	public void SetSpeed(double sp)
	{
		SetSpeedInternal(sp);
	}

	protected void SetSpeedInternal(double sp)
	{
		// Rate property supports values in the range of 0.5 for half-speed playback to 2.0 for double-speed playback.
		var speedValue = Math.Clamp((float)sp, 0.5f, 2.0f);

		if (float.IsNaN(speedValue))
		{
			speedValue = 1.0f;
		}

		player.Rate = speedValue;
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

	internal AudioPlayer(AudioPlayerOptions audioPlayerOptions)
	{
		this.audioPlayerOptions = audioPlayerOptions;
	}

	public void SetSource(Stream audioStream)
	{
		var data = NSData.FromStream(audioStream)
				   ?? throw new FailedToLoadAudioException("Unable to convert audioStream to NSData.");
		player = AVAudioPlayer.FromData(data)
				 ?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from data.");

		PreparePlayer();
	}

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
			ActiveSessionHelper.FinishSession(audioPlayerOptions);

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
			player.Pause();
			player.CurrentTime = 0;
		}
		else if (CurrentPosition >= Duration)
		{
			player.CurrentTime = 0;
		}

		player.Play();
	}

	public void Seek(double position) => player.CurrentTime = position;

	public void Stop()
	{
		player.Stop();
		Seek(0);
		PlaybackEnded?.Invoke(this, EventArgs.Empty);
	}

	bool PreparePlayer()
	{
		ActiveSessionHelper.InitializeSession(audioPlayerOptions);

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