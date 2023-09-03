using System.Diagnostics;
using Plugin.Maui.Audio;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace AudioPlayerSample.ViewModels;

public class AudioRecorderPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;
	IAudioRecorder audioRecorder;
	IAudioPlayer audioPlayer;
	IAudioSource audioSource = null;
	readonly Stopwatch recordingStopwatch = new Stopwatch();

	public double RecordingTime
	{
		get => recordingStopwatch.ElapsedMilliseconds / 1000;
	}

	public bool IsRecording
	{
		get => audioRecorder?.IsRecording ?? false;
	}

	public Command PlayCommand { get; }
	public Command StartCommand { get; }
	public Command StopCommand { get; }

	public AudioRecorderPageViewModel(
		IAudioManager audioManager,
		IDispatcher dispatcher)
	{
		StartCommand = new Command(Start, () => !IsRecording);
		StopCommand = new Command(Stop, () => IsRecording);
		PlayCommand = new Command(PlayAudio);

		this.audioManager = audioManager;
		this.dispatcher = dispatcher;
	}

	public void PlayAudio()
	{
		if (audioSource != null)
		{
			audioPlayer = this.audioManager.CreatePlayer(((FileAudioSource)audioSource).GetAudioStream());

			audioPlayer.Play();
		}
	}

	async void Start()
	{
		if (await CheckPermissionIsGrantedAsync<Microphone>())
		{
			audioRecorder = audioManager.CreateRecorder();
					
			await audioRecorder.StartAsync();
		}

		recordingStopwatch.Restart();
		UpdateRecordingTime();
		NotifyPropertyChanged(nameof(IsRecording));
		StartCommand.ChangeCanExecute();
		StopCommand.ChangeCanExecute();
	}

	async void Stop()
	{
		audioSource = await audioRecorder.StopAsync();

		recordingStopwatch.Stop();
		NotifyPropertyChanged(nameof(IsRecording));
		StartCommand.ChangeCanExecute();
		StopCommand.ChangeCanExecute();
	}

	void UpdateRecordingTime()
	{
		if (IsRecording is false)
		{
			return;
		}

		dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				NotifyPropertyChanged(nameof(RecordingTime));

				UpdateRecordingTime();
			});
	}
}