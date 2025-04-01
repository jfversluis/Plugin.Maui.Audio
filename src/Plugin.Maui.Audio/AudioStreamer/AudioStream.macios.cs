using System.Diagnostics;
using System.Runtime.InteropServices;
using AudioToolbox;

namespace Plugin.Maui.Audio;

class AudioStream : IDisposable
{
	//
	// inspired by source https://github.com/NateRickard/Plugin.AudioRecorder/blob/master/Plugin.AudioRecorder.iOS/AudioStream.cs
	//

	const int countAudioBuffers = 3;
	const int maxBufferSize = 0x50000; // 320 KB
	const float targetMeasurementTime = 100F; // milliseconds

	InputAudioQueue? audioQueue;

	public AudioStream(AudioStreamOptions options)
	{
		Options = options;
	}

	public AudioStreamOptions Options { get; }

	public event EventHandler<byte[]>? OnBroadcast;
	public event EventHandler<bool>? OnActiveChanged;
	public event EventHandler<Exception>? OnException;

	public bool Active => audioQueue?.IsRunning ?? false;

	public Task Start()
	{
		if (Active)
		{
			return Task.CompletedTask;
		}

		try
		{
			var audioFormat = AudioStreamBasicDescription.CreateLinearPCM(Options.SampleRate, (uint)Options.Channels, (uint)Options.BitDepth);

			audioQueue = new InputAudioQueue(audioFormat);
			audioQueue.InputCompleted += QueueInputCompleted;

			// calculate our buffer size and make sure it's not too big
			var bufferByteSize = (int)(targetMeasurementTime / 1000F /*ms to sec*/ * Options.SampleRate * audioFormat.BytesPerPacket);
			bufferByteSize = bufferByteSize < maxBufferSize ? bufferByteSize : maxBufferSize;

			for (var index = 0; index < countAudioBuffers; index++)
			{
				var bufferPtr = IntPtr.Zero;

				BufferOperation(() => audioQueue.AllocateBuffer(bufferByteSize, out bufferPtr), () =>
				{
					BufferOperation(() => audioQueue.EnqueueBuffer(bufferPtr, bufferByteSize, null), () => Debug.WriteLine("AudioQueue buffer enqueued :: {0} of {1}", index + 1, countAudioBuffers));
				});
			}

			BufferOperation(() => audioQueue.Start(),
				() => OnActiveChanged?.Invoke(this, true),
				status => throw new Exception($"audioQueue.Start() returned non-OK status: {status}"));

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
		if (audioQueue == null)
		{
			return Task.CompletedTask;
		}

		audioQueue.InputCompleted -= QueueInputCompleted;

		if (audioQueue.IsRunning)
		{
			BufferOperation(() => audioQueue.Stop(true),
				() => OnActiveChanged?.Invoke(this, false),
				status => Debug.WriteLine("AudioStream.Stop() :: audioQueue.Stop returned non OK result: {0}", status));
		}

		audioQueue.Dispose();
		audioQueue = null;

		return Task.CompletedTask;
	}

	/// <summary>
	/// Wrapper function to run success/failure callbacks from an operation that returns an AudioQueueStatus.
	/// </summary>
	/// <param name="bufferFunction">The function that returns AudioQueueStatus.</param>
	/// <param name="successAction">The Action to run if the result is AudioQueueStatus.Ok.</param>
	/// <param name="failAction">The Action to run if the result is anything other than AudioQueueStatus.Ok.</param>
	static void BufferOperation(Func<AudioQueueStatus> bufferFunction, Action? successAction = null, Action<AudioQueueStatus?>? failAction = null)
	{
		AudioQueueStatus? status = null;
		try
		{
			status = bufferFunction();
		}
		catch
		{
			// ignore
		}

		if (status is AudioQueueStatus.Ok)
		{
			successAction?.Invoke();
		}
		else
		{
			if (failAction != null)
			{
				failAction(status);
			}
			else
			{
				throw new Exception($"AudioStream buffer error :: buffer operation returned non - Ok status:: {status}");
			}
		}
	}

	/// <summary>
	/// Handles iOS audio buffer queue completed message.
	/// </summary>
	/// <param name='sender'>Sender object</param>
	/// <param name='e'>Input completed parameters.</param>
	void QueueInputCompleted(object? sender, InputCompletedEventArgs e)
	{
		try
		{
			if (!Active)
			{
				return;
			}

			if (e.Buffer.AudioDataByteSize <= 0)
			{
				return;
			}

			var audioBytes = new byte[e.Buffer.AudioDataByteSize];
			Marshal.Copy(e.Buffer.AudioData, audioBytes, 0, (int)e.Buffer.AudioDataByteSize);

			OnBroadcast?.Invoke(this, audioBytes);

			// check if active again, because the auto stop logic may stop the audio queue from within this handler!
			if (Active)
			{
				BufferOperation(() => audioQueue.EnqueueBuffer(e.IntPtrBuffer, null), null, status =>
				{
					Debug.WriteLine("AudioStream.QueueInputCompleted() :: audioQueue.EnqueueBuffer returned non-Ok status :: {0}", status);
					OnException?.Invoke(this, new Exception($"audioQueue.EnqueueBuffer returned non-Ok status :: {status}"));
				});
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine("AudioStream.QueueInputCompleted() :: Error: {0}", ex.Message);

			OnException?.Invoke(this, new Exception($"AudioStream.QueueInputCompleted() :: Error: {ex.Message}"));
		}
	}

	public void Dispose()
	{
		audioQueue?.Dispose();
	}
}