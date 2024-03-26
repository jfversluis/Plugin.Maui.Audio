using System.Diagnostics;
using AudioToolbox;
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


	static void InitAudioSession()
	{
		var error = AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Record, options: AVAudioSessionCategoryOptions.DefaultToSpeaker);

		if (error is not null)
		{
			Trace.TraceWarning(error.ToString());
			//throw new FailedToStartRecordingException(error.ToString());
		}

		AVAudioSession.SharedInstance().SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);
		if (error is not null)
		{
			Console.WriteLine(error.ToString());
			//throw new FailedToStartRecordingException(error.ToString());
		}
	}
	static void EndAudioSession()
	{
		var audioSession = AVAudioSession.SharedInstance();
		audioSession.SetActive(false);
	}


	static string GetTempFilePath()
	{
		return Path.GetTempFileName();
	}

	public Task StartAsync(AudioRecordingOptions options) => StartAsync(GetTempFilePath(), options);
	public Task StartAsync() => StartAsync(GetTempFilePath(), DefaultAudioRecordingOptions.DefaultOptions);
	public Task StartAsync(string filePath) => StartAsync(filePath, DefaultAudioRecordingOptions.DefaultOptions);

	public Task StartAsync(string filePath, AudioRecordingOptions options)
	{
		if (IsRecording)
		{
			throw new InvalidOperationException("The recorder is already recording.");
		}

		var url = NSUrl.FromFilename(filePath);
		destinationFilePath = filePath;

		options ??= DefaultAudioRecordingOptions.DefaultOptions;

		NSObject[] objects = new NSObject[]
		{
				NSNumber.FromInt32 (options.SampleRate), //Sample Rate
				NSNumber.FromInt32 ((int)SharedEncodingToiOSEncoding(options.Encoding, options.ThrowIfNotSupported)),
				NSNumber.FromInt32 ((int)options.Channels), //Channels
				NSNumber.FromInt32 ((int)options.BitDepth), //PCMBitDepth
				NSNumber.FromBoolean (false), //IsBigEndianKey
				NSNumber.FromBoolean (false) //IsFloatKey
		};

		var settings = NSDictionary.FromObjectsAndKeys(objects, keys);

		InitAudioSession();
		recorder = AVAudioRecorder.Create(url, new AudioSettings(settings), out NSError? error) ?? throw new FailedToStartRecordingException("could not create native AVAudioRecorder");

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


	void Recorder_FinishedRecording(object? sender, AVStatusEventArgs e)
	{
		EndAudioSession();
		finishedRecordingCompletionSource?.SetResult(true);
	}

	static AudioFormatType SharedEncodingToiOSEncoding(Encoding type, bool throwIfNotSupported)
	{
		return type switch
		{
			Encoding.LinearPCM => AudioFormatType.LinearPCM,
			Encoding.ULaw => AudioFormatType.ULaw,
			_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedEncodingToiOSEncoding(AudioRecordingOptions.DefaultEncoding, true)
		};
	}
}