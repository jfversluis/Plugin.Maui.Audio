using AVFoundation;

namespace Plugin.Maui.Audio;

public partial class AudioStreamOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AudioStreamOptions"/> class with default settings for macOS/iOS.
	/// </summary>
	public AudioStreamOptions()
	{
		Category = AVAudioSessionCategory.Record;
	}
}