namespace Plugin.Maui.Audio.Sample.ViewModels;

public class SilenceDetectionPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	readonly IAudioRecorder audioRecorder;
	IAudioSource audioSource;
	AsyncAudioPlayer audioPlayer;

	CancellationTokenSource cancellationTokenSource;

	bool isRecording;
	bool isPlaying;

	public SilenceDetectionPageViewModel(IAudioManager audioManager)
	{
		this.audioManager = audioManager;
		audioRecorder = audioManager.CreateRecorder();

		StartStopRecordToggleCommand = new Command(async() => await StartStopRecordToggleAsync(), () => !IsPlaying);
		PlayRecordCommand = new Command(async () => await PlayRecordAsync(), () => !IsRecording && !IsPlaying && audioSource is not null);
	}

	public bool IsRecording
	{
		get => isRecording; 
		set
		{
			isRecording = value;
			StartStopRecordToggleCommand.ChangeCanExecute();
			PlayRecordCommand.ChangeCanExecute();
			NotifyPropertyChanged();
		}
	}

	public Command StartStopRecordToggleCommand { get; }

	public bool IsPlaying
	{
		get => isPlaying; 
		set
		{
			isPlaying = value;
			StartStopRecordToggleCommand.ChangeCanExecute();
			PlayRecordCommand.ChangeCanExecute();
			NotifyPropertyChanged();
		}
	}

	public Command PlayRecordCommand { get; }

	async Task StartStopRecordToggleAsync()
	{
		if (!IsRecording)
		{
			await RecordAsync();
		}
		else
		{
			StopRecording();
		}
	}

	async Task RecordAsync()
	{
		IsRecording = true;

		if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
		{
			await Shell.Current.DisplayAlert("Permission Denied", "The app needs microphone permission to record audio.", "OK");
			return;
		}

		await RecordUntilSilenceDetectedAsync();
		StopRecording();
		audioSource = await GetRecordingAsync();

		IsRecording = false;
	}

	public async Task RecordUntilSilenceDetectedAsync()
	{
		cancellationTokenSource = new();

		try
		{
			if (!audioRecorder.IsRecording)
			{
				string tempRecordFilePath = Path.Combine(FileSystem.CacheDirectory, "rec.tmp");

				if (!File.Exists(tempRecordFilePath))
				{
					File.Create(tempRecordFilePath).Dispose();
				}

				await audioRecorder.StartAsync(tempRecordFilePath);
				await audioRecorder.DetectSilenceAsync(cancellationToken: cancellationTokenSource.Token); // TODO: add params
			}
		}
		catch (OperationCanceledException)
		{
			return;
		}
	}

	void StopRecording() => cancellationTokenSource?.Cancel();

	public async Task<IAudioSource> GetRecordingAsync()
	{
		IAudioSource audioSource = await audioRecorder.StopAsync();

		if (audioRecorder.SoundDetected)
		{
			return audioSource;
		}
		else
		{
			return null;
		}
	}

	async Task PlayRecordAsync()
	{
		if (audioSource != null)
		{
			audioPlayer = this.audioManager.CreateAsyncPlayer(((FileAudioSource)audioSource).GetAudioStream());
			IsPlaying = true;
			await audioPlayer.PlayAsync(CancellationToken.None);
			IsPlaying = false;
		}
	}
}
