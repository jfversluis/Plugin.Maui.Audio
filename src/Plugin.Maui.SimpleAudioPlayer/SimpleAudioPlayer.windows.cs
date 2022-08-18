using System;
using System.IO;

namespace Plugin.Maui.SimpleAudioPlayer;

class SimpleAudioPlayerImplementation : ISimpleAudioPlayer
{
    public double Duration => throw new NotImplementedException();

    public double CurrentPosition => throw new NotImplementedException();

    public double Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public double Balance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsPlaying => throw new NotImplementedException();

    public bool Loop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool CanSeek => throw new NotImplementedException();

    public event EventHandler PlaybackEnded;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool Load(Stream audioStream)
    {
        throw new NotImplementedException();
    }

    public bool Load(string fileName)
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Play()
    {
        throw new NotImplementedException();
    }

    public void Seek(double position)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
