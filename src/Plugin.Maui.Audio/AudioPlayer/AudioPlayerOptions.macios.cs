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
	/// Gets or sets whether the audio session for this player should only be active while playing audio. Default value: <see langword="false"/>.
	/// </summary>
	/// <remarks>
	/// When enabled, the player will automatically create an audio session when playing, and close the audio session when stopped, finished, or paused.
	/// When disabled, the player will create an audio session on initialization, and only close this session on disposal.
	/// </remarks>
	public bool OnPlayAudioSession { get; set; } = false;
}