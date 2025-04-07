namespace Plugin.Maui.Audio.AudioListeners;

public class IsSilentChangedEventArgs
{
	public bool? IsSilent { get; }

	public IsSilentChangedEventArgs(bool? isSilent)
	{
		IsSilent = isSilent;
	}
}