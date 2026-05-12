using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioPlayerOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioPlayerOptions"/> class with default settings for macOS/iOS.
    /// </summary>
    public AudioPlayerOptions()
    {
        Category = AVAudioSessionCategory.Playback;
    }

    /// <summary>
    /// Gets or sets whether audio interruptions should be automatically handled. Default value: <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// When enabled (default), the player will automatically pause when interrupted (e.g., phone calls) and resume when appropriate.
    /// When disabled, the player will not respond to audio interruptions, giving you full control over interruption handling.
    /// See https://developer.apple.com/documentation/avfaudio/handling-audio-interruptions for more information.
    /// </remarks>
    public bool HandleAudioInterruptions { get; set; } = true;

    /// <summary>
    /// Gets or sets the preferred audio output port override for iOS/macOS. Default value: <see cref="AudioOutputPort.Default"/>.
    /// </summary>
    /// <remarks>
    /// This property allows you to override the audio output routing on iOS/macOS.
    /// For example, you can force audio to play through the device speaker even when Bluetooth is connected.
    /// <para>
    /// Note: This is a session-wide setting that affects all audio output on the device, not just this player.
    /// The override remains in effect until explicitly changed back to <see cref="AudioOutputPort.Default"/>.
    /// </para>
    /// </remarks>
    public AudioOutputPort PreferredOutputPort { get; set; } = AudioOutputPort.Default;
}