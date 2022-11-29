using System.Runtime.Versioning;
using Android.Content.Res;
using Android.Media;
using Stream = System.IO.Stream;
using Uri = Android.Net.Uri;

namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
    readonly MediaPlayer player;
    int index = 0;
    double volume = 0.5;
    double balance = 0;
    string path = string.Empty;
    readonly MemoryStream? stream;
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

	[SupportedOSPlatform("Android23.0")]
	public double Speed
	{
		get => player.PlaybackParams.Speed;
		set
		{
			// Check if set speed is supported
			if (CanSetSpeed)
			{
                // Speed on Android can be between 0 and 6
                var speedValue = Math.Clamp((float)value, 0.0f, 6.0f);

                if (float.IsNaN(speedValue))
                    speedValue = 1.0f;

				player.PlaybackParams = player.PlaybackParams.SetSpeed(speedValue) ?? player.PlaybackParams;
			}
			else
			{
				throw new NotSupportedException("Set playback speed is not supported!");
			}
		}
	}

    public double MinimumSpeed => 0;

    public double MaximumSpeed => 6;

	public bool CanSetSpeed => Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M;

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

        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            stream = new MemoryStream();
            audioStream.CopyTo(stream);
            var mediaDataSource = new StreamMediaDataSource(stream);
            player.SetDataSource(mediaDataSource);
            player.Prepare();
        }
        else
        {
            PreparePlayerLegacy(audioStream);
        }
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

    void PreparePlayerLegacy(Stream audioStream)
    {
        //cache to the file system
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"cache{index++}.wav");

        DeleteFile(path);

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
                var encodedPath = Uri.Encode(path)
                    ?? throw new FailedToLoadAudioException("Unable to generate encoded path.");
                var uri = Uri.Parse(encodedPath)
                    ?? throw new FailedToLoadAudioException("Unable to parse encoded path.");

                player.SetDataSource(context, uri);
            }
            catch
            {
                //return false;
            }
        }

        player.Prepare();
	}

    static void DeleteFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) { return; }
        
        try
        {
            File.Delete(path);
        }
        catch
        {
        }
    }

    public void Play()
    {
        if (IsPlaying)
        {
            Pause();
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
            DeleteFile(path);
            path = string.Empty;
            stream?.Dispose();
        }

        isDisposed = true;
    }
}
