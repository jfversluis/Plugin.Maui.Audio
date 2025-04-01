using System.Diagnostics;
using Android.Media;

namespace Plugin.Maui.Audio;

class AudioStream : IDisposable
{
	AudioRecord? audioRecord;

	public AudioStream(AudioStreamOptions options)
	{
		Options = options;
	}

	public AudioStreamOptions Options { get; }

	public event EventHandler<byte[]>? OnBroadcast;
	public event EventHandler<bool>? OnActiveChanged;
	public event EventHandler<Exception>? OnException;

	public bool Active => audioRecord?.RecordingState == RecordState.Recording;

	public Task Start()
	{
		var channelIn = Options.Channels switch
		{
			ChannelType.Stereo => ChannelIn.Stereo,
			_ => ChannelIn.Mono
		};

		var encoding = Options.BitDepth switch
		{
			BitDepth.Pcm8bit => Android.Media.Encoding.Pcm8bit,
			_ => Android.Media.Encoding.Pcm16bit
		};

		try
		{
			var bufferSize = AudioRecord.GetMinBufferSize(Options.SampleRate, channelIn, encoding);

			// If the bufferSize is less than or equal to 0, then this device does not support the provided options
			if (bufferSize <= 0)
			{
				throw new FailedToStartRecordingException("Unable to get bufferSize with provided options.");
			}

			audioRecord = new AudioRecord(AudioSource.Mic, Options.SampleRate, channelIn, encoding, bufferSize);
			audioRecord.StartRecording();

			Task.Run(() => WriteAudioDataToEvent(bufferSize));
			OnActiveChanged?.Invoke(this, true);
		}
		catch (Exception ex)
		{
			Trace.WriteLine("Error in AudioStream.Start(): {0}", ex.Message);

			Stop();
			throw;
		}

		return Task.CompletedTask;
	}

	public Task Stop()
	{
		if (Active)
		{
			audioRecord?.Stop();
			OnActiveChanged?.Invoke(this, false);
		}

		return Task.CompletedTask;
	}

	void WriteAudioDataToEvent(int bufferSize)
	{
		var data = new byte[bufferSize];

		try
		{
			if (audioRecord == null)
			{
				throw new NullReferenceException("AudioRecord has not been set");
			}

			while (audioRecord.RecordingState == RecordState.Recording)
			{
				var read = audioRecord.Read(data, 0, bufferSize);
				var readData = data.Take(read).ToArray();

				OnBroadcast?.Invoke(this, readData);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"AudioStream.WriteAudioDataToEvent() :: Error: {ex.Message}");
			OnException?.Invoke(this, new Exception($"AudioStream.WriteAudioDataToEvent() :: Error: {ex.Message}"));
		}
	}
	
	public void Dispose()
	{
		audioRecord?.Dispose();
	}
}