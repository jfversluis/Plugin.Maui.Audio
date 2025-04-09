using System.Diagnostics;
using Plugin.Maui.Audio.AudioListeners;

namespace Plugin.Maui.Audio.Sample.ViewModels;

public class AudioStreamerPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;

	SilenceListener silenceListener;
	DecibelListener decibelListener;
	RmsListener rmsListener;
	PcmAudioHandler pcmAudioHandler;

	IAudioStreamer audioStreamer;
	AsyncAudioPlayer audioPlayer;
	readonly Stopwatch recordingStopwatch = new ();
	bool isPlaying;
	bool? isSilent;
	MemoryStream capturedAudioStream;

	string capturedAudioWavFileOriginal;
	string capturedAudioWavFileLeft;
	string capturedAudioWavFileRight;

	public AudioStreamerPageViewModel(
		IAudioManager audioManager,
		IDispatcher dispatcher)
	{
		StartCommand = new Command(Start, () => !IsStreaming);
		StopCommand = new Command(Stop, () => IsStreaming);
		PlayCommand = new Command(() => PlayAudio(capturedAudioWavFileOriginal), () => !IsPlaying && !string.IsNullOrWhiteSpace(capturedAudioWavFileOriginal));
		PlayLeftCommand = new Command(() => PlayAudio(capturedAudioWavFileLeft), () => !IsPlaying && !string.IsNullOrWhiteSpace(capturedAudioWavFileLeft));
		PlayRightCommand = new Command(() => PlayAudio(capturedAudioWavFileRight), () => !IsPlaying && !string.IsNullOrWhiteSpace(capturedAudioWavFileRight));
		StopPlayCommand = new Command(StopPlay, () => IsPlaying);

		this.audioManager = audioManager;
		this.dispatcher = dispatcher;

		SetDefaults();
	}

	double measuredDecibel;
	public double MeasuredDecibel
	{
		get => measuredDecibel;
		set
		{
			measuredDecibel = value;
			NotifyPropertyChanged();
		}
	}

	double measuredRms;
	public double MeasuredRms
	{
		get => measuredRms;
		set
		{
			measuredRms = value;
			NotifyPropertyChanged();
		}
	}

	public List<ChannelType> ChannelTypes { get; set; } = Enum.GetValues(typeof(ChannelType)).Cast<ChannelType>().ToList();
	public List<BitDepth> BitDepths { get; set; } = Enum.GetValues(typeof(BitDepth)).Cast<BitDepth>().ToList();
	public List<int> SampleRates { get; set; } =
	[
		8000,
		16000,
		44100,
		48000
	];

	public double RecordingTime => recordingStopwatch.ElapsedMilliseconds / 1000;

	public bool IsPlaying
	{
		get => isPlaying;
		set
		{
			isPlaying = value;
			PlayCommand.ChangeCanExecute();
			PlayLeftCommand.ChangeCanExecute();
			PlayRightCommand.ChangeCanExecute();
			StopPlayCommand.ChangeCanExecute();
		}
	}

	public bool IsStreaming => audioStreamer?.IsStreaming ?? false;

	public Command PlayCommand { get; }
	public Command PlayLeftCommand { get; }
	public Command PlayRightCommand { get; }
	public Command StartCommand { get; }
	public Command StopCommand { get; }
	public Command StopPlayCommand { get; }

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

	public bool? IsSilent
	{
		get => isSilent;
		set
		{
			isSilent = value;
			NotifyPropertyChanged();
		}
	}

	void SetDefaults()
	{
		SelectedSampleRate = (DeviceInfo.Platform == DevicePlatform.Android) 
								? 44100 
								: 48000;

		SelectedBitDepth = BitDepth.Pcm16bit;
		SelectedChannelType = ChannelType.Mono;
	}

	async void PlayAudio(string audioFile)
	{
		if (!string.IsNullOrWhiteSpace(audioFile))
		{
			var stream = new FileStream(audioFile, FileMode.Open, FileAccess.Read);
			audioPlayer = audioManager.CreateAsyncPlayer(stream);

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
		if (await CheckPermissionIsGrantedAsync<Permissions.Microphone>())
		{
			if (capturedAudioStream is not null)
			{
				capturedAudioStream.Dispose();
			}
			capturedAudioStream = new MemoryStream();
			capturedAudioWavFileOriginal = string.Empty;
			capturedAudioWavFileLeft = string.Empty;
			capturedAudioWavFileRight = string.Empty;

			PlayCommand.ChangeCanExecute();
			PlayLeftCommand.ChangeCanExecute();
			PlayRightCommand.ChangeCanExecute();

			if (pcmAudioHandler is null)
			{
				pcmAudioHandler = new PcmAudioHandler(SelectedSampleRate, SelectedChannelType, SelectedBitDepth);
				silenceListener = new SilenceListener()
				{
					MinimalSilenceTimespanInMilliseconds = 300,
					SilenceThresholdInDb = -40
				};
				silenceListener.IsSilentChanged += (sender, args) =>
				{
					dispatcher.Dispatch(() =>
					{
						IsSilent = args.IsSilent;
					});
				};

				decibelListener = new DecibelListener
				{
					MeasuringIntervalInMilliseconds = 100
				};
				decibelListener.DecibelChanged += (sender, args) =>
				{
					dispatcher.Dispatch(() =>
					{
						MeasuredDecibel = args.Decibel;
					});
				};

				rmsListener = new RmsListener
				{
					MeasuringIntervalInMilliseconds = 100
				};
				rmsListener.RmsChanged += (sender, args) =>
				{
					dispatcher.Dispatch(() =>
					{
						MeasuredRms = args.Rms;
					});
				};

				pcmAudioHandler.Subscribe(silenceListener);
				pcmAudioHandler.Subscribe(decibelListener);
				pcmAudioHandler.Subscribe(rmsListener);
			}
			else
			{
				audioStreamer.Options.Channels = SelectedChannelType;
				audioStreamer.Options.BitDepth = SelectedBitDepth;
				audioStreamer.Options.SampleRate = SelectedSampleRate;
			}
			
			if (audioStreamer is null)
			{
				audioStreamer = audioManager.CreateStreamer();
				audioStreamer.OnAudioCaptured += OnAudioStreamerCapturedData;
			}

			audioStreamer.Options.Channels = SelectedChannelType;
			audioStreamer.Options.BitDepth = SelectedBitDepth;
			audioStreamer.Options.SampleRate = SelectedSampleRate;

			try
			{
				await audioStreamer.StartAsync();
			}
			catch
			{
				var res = await AppShell.Current.DisplayActionSheet("Options not supported. Use Default?", "Yes", "No");
				if (res == "Yes")
				{
					dispatcher.Dispatch(SetDefaults);
					return;
				}
			}
		}

		recordingStopwatch.Restart();
		UpdateRecordingTime();
		NotifyPropertyChanged(nameof(IsStreaming));
		StartCommand.ChangeCanExecute();
		StopCommand.ChangeCanExecute();
	}

	void OnAudioStreamerCapturedData(object sender, AudioStreamEventArgs args)
	{
		capturedAudioStream ??= new MemoryStream();
		capturedAudioStream.Write(args.Audio);

		pcmAudioHandler.HandlePcmAudio(args.Audio);
	}

	async void Stop()
	{
		await audioStreamer.StopAsync();

		recordingStopwatch.Stop();
		NotifyPropertyChanged(nameof(IsStreaming));
		StartCommand.ChangeCanExecute();
		StopCommand.ChangeCanExecute();

		capturedAudioWavFileOriginal = Path.Combine(FileSystem.CacheDirectory, Path.GetTempFileName());
		var capturedAudio = capturedAudioStream.ToArray();

		WriteAudioAsWavFile(capturedAudio, capturedAudioWavFileOriginal, 
			audioStreamer.Options.SampleRate, audioStreamer.Options.Channels, audioStreamer.Options.BitDepth);

		if (audioStreamer.Options.Channels == ChannelType.Stereo)
		{
			var audioSamples = PcmAudioHelpers.ConvertRawPcmAudioBytesToOrderedAudioSamples(capturedAudio, audioStreamer.Options.BitDepth);
			var audioSamplesPerChannel = PcmAudioHelpers.ConvertToSamplesPerChannel(audioSamples, audioStreamer.Options.Channels);

			var rawAudioChannelLeft = PcmAudioHelpers.ConvertOrderedAudioSamplesToRawPcmAudioBytes(audioSamplesPerChannel[0], audioStreamer.Options.BitDepth);
			capturedAudioWavFileLeft = Path.Combine(FileSystem.CacheDirectory, Path.GetTempFileName());
			WriteAudioAsWavFile(rawAudioChannelLeft, capturedAudioWavFileLeft,
				audioStreamer.Options.SampleRate, ChannelType.Mono, audioStreamer.Options.BitDepth);

			var rawAudioChannelRight = PcmAudioHelpers.ConvertOrderedAudioSamplesToRawPcmAudioBytes(audioSamplesPerChannel[1], audioStreamer.Options.BitDepth);
			capturedAudioWavFileRight = Path.Combine(FileSystem.CacheDirectory, Path.GetTempFileName());
			WriteAudioAsWavFile(rawAudioChannelRight, capturedAudioWavFileRight,
				audioStreamer.Options.SampleRate, ChannelType.Mono, audioStreamer.Options.BitDepth);
		}

		PlayCommand.ChangeCanExecute();
		PlayLeftCommand.ChangeCanExecute();
		PlayRightCommand.ChangeCanExecute();

		dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(100),
			() =>
			{
				pcmAudioHandler.Clear();
			});
	}

	void WriteAudioAsWavFile(byte[] audio, string file, int sampleRate, ChannelType channels, BitDepth bitDepth)
	{
		var header = PcmAudioHelpers.CreateWavFileHeader(audio.Length, sampleRate, (int)channels, (int)bitDepth);

		var audioFileStream = File.OpenWrite(file);
		audioFileStream.Write(header);
		audioFileStream.Write(audio);
		audioFileStream.Close();
	}

	void UpdateRecordingTime()
	{
		if (IsStreaming is false)
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