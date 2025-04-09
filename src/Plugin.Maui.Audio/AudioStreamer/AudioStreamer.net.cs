namespace Plugin.Maui.Audio;

/// <summary>
/// Platform-specific implementation of the <see cref="IAudioStreamer"/> interface for .NET platforms.
/// </summary>
public partial class AudioStreamer : IAudioStreamer
{
	/// <summary>
	/// Gets whether the device is capable of streaming audio.
	/// </summary>
	public bool CanStreamAudio => false;

	/// <summary>
	/// Gets whether the streamer is currently streaming audio.
	/// </summary>
	public bool IsStreaming => false;

	/// <summary>
	/// Gets the audio streaming options.
	/// </summary>
	public AudioStreamOptions Options { get; } = AudioManager.Current.DefaultStreamerOptions;

	/// <summary>
	/// Captured linear PCM audio (raw WAV audio)
	/// </summary>
	public event EventHandler<AudioStreamEventArgs>? OnAudioCaptured;

	/// <summary>
	/// Start streaming audio to <see cref="OnAudioCaptured"/>.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public Task StartAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Stop streaming.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public Task StopAsync()
	{
		return Task.CompletedTask;
	}
}