﻿using System.Diagnostics;
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

	string? rawFilePath;
	string? audioFilePath;

	//Audio recording options
	readonly AudioRecorderOptions constructorOptions; // constructor arguments, not used in any code here
	AudioRecordingOptions recordingOptions { get; set; } // startAsync arguments (bitrate, etc)

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
		this.constructorOptions = options;

	}

	// Start Recording Wrapper Functions
	public Task StartAsync(AudioRecordingOptions options) => StartAsync(GetTempFilePath(), options);
	public Task StartAsync() => StartAsync(GetTempFilePath(), DefaultAudioRecordingOptions.DefaultOptions);
	public Task StartAsync(string filePath) => StartAsync(filePath, DefaultAudioRecordingOptions.DefaultOptions);

	// Start Recording Main Function
	public Task StartAsync(string filePath, AudioRecordingOptions recordingOptions) {

		// Check if can record or already recording
		if (CanRecordAudio == false 
			|| audioRecord?.RecordingState == RecordState.Recording  // AudioRecord is recording
			|| mediaRecorderIsRecording) { // MediaRecorder is recording

			return Task.CompletedTask;
		}

		recordingOptions ??= DefaultAudioRecordingOptions.DefaultOptions;
		this.recordingOptions = recordingOptions; // save reference for future checks

		audioFilePath = filePath;

		// solve some parameters needed for AudioRecord/MediaRecorder
		ChannelIn channelIn = SharedChannelTypesToAndroidChannelTypes(recordingOptions.Channels, recordingOptions.ThrowIfNotSupported);
		this.sampleRate = recordingOptions.SampleRate;
		this.bitDepth = (int)recordingOptions.BitDepth;
		this.channels = channelIn == ChannelIn.Mono ? 1 : 2;
		int bitRate = recordingOptions.BitRate;
		int numChannels = 1;
		if (recordingOptions.Channels == ChannelType.Stereo) { numChannels = 2; }

		// Wav: AudioRecord Method
		if (recordingOptions.Encoding == Encoding.Wav) {

			// Get encoding
			Android.Media.Encoding encoding = SharedWavEncodingToAndroidEncoding(recordingOptions.Encoding, recordingOptions.BitDepth, recordingOptions.ThrowIfNotSupported);

			// Check buffer size 
			int bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelIn, encoding);

			// If the bufferSize is less than or equal to 0, then this device does not support the provided options
			if (bufferSize <= 0) {
				if (recordingOptions.ThrowIfNotSupported) {
					throw new FailedToStartRecordingException("Unable to get bufferSize with provided reording options.");
				}
				else {
					sampleRate = AudioRecordingOptions.DefaultSampleRate;
					bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelIn, encoding);

					if (bufferSize <= 0) {
						var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as Android.Media.AudioManager;
						var rate = (audioManager?.GetProperty(Android.Media.AudioManager.PropertyOutputSampleRate)) ?? throw new FailedToStartRecordingException("Unable to get the sample rate.");
						sampleRate = int.Parse(rate);
					}
				}
			}
			this.bufferSize = bufferSize; //save for later AudioRecord calculations

			// Start Recording
			audioRecord = new AudioRecord(AudioSource.Mic, sampleRate, channelIn, encoding, bufferSize);
			audioRecord.StartRecording();
			Task.Run(WriteAudioDataToFile);
			return Task.CompletedTask;
		}

		// Aac: MediaRecorder Method
		else {
			
			// Solve encoding
			AudioEncoder audioEncoder = AudioEncoder.Default;
			OutputFormat outputFormat = OutputFormat.Default;

			// Parse the RecordingOptions into AudioEncoder & OutputFormat
			if (recordingOptions.Encoding == Encoding.Aac) {
				audioEncoder = AudioEncoder.Aac;
				outputFormat = OutputFormat.Mpeg4; //creates mp4 aac file (functionally identical to an M4A) 
			}
			
			// Create MediaRecorder
			mediaRecorder = new MediaRecorder(Platform.CurrentActivity.ApplicationContext); //needs context, obsoleted without context//https://stackoverflow.com/questions/73598179/deprecated-mediarecorder-new-mediarecorder#73598440
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

			return Task.CompletedTask;

		}

	}

	// Stop Recording
	public Task<IAudioSource> StopAsync() {
		// stop AudioRecord
		if (audioRecord?.RecordingState == RecordState.Recording) {
			audioRecord?.Stop();
		}

		// stop MediaRecorder
		if (mediaRecorderIsRecording) {
			mediaRecorderIsRecording = false;
			mediaRecorder?.Stop();
		}

		if (audioFilePath is null) {
			throw new InvalidOperationException("'audioFilePath' is null, this really should not happen.");
		}

		// Check saved RecordingOptions for which recorder method to use
		if (this.recordingOptions?.Encoding == Encoding.Wav) {
			CopyWaveFile(rawFilePath, audioFilePath);
		}
		else {
			CopyAudioFile(rawFilePath, audioFilePath);
		}

		try {
			// lets delete the temp file with the raw data, after we have created the WAVE file
			if (System.IO.File.Exists(rawFilePath)) {
				System.IO.File.Delete(rawFilePath);
			}
		}
		catch {
			Trace.TraceWarning("delete raw wav file failed.");
		}

		return Task.FromResult(GetRecording());
	}

	IAudioSource GetRecording() {
		if ((audioRecord is null && mediaRecorder is null)
			|| audioRecord?.RecordingState == RecordState.Recording
			|| mediaRecorderIsRecording
			|| System.IO.File.Exists(audioFilePath) == false)
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
				var read = audioRecord.Read(data, 0, bufferSize);
				outputStream.Write(data, 0, read);
			}

			outputStream.Close();
		}
	}

	// Copy Function for MediaRecorder Files
	void CopyAudioFile(string? sourcePath, string destinationPath) {

		var data = new byte[8192]; // arbitrary size for copy processing, 4096 or 8192 perhaps

		try {
			FileInputStream inputStream = new(sourcePath);
			FileOutputStream outputStream = new(destinationPath);

			if (inputStream?.Channel is not null) {
				var totalAudioLength = inputStream.Channel.Size();
				var totalDataLength = totalAudioLength;

				while (inputStream.Read(data) != -1) {
					outputStream.Write(data);
				}

				inputStream.Close();
				outputStream.Close();
			}
		}
		catch (Exception ex) {
			// Trace the exception
			Trace.WriteLine($"An error occurred while copying the wave file: {ex.Message}");
			Trace.WriteLine($"Stack Trace: {ex.StackTrace}");
		}
	}
	
	// Copy Function for AudioRecord Files (Wav)
	void CopyWaveFile(string? sourcePath, string destinationPath)
	{
		long byteRate = sampleRate * bitDepth * channels / 8;

		var data = new byte[bufferSize];

		try
		{
			FileInputStream inputStream = new(sourcePath);
			FileOutputStream outputStream = new(destinationPath);

			if (inputStream?.Channel is not null)
			{
				var totalAudioLength = inputStream.Channel.Size();
				var totalDataLength = totalAudioLength + 36;

				WriteWaveFileHeader(outputStream, totalAudioLength, totalDataLength, sampleRate, channels, byteRate);

				while (inputStream.Read(data) != -1)
				{
					outputStream.Write(data);
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

	static Android.Media.Encoding SharedWavEncodingToAndroidEncoding(Encoding type, BitDepth bitDepth, bool throwIfNotSupported)
	{

		return bitDepth switch
		{
			BitDepth.Pcm8bit => type switch
			{
				Encoding.Wav => Android.Media.Encoding.Pcm8bit,
				_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedWavEncodingToAndroidEncoding(Encoding.Wav, bitDepth, true)
			},
			BitDepth.Pcm16bit => type switch
			{
				Encoding.Wav => Android.Media.Encoding.Pcm16bit,
				_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedWavEncodingToAndroidEncoding(Encoding.Wav, bitDepth, true)
			},

			_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedWavEncodingToAndroidEncoding(Encoding.Wav, AudioRecordingOptions.DefaultBitDepth, true)
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
}
