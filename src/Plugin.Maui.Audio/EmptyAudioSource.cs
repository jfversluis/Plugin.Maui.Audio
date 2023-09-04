namespace Plugin.Maui.Audio;

public class EmptyAudioSource : IAudioSource
{
	public Stream GetAudioStream() => Stream.Null;
}