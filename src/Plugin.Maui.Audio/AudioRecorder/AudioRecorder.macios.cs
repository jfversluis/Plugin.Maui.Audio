using System;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public bool CanRecordAudio => AVAudioSession.SharedInstance().InputAvailable;
	public bool IsRecording => recorder?.Recording ?? false;

	string? destinationFilePath;
	AVAudioRecorder? recorder;
	TaskCompletionSource<bool>? finishedRecordingCompletionSource;

	public AudioRecorder()
	{
		InitAudioSession();
	}

	static void InitAudioSession()
	{
		var audioSession = AVAudioSession.SharedInstance();

		var error = audioSession.SetCategory(AVAudioSessionCategory.Record);
		if (error is not null)
		{
			throw new Exception(error.ToString());
		}

		error = audioSession.SetActive(true);
		if (error is not null)
		{
			throw new Exception(error.ToString());
		}
	}

	static string GetTempFilePath()
	{
		return Path.GetTempFileName();
	}

	public Task StartAsync() => StartAsync(AudioRecorder.GetTempFilePath());

	public Task StartAsync(string filePath)
	{
		if (IsRecording)
		{
			throw new InvalidOperationException("The recorder is already recording.");
		}

		var url = NSUrl.FromFilename(filePath);
		destinationFilePath = filePath;

		var settings = NSDictionary.FromObjectsAndKeys(objects, keys);

		recorder = AVAudioRecorder.Create(url, new AudioSettings(settings), out NSError? error) ?? throw new Exception();

		recorder.FinishedRecording += Recorder_FinishedRecording;
		finishedRecordingCompletionSource = new TaskCompletionSource<bool>();
		recorder.PrepareToRecord();

		return Task.FromResult(recorder.Record());
	}

	public async Task<IAudioSource> StopAsync()
	{
		if (recorder is null ||
			destinationFilePath is null ||
			finishedRecordingCompletionSource is null)
		{
			throw new InvalidOperationException("The recorder is not recording, call StartAsync first.");
		}

		recorder.Stop();

		await finishedRecordingCompletionSource.Task;

		recorder.FinishedRecording -= Recorder_FinishedRecording;

		return new FileAudioSource(destinationFilePath);
	}

	static readonly NSObject[] keys = new NSObject[]
	{
		AVAudioSettings.AVSampleRateKey,
		AVAudioSettings.AVFormatIDKey,
		AVAudioSettings.AVNumberOfChannelsKey,
		AVAudioSettings.AVLinearPCMBitDepthKey,
		AVAudioSettings.AVLinearPCMIsBigEndianKey,
		AVAudioSettings.AVLinearPCMIsFloatKey
	};

	static readonly NSObject[] objects = new NSObject[]
	{
		NSNumber.FromFloat (16000), //Sample Rate
        NSNumber.FromInt32 ((int)AudioToolbox.AudioFormatType.LinearPCM), //AVFormat
        NSNumber.FromInt32 (1), //Channels
        NSNumber.FromInt32 (16), //PCMBitDepth
        NSNumber.FromBoolean (false), //IsBigEndianKey
        NSNumber.FromBoolean (false) //IsFloatKey
	};

	void Recorder_FinishedRecording(object? sender, AVStatusEventArgs e)
	{
		finishedRecordingCompletionSource?.SetResult(true);
	}
}