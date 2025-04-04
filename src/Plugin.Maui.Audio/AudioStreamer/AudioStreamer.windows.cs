using System.Diagnostics;

namespace Plugin.Maui.Audio;

public partial class AudioStreamer : IAudioStreamer
{
	AudioStream? audioStream;

	public bool CanStreamAudio { get; private set; } = true;

	public bool IsStreaming => audioStream is { Active: true };

	public AudioStreamOptions Options { get; } = AudioManager.Current.DefaultStreamerOptions;

	public event EventHandler<AudioStreamEventArgs>? OnAudioCaptured;
	
	public async Task StartAsync()
	{
		if (IsStreaming)
		{
			Trace.WriteLine("AudioStreamer already streaming");
			return;
		}

		if (audioStream is not null
		    && !Options.Equals(audioStream.Options))
		{
			audioStream.OnBroadcast -= OnAudioStreamBroadcast;
			audioStream.Dispose();
			audioStream = null;
		}

		if (Options.BitDepth != BitDepth.Pcm16bit)
		{
			throw new NotSupportedException($"Windows only supports BitDepth {BitDepth.Pcm16bit}");
		}

		if (audioStream is null)
		{
			audioStream = new AudioStream(Options);
			audioStream.OnBroadcast += OnAudioStreamBroadcast;
		}

		await audioStream.Start();
	}

	public async Task StopAsync()
	{
		if (audioStream is not null)
		{
			await audioStream.Stop();
			audioStream.Flush();
		}
	}

	void OnAudioStreamBroadcast(object? sender, byte[] audio)
	{
		OnAudioCaptured?.Invoke(this, new AudioStreamEventArgs(audio));
	}
}
