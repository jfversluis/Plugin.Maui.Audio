namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Specifies how audio data should be processed and broadcast to listeners.
/// </summary>
public enum AudioHandlerOutputMode
{
	/// <summary>
	/// Conversion is done per sample
	/// </summary>
	Sample,
	/// <summary>
	/// Conversion is done per sample channel pair
	/// </summary>
	ChannelPair,
	/// <summary>
	/// Conversion is done for complete block
	/// </summary>
	Block
}