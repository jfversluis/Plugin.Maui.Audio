using Android.Content.Res;
using Android.Media;
using Stream = System.IO.Stream;
using Uri = Android.Net.Uri;

namespace Plugin.Maui.SimpleAudioPlayer;

partial class AudioPlayer : ISimpleAudioPlayer
{
    readonly MediaPlayer player;
    static int index = 0;
    double _volume = 0.5;
    double _balance = 0;
    bool _loop;
    string path;
    bool isDisposed = false;

    public event EventHandler PlaybackEnded;

    public double Duration => player == null ? 0 : player.Duration / 1000.0;

    public double CurrentPosition => player == null ? 0 : (player.CurrentPosition) / 1000.0;

    public double Volume
    {
        get => _volume;
        set
        {
            SetVolume(_volume = value, Balance);
        }
    }

    public double Balance
    {
        get => _balance;
        set
        {
            SetVolume(Volume, _balance = value);
        }
    }

    public bool IsPlaying => player != null && player.IsPlaying;

    public bool Loop
    {
        get => _loop;
        set { _loop = value; if (player != null) player.Looping = _loop; }
    }

    public bool CanSeek => player != null;

    public AudioPlayer(Stream audioStream)
    {
        player = new Android.Media.MediaPlayer() { Looping = Loop };
        player.Completion += OnPlaybackEnded;

        DeleteFile(path);

        //cache to the file system
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"cache{index++}.wav");

        var fileStream = File.Create(path);
        audioStream.CopyTo(fileStream);
        fileStream.Close();

        try
        {
            player.SetDataSource(path);
        }
        catch
        {
            try
            {
                var context = Android.App.Application.Context;
                player?.SetDataSource(context, Uri.Parse(Uri.Encode(path)));
            }
            catch
            {
                //return false;
            }
        }

        PreparePlayer();
    }

    public AudioPlayer(string fileName)
    {
        player = new Android.Media.MediaPlayer() { Looping = Loop };
        player.Completion += OnPlaybackEnded;

        AssetFileDescriptor afd = Android.App.Application.Context.Assets.OpenFd(fileName);

        player?.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);

        PreparePlayer();
    }

    bool PreparePlayer()
    {
        player?.Prepare();

        return player != null;
    }

    void DeleteFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) == false)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
        }
    }

    public void Play()
    {
        if (player == null)
            return;

        if (IsPlaying)
        {
            Pause();
            Seek(0);
        }

        player.Start();
    }

    public void Stop()
    {
        if (!IsPlaying)
            return;

        Pause();
        Seek(0);
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    public void Pause()
    {
        player?.Pause();
    }

    public void Seek(double position)
    {
        if (CanSeek)
            player?.SeekTo((int)(position * 1000D));
    }

    void SetVolume(double volume, double balance)
    {
        volume = Math.Max(0, volume);
        volume = Math.Min(1, volume);

        balance = Math.Max(-1, balance);
        balance = Math.Min(1, balance);

        // Using the "constant power pan rule." See: http://www.rs-met.com/documents/tutorials/PanRules.pdf
        var left = Math.Cos((Math.PI * (balance + 1)) / 4) * volume;
        var right = Math.Sin((Math.PI * (balance + 1)) / 4) * volume;

        player?.SetVolume((float)left, (float)right);
    }

    void OnPlaybackEnded(object sender, EventArgs e)
    {
        PlaybackEnded?.Invoke(sender, e);

        //this improves stability on older devices but has minor performance impact
        // We need to check whether the player is null or not as the user might have dipsosed it in an event handler to PlaybackEnded above.
        if (player != null && Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.M)
        {
            player.SeekTo(0);
            player.Stop();
            player.Prepare();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed || player == null)
            return;

        if (disposing)
        {
            if (player != null)
            {
                player.Completion -= OnPlaybackEnded;
                player.Release();
                player.Dispose();
            }

            DeleteFile(path);
            path = string.Empty;
        }

        isDisposed = true;
    }
}
