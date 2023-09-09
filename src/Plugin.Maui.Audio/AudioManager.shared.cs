namespace Plugin.Maui.Audio;

public class AudioManager : IAudioManager
{
	static IAudioManager? currentImplementation;

	public static IAudioManager Current => currentImplementation ??= new AudioManager();

	/// <inheritdoc />
	public IAudioPlayer CreatePlayer(Stream audioStream)
	{
		ArgumentNullException.ThrowIfNull(audioStream);

		return new AudioPlayer(audioStream);
	}

	/// <inheritdoc />
	public IAudioPlayer CreatePlayer(string fileName)
	{
		ArgumentNullException.ThrowIfNull(fileName);

        return new AudioPlayer(fileName);
    }

	/// <inheritdoc />
	public AsyncAudioPlayer CreateAsyncPlayer(Stream audioStream) =>
		new AsyncAudioPlayer(CreatePlayer(audioStream));

	/// <inheritdoc />
	public AsyncAudioPlayer CreateAsyncPlayer(string fileName) =>
        new AsyncAudioPlayer(CreatePlayer(fileName));

	/// <inheritdoc />
	public IAudioRecorder CreateRecorder()
	{
		return new AudioRecorder();
	}
}
