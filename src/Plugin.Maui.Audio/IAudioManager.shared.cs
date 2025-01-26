namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to create audio playback and recording instances.
/// </summary>
public interface IAudioManager
{
	/// <summary>
	/// Gets or sets the default options to use when creating a new <see cref="IAudioPlayer"/>.
	/// </summary>
	AudioPlayerOptions DefaultPlayerOptions { get; set; }

	/// <summary>
	/// Gets or sets the default options to use when creating a new <see cref="IAudioRecorder"/>.
	/// </summary>
	AudioRecorderOptions DefaultRecorderOptions { get; set; }

	/// <summary>
	/// Creates a new <see cref="IAudioPlayer"/> with with an empty source.
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	IAudioPlayer CreatePlayer(AudioPlayerOptions? options = default);

	/// <summary>
	/// Creates a new <see cref="IAudioPlayer"/> with the supplied <paramref name="audioStream"/> ready to play.
	/// </summary>
	/// <param name="audioStream">The <see cref="Stream"/> containing the audio to play.</param>
	/// <returns>A new <see cref="IAudioPlayer"/> with the supplied <paramref name="audioStream"/> ready to play.</returns>
	IAudioPlayer CreatePlayer(Stream audioStream, AudioPlayerOptions? options = default);

	/// <summary>
	/// Creates a new <see cref="IAudioPlayer"/> with the supplied <paramref name="fileName"/> ready to play.
	/// </summary>
	/// <param name="fileName">The name of the file containing the audio to play.</param>
	/// <returns>A new <see cref="IAudioPlayer"/> with the supplied <paramref name="fileName"/> ready to play.</returns>
	IAudioPlayer CreatePlayer(string fileName, AudioPlayerOptions? options = default);

	/// <summary>
	/// Creates a new <see cref="AsyncAudioPlayer"/> with the supplied <paramref name="audioStream"/> ready to play audio using async/await.
	/// </summary>
	/// <param name="audioStream">The <see cref="Stream"/> containing the audio to play.</param>
	/// <returns>A new <see cref="AsyncAudioPlayer"/> with the supplied <paramref name="audioStream"/> ready to play.</returns>
	AsyncAudioPlayer CreateAsyncPlayer(Stream audioStream, AudioPlayerOptions? options = default);

	/// <summary>
	/// Creates a new <see cref="AsyncAudioPlayer"/> with the supplied <paramref name="fileName"/> ready to play audio using async/await.
	/// </summary>
	/// <param name="fileName">The name of the file containing the audio to play.</param>
	/// <returns>A new <see cref="AsyncAudioPlayer"/> with the supplied <paramref name="fileName"/> ready to play.</returns>
	AsyncAudioPlayer CreateAsyncPlayer(string fileName, AudioPlayerOptions? options = default);

	/// <summary>
	/// Creates a new <see cref="IAudioRecorder"/> ready to begin recording audio from the current device.
	/// </summary>
	/// <returns>A new <see cref="IAudioRecorder"/> ready to begin recording.</returns>
	IAudioRecorder CreateRecorder(AudioRecorderOptions? options = default);
}
