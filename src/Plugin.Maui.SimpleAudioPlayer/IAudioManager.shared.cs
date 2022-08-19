namespace Plugin.Maui.SimpleAudioPlayer;

public interface IAudioManager
{
    /// <summary>
    /// Creates a new <see cref="ISimpleAudioPlayer"/> with the supplied <paramref name="audioStream"/> ready to play.
    /// </summary>
    /// <param name="audioStream">The <see cref="Stream"/> containing the audio to play.</param>
    /// <returns>A new <see cref="ISimpleAudioPlayer"/> with the supplied <paramref name="audioStream"/> ready to play.</returns>
    ISimpleAudioPlayer CreatePlayer(Stream audioStream) => new AudioPlayer(audioStream);

    /// <summary>
    /// Creates a new <see cref="ISimpleAudioPlayer"/> with the supplied <paramref name="fileName"/> ready to play.
    /// </summary>
    /// <param name="fileName">The name of the file containing the audio to play.</param>
    /// <returns>A new <see cref="ISimpleAudioPlayer"/> with the supplied <paramref name="fileName"/> ready to play.</returns>
    ISimpleAudioPlayer CreatePlayer(string fileName) => new AudioPlayer(fileName);
}
