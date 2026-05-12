using System.Diagnostics;
using static Microsoft.Maui.ApplicationModel.Permissions;
#if ANDROID
using Android.Media;
#endif
#if IOS || MACCATALYST
using AVFoundation;
#endif
#if WINDOWS
using Windows.Devices.Enumeration;
using Windows.Media.Devices;
#endif

namespace Plugin.Maui.Audio.Sample.ViewModels;

public class AudioRecorderPageViewModel : BaseViewModel
{
	readonly IAudioManager audioManager;
	readonly IDispatcher dispatcher;
	IAudioRecorder audioRecorder;
	AsyncAudioPlayer audioPlayer;
	IAudioSource audioSource = null;
	readonly Stopwatch recordingStopwatch = new Stopwatch();
	bool isPlaying;

#if ANDROID
	AudioDeviceInfo[] availableInputDevices = [];
#endif
#if IOS || MACCATALYST
	AVAudioSessionPortDescription[] availableInputPorts = [];
#endif
#if WINDOWS
	DeviceInformation[] availableWindowsInputDevices = [];
#endif

	public double RecordingTime
	{
		get => recordingStopwatch.ElapsedMilliseconds / 1000;
	}

	public bool IsPlaying
	{
		get => isPlaying;
		set
		{
			isPlaying = value;
			PlayCommand.ChangeCanExecute();
			StopPlayCommand.ChangeCanExecute();
		}
	}

	public bool IsRecording
	{
		get => audioRecorder?.IsRecording ?? false;
	}

	public Command PlayCommand { get; }
	public Command StartCommand { get; }
	public Command StopCommand { get; }
	public Command StopPlayCommand { get; }

	public AudioRecorderPageViewModel(
		IAudioManager audioManager,
		IDispatcher dispatcher)
	{
		StartCommand = new Command(Start, () => !IsRecording);
		StopCommand = new Command(Stop, () => IsRecording);
		PlayCommand = new Command(PlayAudio, () => !IsPlaying);
		StopPlayCommand = new Command(StopPlay, () => IsPlaying);

		this.audioManager = audioManager;
		this.dispatcher = dispatcher;

		LoadInputDevices();
		_ = LoadWindowsInputDevicesAsync();
	}

	ChannelType selectedChannelType;
	public ChannelType SelectedChannelType
	{
		get => selectedChannelType;
		set
		{
			selectedChannelType = value;
			NotifyPropertyChanged();
		}
	}
	public List<ChannelType> ChannelTypes { get; set; } = Enum.GetValues(typeof(ChannelType)).Cast<ChannelType>().ToList();

	BitDepth selectedBitDepth;
	public BitDepth SelectedBitDepth
	{
		get => selectedBitDepth;
		set
		{
			selectedBitDepth = value;
			NotifyPropertyChanged();
		}
	}
	public List<BitDepth> BitDepths { get; set; } = Enum.GetValues(typeof(BitDepth)).Cast<BitDepth>().ToList();

	Encoding selectedEncoding;
	public Encoding SelectedEncoding
	{
		get => selectedEncoding;
		set
		{
			selectedEncoding = value;
			NotifyPropertyChanged();
		}
	}

	public List<Encoding> EncodingOptions { get; set; } = Enum.GetValues(typeof(Encoding)).Cast<Encoding>().ToList();

	int selectedSampleRate = -1;
	public int SelectedSampleRate
	{
		get => selectedSampleRate;
		set
		{
			selectedSampleRate = value;
			NotifyPropertyChanged();
		}
	}
	
	public List<int> SampleRates { get; set; } =
	[
		8000,
		16000,
		44100,
		48000
	];

	List<string> inputDevices = ["Default"];
	public List<string> InputDevices
	{
		get => inputDevices;
		set
		{
			inputDevices = value;
			NotifyPropertyChanged();
		}
	}

	string selectedInputDevice = "Default";
	public string SelectedInputDevice
	{
		get => selectedInputDevice;
		set
		{
			selectedInputDevice = value;
			NotifyPropertyChanged();
		}
	}

	bool allowBluetooth;
	public bool AllowBluetooth
	{
		get => allowBluetooth;
		set
		{
			allowBluetooth = value;
			NotifyPropertyChanged();
			LoadInputDevices();
		}
	}

	void LoadInputDevices()
	{
#if ANDROID
		if (!OperatingSystem.IsAndroidVersionAtLeast(23))
		{
			InputDevices = ["Default"];
			SelectedInputDevice = "Default";
			return;
		}

		var androidAudioManager = (Android.Media.AudioManager?)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);
		if (androidAudioManager is null)
		{
			return;
		}

		availableInputDevices = androidAudioManager.GetDevices(GetDevicesTargets.Inputs) ?? [];

