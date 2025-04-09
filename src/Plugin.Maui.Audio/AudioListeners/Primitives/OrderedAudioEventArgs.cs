namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Event arguments containing ordered PCM audio data.
/// </summary>
public class OrderedAudioEventArgs
{
	/// <summary>
	/// Little endian ordered audio data
	/// </summary>
	public int[] OrderedAudio { get; }

	/// <summary>
	/// Original (L)PCM audio data
	/// </summary>
	public byte[] OriginalAudio { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderedAudioEventArgs"/> class.
	/// </summary>
	/// <param name="littleEndianOrderedAudio">Audio data in little-endian format as integers.</param>
	/// <param name="originalAudio">Original raw audio data as bytes.</param>
	public OrderedAudioEventArgs(int[] littleEndianOrderedAudio, byte[] originalAudio)
	{
		OrderedAudio = littleEndianOrderedAudio;
		OriginalAudio = originalAudio;
	}
}