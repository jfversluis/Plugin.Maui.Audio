using Microsoft.Maui.Controls.PlatformConfiguration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	MediaCapture? mediaCapture;
	string audioFilePath = string.Empty;

	public bool CanRecordAudio { get; private set; } = true;
	public bool IsRecording => mediaCapture != null;

	readonly AudioRecorderOptions options;

	public AudioRecorder(AudioRecorderOptions options)
	{
		this.options = options;
	}

	public Task StartAsync() => StartAsync(DefaultAudioRecordingOptions.DefaultOptions);
	public Task StartAsync(string filePath) => StartAsync(filePath, DefaultAudioRecordingOptions.DefaultOptions);

	public async Task StartAsync(AudioRecordingOptions options)
	{
		var localFolder = ApplicationData.Current.LocalFolder;
		var fileName = Path.GetRandomFileName();

		var fileOnDisk = await localFolder.CreateFileAsync(fileName);

		await StartAsync(fileOnDisk.Path, options);
	}



	public async Task StartAsync(string filePath, AudioRecordingOptions options)
	{
		if (mediaCapture is not null)
		{
			throw new InvalidOperationException("Recording already in progress");
		}

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

		var fileOnDisk = await StorageFile.GetFileFromPathAsync(filePath);

		try
		{
			try
			{
				var profile = SharedOptionsToWindowsMediaProfile(options);
				await mediaCapture?.StartRecordToStorageFileAsync(profile, fileOnDisk);
			}
			catch
			{
				if(options.ThrowIfNotSupported)
				{
					throw;
				}

				var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto);

				uint sampleRate =  (uint)DefaultAudioRecordingOptions.DefaultOptions.SampleRate;
				uint channelCount = (uint)DefaultAudioRecordingOptions.DefaultOptions.Channels;
				uint bitsPerSample = (uint)DefaultAudioRecordingOptions.DefaultOptions.BitDepth;

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

		audioFilePath = fileOnDisk.Path;
	}

	static MediaEncodingProfile SharedOptionsToWindowsMediaProfile(AudioRecordingOptions options)
	{
		uint sampleRate = (uint)options.SampleRate;
		uint channelCount = (uint)options.Channels;
		uint bitsPerSample = (uint)options.BitDepth;

		switch (options.Encoding)
		{
			case Encoding.LinearPCM:
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
		if (mediaCapture == null)
		{
			throw new InvalidOperationException("No recording in progress");
		}

		await mediaCapture.StopRecordAsync();

		mediaCapture.Dispose();
		mediaCapture = null;

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
