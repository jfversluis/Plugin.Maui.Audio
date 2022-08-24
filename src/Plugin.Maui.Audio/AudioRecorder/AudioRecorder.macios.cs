using System;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;

namespace Plugin.Maui.Audio;

public class AudioRecorder
{
	public bool CanRecordAudio => AVAudioSession.SharedInstance().InputAvailable;

	public bool IsRecording => recorder.Recording;

	AVAudioRecorder recorder;

	public AudioRecorder()
	{
		InitAudioSession();

		var url = NSUrl.FromFilename(GetTempFileName());

		var settings = NSDictionary.FromObjectsAndKeys(objects, keys);

		recorder = AVAudioRecorder.Create(url, new AudioSettings(settings), out NSError? error) ?? throw new Exception();

		recorder.PrepareToRecord();
	}

	void InitAudioSession()
	{
	    var audioSession = AVAudioSession.SharedInstance();

	    var err = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
	    if (err != null) throw new Exception(err.ToString());

	    err = audioSession.SetActive(true);
	    if (err != null) throw new Exception(err.ToString());
	}

	string GetTempFileName()
	{
		var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		var libFolder = Path.Combine(docFolder, "..", "Library");
		var tempFileName = Path.Combine(libFolder, Path.GetTempFileName());

		return tempFileName;
	}

	public Task RecordAsync()
	{
		return Task.FromResult(recorder.Record());
	}

	public Task<AudioRecording> StopAsync()
	{
		recorder.Stop();

		var recording = new AudioRecording(recorder.Url.Path);

		return Task.FromResult(recording);
	}

	static NSObject[] keys = new NSObject[]
	{
		AVAudioSettings.AVSampleRateKey,
		AVAudioSettings.AVFormatIDKey,
		AVAudioSettings.AVNumberOfChannelsKey,
		AVAudioSettings.AVLinearPCMBitDepthKey,
		AVAudioSettings.AVLinearPCMIsBigEndianKey,
		AVAudioSettings.AVLinearPCMIsFloatKey
	};

	static NSObject[] objects = new NSObject[]
	{
		NSNumber.FromFloat (16000), //Sample Rate
	    NSNumber.FromInt32 ((int)AudioToolbox.AudioFormatType.LinearPCM), //AVFormat
	    NSNumber.FromInt32 (1), //Channels
	    NSNumber.FromInt32 (16), //PCMBitDepth
	    NSNumber.FromBoolean (false), //IsBigEndianKey
	    NSNumber.FromBoolean (false) //IsFloatKey
	};
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