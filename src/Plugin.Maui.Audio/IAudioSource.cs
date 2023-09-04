namespace Plugin.Maui.Audio;

/// <summary>
/// Definition of a source of audio.
/// </summary>
public interface IAudioSource
{
	/// <summary>
	/// Provides a <see cref="Stream"/> to allow for the playback of the audio contents.
	/// </summary>
	/// <returns>A <see cref="Stream"/> with the contents of the audio to play.</returns>
	Stream GetAudioStream();
}