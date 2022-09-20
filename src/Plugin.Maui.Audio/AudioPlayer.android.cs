using Android.Content.Res;
using Android.Media;
using Stream = System.IO.Stream;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
    readonly MediaPlayer player;
    double volume = 0.5;
    double balance = 0;
	readonly MemoryStream stream;
    bool isDisposed = false;

    public double Duration => player.Duration / 1000.0;

    public double CurrentPosition => player.CurrentPosition / 1000.0;

    public double Volume
    {
        get => volume;
        set => SetVolume(volume = value, Balance);
    }

    public double Balance
    {
        get => balance;
        set => SetVolume(Volume, balance = value);
    }

    public bool IsPlaying => player.IsPlaying;

    public bool Loop
    {
        get => player.Looping;
        set => player.Looping = value;
    }

    public bool CanSeek => true;

	internal AudioPlayer(Stream audioStream)
	{
		player = new MediaPlayer();
		player.Completion += OnPlaybackEnded;


		stream = new MemoryStream();
		audioStream.CopyTo(stream);
		var mediaDataSource = new StreamMediaDataSource(stream);
		player.SetDataSource(mediaDataSource);
		player.Prepare();
	}

    internal AudioPlayer(string fileName)
    {
        player = new MediaPlayer() { Looping = Loop };
        player.Completion += OnPlaybackEnded;

        AssetFileDescriptor afd = Android.App.Application.Context.Assets?.OpenFd(fileName)
            ?? throw new FailedToLoadAudioException("Unable to create AssetFileDescriptor.");

        player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);

        player.Prepare();
    }


    public void Play()
    {
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
        {
            return;
        }

        Pause();
        Seek(0);
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    public void Pause()
    {
        player.Pause();
    }

    public void Seek(double position)
    {
        player.SeekTo((int)(position * 1000D));
    }

    void SetVolume(double volume, double balance)
    {
        volume = Math.Clamp(volume, 0, 1);

        balance = Math.Clamp(balance, -1, 1);

        // Using the "constant power pan rule." See: http://www.rs-met.com/documents/tutorials/PanRules.pdf
        var left = Math.Cos((Math.PI * (balance + 1)) / 4) * volume;
        var right = Math.Sin((Math.PI * (balance + 1)) / 4) * volume;

        player.SetVolume((float)left, (float)right);
    }

    void OnPlaybackEnded(object? sender, EventArgs e)
    {
        PlaybackEnded?.Invoke(this, e);

        //this improves stability on older devices but has minor performance impact
        // We need to check whether the player is null or not as the user might have dipsosed it in an event handler to PlaybackEnded above.
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.M)
        {
            player.SeekTo(0);
            player.Stop();
            player.Prepare();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            player.Completion -= OnPlaybackEnded;
            player.Release();
            player.Dispose();
			stream?.Dispose();
         
        }

        isDisposed = true;
    }
}
