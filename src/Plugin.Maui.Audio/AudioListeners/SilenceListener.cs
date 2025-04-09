namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// A listener that detects silence in an audio stream based on configurable thresholds.
/// </summary>
public class SilenceListener : IPcmAudioListener
{
	uint channelCount;
	uint bytesPerSample;

	float amplitudeSampleThreshold;
	double ellapsedMillisecondsSinceDetectedSound;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SilenceListener"/> class with default settings.
	/// </summary>
	public SilenceListener()
	{
		Channels = ChannelType.Mono;
		BitDepth = BitDepth.Pcm16bit;
		SampleRate = DeviceInfo.Platform == DevicePlatform.Android ? 44100 : 48000;
		SilenceThresholdInDb = -40;
		MinimalSilenceTimespanInMilliseconds = 200;

		Clear();
	}

	/// <summary>
	/// Gets or sets the sample rate in Hz used for audio processing.
	/// </summary>
	public int SampleRate { get; set; }

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
			channelCount = (uint)channels;
		}
	}
	
	BitDepth bitDepth;
	/// <summary>
	/// Gets or sets the bit depth used for audio processing.
	/// </summary>
	public BitDepth BitDepth
	{
		get => bitDepth;
		set
		{
			bitDepth = value;
			bytesPerSample = (uint)bitDepth / 8;
		}
	}

	/// <summary>
	/// Event raised when the silence state changes.
	/// </summary>
	public event EventHandler<IsSilentChangedEventArgs>? IsSilentChanged;

	bool? isSilent;
	/// <summary>
	/// Gets a value indicating whether the audio stream is currently silent.
	/// </summary>
	/// <remarks>
	/// Returns null when the silence state is undetermined.
	/// </remarks>
	public bool? IsSilent
	{
		get => isSilent;
		private set
		{
			if (isSilent == value)
			{
				return;
			}

			isSilent = value;
			if (isSilent.HasValue)
			{
				IsSilentChanged?.Invoke(this, new IsSilentChangedEventArgs(isSilent.Value));
			}
		}
	}

	/// <summary>
	/// Minimal time it has to be silent to be considered as silence.
	/// </summary>
	public uint MinimalSilenceTimespanInMilliseconds { get; set; }

	int silenceThresholdInDb;
	/// <summary>
	/// Silence threshold in decibel (dB) value 
	/// (this system works with negative dB values, where lower in the negative value the softer the sound is)
	/// </summary>
	public int SilenceThresholdInDb
	{
		get => silenceThresholdInDb;
		set
		{
			if (silenceThresholdInDb == value)
			{
				return;
			}

			silenceThresholdInDb = value;
			SetAmplitudeSampleThreshold(silenceThresholdInDb);
		}
	}

	/// <summary>
	/// Clears all data and resets values to their initial state.
	/// </summary>
	public void Clear()
	{
		IsSilent = null;
		ellapsedMillisecondsSinceDetectedSound = MinimalSilenceTimespanInMilliseconds;
	}

	/// <summary>
	/// Processes ordered PCM audio data to detect silence.
	/// </summary>
	/// <param name="audioEventArgs">The ordered audio data to process.</param>
	public void HandleOrderedPcmAudio(OrderedAudioEventArgs audioEventArgs)
	{
		var orderedAudioSamples = audioEventArgs.OrderedAudio;

		if (ellapsedMillisecondsSinceDetectedSound > MinimalSilenceTimespanInMilliseconds)
		{
			ellapsedMillisecondsSinceDetectedSound = MinimalSilenceTimespanInMilliseconds;
		}

		var sampleCount = orderedAudioSamples.Length;
		var samplesTotalSeconds = (double)sampleCount / channelCount / (double)SampleRate;
		var samplesTotalMilliseconds = samplesTotalSeconds * 1000;
		ellapsedMillisecondsSinceDetectedSound += samplesTotalMilliseconds;

		bool isOverTimeSilenceDetected;

		for (var n = sampleCount; n > 0; n--)
		{
			var audioSample = orderedAudioSamples[n - 1];

			var soundDetected = Math.Abs(audioSample) > amplitudeSampleThreshold;
			if (!soundDetected)
			{
				continue;
			}

			var millisecondsPerSample = samplesTotalMilliseconds / sampleCount;
			var samplesChecktSinceNow = sampleCount - n;

			ellapsedMillisecondsSinceDetectedSound = millisecondsPerSample * samplesChecktSinceNow;

			isOverTimeSilenceDetected = ellapsedMillisecondsSinceDetectedSound > MinimalSilenceTimespanInMilliseconds;
			if (isOverTimeSilenceDetected)
			{
				IsSilent = isOverTimeSilenceDetected;
				return;
			}
		}

		isOverTimeSilenceDetected = ellapsedMillisecondsSinceDetectedSound > MinimalSilenceTimespanInMilliseconds;
		IsSilent = isOverTimeSilenceDetected;
	}

	void SetAmplitudeSampleThreshold(int silenceThresholdInDecibel)
	{
		var silenceThresholdAmplitude = PcmAudioHelpers.DecibelToAbsoluteRms(silenceThresholdInDecibel);
		amplitudeSampleThreshold = (float)(silenceThresholdAmplitude * Math.Pow(2, bytesPerSample * 8));
	}
}