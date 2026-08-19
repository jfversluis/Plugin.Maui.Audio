using AVFoundation;

namespace Plugin.Maui.Audio;

public partial class AudioStreamOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AudioStreamOptions"/> class with default settings for macOS/iOS.
	/// Sets the audio session category to <see cref="AVAudioSessionCategory.PlayAndRecord"/> to allow
	/// simultaneous audio recording and playback (e.g. using <see cref="IAudioPlayer"/> while streaming).
	/// <para>
	/// To enable Bluetooth microphone streaming (e.g. AirPods, headsets), set
	/// <see cref="BaseOptions.CategoryOptions"/> to <see cref="AVAudioSessionCategoryOptions.AllowBluetooth"/>
	/// and optionally set <see cref="BaseOptions.PreferredInput"/> to route to a specific device.
	/// </para>
	/// <para>
	/// If your app only needs recording without any playback, you can explicitly set
	/// <see cref="BaseOptions.Category"/> to <see cref="AVAudioSessionCategory.Record"/> for
	/// stricter audio routing behavior.
	/// </para>
	/// </summary>
	public AudioStreamOptions()
	{
		Category = AVAudioSessionCategory.PlayAndRecord;
	}
}