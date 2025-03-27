using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	MediaCapture? mediaCapture; // can record audio in both AAC (M4A) and WAV formats
	string audioFilePath = string.Empty;
	AudioStream? audioStream;

	public bool CanRecordAudio { get; private set; } = true;
	public bool IsRecording => mediaCapture != null
	                           || audioStream is { Active: true };

	AudioRecorderOptions audioRecorderOptions;
	static readonly AudioRecorderOptions defaultOptions = new AudioRecorderOptions();

	public event EventHandler<AudioStreamEventArgs>? AudioStreamCaptured;

	public AudioRecorder(AudioRecorderOptions options)
	{
		this.audioRecorderOptions = options;
	}

	public async Task StartAsync(AudioRecorderOptions? options = null)
	{
		var localFolder = ApplicationData.Current.LocalFolder;
		var fileName = Path.GetRandomFileName();

		var fileOnDisk = await localFolder.CreateFileAsync(fileName);

		await StartAsync(fileOnDisk.Path, options);
	}

	public async Task StartAsync(string filePath, AudioRecorderOptions? options = null)
	{
		if (mediaCapture is not null)
		{
			throw new InvalidOperationException("Recording already in progress");
		}
		
		if (options is not null)
		{
			audioRecorderOptions = options;
		}
		
		if (options.CaptureMode == CaptureMode.Bundling)
		{
			var fileOnDisk = await StorageFile.GetFileFromPathAsync(filePath);
			audioFilePath = fileOnDisk.Path;

			try
			{
				var captureSettings = new MediaCaptureInitializationSettings()
				{
					StreamingCaptureMode = StreamingCaptureMode.Audio
				};
				await InitMediaCapture(captureSettings);
			}
			catch (Exception ex)
			{
				CanRecordAudio = false;
				DeleteMediaCapture();

				if (ex.InnerException != null && ex.InnerException.GetType() == typeof(UnauthorizedAccessException))
				{
					throw ex.InnerException;
				}
				throw;
			}

			try
			{
				try
				{
					var profile = SharedOptionsToWindowsMediaProfile(audioRecorderOptions);
					await mediaCapture?.StartRecordToStorageFileAsync(profile, fileOnDisk);
				}
				catch
				{
					if (audioRecorderOptions.ThrowIfNotSupported)
					{
						throw;
					}

					var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto);

					uint sampleRate = (uint)defaultOptions.SampleRate;
					uint channelCount = (uint)defaultOptions.Channels;
					uint bitsPerSample = (uint)defaultOptions.BitDepth;

					profile.Audio = AudioEncodingProperties.CreatePcm(sampleRate, channelCount, bitsPerSample);

					await mediaCapture?.StartRecordToStorageFileAsync(profile, fileOnDisk);
				}
			}
			catch
			{
				CanRecordAudio = false;
				DeleteMediaCapture();
				throw;
			}
		}
		else
		{
			if (options.Encoding != Encoding.Wav)
			{
				throw new NotSupportedException(
					$"Encoding '{options.Encoding}' is not supported with '{options.CaptureMode}' mode");
			}

			if (audioStream == null)
			{
				audioStream = new AudioStream(
					options.SampleRate,
					(int)options.Channels,
					(int)options.BitDepth);

				audioStream.OnBroadcast += (sender, bytes) =>
				{
					AudioStreamCaptured?.Invoke(this, new AudioStreamEventArgs(bytes));
				};
			}

			if (!audioStream.Active)
			{
				await audioStream.Start();
			}
		}
	}

	static MediaEncodingProfile SharedOptionsToWindowsMediaProfile(AudioRecorderOptions options)
	{
		uint sampleRate = (uint)options.SampleRate;
		uint channelCount = (uint)options.Channels;
		uint bitsPerSample = (uint)options.BitDepth;

		switch (options.Encoding)
		{
			case Encoding.Wav:
				var profilePCM = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto);
				profilePCM.Audio = AudioEncodingProperties.CreatePcm(sampleRate, channelCount, bitsPerSample);
				return profilePCM;
			case Encoding.Flac:
				var profileFlac = MediaEncodingProfile.CreateFlac(AudioEncodingQuality.Auto);
				profileFlac.Audio = AudioEncodingProperties.CreateFlac(sampleRate, channelCount, bitsPerSample);
				return profileFlac;
			case Encoding.Alac:
				var profileAlac = MediaEncodingProfile.CreateAlac(AudioEncodingQuality.Auto);
				profileAlac.Audio = AudioEncodingProperties.CreateAlac(sampleRate, channelCount, bitsPerSample);
				return profileAlac;
			case Encoding.Aac: // create aac in .m4a file
				var profileAac = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.Auto);
				profileAac.Audio = AudioEncodingProperties.CreateAac(sampleRate, channelCount, bitsPerSample);
				return profileAac;
			default:
				throw new NotSupportedException("Encoding not supported");
		}
	}

	async Task InitMediaCapture(MediaCaptureInitializationSettings settings)
	{
		mediaCapture = new MediaCapture();

		await mediaCapture.InitializeAsync(settings);

		mediaCapture.RecordLimitationExceeded += (MediaCapture sender) =>
		{
			CanRecordAudio = false;
			DeleteMediaCapture();
			throw new Exception("Record Limitation Exceeded");
		};

		mediaCapture.Failed += (MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs) =>
		{
			CanRecordAudio = false;
			DeleteMediaCapture();
			throw new Exception(string.Format("Code: {0}. {1}", errorEventArgs.Code, errorEventArgs.Message));
		};
	}

	public async Task<IAudioSource> StopAsync()
	{
		if (audioStream != null)
		{
			await audioStream.Stop();
			audioStream.Flush();
		}

		if (mediaCapture != null)
		{
			await mediaCapture?.StopRecordAsync();

			mediaCapture?.Dispose();
			mediaCapture = null;
		}

		return GetRecording();
	}

	IAudioSource GetRecording()
	{
		if (File.Exists(audioFilePath))
		{
			return new FileAudioSource(audioFilePath);
		}

		return new EmptyAudioSource();
	}

	void DeleteMediaCapture()
	{
		try
		{
			mediaCapture?.Dispose();
		}
		catch
		{
			//ignore
		}

		try
		{
			if (!string.IsNullOrWhiteSpace(audioFilePath) && File.Exists(audioFilePath))
			{
				File.Delete(audioFilePath);
			}
		}
		catch
		{
			//ignore
		}

		audioFilePath = string.Empty;
		mediaCapture = null;
	}
}
