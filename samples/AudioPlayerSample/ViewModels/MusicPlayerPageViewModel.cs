using System.ComponentModel;
using Microsoft.Maui.Dispatching;
using Plugin.Maui.Audio;

namespace AudioPlayerSample.ViewModels;

public class MusicPlayerPageViewModel : BaseViewModel, IQueryAttributable, IDisposable
{
	readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;
	IAudioPlayer audioPlayer;
	TimeSpan animationProgress;
	MusicItemViewModel musicItemViewModel;
	bool isPositionChangeSystemDriven;
	bool isDisposed;

	public MusicPlayerPageViewModel(
		IAudioManager audioManager,
		IDispatcher dispatcher)
	{
		this.audioManager = audioManager;
		this.dispatcher = dispatcher;

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

			audioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(musicItem.Filename));

			NotifyPropertyChanged(nameof(HasAudioSource));
			NotifyPropertyChanged(nameof(Duration));
			NotifyPropertyChanged(nameof(CanSetSpeed));
			NotifyPropertyChanged(nameof(MinimumSpeed));
			NotifyPropertyChanged(nameof(MaximumSpeed));
		}
	}

	public double CurrentPosition
	{
		get => audioPlayer?.CurrentPosition ?? 0;
		set
		{
			if (audioPlayer is not null &&
				audioPlayer.CanSeek &&
				isPositionChangeSystemDriven is false)
			{
				audioPlayer.Seek(value);
			}
		}
	}

	public double Duration => audioPlayer?.Duration ?? 1;

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

	public Command PlayCommand { get; }
	public Command PauseCommand { get; }
	public Command StopCommand { get; }

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

	public bool CanSetSpeed => audioPlayer?.CanSetSpeed ?? false;

	public double Speed
	{
		get => audioPlayer?.Speed ?? 1;
		set
		{
			try
			{
				if (audioPlayer?.CanSetSpeed ?? false)
				{
					audioPlayer.Speed = Math.Round(value, 1, MidpointRounding.AwayFromZero);
					NotifyPropertyChanged();
				}
			}
			catch (Exception ex)
			{
				App.Current.MainPage.DisplayAlert("Speed", ex.Message, "OK");
			}
		}
	}

	public double MinimumSpeed => audioPlayer?.MinimumSpeed ?? 1;
	public double MaximumSpeed => audioPlayer?.MaximumSpeed ?? 1;

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

		UpdatePlaybackPosition();
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

		UpdatePlaybackPosition();
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

	void UpdatePlaybackPosition()
	{
		if (audioPlayer?.IsPlaying is false)
		{
			return;
		}

		dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				Console.WriteLine($"{CurrentPosition} with duration of {Duration}");

				isPositionChangeSystemDriven = true;

				NotifyPropertyChanged(nameof(CurrentPosition));

				isPositionChangeSystemDriven = false;

				UpdatePlaybackPosition();
			});
	}

	public void TidyUp()
	{
		audioPlayer?.Dispose();
		audioPlayer = null;
	}

	~MusicPlayerPageViewModel()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}

		if (disposing)
		{
			TidyUp();
		}

		isDisposed = true;
	}
}