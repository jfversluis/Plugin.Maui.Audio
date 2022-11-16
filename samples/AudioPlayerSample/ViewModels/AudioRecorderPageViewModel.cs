using Plugin.Maui.Audio;

namespace AudioPlayerSample.ViewModels;

public class AudioRecorderPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	IAudioRecorder? audioRecorder;

	public AudioRecorderPageViewModel(
		IAudioManager audioManager)
	{
		StartCommand = new Command(Start);

		this.audioManager = audioManager;
	}

	public Command StartCommand { get; set; }

	async void Start()
	{
		audioRecorder = audioManager.CreateRecorder();

		try
		{
			await audioRecorder.StartAsync();

			await Task.Delay(5000);

			var audioSource = await audioRecorder.StopAsync();

			this.audioManager.CreatePlayer(((FileAudioSource)audioSource).GetAudioStream()).Play();
		}
		catch (Exception ex)
		{

		}
	}
}
