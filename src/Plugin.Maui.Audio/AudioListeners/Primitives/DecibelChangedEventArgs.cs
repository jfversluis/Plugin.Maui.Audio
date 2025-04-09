namespace Plugin.Maui.Audio.AudioListeners;

public class DecibelChangedEventArgs
{
	public double Decibel { get; }

	public DecibelChangedEventArgs(double decibel)
	{
		Decibel = decibel;
	}
}