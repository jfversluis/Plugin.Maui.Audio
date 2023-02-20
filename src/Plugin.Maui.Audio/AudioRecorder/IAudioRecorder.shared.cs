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
	/// Start recording
	///</Summary>
	Task StartAsync();

	///<Summary>
	/// Start recording 
	///</Summary>
	Task StartAsync(string filePath);

	///<Summary>
	/// Stop recording and return the AudioRecording instance with the recording data
	///</Summary>
	Task<IAudioSource> StopAsync();
}