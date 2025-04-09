using System.Diagnostics;
using AVFoundation;

namespace Plugin.Maui.Audio;

/// <summary>
/// Platform-specific implementation of the <see cref="IAudioStreamer"/> interface for macOS/iOS platforms.
/// </summary>
public partial class AudioStreamer : IAudioStreamer
{
	AudioStream? audioStream;

	/// <summary>
	/// Gets whether the device is capable of streaming audio.
	/// </summary>
	public bool CanStreamAudio => AVAudioSession.SharedInstance().InputAvailable;

	/// <summary>
	/// Gets whether the streamer is currently streaming audio.
	/// </summary>
	public bool IsStreaming => audioStream is { Active: true };

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

	/// <summary>
	/// Stop streaming.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
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