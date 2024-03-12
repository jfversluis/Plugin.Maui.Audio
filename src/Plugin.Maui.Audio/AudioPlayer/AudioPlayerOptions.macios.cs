using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioPlayerOptions
{
    /// <summary>
    /// Gets or sets the category for the playback session.
    /// </summary>
    public AVAudioSessionCategory Category { get; set; } = AVAudioSessionCategory.Playback;
    
    /// <summary>
    /// Gets or sets the mode for the playback session.
    /// </summary>
    public AVAudioSessionMode Mode { get; set; } = default;
    
    /// <summary>
    /// Gets or sets the options for the playback session category.
    /// </summary>
    public AVAudioSessionCategoryOptions CategoryOptions { get; set; } = default;
}