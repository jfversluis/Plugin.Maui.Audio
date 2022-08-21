using System.ComponentModel;
using Plugin.Maui.Audio;

namespace AudioPlayerSample.ViewModels;

public class MusicPlayerPageViewModel : BaseViewModel, IQueryAttributable
{
	readonly IAudioManager audioManager;
	IAudioPlayer audioPlayer;
	private TimeSpan animationProgress;
	MusicItemViewModel musicItemViewModel;

	public MusicPlayerPageViewModel(IAudioManager audioManager)
	{
		this.audioManager = audioManager;

		PlayCommand = new Command(Play);
		PauseCommand = new Command(Pause);
		StopCommand = new Command(Stop);
	}

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(Routes.MusicPlayer.Arguments.Music, out object musicObject) &&
			musicObject is MusicItemViewModel musicItem)
		{
			MusicItemViewModel = musicItem;

			audioPlayer = audioManager.CreatePlayer(
				await FileSystem.OpenAppPackageFileAsync(musicItem.Filename));

			NotifyPropertyChanged(nameof(HasAudioSource));
		}
	}

	public MusicItemViewModel MusicItemViewModel
	{
		get => musicItemViewModel;
		set
		{
			musicItemViewModel = value;
			NotifyPropertyChanged();
		}
	}

	public bool HasAudioSource => audioPlayer is not null;

	public bool IsPlaying => audioPlayer?.IsPlaying ?? false;

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
		get => audioPlayer?.Volume ?? 1;
		set
		{
			if (audioPlayer != null)
			{
				audioPlayer.Volume = value;
			}
		}
	}

	public double Balance
	{
		get => audioPlayer?.Balance ?? 0;
		set
		{
			if (audioPlayer != null)
			{
				audioPlayer.Balance = value;
			}
		}
	}

	public bool Loop
	{
		get => audioPlayer?.Loop ?? false;
		set
		{
			audioPlayer.Loop = value;
		}
	}

	void Play()
	{
		audioPlayer.Play();

		NotifyPropertyChanged(nameof(IsPlaying));
	}

	void Pause()
	{
		if (audioPlayer.IsPlaying)
		{
			audioPlayer.Pause();
		}
		else
		{
			audioPlayer.Play();
		}

		NotifyPropertyChanged(nameof(IsPlaying));
	}

	void Stop()
	{
		if (audioPlayer.IsPlaying)
		{
			audioPlayer.Stop();

			AnimationProgress = TimeSpan.Zero;

			NotifyPropertyChanged(nameof(IsPlaying));
		}
	}
}
