using AVFoundation;

namespace Plugin.Maui.Audio;

public partial class AudioStreamOptions
{
	public AudioStreamOptions()
	{
		Category = AVAudioSessionCategory.Record;
	}
}