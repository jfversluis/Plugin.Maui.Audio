namespace Plugin.Maui.Audio;

/// <summary>
/// Platform-specific implementation of the <see cref="IAudioPlayer"/> interface for .NET.
/// </summary>
partial class AudioPlayer : IAudioPlayer
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AudioPlayer"/> class with the specified options.
	/// </summary>
	/// <param name="audioPlayerOptions">Options to configure the audio player behavior.</param>
	public AudioPlayer(AudioPlayerOptions audioPlayerOptions) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="AudioPlayer"/> class with the specified audio stream and options.
	/// </summary>
	/// <param name="audioStream">The audio stream to play.</param>
	/// <param name="audioPlayerOptions">Options to configure the audio player behavior.</param>
	public AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="AudioPlayer"/> class with the specified file name and options.
	/// </summary>
	/// <param name="fileName">The name of the audio file to play.</param>
	/// <param name="audioPlayerOptions">Options to configure the audio player behavior.</param>
	public AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions) { }

	/// <summary>
	/// Sets the audio source to the specified stream.
	/// </summary>
	/// <param name="audioStream">The audio stream to use as the source.</param>
	public void SetSource(Stream audioStream) { }

	/// <summary>
	/// Releases the unmanaged resources used by the <see cref="AudioPlayer"/> and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing) { }

	/// <summary>
	/// Gets the length of audio in seconds.
	/// </summary>
	public double Duration { get; }

	/// <summary>
	/// Gets the current position of audio playback in seconds.
	/// </summary>
	public double CurrentPosition { get; }

	/// <summary>
	/// Gets or sets the playback volume 0 to 1 where 0 is no-sound and 1 is full volume.
	/// </summary>
	public double Volume { get; set; }

	/// <summary>
	/// Gets or sets the balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right.
	/// </summary>
	public double Balance { get; set; }

	/// <summary>
	/// Gets a value indicating whether the currently loaded audio file is playing.
	/// </summary>
	public bool IsPlaying { get; }

	/// <summary>
	/// Gets or sets whether the player will continuously repeat the currently playing sound.
	/// </summary>
	public bool Loop { get; set; }

	/// <summary>
	/// Gets a value indicating whether the position of the loaded audio file can be updated.
	/// </summary>
	public bool CanSeek { get; }

	/// <summary>
	/// Begin playback or resume if paused.
	/// </summary>
	public void Play() { }

	/// <summary>
	/// Pause playback if playing (does not resume).
	/// </summary>
	public void Pause() { }

	/// <summary>
	/// Stop playback and set the current position to the beginning.
	/// </summary>
	public void Stop() { }

	/// <summary>
	/// Set the current playback position (in seconds).
	/// </summary>
	/// <param name="position">The position in seconds.</param>
	public void Seek(double position) { }

	/// <summary>
	/// Gets or sets the playback speed where 1 is normal speed.
	/// </summary>
	public double Speed { get; set; }

	/// <summary>
	/// Gets the minimum speed value that can be set for playback on this platform.
	/// </summary>
	public double MinimumSpeed { get; }

	/// <summary>
	/// Gets the maximum speed value that can be set for playback on this platform.
	/// </summary>
	public double MaximumSpeed { get; }

	/// <summary>
	/// Gets a value indicating whether the playback speed can be changed.
	/// </summary>
	public bool CanSetSpeed { get; }
}