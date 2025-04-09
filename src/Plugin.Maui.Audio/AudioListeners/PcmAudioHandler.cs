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

	/// <summary>
	/// Initializes a new instance of the <see cref="PcmAudioHandler"/> class.
	/// </summary>
	/// <param name="sampleRate">The sample rate in Hz used for audio processing.</param>
	/// <param name="channels">The channel type (mono or stereo) used for audio processing.</param>
	/// <param name="bitDepth">The bit depth used for audio processing.</param>
	public PcmAudioHandler(int sampleRate, ChannelType channels, BitDepth bitDepth)
	{
		this.sampleRate = sampleRate;
		this.channels = channels;
		this.bitDepth = bitDepth;

		bytesPerSample = (uint)BitDepth / 8;
	}

	/// <summary>
	/// Gets or sets the mode that determines how audio data is processed and broadcast to listeners.
	/// </summary>
	public AudioHandlerOutputMode HandlerOutputMode { get; set; } = AudioHandlerOutputMode.Block;

	/// <summary>
	/// Optional audio output event raised when audio data is converted, even when not using listeners.
	/// </summary>
	public event EventHandler<OrderedAudioEventArgs>? ConvertedAudio;

	/// <summary>
	/// Gets or sets the sample rate in Hz used for audio processing.
	/// </summary>
	/// <remarks>
	/// When changed, the new value is propagated to all registered listeners.
	/// </remarks>
	public int SampleRate
	{
		get => sampleRate;
		set
		{
			sampleRate = value;
			AlignAllListenerSettings();
		}
	}

	/// <summary>
	/// Gets or sets the channel type (mono or stereo) used for audio processing.
	/// </summary>
	/// <remarks>
	/// When changed, the new value is propagated to all registered listeners.
	/// </remarks>
	public ChannelType Channels
	{
		get => channels;
		set
		{
			channels = value;
			AlignAllListenerSettings();
		}
	}

	/// <summary>
	/// Gets or sets the bit depth used for audio processing.
	/// </summary>
	/// <remarks>
	/// When changed, the new value is propagated to all registered listeners.
	/// </remarks>
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

	/// <summary>
	/// Registers a listener to receive audio data events.
	/// </summary>
	/// <param name="listener">The audio listener to register.</param>
	/// <remarks>
	/// The listener's settings will be aligned with the handler's current settings.
	/// </remarks>
	public void Subscribe(IPcmAudioListener listener)
	{
		if (listeners.Contains(listener))
		{
			return;
		}

		listeners.Add(listener);
		AlignListenerSettings(listener);
	}

	/// <summary>
	/// Unregisters a listener from receiving audio data events.
	/// </summary>
	/// <param name="listener">The audio listener to unregister.</param>
	public void Unsubscribe(IPcmAudioListener listener)
	{
		if (listeners.Contains(listener))
		{
			listeners.Remove(listener);
		}
	}

	/// <summary>
	/// Clears all data and resets values in all registered listeners.
	/// </summary>
	public void Clear()
	{
		foreach (var listener in listeners)
		{
			listener.Clear();
		}
	}

	/// <summary>
	/// Processes incoming PCM audio data and converts it for listeners.
	/// </summary>
	/// <param name="audio">The raw PCM audio data to process.</param>
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

	/// <summary>
	/// Broadcasts audio data to all registered listeners and raises the ConvertedAudio event.
	/// </summary>
	/// <param name="littleEndian">The audio data converted to little-endian formatted integers.</param>
	/// <param name="original">The original raw audio data bytes.</param>
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