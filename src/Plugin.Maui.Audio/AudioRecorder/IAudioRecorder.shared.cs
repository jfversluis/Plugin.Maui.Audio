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
	/// Stop recording and return the <see cref="IAudioSource"/> instance with the recording data.
	///</Summary>
	Task<IAudioSource> StopAsync();
}