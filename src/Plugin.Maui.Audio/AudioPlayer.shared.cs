namespace Plugin.Maui.Audio;

public partial class AudioPlayer : IAudioPlayer
{
#pragma warning disable CS0067
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

#if (NET6_0 && !ANDROID && !IOS && !MACCATALYST && !WINDOWS)

	public AudioPlayer(Stream audioStream) {}

	public AudioPlayer(string fileName) {}

	protected virtual void Dispose(bool disposing) {}

	public double Duration { get; }

	public double CurrentPosition { get; }

	public double Volume { get; set; }

	public double Balance { get; set; }

	public bool IsPlaying { get; }

	public bool Loop { get; set; }

	public bool CanSeek { get; }

	public void Play() {}

	public void Pause() {}

	public void Stop() {}

	public void Seek(double position) {}

#endif
}
