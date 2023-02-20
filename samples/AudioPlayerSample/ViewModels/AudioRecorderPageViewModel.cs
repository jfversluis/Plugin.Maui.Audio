using System.Diagnostics;
using Microsoft.Maui.Dispatching;
using Plugin.Maui.Audio;

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

	void Start()
	{
		_ = Task.Run(async () =>
		{
			try
			{
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				Debug.WriteLine($"{stopwatch.Elapsed} Before permission check");

				// This must be done for Android to avoid an exception but I don't think it would hurt in any case
				if (await HavePermissionMicrophoneAsync())
				{
					Debug.WriteLine($"{stopwatch.Elapsed} After permission check");

					Debug.WriteLine($"{stopwatch.Elapsed} Before recorder create");
					audioRecorder = audioManager.CreateRecorder();
					Debug.WriteLine($"{stopwatch.Elapsed} After recorder create");

					Debug.WriteLine($"{stopwatch.Elapsed} Before recorder start");
					_ = audioRecorder.StartAsync();
					Debug.WriteLine($"{stopwatch.Elapsed} After recorder start");
				}
				else
				{
					//await page.DisplayAlert("Alert", $"It is necessary to go to the settings for this app and give permission for the microphone.", "OK");
				}
			}
			catch (Exception ex)
			{
			}

			MainThread.BeginInvokeOnMainThread(() =>
			{
				recordingStopwatch.Restart();
				UpdateRecordingTime();
				NotifyPropertyChanged(nameof(IsRecording));
				StartCommand.ChangeCanExecute();
				StopCommand.ChangeCanExecute();
			});
		});
	}

	async void Stop()
	{
		await audioRecorder.StopAsync();

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