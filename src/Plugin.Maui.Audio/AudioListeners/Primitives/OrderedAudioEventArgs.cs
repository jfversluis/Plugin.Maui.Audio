namespace Plugin.Maui.Audio.AudioListeners;

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

	public OrderedAudioEventArgs(int[] littleEndianOrderedAudio, byte[] originalAudio)
	{
		OrderedAudio = littleEndianOrderedAudio;
		OriginalAudio = originalAudio;
	}
}