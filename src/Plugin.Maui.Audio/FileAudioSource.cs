namespace Plugin.Maui.Audio;

/// <summary>
/// A file based <see cref="IAudioSource"/> implementation.
/// </summary>
public class FileAudioSource : IAudioSource
{
	public FileAudioSource(string filePath)
	{
		this.filePath = filePath;
	}

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