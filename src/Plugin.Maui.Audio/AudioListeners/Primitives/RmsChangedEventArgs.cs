namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Event arguments for when the RMS (Root Mean Square) value changes.
/// </summary>
public class RmsChangedEventArgs
{
	/// <summary>
	/// Gets the current RMS (Root Mean Square) value.
	/// </summary>
	public double Rms { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RmsChangedEventArgs"/> class.
	/// </summary>
	/// <param name="rms">The current RMS value.</param>
	public RmsChangedEventArgs(double rms)
	{
		Rms = rms;
	}
}