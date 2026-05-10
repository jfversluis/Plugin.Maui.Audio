using AVFoundation;

namespace Plugin.Maui.Audio;

public partial class AudioStreamOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AudioStreamOptions"/> class with default settings for macOS/iOS.
	/// Sets the audio session category to <see cref="AVAudioSessionCategory.Record"/>.
	/// <para>
	/// To enable Bluetooth microphone streaming (e.g. AirPods, headsets), set
	/// <see cref="BaseOptions.CategoryOptions"/> to <see cref="AVAudioSessionCategoryOptions.AllowBluetooth"/>
	/// and optionally set <see cref="BaseOptions.PreferredInput"/> to route to a specific device.
	/// </para>
	/// </summary>
	public AudioStreamOptions()
	{
		Category = AVAudioSessionCategory.Record;
	}
}