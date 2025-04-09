namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Event arguments for when the silence state changes.
/// </summary>
public class IsSilentChangedEventArgs
{
	/// <summary>
	/// Gets a value indicating whether the audio is currently silent.
	/// </summary>
	/// <remarks>
	/// Returns null when the silence state is undetermined.
	/// </remarks>
	public bool? IsSilent { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="IsSilentChangedEventArgs"/> class.
	/// </summary>
	/// <param name="isSilent">The current silence state.</param>
	public IsSilentChangedEventArgs(bool? isSilent)
	{
		IsSilent = isSilent;
	}
}