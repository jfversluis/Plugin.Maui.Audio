namespace Plugin.Maui.Audio.AudioListeners;

public class RmsChangedEventArgs
{
	public double Rms { get; }

	public RmsChangedEventArgs(double rms)
	{
		Rms = rms;
	}
}