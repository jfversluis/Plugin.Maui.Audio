namespace Plugin.Maui.Audio;

public class AudioStreamEventArgs
{
	public byte[] Data { get; private set; }

	public AudioStreamEventArgs(byte[] data)
	{
		Data = data;
	}
}