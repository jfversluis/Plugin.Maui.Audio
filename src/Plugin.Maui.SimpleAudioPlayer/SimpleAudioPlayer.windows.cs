using Windows.Media.Core;
using Windows.Media.Playback;

namespace Plugin.Maui.SimpleAudioPlayer;

class SimpleAudioPlayerImplementation : ISimpleAudioPlayer
{
    public event EventHandler PlaybackEnded;

    bool isDisposed = false;
    bool _loop;
    MediaPlayer player;

    ///<Summary>
    /// Length of audio in seconds
    ///</Summary>
    public double Duration => player is null ? 0 : player.PlaybackSession.NaturalDuration.TotalSeconds;

    ///<Summary>
    /// Current position of audio in seconds
    ///</Summary>
    public double CurrentPosition => player is null ? 0 : player.PlaybackSession.Position.TotalSeconds;

    ///<Summary>
    /// Playback volume (0 to 1)
    ///</Summary>
    public double Volume
    {
        get => player?.Volume ?? 0;
        set => SetVolume(value, Balance);
    }

    ///<Summary>
    /// Balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right
    ///</Summary>
    public double Balance
    {
        get => player?.AudioBalance ?? 0;
        set { SetVolume(Volume, value); }
    }

    ///<Summary>
    /// Indicates if the currently loaded audio file is playing
    ///</Summary>
    public bool IsPlaying =>
        player?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing; //might need to expand

    ///<Summary>
    /// Continously repeats the currently playing sound
    ///</Summary>
    public bool Loop
    {
        get => _loop;
        set
        {
            _loop = value;
            if (player is not null)
            {
                player.IsLoopingEnabled = _loop;
            }
        }
    }

    ///<Summary>
    /// Indicates if the position of the loaded audio file can be updated
    ///</Summary>
    public bool CanSeek => player is not null && player.PlaybackSession.CanSeek;

    ///<Summary>
    /// Load wave or mp3 audio file from a stream
    ///</Summary>
    public bool Load(Stream audioStream)
    {
        DeletePlayer();

        player = GetPlayer();

        if (player is null)
        {
            return false;
        }

        player.Source = MediaSource.CreateFromStream(audioStream?.AsRandomAccessStream(), string.Empty);
        player.MediaEnded += OnPlaybackEnded;

        return player.Source != null;
    }

    ///<Summary>
    /// Load wave or mp3 audio file from assets folder in the UWP project
    ///</Summary>
    public bool Load(string fileName)
    {
        DeletePlayer();

        player = GetPlayer();

        if (player is null)
        {
            return false;
        }

        player.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/" + fileName));
        player.MediaEnded += OnPlaybackEnded;

        return player.Source != null;
    }

    void DeletePlayer()
    {
        Stop();

        if (player is not null)
        {
            player.MediaEnded -= OnPlaybackEnded;
            player.Dispose();
            player = null;
        }
    }

    private void OnPlaybackEnded(MediaPlayer sender, object args)
    {
        PlaybackEnded?.Invoke(sender, EventArgs.Empty);
    }

    ///<Summary>
    /// Begin playback or resume if paused
    ///</Summary>
    public void Play()
    {
        if (player?.Source is null)
        {
            return;
        }

        if (player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
        {
            Pause();
            Seek(0);   
        }

        player.Play();
    }

    ///<Summary>
    /// Pause playback if playing (does not resume)
    ///</Summary>
    public void Pause()
    {
        player?.Pause();
    }

    ///<Summary>
    /// Stop playack and set the current position to the beginning
    ///</Summary>
    public void Stop()
    {
        if (player is null)
        {
            return;
        }

        Pause();
        Seek(0);
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    ///<Summary>
    /// Seek a position in seconds in the currently loaded sound file 
    ///</Summary>
    public void Seek(double position)
    {
        if (player?.PlaybackSession is null)
        {
            return;
        }

        if (player.PlaybackSession.CanSeek)
        {
            player.PlaybackSession.Position = TimeSpan.FromSeconds(position);
        }
    }

    void SetVolume(double volume, double balance)
    {
        if (player is null || isDisposed)
        {
            return;
        }

        player.Volume = Math.Clamp(volume, 0, 1);
        player.AudioBalance = Math.Clamp(balance, -1, 1);
    }

    MediaPlayer GetPlayer()
    {
        return new MediaPlayer() { AutoPlay = false, IsLoopingEnabled = _loop };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed || player is null)
        {
            return;
        }

        if (disposing)
        {
            DeletePlayer();
        }

        isDisposed = true;
    }

    ~SimpleAudioPlayerImplementation()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }
}
