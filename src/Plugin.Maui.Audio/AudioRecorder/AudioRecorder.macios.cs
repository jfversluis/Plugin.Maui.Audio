using System;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public bool CanRecordAudio => AVAudioSession.SharedInstance().InputAvailable;

	public bool IsRecording => recorder.Recording;

	readonly string destinationFilePath;
	readonly AVAudioRecorder recorder;
	readonly TaskCompletionSource<bool> finishedRecordingCompletionSource;

	public AudioRecorder()
	{
		InitAudioSession();

		destinationFilePath = GetTempFileName();
		var url = NSUrl.FromFilename(destinationFilePath);

		var settings = NSDictionary.FromObjectsAndKeys(objects, keys);

		recorder = AVAudioRecorder.Create(url, new AudioSettings(settings), out NSError? error) ?? throw new Exception();

		// TODO: need to tidy this up.
		recorder.FinishedRecording += Recorder_FinishedRecording;
		finishedRecordingCompletionSource = new TaskCompletionSource<bool>();
		recorder.PrepareToRecord();
	}

	static void InitAudioSession()
	{
	    var audioSession = AVAudioSession.SharedInstance();

	    var err = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
		if (err is not null)
		{
			throw new Exception(err.ToString());
		}

	    err = audioSession.SetActive(true);
		if (err is not null)
		{
			throw new Exception(err.ToString());
		}
	}

	string GetTempFileName()
	{
		// TODO: Better MAUI options?
		var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		var libFolder = Path.Combine(docFolder, "..", "Library");
		var tempFileName = Path.Combine(libFolder, Path.GetTempFileName());

		return tempFileName;
	}

	public Task StartAsync()
	{
		return Task.FromResult(recorder.Record());
	}

	public async Task<IAudioSource> StopAsync()
	{
		recorder.Stop();

		await finishedRecordingCompletionSource.Task;

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
		finishedRecordingCompletionSource.SetResult(true);
	}
}




//namespace Plugin.SimpleAudioRecorder
//{
//	public class SimpleAudioRecorderImplementation : ISimpleAudioRecorder
//	{
//#if __IOS__
//        public bool CanRecordAudio => AVAudioSession.SharedInstance().InputAvailable;
//#else
//		public bool CanRecordAudio => true;
//#endif

//		public bool IsRecording => recorder.Recording;

//		AVAudioRecorder recorder;

//		public SimpleAudioRecorderImplementation()
//		{
//			InitAudioSession();
//			InitAudioRecorder();
//		}

//		void InitAudioSession()
//		{
//#if __IOS__
//            var audioSession = AVAudioSession.SharedInstance();

//            var err = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
//            if (err != null) throw new Exception(err.ToString());

//            err = audioSession.SetActive(true);
//            if (err != null) throw new Exception(err.ToString());
//#endif
//		}

//		void InitAudioRecorder()
//		{
//			var url = NSUrl.FromFilename(GetTempFileName());

//			var settings = NSDictionary.FromObjectsAndKeys(objects, keys);

//			recorder = AVAudioRecorder.Create(url, new AudioSettings(settings), out NSError error);

//			recorder.PrepareToRecord();
//		}

//		string GetTempFileName()
//		{
//			var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
//			var libFolder = Path.Combine(docFolder, "..", "Library");
//			var tempFileName = Path.Combine(libFolder, Path.GetTempFileName());

//			return tempFileName;
//		}

//		public Task RecordAsync()
//		{
//			return Task.FromResult(recorder.Record());
//		}

//		public Task<AudioRecording> StopAsync()
//		{
//			recorder.Stop();

//			var recording = new AudioRecording(recorder.Url.Path);

//			return Task.FromResult(recording);
//		}

//		static NSObject[] keys = new NSObject[]
//		{
//			AVAudioSettings.AVSampleRateKey,
//			AVAudioSettings.AVFormatIDKey,
//			AVAudioSettings.AVNumberOfChannelsKey,
//			AVAudioSettings.AVLinearPCMBitDepthKey,
//			AVAudioSettings.AVLinearPCMIsBigEndianKey,
//			AVAudioSettings.AVLinearPCMIsFloatKey
//		};

//		static NSObject[] objects = new NSObject[]
//		{
//			NSNumber.FromFloat (16000), //Sample Rate
//            NSNumber.FromInt32 ((int)AudioToolbox.AudioFormatType.LinearPCM), //AVFormat
//            NSNumber.FromInt32 (1), //Channels
//            NSNumber.FromInt32 (16), //PCMBitDepth
//            NSNumber.FromBoolean (false), //IsBigEndianKey
//            NSNumber.FromBoolean (false) //IsFloatKey
//        };
//	}
//}