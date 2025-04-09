namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Interface for PCM audio listeners that can process ordered PCM audio data.
/// </summary>
public interface IPcmAudioListener
{
	/// <summary>
	/// Gets or sets the sample rate in Hz used for audio processing.
	/// </summary>
	int SampleRate { get; set; }
	
	/// <summary>
	/// Gets or sets the channel type (mono or stereo) used for audio processing.
	/// </summary>
	ChannelType Channels { get; set; }
	
	/// <summary>
	/// Gets or sets the bit depth used for audio processing.
	/// </summary>
	BitDepth BitDepth { get; set; }

	/// <summary>
	/// Processes ordered PCM audio data.
	/// </summary>
	/// <param name="audioEventArgs">The ordered audio data to process.</param>
	void HandleOrderedPcmAudio(OrderedAudioEventArgs audioEventArgs);

	/// <summary>
	/// Clear all data and reset values to initial state.
	/// </summary>
	void Clear();
}