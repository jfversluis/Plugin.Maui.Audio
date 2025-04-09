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
	string capturedAudioWavFile;
	double measuredDecibels = 0.0;

	public AudioStreamerPageViewModel(
		IAudioManager audioManager,
		IDispatcher dispatcher)
	{
		StartCommand = new Command(Start, () => !IsStreaming);
		StopCommand = new Command(Stop, () => IsStreaming);
		PlayCommand = new Command(PlayAudio, () => !IsPlaying);
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
			StopPlayCommand.ChangeCanExecute();
		}
	}

	public bool IsStreaming => audioStreamer?.IsStreaming ?? false;

	public Command PlayCommand { get; }
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

	async void PlayAudio()
	{
		if (!string.IsNullOrWhiteSpace(capturedAudioWavFile))
		{
			var stream = new FileStream(capturedAudioWavFile, FileMode.Open, FileAccess.Read);
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
			capturedAudioWavFile = string.Empty;

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

		capturedAudioWavFile = Path.Combine(FileSystem.CacheDirectory, Path.GetTempFileName());
		var audioFileStream = File.OpenWrite(capturedAudioWavFile);
		var totalAudioLength = capturedAudioStream.Length;

		var header = PcmAudioHelpers.CreateWavFileHeader(totalAudioLength,
			audioStreamer.Options.SampleRate,
			(int)audioStreamer.Options.Channels,
			(int)audioStreamer.Options.BitDepth);

		audioFileStream.Write(header);
		audioFileStream.Write(capturedAudioStream.ToArray());
		audioFileStream.Close();

		dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(100),
			() =>
			{
				pcmAudioHandler.Clear();
			});
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