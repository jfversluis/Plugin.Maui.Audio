using System.Diagnostics;
using Android.Content;
using Android.Media;
using Java.IO;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public bool CanRecordAudio { get; private set; }
	public bool IsRecording => audioRecord?.RecordingState == RecordState.Recording;

	AudioRecord? audioRecord;
	string? rawFilePath;
	string? audioFilePath;

	int bufferSize;
	int sampleRate;
	readonly AudioRecorderOptions options;
	int channels;
	int bitDepth;
	byte[] audioData;
    byte[]? audioDataChunk;

    public AudioRecorder(AudioRecorderOptions options)
	{
		var packageManager = Android.App.Application.Context.PackageManager;

		CanRecordAudio = packageManager?.HasSystemFeature(Android.Content.PM.PackageManager.FeatureMicrophone) ?? false;
		this.options = options;
	}

	public Task StartAsync(AudioRecordingOptions options) => StartAsync(GetTempFilePath(), options);
	public Task StartAsync() => StartAsync(GetTempFilePath(), DefaultAudioRecordingOptions.DefaultOptions);
	public Task StartAsync(string filePath) => StartAsync(filePath, DefaultAudioRecordingOptions.DefaultOptions);


	public Task StartAsync(string filePath, AudioRecordingOptions options)
	{
		if (CanRecordAudio == false || audioRecord?.RecordingState == RecordState.Recording)
		{
			return Task.CompletedTask;
		}
		options ??= DefaultAudioRecordingOptions.DefaultOptions;

		audioFilePath = filePath;

		var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as Android.Media.AudioManager;

		Android.Media.Encoding encoding = SharedEncodingToAndroidEncoding(options.Encoding, options.BitDepth, options.ThrowIfNotSupported);
		ChannelIn channelIn = SharedChannelTypesToAndroidChannelTypes(options.Channels, options.ThrowIfNotSupported);

		int sampleRate = options.SampleRate;
		int bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelIn, encoding);

		// If the bufferSize is less than or equal to 0, then this device does not support the provided options
		if (bufferSize <= 0)
		{
			if (options.ThrowIfNotSupported)
			{
				throw new FailedToStartRecordingException("Unable to get bufferSize with provided reording options.");
			}
			else
			{
				sampleRate = AudioRecordingOptions.DefaultSampleRate;
				bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelIn, encoding);

				if (bufferSize <= 0)
				{
					var rate = (audioManager?.GetProperty(Android.Media.AudioManager.PropertyOutputSampleRate)) ?? throw new FailedToStartRecordingException("Unable to get the sample rate.");
					sampleRate = int.Parse(rate);
				}
			}
		}

			audioRecord = GetAudioRecord(sampleRate, channelIn, encoding, (int)options.BitDepth);
			audioData = new byte[bufferSize];

			audioRecord.StartRecording();
			SoundDetected = false;
			Task.Run(WriteAudioDataToFile);
		}
		return Task.CompletedTask;
	}

	AudioRecord GetAudioRecord(int sampleRate, ChannelIn channels, Android.Media.Encoding encoding, int bitDepth)
	{
		this.sampleRate = sampleRate;
		this.bitDepth = bitDepth;
		this.channels = channels == ChannelIn.Mono ? 1 : 2;
		this.bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channels, encoding) * bitDepth;

		return new AudioRecord(AudioSource.Mic, sampleRate, channels, encoding, bufferSize);
	}

	public Task<IAudioSource> StopAsync()
	{
		if (audioRecord?.RecordingState == RecordState.Recording)
		{
			audioRecord?.Stop();
		}

		if (audioFilePath is null)
		{
			throw new InvalidOperationException("'audioFilePath' is null, this really should not happen.");
		}

		CopyWaveFile(rawFilePath, audioFilePath);

		try
		{
			// lets delete the temp file with the raw data, after we have created the WAVE file
			if (System.IO.File.Exists(rawFilePath))
			{
				System.IO.File.Delete(rawFilePath);
			}
		}
		catch
		{
			Trace.TraceWarning("delete raw wav file failed.");
		}

		return Task.FromResult(GetRecording());
	}

	IAudioSource GetRecording()
	{
		if (audioRecord is null ||
			audioRecord.RecordingState == RecordState.Recording ||
			System.IO.File.Exists(audioFilePath) == false)
		{
			return new EmptyAudioSource();
		}

		return new FileAudioSource(audioFilePath);
	}

	public string? GetAudioFilePath()
	{
		return audioFilePath;
	}

	static string GetTempFilePath()
	{
		return Path.Combine("/sdcard/", Path.GetTempFileName());
	}

	void WriteAudioDataToFile()
	{
		rawFilePath = GetTempFilePath();

		FileOutputStream? outputStream;

		try
		{
			outputStream = new FileOutputStream(rawFilePath);
		}
		catch (Exception ex)
		{
			throw new FileLoadException($"unable to create a new file: {ex.Message}");
		}

		if (audioRecord is not null && outputStream is not null)
		{
			while (audioRecord.RecordingState == RecordState.Recording)
			{
				audioRecord.Read(audioData, 0, bufferSize);

				outputStream.Write(audioData);
			}

			outputStream.Close();
		}
	}

	void CopyWaveFile(string? sourcePath, string destinationPath)
	{
		long byteRate = sampleRate * bitDepth * channels / 8;


		try
		{
			FileInputStream inputStream = new(sourcePath);
			FileOutputStream outputStream = new(destinationPath);

			if (inputStream?.Channel is not null)
			{
				var totalAudioLength = inputStream.Channel.Size();
				var totalDataLength = totalAudioLength + 36;

				WriteWaveFileHeader(outputStream, totalAudioLength, totalDataLength, sampleRate, channels, byteRate);

				while (inputStream.Read(audioData) != -1)
				{
					outputStream.Write(audioData);
				}

				inputStream.Close();
				outputStream.Close();
			}
		}
		catch (Exception ex)
		{
			// Trace the exception
			Trace.WriteLine($"An error occurred while copying the wave file: {ex.Message}");
			Trace.WriteLine($"Stack Trace: {ex.StackTrace}");
		}
	}

	static void WriteWaveFileHeader(FileOutputStream outputStream, long audioLength, long dataLength, long sampleRate, int channels, long byteRate)
	{
		byte[] header = new byte[44];

		header[0] = Convert.ToByte('R'); // RIFF/WAVE header
		header[1] = Convert.ToByte('I'); // (byte)'I'
		header[2] = Convert.ToByte('F');
		header[3] = Convert.ToByte('F');
		header[4] = (byte)(dataLength & 0xff);
		header[5] = (byte)((dataLength >> 8) & 0xff);
		header[6] = (byte)((dataLength >> 16) & 0xff);
		header[7] = (byte)((dataLength >> 24) & 0xff);
		header[8] = Convert.ToByte('W');
		header[9] = Convert.ToByte('A');
		header[10] = Convert.ToByte('V');
		header[11] = Convert.ToByte('E');
		header[12] = Convert.ToByte('f'); // fmt chunk
		header[13] = Convert.ToByte('m');
		header[14] = Convert.ToByte('t');
		header[15] = (byte)' ';
		header[16] = 16; // 4 bytes - size of fmt chunk
		header[17] = 0;
		header[18] = 0;
		header[19] = 0;
		header[20] = 1; // format = 1
		header[21] = 0;
		header[22] = Convert.ToByte(channels);
		header[23] = 0;
		header[24] = (byte)(sampleRate & 0xff);
		header[25] = (byte)((sampleRate >> 8) & 0xff);
		header[26] = (byte)((sampleRate >> 16) & 0xff);
		header[27] = (byte)((sampleRate >> 24) & 0xff);
		header[28] = (byte)(byteRate & 0xff);
		header[29] = (byte)((byteRate >> 8) & 0xff);
		header[30] = (byte)((byteRate >> 16) & 0xff);
		header[31] = (byte)((byteRate >> 24) & 0xff);
		header[32] = (byte)(2 * 16 / 8); // block align
		header[33] = 0;
		header[34] = Convert.ToByte(16); // bits per sample
		header[35] = 0;
		header[36] = Convert.ToByte('d');
		header[37] = Convert.ToByte('a');
		header[38] = Convert.ToByte('t');
		header[39] = Convert.ToByte('a');
		header[40] = (byte)(audioLength & 0xff);
		header[41] = (byte)((audioLength >> 8) & 0xff);
		header[42] = (byte)((audioLength >> 16) & 0xff);
		header[43] = (byte)((audioLength >> 24) & 0xff);

		outputStream.Write(header, 0, 44);
	}

	static Android.Media.Encoding SharedEncodingToAndroidEncoding(Encoding type, BitDepth bitDepth, bool throwIfNotSupported)
	{
		return bitDepth switch
		{
			BitDepth.Pcm8bit => type switch
			{
				Encoding.LinearPCM => Android.Media.Encoding.Pcm8bit,
				_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedEncodingToAndroidEncoding(Encoding.LinearPCM, bitDepth, true)
			},
			BitDepth.Pcm16bit => type switch
			{
				Encoding.LinearPCM => Android.Media.Encoding.Pcm16bit,
				_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedEncodingToAndroidEncoding(Encoding.LinearPCM, bitDepth, true)
			},
			_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedEncodingToAndroidEncoding(Encoding.LinearPCM, AudioRecordingOptions.DefaultBitDepth, true)
		};
	}

	static ChannelIn SharedChannelTypesToAndroidChannelTypes(ChannelType type, bool throwIfNotSupported)
	{
		return type switch
		{
			ChannelType.Mono => ChannelIn.Mono,
			ChannelType.Stereo => ChannelIn.Stereo,
			_ => throwIfNotSupported ? throw new NotSupportedException("channel type not supported") : SharedChannelTypesToAndroidChannelTypes(AudioRecordingOptions.DefaultChannels, true)
		};
	}

	byte[]? GetAudioDataChunk()
	{
		byte[] buffer = new byte[bufferSize];

		audioDataChunk ??= new byte[bufferSize];

		if (!audioDataChunk.SequenceEqual(audioData))
		{
			Array.Copy(audioData, buffer, bufferSize);
			audioDataChunk = buffer;
			return buffer;
		}
		else
		{
			return null;
		}
	}
}
