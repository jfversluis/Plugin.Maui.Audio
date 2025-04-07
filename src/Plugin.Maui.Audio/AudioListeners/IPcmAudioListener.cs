namespace Plugin.Maui.Audio.AudioListeners;

public interface IPcmAudioListener
{
	int SampleRate { get; set; }
	ChannelType Channels { get; set; }
	BitDepth BitDepth { get; set; }

	void HandleOrderedPcmAudio(OrderedAudioEventArgs audioEventArgs);

	/// <summary>
	/// Clear all data and reset values to initial state
	/// </summary>
	void Clear();
}