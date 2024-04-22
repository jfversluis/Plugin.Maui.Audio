using System.Diagnostics;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	MediaCapture? mediaCapture;
	readonly MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
	string audioFilePath = string.Empty;
	StorageFile? fileOnDisk;

	FileStream? audioFileStream;
	long startingAudioFileStreamLength;
	int audioChunkNumber;

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
			SoundDetected = false;
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

	FileStream GetFileStream()
	{
		int wavFileHeaderLength = 44;

		FileStream fileStream = new(audioFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		startingAudioFileStreamLength = fileStream.Length;

		if (startingAudioFileStreamLength == 0)
		{
			startingAudioFileStreamLength = wavFileHeaderLength;
		}

		return fileStream;
	}

	byte[]? GetAudioDataChunk()
	{
		uint bitRate = encodingProfile.Audio.Bitrate;
		uint bufferSize;

		bufferSize = bitRate != 0 ? bitRate / 8 / 10 : 256_000 / 8 / 10; // MediaCapture do not put data about bit rate in EncodingProfile.Audio.Bitrate when AudioEncodingQuality.Auto
		
		//if (fileStream?.Length > 0)
		//{
		//	byte[] buffer = new byte[bufferSize];
		//	fileStream.Seek(0, SeekOrigin.Begin);
		//	fileStream.Read(buffer);
		//}

		if (audioFileStream?.Length > (audioChunkNumber * bufferSize) + startingAudioFileStreamLength)
		{
			byte[] buffer = new byte[bufferSize];
			audioFileStream.Seek(-bufferSize, SeekOrigin.End);
			audioFileStream.Read(buffer);
			audioChunkNumber++;

			return buffer;
		}
		else
		{
			return null;
		}
	}
}
