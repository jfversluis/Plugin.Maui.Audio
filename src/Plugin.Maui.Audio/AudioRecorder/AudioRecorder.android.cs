using System.Diagnostics;
using Android.Content;
using Android.Media;
using Java.IO;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public bool CanRecordAudio { get; private set; }

	public bool IsRecording =>
		audioRecord?.RecordingState == RecordState.Recording
		|| mediaRecorderIsRecording;

	MediaRecorder? mediaRecorder; // allows AAC and other compressed/encoded options
	AudioRecord? audioRecord; // allows WAV PCM only

	bool mediaRecorderIsRecording = false; // Necessary for MediaRecorder as there is no built-in "isRecording" method or property 

	string? audioFilePath;
	
	static readonly AudioRecorderOptions defaultOptions = new AudioRecorderOptions();

	AudioRecorderOptions audioRecorderOptions;

	public event EventHandler<AudioStreamEventArgs>? AudioStreamCaptured;

	// Recording options that are extracted/solved
	int bufferSize; // needed for AudioRecord
	int sampleRate;
	int channels;
	int bitDepth;

	// Constructor
	public AudioRecorder(AudioRecorderOptions options)
	{
		var packageManager = Android.App.Application.Context.PackageManager;

		CanRecordAudio = packageManager?.HasSystemFeature(Android.Content.PM.PackageManager.FeatureMicrophone) ?? false;
		this.audioRecorderOptions = options;
	}

	public Task StartAsync(AudioRecorderOptions? options = null) => StartAsync(GetTempFilePath(), options);

	// Start Recording Main Function
	public Task StartAsync(string filePath, AudioRecorderOptions? recordingOptions = null)
	{
		// Check if can record or already recording
		if (CanRecordAudio == false
		    || audioRecord?.RecordingState == RecordState.Recording // AudioRecord is recording
		    || mediaRecorderIsRecording) // MediaRecorder is recording
		{
			return Task.CompletedTask;
		}

		if (recordingOptions is not null)
		{
			this.audioRecorderOptions = recordingOptions;
		}

		audioFilePath = filePath;

		// solve some parameters needed for AudioRecord/MediaRecorder
		ChannelIn channelIn =
			SharedChannelTypesToAndroidChannelTypes(audioRecorderOptions.Channels, audioRecorderOptions.ThrowIfNotSupported);
		this.sampleRate = audioRecorderOptions.SampleRate;
		this.bitDepth = (int)audioRecorderOptions.BitDepth;
		this.channels = channelIn == ChannelIn.Mono ? 1 : 2;
		int bitRate = audioRecorderOptions.BitRate;
		int numChannels = (int)audioRecorderOptions.Channels;

		// Wav: AudioRecord Method
		if (audioRecorderOptions.Encoding == Encoding.Wav)
		{
			// Get encoding
			Android.Media.Encoding encoding = SharedWavEncodingToAndroidEncoding(audioRecorderOptions.Encoding,
				audioRecorderOptions.BitDepth, audioRecorderOptions.ThrowIfNotSupported);

			// Check buffer size 
			bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelIn, encoding);

			// If the bufferSize is less than or equal to 0, then this device does not support the provided options
			if (bufferSize <= 0)
			{
				if (audioRecorderOptions.ThrowIfNotSupported)
				{
					throw new FailedToStartRecordingException(
						"Unable to get bufferSize with provided recording options.");
				}
				else
				{
					sampleRate = defaultOptions.SampleRate;
					bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelIn, encoding);

					if (bufferSize <= 0)
					{
						var audioManager =
							Android.App.Application.Context.GetSystemService(Context.AudioService) as
								Android.Media.AudioManager;
						var rate = (audioManager?.GetProperty(Android.Media.AudioManager.PropertyOutputSampleRate)) ??
						           throw new FailedToStartRecordingException("Unable to get the sample rate.");
						sampleRate = int.Parse(rate);
					}
				}
			}

			audioRecord = new AudioRecord(AudioSource.Mic, sampleRate, channelIn, encoding, bufferSize);
			audioRecord.StartRecording();

			switch (audioRecorderOptions.CaptureMode)
			{
				case CaptureMode.Bundling:
					Task.Run(WriteAudioDataToFile);
					break;
				case CaptureMode.Streaming:
					Task.Run(WriteAudioDataToEvent);
					break;
			}

			return Task.CompletedTask;
		}
		
		if (audioRecorderOptions.Encoding == Encoding.Aac)
		{
			if (audioRecorderOptions.CaptureMode == CaptureMode.Streaming)
			{
				throw new NotSupportedException(
					$"Encoding '{audioRecorderOptions.Encoding}' is not supported with '{audioRecorderOptions.CaptureMode}' mode");
			}

			// Solve encoding
			AudioEncoder audioEncoder = AudioEncoder.Default;
			OutputFormat outputFormat = OutputFormat.Default;

			// Parse the RecordingOptions into AudioEncoder & OutputFormat
			if (audioRecorderOptions.Encoding == Encoding.Aac)
			{
				audioEncoder = AudioEncoder.Aac;
				outputFormat = OutputFormat.Mpeg4; //creates mp4 aac file (functionally identical to an M4A) 
			}

			// Create MediaRecorder
			mediaRecorder =
				new MediaRecorder(Platform.CurrentActivity
					.ApplicationContext); //needs context, obsoleted without context https://stackoverflow.com/questions/73598179/deprecated-mediarecorder-new-mediarecorder#73598440
			
			mediaRecorder.Reset();
			mediaRecorder.SetAudioSource(AudioSource.Mic);
			mediaRecorder.SetOutputFormat(outputFormat);
			mediaRecorder.SetAudioEncoder(audioEncoder);
			mediaRecorder.SetAudioChannels(numChannels);
			mediaRecorder.SetAudioSamplingRate(sampleRate);
			mediaRecorder.SetAudioEncodingBitRate(bitRate);
			mediaRecorder.SetOutputFile(audioFilePath);
			mediaRecorder.Prepare();
			mediaRecorder.Start();

			// Set MediaRecorder "is recording" flag true
			mediaRecorderIsRecording = true;
		}
		else if (audioRecorderOptions.ThrowIfNotSupported)
		{
			throw new FailedToStartRecordingException($"Encoding '{audioRecorderOptions.Encoding}' is not supported.");
		}

		return Task.CompletedTask;
	}

	// Stop Recording
	public Task<IAudioSource> StopAsync()
	{
		// stop AudioRecord
		if (audioRecord?.RecordingState == RecordState.Recording)
		{
			audioRecord?.Stop();
		}

		// stop MediaRecorder
		if (mediaRecorderIsRecording)
		{
			mediaRecorderIsRecording = false;
			mediaRecorder?.Stop();
		}
		
		if (audioRecorderOptions.CaptureMode == CaptureMode.Bundling)
		{
			if (audioFilePath is null)
			{
				throw new InvalidOperationException("'audioFilePath' is null, this really should not happen.");
			}

			if (audioRecorderOptions.Encoding == Encoding.Wav)
			{
				UpdateAudioHeaderToFile();
			}
		}

		return Task.FromResult(GetRecording());
	}

	IAudioSource GetRecording()
	{
		if ((audioRecord is null && mediaRecorder is null)
		    || audioRecord?.RecordingState == RecordState.Recording
		    || mediaRecorderIsRecording
		    || System.IO.File.Exists(audioFilePath) == false
		    || audioRecorderOptions.CaptureMode == CaptureMode.Streaming)
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

	// AudioRecord Function to Write Data to File (Wav)
	void WriteAudioDataToFile()
	{
		var data = new byte[bufferSize];

		FileOutputStream? outputStream;

		try
		{
			outputStream = new FileOutputStream(audioFilePath);
		}
		catch (Exception ex)
		{
			throw new FileLoadException($"unable to create a new file: {ex.Message}");
		}

		if (audioRecord is not null)
		{
			var header = WaveFileHelper.GetWaveFileHeader(0, 0, sampleRate, channels, bitDepth);
			outputStream.Write(header, 0, WaveFileHelper.WavHeaderLength);

			while (audioRecord.RecordingState == RecordState.Recording)
			{
				var read = audioRecord.Read(data, 0, bufferSize);
				outputStream.Write(data, 0, read);
			}

			outputStream.Close();
		}
	}

	void WriteAudioDataToEvent()
	{
		if (AudioStreamCaptured == null)
		{
			throw new InvalidOperationException("'AudioCaptured' event is empty while CaptureMode is 'Live'.");
		}

		var data = new byte[bufferSize];
		
		if (audioRecord is not null && AudioStreamCaptured is not null)
		{
			while (audioRecord.RecordingState == RecordState.Recording)
			{
				var read = audioRecord.Read(data, 0, bufferSize);
				var readData = data.Take(read).ToArray();

				AudioStreamCaptured?.Invoke(this, new AudioStreamEventArgs(readData));
			}
		}
	}

	void UpdateAudioHeaderToFile()
	{
		try
		{
			RandomAccessFile randomAccessFile = new(audioFilePath, "rw");
			
			var totalAudioLength = randomAccessFile.Length();
			var totalDataLength = totalAudioLength + 36;

			var header = WaveFileHelper.GetWaveFileHeader(totalAudioLength, totalDataLength, sampleRate, channels, bitDepth);

			randomAccessFile.Seek(0);
			randomAccessFile.Write(header, 0, WaveFileHelper.WavHeaderLength);

			randomAccessFile.Close();
		}
		catch (Exception ex)
		{
			// Trace the exception
			Trace.WriteLine($"An error occurred while updating the wave header: {ex.Message}");
			Trace.WriteLine($"Stack Trace: {ex.StackTrace}");
		}
	}

	static Android.Media.Encoding SharedWavEncodingToAndroidEncoding(Encoding type, BitDepth bitDepth,
		bool throwIfNotSupported)
	{
		return bitDepth switch
		{
			BitDepth.Pcm8bit => type switch
			{
				Encoding.Wav => Android.Media.Encoding.Pcm8bit,
				_ => throwIfNotSupported
					? throw new NotSupportedException("Encoding type not supported")
					: SharedWavEncodingToAndroidEncoding(Encoding.Wav, bitDepth, true)
			},
			BitDepth.Pcm16bit => type switch
			{
				Encoding.Wav => Android.Media.Encoding.Pcm16bit,
				_ => throwIfNotSupported
					? throw new NotSupportedException("Encoding type not supported")
					: SharedWavEncodingToAndroidEncoding(Encoding.Wav, bitDepth, true)
			},

			_ => throwIfNotSupported
				? throw new NotSupportedException("Encoding type not supported")
				: SharedWavEncodingToAndroidEncoding(Encoding.Wav, defaultOptions.BitDepth, true)
		};
	}

	static ChannelIn SharedChannelTypesToAndroidChannelTypes(ChannelType type, bool throwIfNotSupported)
	{
		return type switch
		{
			ChannelType.Mono => ChannelIn.Mono,
			ChannelType.Stereo => ChannelIn.Stereo,
			_ => throwIfNotSupported
				? throw new NotSupportedException("channel type not supported")
				: SharedChannelTypesToAndroidChannelTypes(defaultOptions.Channels, true)
		};
	}
}
