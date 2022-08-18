namespace Plugin.Maui.SimpleAudioPlayer;

public interface IAudioManager
{
    /// <inheritdoc />
    ISimpleAudioPlayer CreatePlayer(Stream audioStream) => new AudioPlayer(audioStream);

    /// <inheritdoc />
    ISimpleAudioPlayer CreatePlayer(string fileName) => new AudioPlayer(fileName);
}
