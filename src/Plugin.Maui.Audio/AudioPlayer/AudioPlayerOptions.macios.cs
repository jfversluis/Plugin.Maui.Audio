using AVFoundation;

namespace Plugin.Maui.Audio;

partial class AudioPlayerOptions
{
    public AudioPlayerOptions()
    {
        Category = AVAudioSessionCategory.Playback;
    }
}