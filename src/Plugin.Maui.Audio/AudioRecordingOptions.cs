namespace Plugin.Maui.Audio;

static class DefaultAudioRecordingOptions
{
	public static AudioRecordingOptions DefaultOptions = new()
	{
		SampleRate = AudioRecordingOptions.DefaultSampleRate,
		Channels = AudioRecordingOptions.DefaultChannels,
		BitDepth = AudioRecordingOptions.DefaultBitDepth,
		Encoding = AudioRecordingOptions.DefaultEncoding,
		ThrowIfNotSupported = true
	};
}

public class AudioRecordingOptions
{
	public const int DefaultSampleRate = 44100;
	public const ChannelType DefaultChannels = ChannelType.Mono;
	public const BitDepth DefaultBitDepth = BitDepth.Pcm16bit;
	public const Encoding DefaultEncoding = Encoding.LinearPCM;

	

	/// <summary>
	/// Sample rate of the audio recording
	/// 44100Hz is currently the only rate that is guaranteed to work on all devices on android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">android audio record documentation</see>
	/// </summary>
	public int SampleRate { get; set; } = DefaultSampleRate;

	/// <summary>
	/// Channels of the audio recording.
	/// MONO is guaranteed to work on all devices on android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">android audio record documentation</see>
	/// </summary>
	public ChannelType Channels { get; set; } = DefaultChannels;

	/// <summary>
	/// Bit depth of the audio recording
	/// </summary>
	public BitDepth BitDepth { get; set; } = DefaultBitDepth;

	/// <summary>
	/// encoding type of the audio recording.
	/// </summary>
	public Encoding Encoding { get; set; } = DefaultEncoding;

	/// <summary>
	/// If true, will throw an exception if the recording options are not supported
	/// </summary>
	public bool ThrowIfNotSupported { get; set; } = false;


	
}
