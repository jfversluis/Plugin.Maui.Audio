namespace Plugin.Maui.Audio;

/// <summary>
/// Event arguments containing audio stream data.
/// </summary>
public class AudioStreamEventArgs
{
	/// <summary>
	/// Gets the audio data as a byte array.
	/// </summary>
	public byte[] Audio { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="AudioStreamEventArgs"/> class.
	/// </summary>
	/// <param name="audio">The audio data as a byte array.</param>
	public AudioStreamEventArgs(byte[] audio)
	{
		Audio = audio;
	}
}