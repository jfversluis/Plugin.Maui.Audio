using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	MediaCapture? mediaCapture;
	readonly MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto);
	string audioFilePath = string.Empty;
	StorageFile? fileOnDisk;

	public bool CanRecordAudio { get; private set; } = true;
	public bool IsRecording => mediaCapture != null;

	public async Task StartAsync()
	{
		var localFolder = ApplicationData.Current.LocalFolder;
		var fileName = Path.GetRandomFileName();

		fileOnDisk = await localFolder.CreateFileAsync(fileName);

		await StartAsync(fileOnDisk.Path);
	}

	public async Task StartAsync(string filePath)
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
			await mediaCapture?.StartRecordToStorageFileAsync(encodingProfile, fileOnDisk); 
		}
		catch
		{
			CanRecordAudio = false;
			DeleteMediaCapture();
			throw;
		}

		audioFilePath = fileOnDisk.Path;
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

	public async Task DetectSilenceAsync(double silenceThreshold, int silenceDuration)
	{
		int wavFileHeaderLength = 44;
		uint bitRate = encodingProfile.Audio.Bitrate;
		uint bufferSize;
		int bufferNumber = 1;

		lastSoundDetectedTime = default;
		noiseLevel = 0;
		readingsComplete = false;

		bufferSize = bitRate != 0 ? bitRate / 8 / 10 : 192_000 / 10;

		await Task.Run(() =>
		{
			byte[] buffer = new byte[bufferSize];

			using FileStream fileStream = new(audioFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			while (this.IsRecording)
			{
				if (fileStream.Length > (bufferNumber * bufferSize) + wavFileHeaderLength)
				{
					fileStream.Seek(-bufferSize, SeekOrigin.End);
					fileStream.Read(buffer);

					if (DetectSilence(buffer, silenceThreshold, silenceDuration))
					{
						return;
					}

					bufferNumber++;
				}
			}
			return;
		});
	}
}
