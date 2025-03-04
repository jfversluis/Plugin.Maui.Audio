using AudioToolbox;
using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public bool CanRecordAudio => AVAudioSession.SharedInstance().InputAvailable;
	public bool IsRecording => recorder is { Recording: true }
	                           || audioStream is { Active: true };

	string? destinationFilePath;
	AVAudioRecorder? recorder;
	TaskCompletionSource<bool>? finishedRecordingCompletionSource;
	AudioStream? audioStream;

	AudioRecorderOptions audioRecorderOptions;

	public event EventHandler<AudioStreamEventArgs>? AudioStreamCaptured;

	public AudioRecorder(AudioRecorderOptions audioRecorderOptions)
	{
		this.audioRecorderOptions = audioRecorderOptions;
		ActiveSessionHelper.FinishSession(audioRecorderOptions);
	}
	
	static string GetTempFilePath()
	{
		return Path.GetTempFileName();
	}

	public Task StartAsync(AudioRecorderOptions? options = null) => StartAsync(GetTempFilePath(), options);

	public async Task StartAsync(string filePath, AudioRecorderOptions? options = null)
	{
		if (IsRecording)
		{
			throw new InvalidOperationException("The recorder is already recording.");
		}

		if (options is not null)
		{
			audioRecorderOptions = options;
		}

		ActiveSessionHelper.InitializeSession(audioRecorderOptions);

		finishedRecordingCompletionSource = new TaskCompletionSource<bool>();
		recorder = null;


		if (options.CaptureMode == CaptureMode.Bundling)
		{
			var url = NSUrl.FromFilename(filePath);
			destinationFilePath = filePath;

			NSObject[] objects = new NSObject[]
			{
				NSNumber.FromInt32 (audioRecorderOptions.SampleRate), //Sample Rate
				NSNumber.FromInt32 ((int)SharedEncodingToiOSEncoding(audioRecorderOptions.Encoding, audioRecorderOptions.ThrowIfNotSupported)),
				NSNumber.FromInt32 ((int)audioRecorderOptions.Channels), //Channels
				NSNumber.FromInt32 ((int)audioRecorderOptions.BitDepth), //PCMBitDepth
				NSNumber.FromBoolean (false), //IsBigEndianKey
				NSNumber.FromBoolean (false) //IsFloatKey
			};

			var settings = NSDictionary.FromObjectsAndKeys(objects, keys);

			recorder = AVAudioRecorder.Create(url, new AudioSettings(settings), out NSError? error) 
			           ?? throw new FailedToStartRecordingException("could not create native AVAudioRecorder");

			recorder.FinishedRecording += Recorder_FinishedRecording;

			recorder.PrepareToRecord();

			recorder.Record();
		}
		else
		{
			if (options.Encoding != Encoding.Wav)
			{
				throw new NotSupportedException(
					$"Encoding '{options.Encoding}' is not supported with '{options.CaptureMode}' mode");
			}

			if (audioStream == null)
			{
				audioStream = new AudioStream(
					options.SampleRate,
					(int)options.Channels,
					(int)options.BitDepth);

				audioStream.OnBroadcast += (sender, bytes) =>
				{
					AudioStreamCaptured?.Invoke(this, new AudioStreamEventArgs(bytes));
				};
			}

			if (!audioStream.Active)
			{
				await audioStream.Start();
			}

			finishedRecordingCompletionSource?.SetResult(true);
		}
	}

	public async Task<IAudioSource> StopAsync()
	{
		if (finishedRecordingCompletionSource is null)
		{
			throw new InvalidOperationException("The recorder is not recording, call StartAsync first.");
		}

		if (audioRecorderOptions.CaptureMode == CaptureMode.Bundling
		    && destinationFilePath is null)
		{
			throw new InvalidOperationException("The recorder has not started, call StartAsync first.");
		}

		if (audioStream != null)
		{
			await audioStream.Stop();
		}

		recorder?.Stop();

		await finishedRecordingCompletionSource.Task;

		if (recorder != null)
		{
			recorder.FinishedRecording -= Recorder_FinishedRecording;
		}

		ActiveSessionHelper.FinishSession(audioRecorderOptions);

		IAudioSource audioSource = (audioRecorderOptions.CaptureMode == CaptureMode.Bundling)
			? new FileAudioSource(destinationFilePath)
			: new EmptyAudioSource();

		return audioSource;
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

	AudioFormatType SharedEncodingToiOSEncoding(Encoding type, bool throwIfNotSupported)
	{
		return type switch
		{
			Encoding.Wav => AudioFormatType.LinearPCM,
			Encoding.ULaw => AudioFormatType.ULaw,
			Encoding.Flac => AudioFormatType.Flac,
			Encoding.Alac => AudioFormatType.AppleLossless,
			Encoding.Aac => AudioFormatType.MPEG4AAC,
			_ => throwIfNotSupported ? throw new NotSupportedException("Encoding type not supported") : SharedEncodingToiOSEncoding(audioRecorderOptions.Encoding, true)
		};
	}
}
