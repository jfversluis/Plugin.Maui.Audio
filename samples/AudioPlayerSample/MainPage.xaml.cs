using Plugin.Maui.Audio;

namespace AudioPlayerSample;

public partial class MainPage : ContentPage
{
    readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;
    readonly Animation playingAnimation;
	IAudioPlayer simpleAudioPlayer;
    bool isPositionChangeSystemDriven;

    public MainPage(
        IAudioManager audioManager,
        IDispatcher dispatcher)
	{
		InitializeComponent();

		this.audioManager = audioManager;
		this.dispatcher = dispatcher;

        playingAnimation = new Animation
        {
            { 0.0, 0.5, new Animation(v => DotnetBotImage.Scale = v, 1.00, 1.05) },
			{ 0.5, 1.0, new Animation(v => DotnetBotImage.Scale = v, 1.05, 1.00) }
		};
	}

    async void btnPlay_Clicked(Object sender, EventArgs e)
    {
        // TODO: some kind of Lazy access might be nice but it gets complicated with async/await.
        if (simpleAudioPlayer is null)
        {
            simpleAudioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"));
        }

        simpleAudioPlayer.Play();

        // TODO: Possible MAUI bug as this appears to do nothing (only tested on iOS and macOS).
        sliderPosition.Maximum = simpleAudioPlayer.Duration;

		UpdateAnimation();
		UpdatePlaybackPosition();
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

            UpdatePlaybackPosition();
        }

        UpdateAnimation();
    }

    void btnStop_Clicked(object sender, EventArgs e)
    {
        if (simpleAudioPlayer.IsPlaying)
        {
            simpleAudioPlayer.Stop();

			UpdateAnimation();
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

	void sliderPosition_ValueChanged(object sender, ValueChangedEventArgs e)
	{
        if (isPositionChangeSystemDriven || simpleAudioPlayer.CanSeek is false)
        {
            return;
        }

        simpleAudioPlayer.Seek(e.NewValue);
	}

    void UpdateAnimation()
    {
        const string animationName = "MusicPlaying";

        if (simpleAudioPlayer.IsPlaying)
        {
            playingAnimation.Commit(
                DotnetBotImage,
                animationName,
                length: 1000,
                repeat: () => simpleAudioPlayer.IsPlaying);
        }
        else
        {
            DotnetBotImage.AbortAnimation(animationName);
        }
    }

	void UpdatePlaybackPosition()
	{
		if (simpleAudioPlayer.IsPlaying is false)
		{
			return;
		}

		dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				Console.WriteLine($"{simpleAudioPlayer.CurrentPosition} with duration of {simpleAudioPlayer.Duration}");

				isPositionChangeSystemDriven = true;

				sliderPosition.Value = simpleAudioPlayer.CurrentPosition;
				lblPosition.Text = simpleAudioPlayer.CurrentPosition.ToString();

				isPositionChangeSystemDriven = false;

				UpdatePlaybackPosition();
			});
	}
}
