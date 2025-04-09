namespace Plugin.Maui.Audio;

/// <summary>
/// A file based <see cref="IAudioSource"/> implementation.
/// </summary>
public class FileAudioSource : IAudioSource
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FileAudioSource"/> class.
	/// </summary>
	/// <param name="filePath">The path to the audio file.</param>
	public FileAudioSource(string filePath)
	{
		this.filePath = filePath;
	}

	readonly string filePath;

	/// <summary>
	/// Gets the file path of this audio source.
	/// </summary>
	/// <returns>The file path of the audio source.</returns>
	public string GetFilePath()
	{
		return filePath;
	}

	/// <summary>
	/// Provides a <see cref="Stream"/> to allow for the playback of the audio contents.
	/// </summary>
	/// <returns>A <see cref="Stream"/> with the contents of the audio file, or a null stream if the file doesn't exist.</returns>
	public Stream GetAudioStream()
	{
		if (File.Exists(filePath))
		{
			return new FileStream(filePath, FileMode.Open, FileAccess.Read);
		}

		return Stream.Null;
	}
}