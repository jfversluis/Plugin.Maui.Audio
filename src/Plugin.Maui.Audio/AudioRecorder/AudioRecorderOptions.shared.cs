namespace Plugin.Maui.Audio;

/// <summary>
/// Options that can be configured for an audio recording session.
/// </summary>
public partial class AudioRecorderOptions : BaseOptions
{
	/// <summary>
	/// Sample rate of the audio recording.
	/// 44,100Hz is currently the only rate that is guaranteed to work on all devices on Android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public int SampleRate { get; set; } = 44100;

	/// <summary>
	/// Channels of the audio recording.
	/// Mono is guaranteed to work on all devices on Android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public ChannelType Channels { get; set; } = ChannelType.Mono;

	/// <summary>
	/// Bit depth of the audio recording.
	/// </summary>
	public BitDepth BitDepth { get; set; } = BitDepth.Pcm16bit;

	/// <summary>
	/// Encoding type of the audio recording.
	/// </summary>
	public Encoding Encoding { get; set; } = Encoding.Aac;

	/// <summary>
	/// Bit rate of the audio recording if using audio compression like AAC
	/// Common bit rates such as 64 kbps, 96 kbps, 128 kbps, 192 kbps, 256 kbps, or 320 kbps
	/// </summary>
	public int BitRate { get; set; } = 128000;

	/// <summary>
	/// Gets or sets whether the functionality will throw an exception if the configured recording options are not supported.
	/// </summary>
	public bool ThrowIfNotSupported { get; set; } = false;
}