using System.Buffers.Binary;

namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Orders PCM audio into little endian formatted audio and broadcasts it to registered listeners
/// Settings (SampleRate, ChannelType, BitDepth) are aligned to all listeners.
/// </summary>
public class PcmAudioHandler
{
	readonly List<IPcmAudioListener> listeners = [];
	int sampleRate;
	ChannelType channels;
	BitDepth bitDepth;
	uint bytesPerSample;

	public PcmAudioHandler(int sampleRate, ChannelType channels, BitDepth bitDepth)
	{
		this.sampleRate = sampleRate;
		this.channels = channels;
		this.bitDepth = bitDepth;

		bytesPerSample = (uint)BitDepth / 8;
	}

	public AudioHandlerOutputMode HandlerOutputMode { get; set; } = AudioHandlerOutputMode.Block;

	/// <summary>
	/// Optional audio output when not using listeners
	/// </summary>
	public event EventHandler<OrderedAudioEventArgs>? ConvertedAudio;

	public int SampleRate
	{
		get => sampleRate;
		set
		{
			sampleRate = value;
			AlignAllListenerSettings();
		}
	}

	public ChannelType Channels
	{
		get => channels;
		set
		{
			channels = value;
			AlignAllListenerSettings();
		}
	}

	public BitDepth BitDepth
	{
		get => bitDepth;
		set
		{
			bitDepth = value;
			bytesPerSample = (uint)BitDepth / 8;
			AlignAllListenerSettings();
		}
	}

	public void Subscribe(IPcmAudioListener listener)
	{
		if (listeners.Contains(listener))
		{
			return;
		}

		listeners.Add(listener);
		AlignListenerSettings(listener);
	}

	public void Unsubscribe(IPcmAudioListener listener)
	{
		if (listeners.Contains(listener))
		{
			listeners.Remove(listener);
		}
	}

	public void Clear()
	{
		foreach (var listener in listeners)
		{
			listener.Clear();
		}
	}

	public void HandlePcmAudio(byte[] audio)
	{
		var samples = new List<int>();
		var sampleBytesCache = new List<byte>();

		int sample = 0;

		for (var n = 0; n < audio.Length / bytesPerSample; n++)
		{
			byte[] sampleBytes = [];
			if (bytesPerSample == 1)
			{
				sampleBytes = new []{ audio[n] };
				sample = sampleBytes[0] - 128;
			}
			else if (bytesPerSample == 2)
			{
				sampleBytes = new byte[2] { audio[n * bytesPerSample], audio[n * bytesPerSample + 1] };
				sample = BinaryPrimitives.ReadInt16LittleEndian(sampleBytes);
			}
			else if (bytesPerSample == 4)
			{
				sampleBytes = new byte[4]
				{
					audio[n * bytesPerSample],
					audio[n * bytesPerSample + 1],
					audio[n * bytesPerSample + 2],
					audio[n * bytesPerSample + 3]
				};
				sample = BinaryPrimitives.ReadInt32LittleEndian(sampleBytes);
			}

			if (HandlerOutputMode == AudioHandlerOutputMode.Sample)
			{
				BroadcastAudio([sample], sampleBytes);
				break;
			}

			samples.Add(sample);

			if (HandlerOutputMode == AudioHandlerOutputMode.ChannelPair)
			{
				sampleBytesCache.AddRange(sampleBytes);

				if (samples.Count == (uint)Channels)
				{
					BroadcastAudio(samples.ToArray(), sampleBytesCache.ToArray());
					samples.Clear();
					sampleBytesCache.Clear();
				}
			}
		}

		if (HandlerOutputMode == AudioHandlerOutputMode.Block)
		{
			BroadcastAudio(samples.ToArray(), audio);
		}
	}

	public void BroadcastAudio(int[] littleEndian, byte[] original)
	{
		ConvertedAudio?.Invoke(this, new OrderedAudioEventArgs(littleEndian, original));

		foreach (var listener in listeners)
		{
			listener.HandleOrderedPcmAudio(new OrderedAudioEventArgs(littleEndian, original));
		}
	}

	void AlignListenerSettings(IPcmAudioListener listener)
	{
		listener.SampleRate = SampleRate;
		listener.BitDepth = BitDepth;
		listener.Channels = Channels;
	}

	void AlignAllListenerSettings()
	{
		foreach (var listener in listeners)
		{
			AlignListenerSettings(listener);
		}
	}
}