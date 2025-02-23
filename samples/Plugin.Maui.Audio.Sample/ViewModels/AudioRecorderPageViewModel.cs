using System.Diagnostics;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Plugin.Maui.Audio.Sample.ViewModels;

public class AudioRecorderPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;
	IAudioRecorder audioRecorder;
	AsyncAudioPlayer audioPlayer;
	IAudioSource audioSource = null;
	readonly Stopwatch recordingStopwatch = new Stopwatch();
	bool isPlaying;

	public double RecordingTime
	{
		get => recordingStopwatch.ElapsedMilliseconds / 1000;
	}

	public bool IsPlaying
	{
		get => isPlaying;
		set
		{
			isPlaying = value;
			PlayCommand.ChangeCanExecute();
			StopPlayCommand.ChangeCanExecute();
		}
	}

	public bool IsRecording
	{
		get => audioRecorder?.IsRecording ?? false;
	}

	public Command PlayCommand { get; }
	public Command StartCommand { get; }
	public Command StopCommand { get; }
	public Command StopPlayCommand { get; }

	public AudioRecorderPageViewModel(
		IAudioManager audioManager,
		IDispatcher dispatcher)
	{
		StartCommand = new Command(Start, () => !IsRecording);
		StopCommand = new Command(Stop, () => IsRecording);
		PlayCommand = new Command(PlayAudio, () => !IsPlaying);
		StopPlayCommand = new Command(StopPlay, () => IsPlaying);

		this.audioManager = audioManager;
		this.dispatcher = dispatcher;
	}

	ChannelType selectedChannelType;
	public ChannelType SelectedChannelType
	{
		get => selectedChannelType;
		set
		{
			selectedChannelType = value;
			NotifyPropertyChanged();
		}
	}
	public List<ChannelType> ChannelTypes { get; set; } = Enum.GetValues(typeof(ChannelType)).Cast<ChannelType>().ToList();

	BitDepth selectedBitDepth;
	public BitDepth SelectedBitDepth
	{
		get => selectedBitDepth;
		set
		{
			selectedBitDepth = value;
			NotifyPropertyChanged();
		}
	}
	public List<BitDepth> BitDepths { get; set; } = Enum.GetValues(typeof(BitDepth)).Cast<BitDepth>().ToList();

	Encoding selectedEncoding;
	public Encoding SelectedEncoding
	{
		get => selectedEncoding;
		set
		{
			selectedEncoding = value;
			NotifyPropertyChanged();
		}
	}

	public List<Encoding> EncodingOptions { get; set; } = Enum.GetValues(typeof(Encoding)).Cast<Encoding>().ToList();

	int selectedSampleRate = -1;
	public int SelectedSampleRate
	{
		get => selectedSampleRate;
		set
		{
			selectedSampleRate = value;
			NotifyPropertyChanged();
		}
	}
	
	public List<int> SampleRates { get; set; } =
	[
		8000,
		16000,
		44100,
		48000
	];


	async void PlayAudio()
	{
		if (audioSource != null)
		{
			audioPlayer = this.audioManager.CreateAsyncPlayer(((FileAudioSource)audioSource).GetAudioStream());

			IsPlaying = true;

			await audioPlayer.PlayAsync(CancellationToken.None);

			IsPlaying = false;
		}
	}

	void StopPlay()
	{
		audioPlayer.Stop();
	}

	async void Start()
	{
		if (await CheckPermissionIsGrantedAsync<Microphone>())
		{
			audioRecorder = audioManager.CreateRecorder();

			var options = new AudioRecordingOptions()
			{
				SampleRate = SelectedSampleRate == -1 ? AudioRecordingOptions.DefaultSampleRate : SelectedSampleRate,
				Channels = SelectedChannelType,
				BitDepth = SelectedBitDepth,
				Encoding = SelectedEncoding,
				ThrowIfNotSupported = true
			};

			try
			{
				await audioRecorder.StartAsync(options);
			}
			catch
			{
				var res = await AppShell.Current.DisplayActionSheet("Options not supported. Use Default?", "Yes", "No");
				if (res != "Yes")
				{
					return;
				}
				await audioRecorder.StartAsync();
			}
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

	internal void OnNavigatedFrom()
	{
		audioPlayer?.Dispose();
	}
}