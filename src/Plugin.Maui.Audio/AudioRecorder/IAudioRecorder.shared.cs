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

	/// <Summary>
	/// Gets whether a sound (other than the ambient) was detected. Works only when DetectSilenceAsync fired.
	/// </Summary>
	bool SoundDetected { get; }

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

	/// <Summary>
	/// Detects silence that persists for a <paramref name="silenceDuration"/> ms period of time.
	/// The silence is when audio level is beyond <paramref name="silenceThreshold"/> multiplied by lowest detected audio level.
	/// </Summary>
	/// <param name="silenceThreshold"></param>
	/// <param name="silenceDuration"></param>
	Task DetectSilenceAsync(double silenceThreshold = 3, int silenceDuration = 1500);
}