using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioRecorderOptions"/> class with default settings for macOS/iOS.
    /// Sets the audio session category to <see cref="AVAudioSessionCategory.Record"/> with
    /// <see cref="AVAudioSessionCategoryOptions.AllowBluetooth"/> enabled so that Bluetooth
    /// microphones (e.g. AirPods, headsets) are available as recording inputs.
    /// </summary>
    public AudioRecorderOptions()
    {
        Category = AVAudioSessionCategory.Record;
        CategoryOptions = AVAudioSessionCategoryOptions.AllowBluetooth;
    }
}