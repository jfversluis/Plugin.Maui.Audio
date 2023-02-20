using System;
using Android.App;
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

	public AudioRecorder()
	{
		var packageManager = Android.App.Application.Context.PackageManager;
		// TODO: exception?
		CanRecordAudio = packageManager?.HasSystemFeature(Android.Content.PM.PackageManager.FeatureMicrophone) ?? false;
	}

	public Task StartAsync() => StartAsync(GetTempFilePath());

	public Task StartAsync(string filePath)
	{
		if (CanRecordAudio == false || audioRecord?.RecordingState == RecordState.Recording)
		{
			return Task.CompletedTask;
		}

		audioFilePath = filePath;

		var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as Android.Media.AudioManager;

		var rate = audioManager?.GetProperty(Android.Media.AudioManager.PropertyOutputSampleRate);
		if (rate != null)
		{
			var micSampleRate = Int32.Parse(rate);

			audioRecord = GetAudioRecord(micSampleRate);

			audioRecord.StartRecording();
			return Task.Run(() => WriteAudioDataToFile());
		}
		return Task.CompletedTask;
	}

	AudioRecord GetAudioRecord(int sampleRate)
	{
		this.sampleRate = sampleRate;
		var channelConfig = ChannelIn.Mono;
		var encoding = Encoding.Pcm16bit;

		bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channelConfig, encoding) * 8;

		return new AudioRecord(AudioSource.Mic, sampleRate, ChannelIn.Stereo, encoding, bufferSize);
	}

	public Task<IAudioSource> StopAsync()
	{
		if (audioRecord?.RecordingState == RecordState.Recording)
		{
			audioRecord?.Stop();
		}

		CopyWaveFile(rawFilePath, audioFilePath);

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

	string GetTempFilePath()
	{
		return Path.Combine("/sdcard/", Path.GetTempFileName());
	}

	void WriteAudioDataToFile()
	{
		var data = new byte[bufferSize];

		rawFilePath = GetTempFilePath();

		FileOutputStream? outputStream = null;

		try
		{
			outputStream = new FileOutputStream(rawFilePath);
		}
		catch (Exception ex)
		{
			throw new FileLoadException($"unable to create a new file: {ex.Message}");
		}

		if ((audioRecord != null)
			&& (outputStream != null))
		{
			while (audioRecord.RecordingState == RecordState.Recording)
			{
				audioRecord.Read(data, 0, bufferSize);

				outputStream.Write(data);
			}

			outputStream.Close();
		}
	}

	void CopyWaveFile(string? sourcePath, string destinationPath)
	{
		int channels = 2;
		long byteRate = 16 * sampleRate * channels / 8;

		var data = new byte[bufferSize];

		try
		{
			FileInputStream inputStream = new FileInputStream(sourcePath);
			FileOutputStream outputStream = new FileOutputStream(destinationPath);
			if ((inputStream != null)
				&& (inputStream.Channel != null))
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
		catch { }
	}

	void WriteWaveFileHeader(FileOutputStream outputStream, long audioLength, long dataLength, long sampleRate, int channels, long byteRate)
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
}
