using System.Diagnostics;

namespace Plugin.Maui.Audio;

public partial class AudioStreamer : IAudioStreamer
{
	AudioStream? audioStream;

	public AudioStreamer()
	{
		var packageManager = Android.App.Application.Context.PackageManager;
		CanStreamAudio = packageManager?.HasSystemFeature(Android.Content.PM.PackageManager.FeatureMicrophone) ?? false;
	}

	public bool CanStreamAudio { get; private set; }

	public bool IsStreaming => audioStream is { Active: true }; 

	public AudioStreamOptions Options { get; } = AudioManager.Current.DefaultStreamerOptions;

	public event EventHandler<AudioStreamEventArgs>? OnAudioCaptured;

	public async Task StartAsync()
	{
		if (CanStreamAudio == false)
		{
			Trace.WriteLine("AudioStreamer is not supported");
			return;
		}

		if (IsStreaming)
		{
			Trace.WriteLine("AudioStreamer already streaming");
			return;
		}

		if (audioStream != null
		    && !Options.Equals(audioStream.Options))
		{
			audioStream.OnBroadcast -= OnAudioStreamBroadcast;
			audioStream.Dispose();
			audioStream = null;
		}

		if (audioStream == null)
		{
			audioStream = new AudioStream(Options);
			audioStream.OnBroadcast += OnAudioStreamBroadcast;
		}

		await audioStream.Start();
	}

	public async Task StopAsync()
	{
		if (audioStream != null)
		{
			await audioStream.Stop();
		}
	}
	
	void OnAudioStreamBroadcast(object? sender, byte[] audio)
	{
		OnAudioCaptured?.Invoke(this, new AudioStreamEventArgs(audio));
	}
}