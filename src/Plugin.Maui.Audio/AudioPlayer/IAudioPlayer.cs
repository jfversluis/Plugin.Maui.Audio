namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to play audio.
/// </summary>
public interface IAudioPlayer : IDisposable
{
	///<Summary>
	/// Raised when audio playback completes successfully.
	///</Summary>
	event EventHandler PlaybackEnded;

	///<Summary>
	/// Gets the length of audio in seconds.
	///</Summary>
	double Duration { get; }

	///<Summary>
	/// Gets the current position of audio playback in seconds.
	///</Summary>
	double CurrentPosition { get; }

	///<Summary>
	/// Gets or sets the playback volume 0 to 1 where 0 is no-sound and 1 is full volume.
	///</Summary>
	double Volume { get; set; }

	///<Summary>
	/// Gets or sets the balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right.
	///</Summary>
	double Balance { get; set; }

	///<Summary>
	/// Gets or sets the playback speed where 1 is normal speed. <see cref="MinimumSpeed"/> and <see cref="MaximumSpeed"/> can be used to determine the minumum and maximum value for each platform.
	///</Summary>
	///<remarks>
	/// The minimum and maximum speeds that can be set here are different per platform. Setting values ouside of these ranges will not throw an exception, it will clamp to the minimum or maximum value.
	///<para>- Android: between 0 and 6. Setting the value to 0 will pause playback, playback will not be resumed when incrementing the value again.</para>
	///<para>- iOS: between 0.5 and 2.</para>
	///<para>- Windows: between 0 and 8. Setting the value to 0 will pause playback, playback will be resumed when incrementing the value again.</para>
	///</remarks>
	double Speed { get; set; }

	/// <summary>
	/// Gets the minimum speed value that can be set for <see cref="Speed"/> on this platform.
	/// </summary>
	double MinimumSpeed { get; }

	/// <summary>
	/// Gets the maximum speed value that can be set for <see cref="Speed"/> on this platform.
	/// </summary>
	double MaximumSpeed { get; }

	///<Summary>
	/// Gets a value indicating whether the playback speed can be changed.
	///</Summary>
	bool CanSetSpeed { get; }

	///<Summary>
	/// Gets a value indicating whether the currently loaded audio file is playing.
	///</Summary>
	bool IsPlaying { get; }

	///<Summary>
	/// Gets or sets whether the player will continuously repeat the currently playing sound.
	///</Summary>
	bool Loop { get; set; }

	///<Summary>
	/// Gets a value indicating whether the position of the loaded audio file can be updated.
	///</Summary>
	bool CanSeek { get; }

	///<Summary>
	/// Begin playback or resume if paused.
	///</Summary>
	void Play();

	///<Summary>
	/// Pause playback if playing (does not resume).
	///</Summary>
	void Pause();

	///<Summary>
	/// Stop playback and set the current position to the beginning.
	///</Summary>
	void Stop();

	///<Summary>
	/// Set the current playback position (in seconds).
	///</Summary>
	void Seek(double position);
}