namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to create <see cref="IAudioPlayer" /> instances.
/// </summary>
public interface IAudioManager
{
	AudioPlayerOptions DefaultPlayerOptions { get; set; }

	AudioRecorderOptions DefaultRecorderOptions { get; set; }

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
