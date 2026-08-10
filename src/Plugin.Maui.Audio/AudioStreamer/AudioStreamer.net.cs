namespace Plugin.Maui.Audio;

/// <summary>
/// Platform-specific implementation of the <see cref="IAudioStreamer"/> interface for .NET platforms.
/// </summary>
public partial class AudioStreamer : IAudioStreamer
{
	/// <summary>
	/// Gets whether the device is capable of streaming audio.
	/// </summary>
	public bool CanStreamAudio => throw new PlatformNotSupportedException();

	/// <summary>
	/// Gets whether the streamer is currently streaming audio.
	/// </summary>
	public bool IsStreaming => throw new PlatformNotSupportedException();

	/// <summary>
	/// Gets the audio streaming options.
	/// </summary>
	public AudioStreamOptions Options => throw new PlatformNotSupportedException();

	/// <summary>
	/// Captured linear PCM audio (raw WAV audio)
	/// </summary>
	public event EventHandler<AudioStreamEventArgs>? OnAudioCaptured
	{
		add 
		{
			throw new PlatformNotSupportedException();
		}

		remove
		{
			throw new PlatformNotSupportedException();
		}
	}

	/// <summary>
	/// Start streaming audio to <see cref="OnAudioCaptured"/>.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public Task StartAsync() => throw new PlatformNotSupportedException();

	/// <summary>
	/// Stop streaming.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public Task StopAsync() => throw new PlatformNotSupportedException();
}