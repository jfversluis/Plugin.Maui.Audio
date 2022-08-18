using Microsoft.Maui.Controls;
using Plugin.Maui.SimpleAudioPlayer;

namespace AudioPlayerSample;

public partial class MainPage : ContentPage
{

/* Unmerged change from project 'AudioPlayerSample(net6.0-ios)'
Before:
    private readonly ISimpleAudioPlayer simpleAudioPlayer;
After:
    readonly ISimpleAudioPlayer simpleAudioPlayer;
*/

/* Unmerged change from project 'AudioPlayerSample(net6.0-maccatalyst)'
Before:
    private readonly ISimpleAudioPlayer simpleAudioPlayer;
After:
    readonly ISimpleAudioPlayer simpleAudioPlayer;
*/
	readonly ISimpleAudioPlayer simpleAudioPlayer;

	public MainPage(ISimpleAudioPlayer simpleAudioPlayer)
	{
		InitializeComponent();
		this.simpleAudioPlayer = simpleAudioPlayer;
	}

	async void btnPlay_Clicked(Object sender, EventArgs e)
	{
		// TODO: attribute https://download1.audiohero.com/track/40778468
		simpleAudioPlayer.Load(await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"));
		simpleAudioPlayer.Play();
	}

	void btnPause_Clicked(Object sender, EventArgs e)
	{
		if (simpleAudioPlayer.IsPlaying)
		{
			simpleAudioPlayer.Pause();
		}
		else
		{
			simpleAudioPlayer.Play();
		}

	}

	void btnStop_Clicked(Object sender, EventArgs e)
	{
		if (simpleAudioPlayer.IsPlaying)
		{
			simpleAudioPlayer.Stop();
		}
	}

	void sliderVolume_ValueChanged(Object sender,
		ValueChangedEventArgs e)
	{
		simpleAudioPlayer.Volume = e.NewValue;
	}

	void sliderBalance_ValueChanged(Object sender,
		ValueChangedEventArgs e)
	{
		simpleAudioPlayer.Balance = e.NewValue;
	}

	void Switch_Toggled(Object sender, ToggledEventArgs e)
	{
		simpleAudioPlayer.Loop = e.Value;
	}
}