using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorderOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioRecorderOptions"/> class with default settings for macOS/iOS.
    /// Sets the audio session category to <see cref="AVAudioSessionCategory.Record"/>.
    /// <para>
    /// To enable Bluetooth microphone recording (e.g. AirPods, headsets), set
    /// <see cref="BaseOptions.CategoryOptions"/> to <see cref="AVAudioSessionCategoryOptions.AllowBluetooth"/>
    /// and optionally set <see cref="BaseOptions.PreferredInput"/> to route to a specific device.
    /// </para>
    /// </summary>
    public AudioRecorderOptions()
    {
        Category = AVAudioSessionCategory.Record;
    }
}