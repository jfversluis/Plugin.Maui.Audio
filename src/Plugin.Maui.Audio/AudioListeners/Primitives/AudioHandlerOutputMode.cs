namespace Plugin.Maui.Audio.AudioListeners;

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