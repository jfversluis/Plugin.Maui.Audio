using Plugin.Maui.Audio;

namespace AudioPlayerSample;

public partial class MainPage : ContentPage
{
    readonly IAudioManager audioManager;
    IAudioPlayer simpleAudioPlayer;

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

    void btnPause_Clicked(object sender, EventArgs e)
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

    void btnStop_Clicked(object sender, EventArgs e)
    {
        if (simpleAudioPlayer.IsPlaying)
        {
            simpleAudioPlayer.Stop();
        }
    }

    void sliderVolume_ValueChanged(object sender,
        ValueChangedEventArgs e)
    {
        simpleAudioPlayer.Volume = e.NewValue;
    }

    void sliderBalance_ValueChanged(object sender,
        ValueChangedEventArgs e)
    {
        simpleAudioPlayer.Balance = e.NewValue;
    }

    void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        simpleAudioPlayer.Loop = e.Value;
    }
}
