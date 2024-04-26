namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to record audio.
/// </summary>
public interface IAudioRecorder
{
	///<Summary>
	/// Gets whether the device is capable of recording audio.
	///</Summary>
	bool CanRecordAudio { get; }

	///<Summary>
	/// Gets whether the recorder is currently recording audio.
	///</Summary>
	bool IsRecording { get; }

	///<Summary>
	/// Start recording audio to disk in a randomly generated file.
	///</Summary>
	Task StartAsync();

	///<Summary>
	/// Start recording audio to disk in the supplied <paramref name="filePath"/>.
	///</Summary>
	///<param name="filePath">The path on disk to store the recording.</param>
	Task StartAsync(string filePath);

	///<Summary>
	/// Start recording audio to disk in a randomly generated file. AudioRecordingOptions are only supported on Android and iOS.
	///</Summary>
	///<param name="options">The audio recording options.</param>
	public Task StartAsync(AudioRecordingOptions options) => Task.CompletedTask;

	///<Summary>
	/// Start recording audio to disk in the supplied <paramref name="filePath"/>.
	///</Summary>
	///<param name="filePath">The path on disk to store the recording. AudioRecordingOptions are only supported on Android and iOS. If options are used, read the audio stream and write your own file. The default header might not match your options.</param>
	///<param name="options">The audio recording options.</param>
	public Task StartAsync(string filePath, AudioRecordingOptions options) => Task.CompletedTask;

	///<Summary>
	/// Stop recording and return the <see cref="IAudioSource"/> instance with the recording data.
	///</Summary>
	Task<IAudioSource> StopAsync();
}