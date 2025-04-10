namespace Plugin.Maui.Audio.Sample.ViewModels;

public class MusicPlayerPageViewModel : BaseViewModel, IQueryAttributable, IDisposable
{
	readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;
	IAudioPlayer audioPlayer;
	TimeSpan animationProgress;
	MusicItemViewModel musicItemViewModel;
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
		UpdateSpeedCommand = new Command(UpdateSpeed);
		SeekStartedCommand = new Command(SeekStarted);
		SeekCommand = new Command(Seek);
	}

	void AudioPlayer_PlaybackEnded(object sender, EventArgs e)
	{
		NotifyPropertyChanged(nameof(IsPlaying));
		NotifyPropertyChanged(nameof(AnimationProgress));
		CurrentPosition = audioPlayer.CurrentPosition;
	}

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(Routes.MusicPlayer.Arguments.Music, out object musicObject) &&
			musicObject is MusicItemViewModel musicItem)
		{
			MusicItemViewModel = musicItem;

			audioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(musicItem.Filename));
			audioPlayer.PlaybackEnded += AudioPlayer_PlaybackEnded;

#if WINDOWS
			// On windows, without this delay, the states are not updated in time
			// instead of this hack, we should update the windows state machine to be more reactive, or use an event based approach to update the UI
			await Task.Delay(50);
#endif
			NotifyPropertyChanged(nameof(HasAudioSource));
			NotifyPropertyChanged(nameof(Duration));
			NotifyPropertyChanged(nameof(CanSetSpeed));
			NotifyPropertyChanged(nameof(MinimumSpeed));
			NotifyPropertyChanged(nameof(MaximumSpeed));
		}
	}

	double currentPosition = 0;
	public double CurrentPosition
	{
		get => currentPosition;
		set
		{
			currentPosition = value;
			NotifyPropertyChanged();
		}
	}

	bool wasPlayingBeforeSeek = false;
	void SeekStarted()
	{
		if (audioPlayer is not null &&
			audioPlayer.CanSeek)
		{
			wasPlayingBeforeSeek = audioPlayer.IsPlaying;
			audioPlayer.Pause();
		}
	}

	void Seek()
	{
		if (audioPlayer is not null &&
			audioPlayer.CanSeek)
		{
			audioPlayer.Seek(CurrentPosition);

			if (wasPlayingBeforeSeek)
			{
				Play();
				wasPlayingBeforeSeek = false;
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
	public Command UpdateSpeedCommand { get; }
	public Command SeekStartedCommand { get; }
	public Command SeekCommand { get; }
	

	public double Volume
	{
		get => audioPlayer?.Volume ?? 1;
		set
		{
			if (audioPlayer is not null)
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
			if (audioPlayer is not null)
			{
				audioPlayer.Balance = value;
			}
		}
	}

	public bool CanSetSpeed => audioPlayer?.CanSetSpeed ?? false;


	double userSpeed = 1.0;
	public double UserSpeed
	{
		get => userSpeed;
		set
		{
			userSpeed = value;
			NotifyPropertyChanged();
		}
	}

	public void UpdateSpeed()
	{
		try
		{
			if ((audioPlayer?.CanSetSpeed ?? false) && UserSpeed >= MinimumSpeed && UserSpeed <= MaximumSpeed)
			{
				audioPlayer.Speed = UserSpeed;
				NotifyPropertyChanged(nameof(AudioSpeed));
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		finally
		{
			UserSpeed = AudioSpeed;
			NotifyPropertyChanged(nameof(UserSpeed));
		}
	}
	public double AudioSpeed => audioPlayer?.Speed ?? 1.0;

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
		UpdateSpeed();
		audioPlayer.Play();

		UpdatePlaybackPosition();
		NotifyPropertyChanged(nameof(IsPlaying));
	}

	void Pause()
	{
		audioPlayer.Pause();
		UpdatePlaybackPosition();
		NotifyPropertyChanged(nameof(IsPlaying));
	}

	void Stop()
	{
		audioPlayer.Stop();
		AnimationProgress = TimeSpan.Zero;
		NotifyPropertyChanged(nameof(IsPlaying));
		NotifyPropertyChanged(nameof(CurrentPosition));
	}

	void UpdatePlaybackPosition()
	{
		if (audioPlayer?.IsPlaying is false)
		{
#if WINDOWS
			// On windows, without this delay, the playback state is not updated in time
			// instead of this hack, we should update the windows state machine to be more reactive, or use an event based approach to update the UI
			Thread.Sleep(50);
#endif

			if (audioPlayer?.IsPlaying is false)
			{
				NotifyPropertyChanged(nameof(IsPlaying));
				NotifyPropertyChanged(nameof(AnimationProgress));
				CurrentPosition = audioPlayer.CurrentPosition;
				return;
			}
		}

		dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				Console.WriteLine($"{CurrentPosition} with duration of {Duration}");
				CurrentPosition = audioPlayer?.CurrentPosition ?? 0;
				UpdatePlaybackPosition();
			});
	}

	public void TidyUp()
	{
		audioPlayer.PlaybackEnded -= AudioPlayer_PlaybackEnded;
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