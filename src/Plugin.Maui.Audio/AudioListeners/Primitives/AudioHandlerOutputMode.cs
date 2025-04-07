namespace Plugin.Maui.Audio.AudioListeners;

public enum AudioHandlerOutputMode
{
	/// <summary>
	/// Convertion is done per sample
	/// </summary>
	Sample,
	/// <summary>
	/// Convertion is done per sample channel pair
	/// </summary>
	ChannelPair,
	/// <summary>
	/// Convertion is done for complete block
	/// </summary>
	Block
}