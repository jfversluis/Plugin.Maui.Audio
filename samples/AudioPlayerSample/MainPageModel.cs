using System.ComponentModel;
using System.Runtime.CompilerServices;
using Plugin.Maui.Audio;

namespace AudioPlayerSample;

public class MainPageModel : INotifyPropertyChanged
{
	readonly IAudioManager audioManager;
	IAudioPlayer audioPlayer;
	bool isPlaying;
	private double volume = 1;
	private double balance;
	private bool loop;
	private double currentPosition;
	private double duration;
	private TimeSpan animationProgress;

	public MainPageModel(IAudioManager audioManager)
	{
		this.audioManager = audioManager;
		PlayCommand = new Command(async () => await Play());
		PauseCommand = new Command(Pause);
		StopCommand = new Command(Stop);
	}

	public bool IsAnimationPlaying
	{
		get => isPlaying;
		private set
		{
			isPlaying = value;
			NotifyPropertyChanged();
		}
	}

	public TimeSpan AnimationProgress
	{
		get => animationProgress;
		set
		{
			animationProgress = value;
			NotifyPropertyChanged();
		}
	}

	public Command PlayCommand { get; set; }
	public Command PauseCommand { get; set; }
	public Command StopCommand { get; set; }

	public double Volume
	{
		get => volume;
		set
		{
			volume = value;

			if (audioPlayer != null)
			{
				audioPlayer.Volume = volume;
			}
		}
	}

	public double Balance
	{
		get => balance;
		set
		{
			balance = value;

			if (audioPlayer != null)
			{
				audioPlayer.Balance = balance;
			}
		}
	}

	public bool Loop
	{
		get => loop;
		set
		{
			loop = value;
			audioPlayer.Loop = loop;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	async Task Play()
	{
		audioPlayer = audioPlayer ?? audioManager.CreatePlayer(
			await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"));

		audioPlayer.Play();

		IsAnimationPlaying = true;
	}

	void Pause()
	{
		if (audioPlayer.IsPlaying)
		{
			audioPlayer.Pause();
			IsAnimationPlaying = false;
		}
		else
		{
			audioPlayer.Play();
			IsAnimationPlaying = true;
		}
	}

	void Stop()
	{
		if (audioPlayer.IsPlaying)
		{
			audioPlayer.Stop();
			IsAnimationPlaying = false;
			AnimationProgress = TimeSpan.Zero;
		}
	}

	void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
