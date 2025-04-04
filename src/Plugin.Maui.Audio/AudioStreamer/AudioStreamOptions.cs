namespace Plugin.Maui.Audio;

public partial class AudioStreamOptions : BaseOptions, IEquatable<AudioStreamOptions>
{
	/// <summary>
	/// Sample rate of the audio recording.
	/// For iOS 44800 Hz is recommended, for Android 44,100Hz is currently the only rate that is guaranteed to work on all devices, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public int SampleRate { get; set; } = DeviceInfo.Platform == DevicePlatform.Android ? 44100 : 48000;

	/// <summary>
	/// Channels of the audio recording.
	/// Mono is guaranteed to work on all devices on Android, <see href="https://developer.android.com/reference/android/media/AudioRecord.html">Android audio record documentation</see>
	/// </summary>
	public ChannelType Channels { get; set; } = ChannelType.Mono;

	/// <summary>
	/// Bit depth of the audio recording.
	/// </summary>
	public BitDepth BitDepth { get; set; } = BitDepth.Pcm16bit;

	public bool Equals(AudioStreamOptions? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return SampleRate == other.SampleRate 
		       && Channels == other.Channels 
		       && BitDepth == other.BitDepth;
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((AudioStreamOptions)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(SampleRate, (int)Channels, (int)BitDepth);
	}
}