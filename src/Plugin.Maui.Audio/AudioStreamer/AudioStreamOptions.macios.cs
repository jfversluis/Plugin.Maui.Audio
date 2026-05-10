using AVFoundation;

namespace Plugin.Maui.Audio;

public partial class AudioStreamOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AudioStreamOptions"/> class with default settings for macOS/iOS.
	/// Sets the audio session category to <see cref="AVAudioSessionCategory.Record"/> with
	/// <see cref="AVAudioSessionCategoryOptions.AllowBluetooth"/> enabled so that Bluetooth
	/// microphones (e.g. AirPods, headsets) are available as streaming inputs.
	/// </summary>
	public AudioStreamOptions()
	{
		Category = AVAudioSessionCategory.Record;
		CategoryOptions = AVAudioSessionCategoryOptions.AllowBluetooth;
	}
}