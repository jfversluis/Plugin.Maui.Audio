namespace Plugin.Maui.Audio;

public class FileAudioSource : IAudioSource
{
	public FileAudioSource(string filePath)
	{
		this.filePath = filePath;
	}

	public bool HasRecording => File.Exists(filePath);

	readonly string filePath;

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
}