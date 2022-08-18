using Windows.Media.Core;
using Windows.Media.Playback;

namespace Plugin.Maui.SimpleAudioPlayer;

class SimpleAudioPlayerImplementation : ISimpleAudioPlayer
{
    public event EventHandler PlaybackEnded;

    bool isDisposed;
    bool loop;
    MediaPlayer player;

    public double Duration => player is null ? 0 : player.PlaybackSession.NaturalDuration.TotalSeconds;

    public double CurrentPosition => player is null ? 0 : player.PlaybackSession.Position.TotalSeconds;

    public double Volume
    {
        get => player?.Volume ?? 0;
        set => SetVolume(value, Balance);
    }

    public double Balance
    {
        get => player?.AudioBalance ?? 0;
        set { SetVolume(Volume, value); }
    }

    public bool IsPlaying =>
        player?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing; //might need to expand

    public bool Loop
    {
        get => loop;
        set
        {
            loop = value;
            if (player is not null)
            {
                player.IsLoopingEnabled = loop;
            }
        }
    }

    public bool CanSeek => player is not null && player.PlaybackSession.CanSeek;

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

    void OnPlaybackEnded(MediaPlayer sender, object args)
    {
        PlaybackEnded?.Invoke(sender, EventArgs.Empty);
    }

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

    public void Pause()
    {
        player?.Pause();
    }

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
        return new MediaPlayer() { AutoPlay = false, IsLoopingEnabled = loop };
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
