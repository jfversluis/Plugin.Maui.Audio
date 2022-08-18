namespace Plugin.Maui.SimpleAudioPlayer;

public class AudioManager : IAudioManager
{
    static IAudioManager? currentImplementation;

    public static IAudioManager Current => currentImplementation ??= new AudioManager();

    /// <inheritdoc />
    public ISimpleAudioPlayer CreatePlayer(Stream audioStream) => new AudioPlayer(audioStream);

    /// <inheritdoc />
    public ISimpleAudioPlayer CreatePlayer(string fileName) => new AudioPlayer(fileName);
}
