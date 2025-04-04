namespace Plugin.Maui.Audio;

public partial class AudioStreamer : IAudioStreamer
{
	public bool CanStreamAudio => false;

	public bool IsStreaming => false;

	public AudioStreamOptions Options { get; } = AudioManager.Current.DefaultStreamerOptions;

	public event EventHandler<AudioStreamEventArgs>? OnAudioCaptured;

	public Task StartAsync()
	{
		return Task.CompletedTask;
	}

	public Task StopAsync()
	{
		return Task.CompletedTask;
	}
}