		var devices = new List<string> { "Default" };
		foreach (var device in availableInputDevices)
		{
			devices.Add($"{device.ProductName} ({device.Type})");
		}

		InputDevices = devices;
		SelectedInputDevice = "Default";
#endif
#if IOS || MACCATALYST
		var session = AVAudioSession.SharedInstance();

		// On iOS, AllowBluetooth is required for BT devices to appear in AvailableInputs.
		// On macCatalyst, devices appear regardless but setting AllowBluetooth is harmless.
		var categoryOptions = AllowBluetooth
			? AVAudioSessionCategoryOptions.AllowBluetooth
			: AVAudioSessionCategoryOptions.DefaultToSpeaker;

		session.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionMode.Default, categoryOptions, out _);
		session.SetActive(true, out _);

		availableInputPorts = session.AvailableInputs ?? [];

		var devices = new List<string> { "Default" };
		foreach (var port in availableInputPorts)
		{
			devices.Add(port.PortName);
		}

		InputDevices = devices;
		SelectedInputDevice = "Default";
#endif
	}

	async Task LoadWindowsInputDevicesAsync()
	{
#if WINDOWS
		var deviceInfoCollection = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioCaptureSelector());
		availableWindowsInputDevices = deviceInfoCollection.ToArray();

		var devices = new List<string> { "Default" };
		foreach (var device in availableWindowsInputDevices)
		{
			devices.Add(device.Name);
		}

		InputDevices = devices;
		SelectedInputDevice = "Default";
#else
		await Task.CompletedTask;
#endif
	}


	async void PlayAudio()
	{
		if (audioSource != null)
		{
			audioPlayer = this.audioManager.CreateAsyncPlayer(((FileAudioSource)audioSource).GetAudioStream());

			IsPlaying = true;

			await audioPlayer.PlayAsync(CancellationToken.None);

			IsPlaying = false;
		}
	}

	void StopPlay()
	{
		audioPlayer.Stop();
	}

	async void Start()
	{
		if (await CheckPermissionIsGrantedAsync<Microphone>())
		{
			audioRecorder = audioManager.CreateRecorder();

			var options = new AudioRecorderOptions
			{
				Channels = SelectedChannelType,
				BitDepth = SelectedBitDepth,
				Encoding = SelectedEncoding,
				ThrowIfNotSupported = true
			};

			if (SelectedSampleRate != -1)
			{
				options.SampleRate = SelectedSampleRate;
			}

#if ANDROID
			if (SelectedInputDevice != "Default")
			{
				var index = InputDevices.IndexOf(SelectedInputDevice) - 1;
				if (index >= 0 && index < availableInputDevices.Length)
				{
					options.PreferredDevice = availableInputDevices[index];
				}
			}
#endif
#if IOS || MACCATALYST
			if (AllowBluetooth)
			{
				options.CategoryOptions = AVAudioSessionCategoryOptions.AllowBluetooth;
			}

			if (SelectedInputDevice != "Default")
			{
				var selectedPort = availableInputPorts.FirstOrDefault(p => p.PortName == SelectedInputDevice);
				if (selectedPort is not null)
				{
					options.PreferredInput = selectedPort;
				}
			}
#endif
#if WINDOWS
			if (SelectedInputDevice != "Default")
			{
				var index = InputDevices.IndexOf(SelectedInputDevice) - 1;
				if (index >= 0 && index < availableWindowsInputDevices.Length)
				{
					options.AudioDeviceId = availableWindowsInputDevices[index].Id;
				}
			}
#endif

			try
			{
				await audioRecorder.StartAsync(options);
			}
			catch
			{
				var res = await AppShell.Current.DisplayActionSheet("Options not supported. Use Default?", "Yes", "No");
				if (res != "Yes")
				{
					return;
				}
				await audioRecorder.StartAsync();
			}
		}

		recordingStopwatch.Restart();
		UpdateRecordingTime();
		NotifyPropertyChanged(nameof(IsRecording));
		StartCommand.ChangeCanExecute();
		StopCommand.ChangeCanExecute();
	}

	async void Stop()
	{
		audioSource = await audioRecorder.StopAsync();

		recordingStopwatch.Stop();
		NotifyPropertyChanged(nameof(IsRecording));
		StartCommand.ChangeCanExecute();
		StopCommand.ChangeCanExecute();
	}

	void UpdateRecordingTime()
	{
		if (IsRecording is false)
		{
			return;
		}

		dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(16),
			() =>
			{
				NotifyPropertyChanged(nameof(RecordingTime));

				UpdateRecordingTime();
			});
	}

	internal void OnNavigatedFrom()
	{
		audioPlayer?.Dispose();
	}
}
