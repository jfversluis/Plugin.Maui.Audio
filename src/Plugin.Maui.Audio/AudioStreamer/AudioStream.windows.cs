using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Media;
using WinRT;

namespace Plugin.Maui.Audio;

class AudioStream : IDisposable
{
	//
	// inspired by source https://github.com/NateRickard/Plugin.AudioRecorder/blob/master/Plugin.AudioRecorder.UWP/AudioStream.cs
	//

	AudioGraph? audioGraph;
	AudioFrameOutputNode? outputNode;

	const int broadcastSize = 10; // we'll accumulate 10 'quantums' before broadcasting them
	int bufferPosition;
	byte[]? audioBytes;


	public AudioStream(AudioStreamOptions options)
	{
		Options = options;
	}

	public AudioStreamOptions Options { get; }

	public event EventHandler<byte[]> OnBroadcast;
	public event EventHandler<bool> OnActiveChanged;
	public event EventHandler<Exception> OnException;

	public bool Active { get; private set; }

	async Task Init()
	{
		try
		{
			await Stop();

			var pcmEncoding = AudioEncodingProperties.CreatePcm((uint)Options.SampleRate, (uint)Options.Channels, (uint)Options.BitDepth);
			// apparently this is not _really_ used/supported here, as the audio data seems to come thru as floats (so basically MediaEncodingSubtypes.Float?)
			pcmEncoding.Subtype = MediaEncodingSubtypes.Pcm;

			var graphSettings = new AudioGraphSettings(AudioRenderCategory.Media)
			{
				EncodingProperties = pcmEncoding,
				DesiredRenderDeviceAudioProcessing = AudioProcessing.Raw
				// these do not seem to take effect on certain hardware and MSFT recommends SystemDefault when recording to a file anyway
				//	We'll buffer audio data ourselves to improve RMS calculation across larger samples
				//QuantumSizeSelectionMode = QuantumSizeSelectionMode.ClosestToDesired,
				//DesiredSamplesPerQuantum = 4096
			};

			// create our audio graph... this will be a device input node feeding audio data into a frame output node
			var graphResult = await AudioGraph.CreateAsync(graphSettings);

			if (graphResult.Status == AudioGraphCreationStatus.Success)
			{
				audioGraph = graphResult.Graph;

				// take input from whatever the default communications device is set to me on windows
				var inputResult = await audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Communications, pcmEncoding);

				if (inputResult.Status == AudioDeviceNodeCreationStatus.Success)
				{
					// create the output node
					outputNode = audioGraph.CreateFrameOutputNode(pcmEncoding);

					// wire the input to the output
					inputResult.DeviceInputNode.AddOutgoingConnection(outputNode);

					// Attach to QuantumStarted event in order to receive synchronous updates from audio graph (to capture incoming audio)
					audioGraph.QuantumStarted += Graph_QuantumStarted;
					audioGraph.UnrecoverableErrorOccurred += Graph_UnrecoverableErrorOccurred;
				}
				else
				{
					throw new Exception($"audioGraph.CreateDeviceInputNodeAsync() returned non-Success status: {inputResult.Status}");
				}
			}
			else
			{
				throw new Exception($"AudioGraph.CreateAsync() returned non-Success status: {graphResult.Status}");
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"AudioStream.Init() :: Error: {ex.Message}");
			OnException?.Invoke(this, new Exception($"AudioStream.Init() :: Error: {ex.Message}"));
		}
	}

	public async Task Start()
	{
		try
		{
			if (!Active)
			{
				await Init();

				audioGraph.Start();

				Active = true;
				OnActiveChanged?.Invoke(this, true);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Error in AudioStream.Start(): {0}", ex.Message);

			await Stop();
			throw;
		}
	}

	public Task Stop()
	{
		if (Active)
		{
			Active = false;

			outputNode?.Stop();
			audioGraph?.Stop();

			OnActiveChanged?.Invoke(this, false);
		}

		outputNode?.Dispose();
		outputNode = null;

		if (audioGraph is not null)
		{
			audioGraph.QuantumStarted -= Graph_QuantumStarted;
			audioGraph.UnrecoverableErrorOccurred -= Graph_UnrecoverableErrorOccurred;
			audioGraph.Dispose();
			audioGraph = null;
		}

		return Task.CompletedTask;
	}

	/// <summary>
	/// IMemoryBuferByteAccess is used to access the underlying audioframe for read and write
	/// </summary>
	[ComImport]
	[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe interface IMemoryBufferByteAccess
	{
		void GetBuffer(out byte* buffer, out uint capacity);
	}

	unsafe void Graph_QuantumStarted(AudioGraph sender, object args)
	{
		// we'll only broadcast if we're actively monitoring audio packets
		if (!Active || outputNode is null)
		{
			return;
		}

		try
		{
			var frame = outputNode.GetFrame();
			if (frame.Duration?.Milliseconds == 0) 
			{
				return;
			}

			using var buffer = frame.LockBuffer(AudioBufferAccessMode.Read);
			using var reference = buffer.CreateReference();

			var memoryBuffer = reference.As<IMemoryBufferByteAccess>();
			memoryBuffer.GetBuffer(out byte* dataInBytes, out uint capacityInBytes);

			float* dataInFloat = (float*)dataInBytes;

			if (audioBytes is null)
			{
				audioBytes = new byte[buffer.Length * broadcastSize / 2]; // buffer length * # of frames we want to accrue / 2 (because we're transforming float audio to Int 16)
			}

			for (int i = 0; i < capacityInBytes / sizeof(float); i++)
			{
				// convert the float into a double byte for 16 bit PCM
				var shortVal = FloatToInt16(dataInFloat[i]);
				byte[] chunkBytes = BitConverter.GetBytes(shortVal);

				audioBytes[bufferPosition++] = chunkBytes[0];
				audioBytes[bufferPosition++] = chunkBytes[1];
			}

			//  we want to wait until we accrue <broadcastSize> # of frames and then broadcast them
			//	in practice, this will take us from 20ms chunks to 100ms chunks and result in more accurate audio level calculations
			//	we could maybe use the audiograph latency settings to achieve similar results but this seems to work well
			if (bufferPosition == audioBytes.Length || !Active)
			{
				// broadcast the audio data to any listeners
				OnBroadcast?.Invoke(this, audioBytes);

				audioBytes = null;
				bufferPosition = 0;
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"AudioStream.QueueInputCompleted() :: Error: {ex.Message}");
			OnException?.Invoke(this, new Exception($"AudioStream.QueueInputCompleted() :: Error: {ex.Message}"));
		}
	}

	async void Graph_UnrecoverableErrorOccurred(AudioGraph sender, AudioGraphUnrecoverableErrorOccurredEventArgs args)
	{
		await Stop();

		throw new Exception($"UnrecoverableErrorOccurred error: {args.Error}");
	}

	public void Flush()
	{
		if (audioBytes is null)
		{
			return;
		}

		OnBroadcast?.Invoke(this, audioBytes);
		audioBytes = null;
	}

	static short FloatToInt16(float value)
	{
		float f = value * short.MaxValue;
		if (f > short.MaxValue)
		{
			f = short.MaxValue;
		}

		if (f < short.MinValue)
		{
			f = short.MinValue;
		}

		return (short)f;
	}

	public void Dispose()
	{
		audioGraph?.Dispose();
		outputNode?.Dispose();
	}
}