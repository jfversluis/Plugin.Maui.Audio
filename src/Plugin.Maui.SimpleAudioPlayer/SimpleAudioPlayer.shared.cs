namespace Plugin.Maui.SimpleAudioPlayer;

/// <summary>
/// Provides simple audio playback support.
/// </summary>
public class SimpleAudioPlayer : ISimpleAudioPlayer
{
    static ISimpleAudioPlayer currentImplementation;
    readonly static SimpleAudioPlayerFactory simpleAudioPlayerFactory = new SimpleAudioPlayerFactory();

    private SimpleAudioPlayer()
    {
    }

    ///// <summary>
    ///// Implementation factory provider for use in registering a transient or scoped implementation of <see cref="ISimpleAudioPlayer"/>.
    ///// </summary>
    //public static Func<IServiceProvider, ISimpleAudioPlayer> ImplementationFactory { get; } =
    //    __ => simpleAudioPlayerFactory.CreatePlayer();

    /// <summary>
    /// Singleton instance of <see cref="ISimpleAudioPlayer"/>.
    /// </summary>
    public static ISimpleAudioPlayer Current => currentImplementation ??= simpleAudioPlayerFactory.CreatePlayer();

    /// <inheritdoc />
    public double Duration => Current.Duration;

    /// <inheritdoc />
    public double CurrentPosition => Current.CurrentPosition;
    
    /// <inheritdoc />
    public double Volume
    {
        get => Current.Volume;
        set => Current.Volume = value;
    }

    /// <inheritdoc />
    public double Balance
    {
        get => Current.Balance;
        set => Current.Balance = value;
    }

    /// <inheritdoc />
    public bool IsPlaying => Current.IsPlaying;

    /// <inheritdoc />
    public bool Loop
    {
        get => Current.Loop;
        set => Current.Loop = value;
    }

    /// <inheritdoc />
    public bool CanSeek => Current.CanSeek;

    /// <inheritdoc />
    public event EventHandler PlaybackEnded
    {
        add => Current.PlaybackEnded += value;
        remove => Current.PlaybackEnded -= value;
    }

    /// <inheritdoc />
    public void Dispose() => Current.Dispose();

    /// <inheritdoc />
    public bool Load(Stream audioStream) => Current.Load(audioStream);

    /// <inheritdoc />
    public bool Load(string fileName) => Current.Load(fileName);

    /// <inheritdoc />
    public void Pause() => Current.Pause();

    /// <inheritdoc />
    public void Play() => Current.Play();

    /// <inheritdoc />
    public void Seek(double position) => Current.Seek(position);

    /// <inheritdoc />
    public void Stop() => Current.Stop();
}