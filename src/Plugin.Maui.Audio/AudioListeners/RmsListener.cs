namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// A listener that calculates the Root Mean Square (RMS) value from the incoming audio stream.
/// </summary>
public class RmsListener : IPcmAudioListener
{
	readonly List<int> orderedAudioCache = [];
	readonly object orderedAudioCacheLock = new();
	int requiredSamplesForGivenTimespan;

	/// <summary>
	/// Initializes a new instance of the <see cref="RmsListener"/> class with default settings.
	/// </summary>
	public RmsListener()
	{
		Channels = ChannelType.Mono;
		BitDepth = BitDepth.Pcm16bit;
		SampleRate = DeviceInfo.Platform == DevicePlatform.Android ? 44100 : 48000;

		MeasuringIntervalInMilliseconds = 100;

		Clear();
	}

	/// <summary>
	/// Event raised when the measured RMS value changes.
	/// </summary>
	public event EventHandler<RmsChangedEventArgs>? RmsChanged;

	double rms;
	/// <summary>
	/// Gets the current RMS (Root Mean Square) value of the audio stream.
	/// </summary>
	public double Rms
	{
		get => rms;
		private set
		{
			if (Math.Abs(rms - value) < 0.00000001)
			{
				return;
			}

			rms = value;
			RmsChanged?.Invoke(this, new RmsChangedEventArgs(rms));
		}
	}

	uint measuringIntervalInMilliseconds;
	/// <summary>
	/// The time interval in milliseconds for which the rms level is calculated.
	/// Value 0 means the rms level is calculated for each sample, regardless of the size.
	/// </summary>
	public uint MeasuringIntervalInMilliseconds
	{
		get => measuringIntervalInMilliseconds;
		set
		{
			measuringIntervalInMilliseconds = value;
			SetRequiredSamplesForGivenTimespan();
		}
	}

	/// <summary>
	/// Gets or sets the bit depth used for audio processing.
	/// </summary>
	public BitDepth BitDepth { get; set; }

	int sampleRate;
	/// <summary>
	/// Gets or sets the sample rate in Hz used for audio processing.
	/// </summary>
	public int SampleRate
	{
		get => sampleRate;
		set
		{
			sampleRate = value;
			SetRequiredSamplesForGivenTimespan();
		}
	}

	ChannelType channels;
	/// <summary>
	/// Gets or sets the channel type (mono or stereo) used for audio processing.
	/// </summary>
	public ChannelType Channels
	{
		get => channels;
		set
		{
			channels = value;
			SetRequiredSamplesForGivenTimespan();
		}
	}

	/// <summary>
	/// Processes ordered PCM audio data to calculate RMS values.
	/// </summary>
	/// <param name="audioEventArgs">The ordered audio data to process.</param>
	public void HandleOrderedPcmAudio(OrderedAudioEventArgs audioEventArgs)
	{
		double? currentRms = null;

		lock (orderedAudioCacheLock)
		{
			if (requiredSamplesForGivenTimespan > 0)
			{
				orderedAudioCache.AddRange(audioEventArgs.OrderedAudio);

				while (orderedAudioCache.Count >= requiredSamplesForGivenTimespan)
				{
					var audioSamples = orderedAudioCache.Take(requiredSamplesForGivenTimespan).ToArray();
					orderedAudioCache.RemoveRange(0, requiredSamplesForGivenTimespan);

					currentRms = PcmAudioHelpers.CalculateRms(audioSamples, BitDepth);
				}
			}
			else
			{
				currentRms = PcmAudioHelpers.CalculateRms(audioEventArgs.OrderedAudio, BitDepth);
			}
		}

		if (currentRms.HasValue)
		{
			Rms = currentRms.Value;
		}
	}

	/// <summary>
	/// Clears all data and resets values to their initial state.
	/// </summary>
	public void Clear()
	{
		Rms = 0.0;

		lock (orderedAudioCacheLock)
		{
			orderedAudioCache.Clear();
		}
	}

	void SetRequiredSamplesForGivenTimespan()
	{
		if (MeasuringIntervalInMilliseconds == 0)
		{
			requiredSamplesForGivenTimespan = 0;
			return;
		}

		var expectedSamplesPerSecond = SampleRate / (uint)Channels;
		requiredSamplesForGivenTimespan = (int)(expectedSamplesPerSecond * ((double)MeasuringIntervalInMilliseconds / 1000));
	}
}