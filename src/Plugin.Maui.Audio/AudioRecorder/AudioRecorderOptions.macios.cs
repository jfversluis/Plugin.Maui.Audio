using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorderOptions
{
    /// <summary>
    /// Gets or sets the category for the recording session.
    /// </summary>
    public AVAudioSessionCategory Category { get; set; } = AVAudioSessionCategory.Record;
    
    /// <summary>
    /// Gets or sets the mode for the recording session.
    /// </summary>
    public AVAudioSessionMode Mode { get; set; } = default;
    
    /// <summary>
    /// Gets or sets the options for the recording session category.
    /// </summary>
    public AVAudioSessionCategoryOptions CategoryOptions { get; set; } = default;
}