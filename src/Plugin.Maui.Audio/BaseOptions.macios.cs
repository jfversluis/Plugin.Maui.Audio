using AVFoundation;

namespace Plugin.Maui.Audio;

partial class BaseOptions
{
    /// <summary>
    /// Gets or sets the category for the audio session.
    /// </summary>
    public AVAudioSessionCategory Category { get; set; } = AVAudioSessionCategory.Record;
    
    /// <summary>
    /// Gets or sets the mode for the audio session.
    /// </summary>
    public AVAudioSessionMode Mode { get; set; } = default;
    
    /// <summary>
    /// Gets or sets the options for the audio session category.
    /// </summary>
    public AVAudioSessionCategoryOptions CategoryOptions { get; set; } = default;

    /// <summary>
    /// Gets or sets the lifetime of the underlying audio session - basically whether the AVAudioSession will stay active or be deactivated.
    /// </summary>
    public SessionLifetime SessionLifetime { get; set; } = default;

    /// <summary>
    /// Gets or sets the preferred input port for recording.
    /// <para>
    /// Set this to an <see cref="AVAudioSessionPortDescription"/> obtained from
    /// <see cref="AVAudioSession.AvailableInputs"/> to record from a specific device
    /// (e.g. a Bluetooth headset or external microphone). When <see langword="null"/>,
    /// the system default input device is used.
    /// </para>
    /// <para>
    /// The preferred input is applied after the audio session is activated. Per Apple's
    /// documentation, <c>SetPreferredInput</c> must be called after setting the session
    /// category and activating the session.
    /// </para>
    /// </summary>
    public AVAudioSessionPortDescription? PreferredInput { get; set; }
}