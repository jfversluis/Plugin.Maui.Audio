namespace Plugin.Maui.Audio;

public class FileAudioSource : IAudioSource
{
	public FileAudioSource(string filePath)
	{
		this.filePath = filePath;
	}

	public bool HasRecording => File.Exists(filePath);

	string filePath;

	public string GetFilePath()
	{
		return filePath;
	}

	public Stream GetAudioStream()
	{
		if (File.Exists(filePath))
		{
			return new FileStream(filePath, FileMode.Open, FileAccess.Read);
		}

		return Stream.Null;
	}

	void DeleteFile()
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		filePath = string.Empty;
	}

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			DeleteFile();
		}
	}
}
