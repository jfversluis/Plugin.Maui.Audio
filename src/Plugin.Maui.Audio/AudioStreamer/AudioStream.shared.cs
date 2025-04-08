
namespace Plugin.Maui.Audio;

partial class AudioStream
{
	public AudioStream(AudioStreamOptions options)
	{
		BitDepth = options.BitDepth;
		Channels = options.Channels;
		SampleRate = options.SampleRate;
	}

	public int SampleRate { get; }

	public ChannelType Channels { get; }

	public BitDepth BitDepth { get; }
}