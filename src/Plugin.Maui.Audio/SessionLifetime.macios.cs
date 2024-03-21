namespace Plugin.Maui.Audio;

/// <summary>
/// Enumeration of the options for controlling the lifetime of an audio session.
/// </summary>
public enum SessionLifetime
{ 
    /// <summary>
    /// Keep the audio session alive after stopping.
    /// </summary>
    KeepSessionAlive,

    /// <summary>
    /// End the audio session after stopping.
    /// </summary>
    EndSession,

    /// <summary>
    /// End the audio session after stopping and notify other audio sessions.
    /// </summary>
    EndSessionAndNotifyOthers
 }