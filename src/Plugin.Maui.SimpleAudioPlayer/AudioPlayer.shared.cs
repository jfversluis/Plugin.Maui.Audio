namespace Plugin.Maui.SimpleAudioPlayer;

public partial class AudioPlayer : ISimpleAudioPlayer
{
    public event EventHandler? PlaybackEnded;

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
