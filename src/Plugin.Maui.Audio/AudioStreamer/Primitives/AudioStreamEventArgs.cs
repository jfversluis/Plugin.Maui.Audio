namespace Plugin.Maui.Audio;

public class AudioStreamEventArgs
{
	public byte[] Audio { get; private set; }

	public AudioStreamEventArgs(byte[] audio)
	{
		Audio = audio;
	}
}