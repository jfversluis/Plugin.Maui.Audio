namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to play audio.
/// </summary>
public interface IAudioPlayer : IAudio
{
	///<Summary>
	/// Raised when audio playback completes successfully.
	///</Summary>
	event EventHandler PlaybackEnded;

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

	/// <summary>
	/// Change current audio source, reusing the player instance.
	/// </summary>
	/// <param name="audioStream"></param>
	void SetSource(Stream audioStream);
}