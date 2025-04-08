using System.Diagnostics;
using AVFoundation;

namespace Plugin.Maui.Audio;

public partial class AudioStreamer : IAudioStreamer
{
	AudioStream? audioStream;

	public bool CanStreamAudio => AVAudioSession.SharedInstance().InputAvailable;

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
		    && (audioStream.BitDepth != Options.BitDepth
		        || audioStream.Channels != Options.Channels
		        || audioStream.SampleRate != Options.SampleRate))
		{
			audioStream.OnBroadcast -= OnAudioStreamBroadcast;
			audioStream.Dispose();
			audioStream = null;
		}

		ActiveSessionHelper.InitializeSession(Options);

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
		}

		ActiveSessionHelper.FinishSession(Options);
	}

	void OnAudioStreamBroadcast(object? sender, byte[] audio)
	{
		OnAudioCaptured?.Invoke(this, new AudioStreamEventArgs(audio));
	}
}