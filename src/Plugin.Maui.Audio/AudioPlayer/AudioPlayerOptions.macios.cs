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
}