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
	AudioRecorderOptions options;
	MemoryStream capturedAudioStream;

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

	public List<CaptureMode> CaptureModes { get; set; } =
	[
		CaptureMode.Bundling,
		CaptureMode.Streaming
	];

	CaptureMode selectedCaptureMode;
	public CaptureMode SelectedCaptureMode
	{
		get => selectedCaptureMode;
		set
		{
			selectedCaptureMode = value;
			NotifyPropertyChanged();
			
			if (selectedCaptureMode == CaptureMode.Streaming)
			{
				// pre-select good streaming values
				SelectedSampleRate = 44100;
				SelectedBitDepth = BitDepth.Pcm16bit;
				SelectedChannelType = ChannelType.Mono;
				SelectedEncoding = Encoding.Wav;
			}
		}
	}

	async void PlayAudio()
	{
		if (audioSource != null
		    && audioSource is not EmptyAudioSource)
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

			options = new AudioRecorderOptions
			{
				Channels = SelectedChannelType,
				BitDepth = SelectedBitDepth,
				Encoding = SelectedEncoding,
				CaptureMode = SelectedCaptureMode,
				ThrowIfNotSupported = true
			};

			if (SelectedSampleRate != -1)
			{
				options.SampleRate = SelectedSampleRate;
			}

			if (options.CaptureMode == CaptureMode.Streaming)
			{
				capturedAudioStream = new MemoryStream();
				audioRecorder.AudioStreamCaptured += (sender, args) =>
				{
					capturedAudioStream.Write(args.Data);
				};
			}

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

		WriteStreamedAudioToAudioSource();
	}

	void WriteStreamedAudioToAudioSource()
	{
		if (options.CaptureMode != CaptureMode.Streaming)
		{
			return;
		}

		var tempWavFile = Path.Combine(FileSystem.CacheDirectory, Path.GetTempFileName());
		var fileAudioSource = new FileAudioSource(tempWavFile);
		audioSource = fileAudioSource;

		var audioFilePath = fileAudioSource.GetFilePath();
		var audioFileStream = File.OpenWrite(audioFilePath);

		var totalAudioLength = capturedAudioStream.Length;

		var header = WaveFileHelper.GetWaveFileHeader(totalAudioLength,
			totalAudioLength + 36,
			options.SampleRate,
			(int)options.Channels,
			(int)options.BitDepth);

		audioFileStream.Write(header);
		audioFileStream.Write(capturedAudioStream.ToArray());
		audioFileStream.Close();
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
