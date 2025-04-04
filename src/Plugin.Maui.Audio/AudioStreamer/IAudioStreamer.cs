namespace Plugin.Maui.Audio;

/// <summary>
/// Provides the ability to stream audio.
/// </summary>
public interface IAudioStreamer
{
	///<Summary>
	/// Gets whether the device is capable of streaming audio.
	///</Summary>
	bool CanStreamAudio { get; }

	///<Summary>
	/// Gets whether the streamer is currently streaming audio.
	///</Summary>
	bool IsStreaming { get; }

	/// <summary>
	/// Gets or sets the audio streaming options.
	/// Android: 44100 Hz is recommended (supported: 8000, 16000 and 44100 Hz)
	/// iOS: 48000 Hz is recommended
	/// Windows: only BitDepth.Pcm16bit is supported
	/// </summary>
	AudioStreamOptions Options { get; }

	///<Summary>
	/// Start streaming audio to <see cref="OnAudioCaptured"/>.
	///</Summary>
	///<param name="options">The audio streaming options</param>
	Task StartAsync();

	///<Summary>
	/// Stop streaming
	///</Summary>
	Task StopAsync();

	/// <summary>
	/// Captured linear PCM audio (raw WAV audio)
	/// </summary>
	event EventHandler<AudioStreamEventArgs> OnAudioCaptured;
}