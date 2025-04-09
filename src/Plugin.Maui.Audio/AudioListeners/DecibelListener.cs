namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// A listener that uses the audio stream to determine the number of decibels currently coming through.
/// </summary>
public class DecibelListener : IPcmAudioListener
{
	readonly List<int> orderedAudioCache = [];
	readonly object orderedAudioCacheLock = new();
	int requiredSamplesForGivenTimespan;

	/// <summary>
	/// Initializes a new instance of the <see cref="DecibelListener"/> class with default settings.
	/// </summary>
	public DecibelListener()
	{
		Channels = ChannelType.Mono;
		BitDepth = BitDepth.Pcm16bit;
		SampleRate = DeviceInfo.Platform == DevicePlatform.Android ? 44100 : 48000;

		MeasuringIntervalInMilliseconds = 100;

		Clear();
	}

	/// <summary>
	/// Event raised when the measured decibel level changes beyond a threshold.
	/// </summary>
	public event EventHandler<DecibelChangedEventArgs>? DecibelChanged;

	double decibel;

	/// <summary>
	/// Gets the decibel value of the current audio stream.
	/// </summary>
	public double Decibel
	{
		get => decibel;
		private set
		{
			if (Math.Abs(decibel - value) < 0.01)
			{
				return;
			}

			decibel = value;
			DecibelChanged?.Invoke(this, new DecibelChangedEventArgs(decibel));
		}
	}

	uint measuringIntervalInMilliseconds;
	/// <summary>
	/// The time interval in milliseconds for which the decibel level is calculated.
	/// Value 0 means the decibel level is calculated for each sample, regardless of the size.
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
	/// Processes ordered PCM audio data to calculate decibel levels.
	/// </summary>
	/// <param name="audioEventArgs">The ordered audio data to process.</param>
	public void HandleOrderedPcmAudio(OrderedAudioEventArgs audioEventArgs)
	{
		double? currentDecibel = null;

		lock (orderedAudioCacheLock)
		{
			if (requiredSamplesForGivenTimespan > 0)
			{
				orderedAudioCache.AddRange(audioEventArgs.OrderedAudio);

				while (orderedAudioCache.Count >= requiredSamplesForGivenTimespan)
				{
					var audioSamples = orderedAudioCache.Take(requiredSamplesForGivenTimespan).ToArray();
					orderedAudioCache.RemoveRange(0, requiredSamplesForGivenTimespan);

					var rms = PcmAudioHelpers.CalculateRms(audioSamples, BitDepth);
					currentDecibel = PcmAudioHelpers.RmsToDecibel(rms);
				}
			}
			else
			{
				var rms = PcmAudioHelpers.CalculateRms(audioEventArgs.OrderedAudio, BitDepth);
				currentDecibel = PcmAudioHelpers.RmsToDecibel(rms);
			}
		}

		if (currentDecibel.HasValue)
		{
			Decibel = currentDecibel.Value;
		}
	}

	/// <inheritdoc/>
	public void Clear()
	{
		Decibel = 0.0;

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