using System;
namespace Plugin.Maui.Audio;

/// <summary>
/// Interface for SimpleAudioPlayer
/// </summary>
public interface IAudioRecorder
{
    ///<Summary>
    /// Check if the executing device is capable of recording audio
    ///</Summary>
    bool CanRecordAudio { get; }

    ///<Summary>
    /// Check if the executing device is capable of recording audio
    ///</Summary>
    bool IsRecording { get; }

    ///<Summary>
    /// Start recording 
    ///</Summary>
    Task StartAsync();

    ///<Summary>
    /// Stop recording and return the AudioRecording instance with the recording data
    ///</Summary>
    Task<IAudioSource> StopAsync();
}
