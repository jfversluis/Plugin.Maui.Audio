namespace Plugin.Maui.Audio.AudioListeners;

public class SilenceListener : IPcmAudioListener
{
	uint channelCount;
	uint bytesPerSample;

	float amplitudeSampleThreshold;
	double ellapsedMillisecondsSinceDetectedSound;
	
	public SilenceListener()
	{
		Channels = ChannelType.Mono;
		BitDepth = BitDepth.Pcm16bit;
		SampleRate = DeviceInfo.Platform == DevicePlatform.Android ? 44100 : 48000;
		SilenceThresholdInDb = -40;
		MinimalSilenceTimespanInMilliseconds = 200;

		Clear();
	}

	public int SampleRate { get; set; }

	ChannelType channels;
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
	public BitDepth BitDepth
	{
		get => bitDepth;
		set
		{
			bitDepth = value;
			bytesPerSample = (uint)bitDepth / 8;
		}
	}

	public event EventHandler<IsSilentChangedEventArgs>? IsSilentChanged;

	bool? isSilent;
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

	public void Clear()
	{
		IsSilent = null;
		ellapsedMillisecondsSinceDetectedSound = MinimalSilenceTimespanInMilliseconds;
	}

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