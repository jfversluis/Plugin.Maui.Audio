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
	AVAudioRecorder? recorder; //can handle wav pcm or aac compressed
	TaskCompletionSource<bool>? finishedRecordingCompletionSource;

	readonly AudioRecorderOptions audioRecorderOptions;

	public AudioRecorder(AudioRecorderOptions audioRecorderOptions)
	{
		this.audioRecorderOptions = audioRecorderOptions;

		ActiveSessionHelper.FinishSession(audioRecorderOptions);
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

		ActiveSessionHelper.InitializeSession(audioRecorderOptions);

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

		ActiveSessionHelper.FinishSession(audioRecorderOptions);

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
		finishedRecordingCompletionSource?.SetResult(true);
	}

	static AudioFormatType SharedEncodingToiOSEncoding(Encoding type, bool throwIfNotSupported)
	{
		return type switch
		{
			Encoding.Wav => AudioFormatType.LinearPCM,
			Encoding.ULaw => AudioFormatType.ULaw,
			Encoding.Flac => AudioFormatType.Flac,
			Encoding.Alac => AudioFormatType.AppleLossless,
			Encoding.Aac => AudioFormatType.MPEG4AAC,
			_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedEncodingToiOSEncoding(AudioRecordingOptions.DefaultEncoding, true)
		};
	}
}