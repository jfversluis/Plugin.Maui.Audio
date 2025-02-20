namespace Plugin.Maui.Audio;

static class DefaultAudioRecordingOptions
{
	public static AudioRecordingOptions DefaultOptions = new()
	{
		SampleRate = AudioRecordingOptions.DefaultSampleRate,
		Channels = AudioRecordingOptions.DefaultChannels,
		BitDepth = AudioRecordingOptions.DefaultBitDepth,
		Encoding = AudioRecordingOptions.DefaultEncoding,
		BitRate = AudioRecordingOptions.DefaultBitRate,
		CompressionQuality = AudioRecordingOptions.DefaultCompressionQuality,
		ThrowIfNotSupported = true
	};

}

public class AudioRecordingOptions
{
	public const int DefaultSampleRate = 44100; // Sets frequency range (max audio freq = 1/2 sample rate)
	public const ChannelType DefaultChannels = ChannelType.Mono; // Mono or Stereo
	public const BitDepth DefaultBitDepth = BitDepth.Pcm16bit; // Used by Wav (pcm), Flac, Alac
	public const Encoding DefaultEncoding = Encoding.Aac; // Uses AAC in M4A/MP4 container by default
	public const CompressionQuality DefaultCompressionQuality = CompressionQuality.Medium; // Not currently implemented
	public const int DefaultBitRate = 128000; // i.e. 128 kbps by default
	
	/// <summary>
	/// Sample rate of the audio recording.
	/// 44,100Hz is currently the only rate that is guaranteed to work on all devices on Android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public int SampleRate { get; set; } = DefaultSampleRate;

	/// <summary>
	/// Channels of the audio recording.
	/// Mono is guaranteed to work on all devices on Android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public ChannelType Channels { get; set; } = DefaultChannels;

	/// <summary>
	/// Bit depth of the audio recording.
	/// </summary>
	public BitDepth BitDepth { get; set; } = DefaultBitDepth;

	/// <summary>
	/// Encoding type of the audio recording.
	/// </summary>
	public Encoding Encoding { get; set; } = DefaultEncoding;

	/// <summary>
	/// Bit rate of the audio recording if using audio compression like AAC
	/// Common bit rates such as 64 kbps, 96 kbps, 128 kbps, 192 kbps, 256 kbps, or 320 kbps
	/// </summary>
	public int BitRate { get; set; } = DefaultBitRate;

	/// <summary>
	/// Compression Quality of the audio recording (if using compression algorithm like AAC).
	/// </summary>
	public CompressionQuality CompressionQuality { get; set; } = DefaultCompressionQuality;

	/// <summary>
	/// Gets or sets whether the functionality will thrown an exception if the configured recording options are not supported.
	/// </summary>
	public bool ThrowIfNotSupported { get; set; } = false;
}
