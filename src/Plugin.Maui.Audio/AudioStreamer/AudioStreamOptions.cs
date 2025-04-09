namespace Plugin.Maui.Audio;

/// <summary>
/// Options that can be configured for an audio streaming session.
/// </summary>
public partial class AudioStreamOptions : BaseOptions
{
	/// <summary>
	/// Sample rate of the audio recording.
	/// For iOS 44800 Hz is recommended, for Android 44,100Hz is currently the only rate that is guaranteed to work on all devices, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public int SampleRate { get; set; } = DeviceInfo.Platform == DevicePlatform.Android ? 44100 : 48000;

	/// <summary>
	/// Channels of the audio recording.
	/// Mono is guaranteed to work on all devices on Android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public ChannelType Channels { get; set; } = ChannelType.Mono;

	/// <summary>
	/// Bit depth of the audio recording.
	/// </summary>
	public BitDepth BitDepth { get; set; } = BitDepth.Pcm16bit;
}