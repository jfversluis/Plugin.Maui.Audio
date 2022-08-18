using Microsoft.Maui.Controls;
using Plugin.Maui.SimpleAudioPlayer;

namespace AudioPlayerSample;

public partial class MainPage : ContentPage
{
    private readonly IAudioManager audioManager;
    private ISimpleAudioPlayer simpleAudioPlayer;

    public MainPage(IAudioManager audioManager)
	{
		InitializeComponent();
        this.audioManager = audioManager;
    }

    async void btnPlay_Clicked(Object sender, EventArgs e)
    {
        // TODO: attribute https://download1.audiohero.com/track/40778468
        simpleAudioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"));
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
