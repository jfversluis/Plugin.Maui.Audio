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

    public AudioRecorder(AudioRecorderOptions options)
    {
        
    }

	public async Task StartAsync()
	{
		var localFolder = ApplicationData.Current.LocalFolder;
		var fileName = Path.GetRandomFileName();

		var fileOnDisk = await localFolder.CreateFileAsync(fileName);

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
			await mediaCapture?.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto), fileOnDisk);
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
}
