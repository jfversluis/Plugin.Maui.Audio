using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

/// <summary>
/// Platform-specific implementation of the <see cref="IAudioPlayer"/> interface for macOS/iOS platforms.
/// </summary>
partial class AudioPlayer : IAudioPlayer
{
	AVAudioPlayer player;
	readonly AudioPlayerOptions audioPlayerOptions;
	bool isDisposed;

	/// <summary>
	/// Gets the current position of audio playback in seconds.
	/// </summary>
	public double CurrentPosition => player.CurrentTime;

	/// <summary>
	/// Gets the length of audio in seconds.
	/// </summary>
	public double Duration => player.Duration;

	/// <summary>
	/// Gets or sets the playback volume 0 to 1 where 0 is no-sound and 1 is full volume.
	/// </summary>
	public double Volume
	{
		get => player.Volume;
		set => player.Volume = (float)Math.Clamp(value, 0, 1);
	}

	/// <summary>
	/// Gets or sets the balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right.
	/// </summary>
	public double Balance
	{
		get => player.Pan;
		set => player.Pan = (float)Math.Clamp(value, -1, 1);
	}

	/// <summary>
	/// Gets or sets the playback speed where 1 is normal speed.
	/// </summary>
	public double Speed
	{
		get => player?.Rate ?? 0;
		set => SetSpeedInternal(value);
	}

	/// <summary>
	/// Internal implementation for setting the playback speed.
	/// </summary>
	/// <param name="sp">The requested speed value to set.</param>
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

	/// <summary>
	/// Gets the minimum speed value that can be set for playback.
	/// </summary>
	public double MinimumSpeed => 0.5;

	/// <summary>
	/// Gets the maximum speed value that can be set for playback.
	/// </summary>
	public double MaximumSpeed => 2;

	/// <summary>
	/// Gets a value indicating whether the playback speed can be changed.
	/// </summary>
	public bool CanSetSpeed => true;

	/// <summary>
	/// Gets a value indicating whether the currently loaded audio file is playing.
	/// </summary>
	public bool IsPlaying => player.Playing;

	/// <summary>
	/// Gets or sets whether the player will continuously repeat the currently playing sound.
	/// </summary>
	public bool Loop
	{
		get => player.NumberOfLoops != 0;
		set => player.NumberOfLoops = value ? -1 : 0;
	}

	/// <summary>
	/// Gets a value indicating whether the position of the loaded audio file can be updated.
	/// </summary>
	public bool CanSeek => true;

	static NSData? emptySource;

	internal AudioPlayer(AudioPlayerOptions audioPlayerOptions)
	{
		if (emptySource == null)
		{
			byte[] empty = new byte[16];
			int sampleRate = 44100;
			var source = new RawAudioSource(empty, sampleRate, 1);
			emptySource = NSData.FromArray(source.Bytes);
		}

		player = AVAudioPlayer.FromData(emptySource)
				 ?? throw new FailedToLoadAudioException("Unable to create AVAudioPlayer from data.");

		this.audioPlayerOptions = audioPlayerOptions;

		PreparePlayer();
	}

	/// <summary>
	/// Sets the audio source to the specified stream.
	/// </summary>
	/// <param name="audioStream">The audio stream to use as the source.</param>
	/// <exception cref="FailedToLoadAudioException">Thrown when the audio stream cannot be loaded.</exception>
	public void SetSource(Stream audioStream)
	{
		if (player != null)
		{
			player.FinishedPlaying -= OnPlayerFinishedPlaying;
			player.DecoderError -= OnPlayerError;
			ActiveSessionHelper.FinishSession(audioPlayerOptions);
			Stop();
			player.Dispose();
		}

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

	/// <summary>
	/// Releases the unmanaged resources used by the <see cref="AudioPlayer"/> and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

	/// <summary>
	/// Pause playback if playing (does not resume).
	/// </summary>
	public void Pause() => player.Pause();

	/// <summary>
	/// Begin playback or resume if paused.
	/// </summary>
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

	/// <summary>
	/// Set the current playback position (in seconds).
	/// </summary>
	/// <param name="position">The position in seconds.</param>
	public void Seek(double position) => player.CurrentTime = position;

	/// <summary>
	/// Stop playback and set the current position to the beginning.
	/// </summary>
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
		player.DecoderError += OnPlayerError;

		player.EnableRate = true;
		player.PrepareToPlay();

		return true;
	}

	void OnPlayerError(object? sender, AVErrorEventArgs e)
	{
		OnError(e);
	}

	void OnPlayerFinishedPlaying(object? sender, AVStatusEventArgs e)
	{
		PlaybackEnded?.Invoke(this, e);
	}
}