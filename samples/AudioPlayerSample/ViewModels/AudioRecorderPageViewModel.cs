using System.Diagnostics;
using Plugin.Maui.Audio;

namespace AudioPlayerSample.ViewModels;

public class AudioRecorderPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	IAudioRecorder audioRecorder;
	IAudioPlayer audioPlayer;
	IAudioSource audioSource = null;
	bool isRecording;
	double audioTime = 0;

	public double AudioTime
	{
		get => audioTime;
		set
		{
			audioTime = value;
			NotifyPropertyChanged();
		}
	}

	public bool IsRecording
	{
		get => isRecording;
		set
		{
			isRecording = value;
			NotifyPropertyChanged();
		}
	}

	public Command PlayCommand { get; }
	public Command StartCommand { get; }
	public Command StopCommand { get; }

	public AudioRecorderPageViewModel(IAudioManager audioManager)
	{
		StartCommand = new Command(Start);
		StopCommand = new Command(Stop);
		PlayCommand = new Command(PlayAudio);
		this.audioManager = audioManager;
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
		this.IsRecording = true;

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
				await audioRecorder.StartAsync();
				Debug.WriteLine($"{stopwatch.Elapsed} After recorder start");
			}
			else
			{
				//await page.DisplayAlert("Alert", $"It is necessary to go to the settings for this app and give permission for the microphone.", "OK");
			}
		}
		catch (Exception ex)
		{
			this.IsRecording = false;
		}
	}

	async void Stop()
	{
		await audioRecorder.StopAsync();
	}
}