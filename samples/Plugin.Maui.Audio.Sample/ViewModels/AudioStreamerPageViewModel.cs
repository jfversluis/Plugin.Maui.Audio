using System.Diagnostics;

namespace Plugin.Maui.Audio.Sample.ViewModels;

public class AudioStreamerPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;
	IAudioStreamer audioStreamer;
	AsyncAudioPlayer audioPlayer;
	readonly Stopwatch recordingStopwatch = new ();
	bool isPlaying;
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

	public double MeasuredDecibels => measuredDecibels;

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
			if (capturedAudioStream != null)
			{
				capturedAudioStream.Dispose();
			}
			capturedAudioStream = new MemoryStream();
			capturedAudioWavFile = string.Empty;

			if (audioStreamer == null)
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

		var decibel = EstimateDecibel(args.Audio,
			(int)audioStreamer.Options.BitDepth,
			(int)audioStreamer.Options.Channels);

		dispatcher.Dispatch(
				() =>
				{
					if (audioStreamer.IsStreaming)
					{
						measuredDecibels = decibel;
						NotifyPropertyChanged(nameof(MeasuredDecibels));
					}
				});
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

		var header = GetWaveFileHeader(totalAudioLength,
			totalAudioLength + 36,
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
				measuredDecibels = 0.0;
				NotifyPropertyChanged(nameof(MeasuredDecibels));
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

	/// <summary>
	/// Calculates an estimate decibel value, calculations are not scientifically correct
	/// </summary>
	public double EstimateDecibel(byte[] samples, int bitsPerSample, int channelCount)
	{
		int length = samples.Length;
		int sampleSizeInBytes = bitsPerSample / 8;

		double sampleSum = 0;
		var checkedSampleCount = 0;
		for (var i = 0; i < length; i += sampleSizeInBytes)
		{
			checkedSampleCount++;
			double sample = 0;

			if (bitsPerSample == 8)
			{
				sample = (127 - (double)samples[i]) / (double)byte.MaxValue;
			}
			else
			{
				double sampleValue = BitConverter.ToInt16(samples, i);
				sample += (sampleValue / short.MaxValue);
			}

			sampleSum += (sample * sample);
		}

		double rootMeanSquared = Math.Sqrt(sampleSum / checkedSampleCount);
		if (rootMeanSquared == 0)
		{
			return 0;
		}

		double logvalue = Math.Log10(rootMeanSquared);
		double decibel = 20 * logvalue;

		return decibel;
	}

	static byte[] GetWaveFileHeader(long audioLength, long dataLength, long sampleRate, int channels, int bitDepth)
	{
		int blockAlign = (int)(channels * (bitDepth / 8));
		long byteRate = sampleRate * blockAlign;

		byte[] header = new byte[44];

		header[0] = Convert.ToByte('R'); // RIFF/WAVE header
		header[1] = Convert.ToByte('I'); // (byte)'I'
		header[2] = Convert.ToByte('F');
		header[3] = Convert.ToByte('F');
		header[4] = (byte)(dataLength & 0xff);
		header[5] = (byte)((dataLength >> 8) & 0xff);
		header[6] = (byte)((dataLength >> 16) & 0xff);
		header[7] = (byte)((dataLength >> 24) & 0xff);
		header[8] = Convert.ToByte('W');
		header[9] = Convert.ToByte('A');
		header[10] = Convert.ToByte('V');
		header[11] = Convert.ToByte('E');
		header[12] = Convert.ToByte('f'); // fmt chunk
		header[13] = Convert.ToByte('m');
		header[14] = Convert.ToByte('t');
		header[15] = (byte)' ';
		header[16] = 16; // 4 bytes - size of fmt chunk
		header[17] = 0;
		header[18] = 0;
		header[19] = 0;
		header[20] = 1; // format = 1
		header[21] = 0;
		header[22] = Convert.ToByte(channels);
		header[23] = 0;
		header[24] = (byte)(sampleRate & 0xff);
		header[25] = (byte)((sampleRate >> 8) & 0xff);
		header[26] = (byte)((sampleRate >> 16) & 0xff);
		header[27] = (byte)((sampleRate >> 24) & 0xff);
		header[28] = (byte)(byteRate & 0xff);
		header[29] = (byte)((byteRate >> 8) & 0xff);
		header[30] = (byte)((byteRate >> 16) & 0xff);
		header[31] = (byte)((byteRate >> 24) & 0xff);
		header[32] = (byte)(blockAlign); // block align
		header[33] = 0;
		header[34] = Convert.ToByte(bitDepth); // bits per sample
		header[35] = 0;
		header[36] = Convert.ToByte('d');
		header[37] = Convert.ToByte('a');
		header[38] = Convert.ToByte('t');
		header[39] = Convert.ToByte('a');
		header[40] = (byte)(audioLength & 0xff);
		header[41] = (byte)((audioLength >> 8) & 0xff);
		header[42] = (byte)((audioLength >> 16) & 0xff);
		header[43] = (byte)((audioLength >> 24) & 0xff);

		return header;
	}
}