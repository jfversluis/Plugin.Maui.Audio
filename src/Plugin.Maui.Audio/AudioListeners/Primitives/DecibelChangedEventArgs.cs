namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Event arguments for when the decibel level changes.
/// </summary>
public class DecibelChangedEventArgs
{
	/// <summary>
	/// Gets the current decibel level.
	/// </summary>
	public double Decibel { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DecibelChangedEventArgs"/> class.
	/// </summary>
	/// <param name="decibel">The current decibel level.</param>
	public DecibelChangedEventArgs(double decibel)
	{
		Decibel = decibel;
	}
}