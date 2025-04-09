using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioRecorderOptions"/> class with default settings for macOS/iOS.
    /// </summary>
    public AudioRecorderOptions()
    {
        Category = AVAudioSessionCategory.Record;
    }
}