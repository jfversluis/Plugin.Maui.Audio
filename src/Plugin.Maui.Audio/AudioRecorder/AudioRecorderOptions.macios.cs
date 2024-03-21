using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorderOptions
{
    public AudioRecorderOptions()
    {
        Category = AVAudioSessionCategory.Record;
    }
}