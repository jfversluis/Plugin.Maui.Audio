namespace Plugin.Maui.SimpleAudioPlayer;

public partial class AudioPlayer : ISimpleAudioPlayer
{
    ~AudioPlayer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }
}
