namespace Plugin.Maui.Audio;

public class AudioManager : IAudioManager
{
	static IAudioManager? currentImplementation;

	public static IAudioManager Current => currentImplementation ??= new AudioManager();

	public AudioPlayerOptions DefaultPlayerOptions { get; set; } = new();

	public AudioRecorderOptions DefaultRecorderOptions { get; set; } = new();

	/// <inheritdoc />
	public IAudioPlayer CreatePlayer(Stream audioStream, AudioPlayerOptions? options = default)
	{
		ArgumentNullException.ThrowIfNull(audioStream);

		return new AudioPlayer(audioStream, options ?? DefaultPlayerOptions);
	}

	/// <inheritdoc />
	public IAudioPlayer CreatePlayer(string fileName, AudioPlayerOptions? options = default)
	{
		ArgumentNullException.ThrowIfNull(fileName);

        return new AudioPlayer(fileName, options ?? DefaultPlayerOptions);
    }

	/// <inheritdoc />
	public AsyncAudioPlayer CreateAsyncPlayer(Stream audioStream, AudioPlayerOptions? options = default) => new (CreatePlayer(audioStream));

	/// <inheritdoc />
	public AsyncAudioPlayer CreateAsyncPlayer(string fileName, AudioPlayerOptions? options = default) => new (CreatePlayer(fileName));

	/// <inheritdoc />
	public IAudioRecorder CreateRecorder(AudioRecorderOptions? options = default)
	{
		return new AudioRecorder(options ?? DefaultRecorderOptions);
	}
}
