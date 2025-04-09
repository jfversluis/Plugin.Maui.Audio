namespace Plugin.Maui.Audio;

/// <summary>
/// An <see cref="IAudioSource"/> implementation that returns an empty audio stream.
/// </summary>
public class EmptyAudioSource : IAudioSource
{
	/// <summary>
	/// Gets an empty audio stream.
	/// </summary>
	/// <returns>A null stream representing empty audio content.</returns>
	public Stream GetAudioStream() => Stream.Null;
}