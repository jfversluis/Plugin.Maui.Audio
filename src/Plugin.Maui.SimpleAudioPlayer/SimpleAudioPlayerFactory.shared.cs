namespace Plugin.Maui.SimpleAudioPlayer;

public class SimpleAudioPlayerFactory : ISimpleAudioPlayerFactory
{
    public ISimpleAudioPlayer CreatePlayer() => new SimpleAudioPlayerImplementation();
}
