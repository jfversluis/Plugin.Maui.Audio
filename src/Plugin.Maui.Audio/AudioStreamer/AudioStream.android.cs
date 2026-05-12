using System.Diagnostics;
using Android.Media;

namespace Plugin.Maui.Audio;

partial class AudioStream : IDisposable
{
	AudioRecord? audioRecord;
	Android.Media.AudioDeviceInfo? PreferredDevice { get; }

	// Bluetooth SCO state
	bool bluetoothScoStarted;
	Android.Media.AudioManager? androidAudioManager;
	Android.Media.Mode previousAudioMode;

	public event EventHandler<byte[]>? OnBroadcast;
	public event EventHandler<bool>? OnActiveChanged;
	public event EventHandler<Exception>? OnException;

	public bool Active => audioRecord?.RecordingState == RecordState.Recording;

	public Task Start()
	{
		var channelIn = Channels switch
		{
			ChannelType.Stereo => ChannelIn.Stereo,
			_ => ChannelIn.Mono
		};

		var encoding = BitDepth switch
		{
			BitDepth.Pcm8bit => Android.Media.Encoding.Pcm8bit,
			_ => Android.Media.Encoding.Pcm16bit
		};

		try
		{
			var bufferSize = AudioRecord.GetMinBufferSize(SampleRate, channelIn, encoding);

			// If the bufferSize is less than or equal to 0, then this device does not support the provided options
			if (bufferSize <= 0)
			{
				throw new FailedToStartRecordingException("Unable to get bufferSize with provided options.");
			}

			// Determine if the preferred device is Bluetooth and requires SCO
			bool isBluetooth = IsBluetoothDevice(PreferredDevice);
			AudioSource audioSource = isBluetooth ? AudioSource.VoiceCommunication : AudioSource.Mic;

			if (isBluetooth)
			{
				StartBluetoothSco();
			}

			audioRecord = new AudioRecord(audioSource, SampleRate, channelIn, encoding, bufferSize);

			if (OperatingSystem.IsAndroidVersionAtLeast(23) && PreferredDevice is not null)
			{
				if (!audioRecord.SetPreferredDevice(PreferredDevice))
				{
					Trace.WriteLine("AudioStream: failed to set preferred device, using default");
				}
			}

			audioRecord.StartRecording();

			Task.Run(() => WriteAudioDataToEvent(bufferSize));
			OnActiveChanged?.Invoke(this, true);
		}
		catch (Exception ex)
		{
			Trace.WriteLine("Error in AudioStream.Start(): {0}", ex.Message);

			Stop();
			throw;
		}

		return Task.CompletedTask;
	}

	public Task Stop()
	{
		if (Active)
		{
			audioRecord?.Stop();
			OnActiveChanged?.Invoke(this, false);

			audioRecord?.Dispose();
			audioRecord = null;
		}

		StopBluetoothSco();

		return Task.CompletedTask;
	}

	void WriteAudioDataToEvent(int bufferSize)
	{
		var data = new byte[bufferSize];

		try
		{
			if (audioRecord is null)
			{
				throw new NullReferenceException("AudioRecord has not been set");
			}

			while (audioRecord.RecordingState == RecordState.Recording)
			{
				var read = audioRecord.Read(data, 0, bufferSize);
				var readData = data.Take(read).ToArray();

				OnBroadcast?.Invoke(this, readData);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"AudioStream.WriteAudioDataToEvent() :: Error: {ex.Message}");
			OnException?.Invoke(this, new Exception($"AudioStream.WriteAudioDataToEvent() :: Error: {ex.Message}"));
		}
	}
	
	public void Dispose()
	{
		StopBluetoothSco();
		audioRecord?.Dispose();
	}

	static bool IsBluetoothDevice(Android.Media.AudioDeviceInfo? device)
	{
		if (device is null || !OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			return false;
		}

		return device.Type == AudioDeviceType.BluetoothSco
			|| (OperatingSystem.IsAndroidVersionAtLeast(31)
				&& (device.Type == AudioDeviceType.BleHeadset
					|| device.Type == AudioDeviceType.BleSpeaker));
	}

	void StartBluetoothSco()
	{
		androidAudioManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService) as Android.Media.AudioManager;
		if (androidAudioManager is null)
		{
			return;
		}

		previousAudioMode = androidAudioManager.Mode;
		androidAudioManager.Mode = Android.Media.Mode.InCommunication;
		androidAudioManager.StartBluetoothSco();
		androidAudioManager.BluetoothScoOn = true;
		bluetoothScoStarted = true;

		Trace.WriteLine("AudioStream: Bluetooth SCO started for BT device recording");
	}

	void StopBluetoothSco()
	{
		if (!bluetoothScoStarted || androidAudioManager is null)
		{
			return;
		}

		androidAudioManager.StopBluetoothSco();
		androidAudioManager.BluetoothScoOn = false;
		androidAudioManager.Mode = previousAudioMode;
		bluetoothScoStarted = false;

		Trace.WriteLine("AudioStream: Bluetooth SCO stopped");
	}
}