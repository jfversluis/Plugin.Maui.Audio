# Record audio

The `AudioRecorder` class provides you with the ability to record audio from a microphone in your .NET MAUI application to a file on disk. In order to create an `AudioRecorder` instance you can make use of the `CreateRecorder` method on the [`AudioManager`](../readme.md#audiomanager) class.

```csharp
public class AudioRecorderViewModel
{
    readonly IAudioManager audioManager;
    readonly IAudioRecorder audioRecorder;

    public AudioPlayerViewModel(IAudioManager audioManager)
    {
        this.audioManager = audioManager;
        this.audioRecorder = audioManager.CreateRecorder();
    }

    public async Task StartRecordingAsync()
    {
        await audioRecorder.StartAsync();
    }

    public async Task StopRecordingAsync()
    {
        IAudioSource audioSource = await audioRecorder.StopAsync();

        // You can use the audioSource to play the file or save it somewhere in your application.
    }
}
```

> [!NOTE]  
> You as the developer are responsible for cleaning up the audio files. For instance, when using `StartAsync()` the random file that is generated is _not_ cleaned up automatically.
> Retrieve the file path which is in the resulting object from `StopAsync()` and use that to remove the file when done. Make sure to cast the resulting `IAudioSource` to the concrete type of `FileAudioSource` to be able to retrieve the file path.

## Configure the recording options

When calling `CreateRecorder` it is possible to provide an optional parameter of type `AudioRecorderOptions`, this parameter makes it possible to customize the recording settings at the platform level.

The following example shows how to enable both recording (input) and playback (output) of audio:

```csharp
audioManager.CreateRecorder(
    new AudioRecorderOptions
    {
#if IOS || MACCATALYST
        Category = AVFoundation.AVAudioSessionCategory.PlayAndRecord
#endif
    });
```

## Recording from a specific audio device (Android)

By default, the recorder uses the system's default microphone (`AudioSource.Mic`). To record from a specific device such as a Bluetooth headset or USB microphone, use the `PreferredDevice` property.

```csharp
#if ANDROID
using Android.Content;
using Android.Media;

var audioManager = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService)!;
var btDevice = audioManager.GetDevices(GetDevicesTargets.Inputs)
    .FirstOrDefault(d => d.Type == AudioDeviceType.BluetoothSco);

var recorder = audioMgr.CreateRecorder(
    new AudioRecorderOptions
    {
        PreferredDevice = btDevice
    });
#endif
```

> [!NOTE]
> `SetPreferredDevice` requires Android API 23+ (Marshmallow) for WAV recording and API 28+ (Pie) for AAC recording. On older API levels, the property is ignored and the system default device is used. If the preferred device is unavailable (e.g. disconnected), recording silently falls back to the default device.

> [!IMPORTANT]
> On Android 12+ (API 31+), the `BLUETOOTH_CONNECT` runtime permission may be required to discover and select Bluetooth audio devices. Add it to your `AndroidManifest.xml`:
> ```xml
> <uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />
> ```

## Recording from a Bluetooth microphone (iOS/macCatalyst)

By default, the recorder uses the system's built-in microphone. To enable recording from Bluetooth devices such as AirPods or other headsets, you need to opt in by setting `CategoryOptions` to `AllowBluetooth`. This makes Bluetooth HFP (Hands-Free Profile) devices available as recording inputs.

> [!IMPORTANT]
> Bluetooth HFP recording uses voice-quality audio (8–16 kHz, mono). This is a hardware limitation of the HFP profile. If high-fidelity recording is a priority and Bluetooth input is not needed, do not enable this option.

### Basic Bluetooth recording

```csharp
audioManager.CreateRecorder(
    new AudioRecorderOptions
    {
#if IOS || MACCATALYST
        CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.AllowBluetooth
#endif
    });
```

### Selecting a specific Bluetooth device

To record from a specific Bluetooth device, use the `PreferredInput` property. This requires enumerating the available inputs after the audio session has been configured:

```csharp
#if IOS || MACCATALYST
using AVFoundation;

// Configure and activate a temporary audio session to discover Bluetooth inputs.
// The recorder will re-initialize the session with the same options when started.
var audioSession = AVAudioSession.SharedInstance();
audioSession.SetCategory(AVAudioSessionCategory.Record,
    AVAudioSessionCategoryOptions.AllowBluetooth, out _);
audioSession.SetActive(true, out _);

// Find the Bluetooth HFP input
var btInput = audioSession.AvailableInputs?
    .FirstOrDefault(i => i.PortType == AVAudioSession.PortBluetoothHfp);

// Create the recorder with the preferred input
var recorder = audioManager.CreateRecorder(
    new AudioRecorderOptions
    {
        CategoryOptions = AVAudioSessionCategoryOptions.AllowBluetooth,
        PreferredInput = btInput
    });
#endif
```

> [!NOTE]
> `PreferredInput` must be set together with `CategoryOptions = AllowBluetooth` — without this option, Bluetooth HFP devices will not appear in `AvailableInputs`. No additional Bluetooth permissions are required beyond `NSMicrophoneUsageDescription` (iOS) and `com.apple.security.device.audio-input` (Mac Catalyst).

### iOS 26+: High-quality Bluetooth recording

Starting with iOS 26, Apple introduced a `bluetoothHighQualityRecording` category option which enables full-bandwidth audio recording from supported Bluetooth devices (certain AirPods models). This removes the HFP 8–16 kHz limitation.

> [!WARNING]
> This feature is iOS 26+ only (not macCatalyst), is **not available in the European Union**, increases input latency, and is not recommended for real-time communication. It requires the `default` audio session mode.

The .NET `AVAudioSessionCategoryOptions` enum does not yet include this flag. For apps using `AVCaptureSession`, it can be enabled via its `ConfiguresApplicationAudioSessionForBluetoothHighQualityRecording` property. Since this plugin uses `AVAudioRecorder`/`AVAudioSession`, the category option binding is needed — check future .NET SDK updates for availability.

To check if a connected Bluetooth device supports high-quality recording:

```csharp
var input = AVAudioSession.SharedInstance().CurrentRoute.Inputs.FirstOrDefault();
var micExt = input?.BluetoothMicrophoneExtension;
if (micExt?.HighQualityRecording.IsSupported == true)
{
    // Device supports high-quality Bluetooth recording
}
```

In the meantime, combine with `AllowBluetooth` for HFP fallback on unsupported devices or regions.

## Recording from a specific audio device (Windows)

By default, the recorder uses the system default audio capture device. To record from a specific device such as a Bluetooth headset or USB microphone, use the `AudioDeviceId` property.

```csharp
#if WINDOWS
using Windows.Devices.Enumeration;
using Windows.Media.Devices;

var devices = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioCaptureSelector());
var usbMic = devices.FirstOrDefault(d => d.Name.Contains("USB"));

var recorder = audioManager.CreateRecorder(
    new AudioRecorderOptions
    {
        AudioDeviceId = usbMic?.Id
    });
#endif
```

> [!NOTE]
> No additional permissions are required beyond `<DeviceCapability Name="microphone"/>` in `Package.appxmanifest`. If an invalid or disconnected device ID is provided, `MediaCapture` initialization will throw an exception — unlike Android, Windows does not silently fall back to the default device.

## AudioRecorder API

Once you have created an `AudioRecorder` you can interact with it in the following ways:

### Properties

The `AudioRecorder` class provides the following properties:

#### `CanRecordAudio`

Gets whether the device is capable of recording audio.

#### `IsRecording`

Gets whether the recorder is currently recording audio.

### Methods

The `AudioRecorder` class provides the following methods:

#### `StartAsync()`

Start recording audio to disk in a randomly generated file.

#### `StartAsync(string filePath)`

Start recording audio to disk in the supplied filePath.

#### `StopAsync()`

Stop recording and return the `IAudioSource` instance with the recording data.

## Platform specifics

In order to record audio some platforms require some extra additional changes.

### Android

The *AndroidManifest.xml* file will need to be modified to include the following `uses-permission` inside the `manifest` tag.

```xml
<uses-permission android:name="android.permission.RECORD_AUDIO"/>
<uses-permission android:name="android.permission.MODIFY_AUDIO_SETTINGS" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

For a full example of this change check out our [**AndroidManifest.xml**](../samples/Plugin.Maui.Audio.Sample/Platforms/Android/AndroidManifest.xml) file.

### iOS

The **Info.plist** file will need to be modified to include the following 2 entries inside the `dict` tag.

```xml
<key>NSMicrophoneUsageDescription</key>
<string>The [app name] wants to use your microphone to record audio.</string>
```

> [!NOTE]
> If you want to record in the background on iOS, you will need to add a key to the **Info.plist** file like show below. \
> \
> `<key>UIBackgroundModes</key>` \
> `<array>` \
> `  <string>audio</string>` \
> `</array>`

**Replacing [app name] with your application name.**

For a full example of this change check out our [**Info.plist**](../samples/Plugin.Maui.Audio.Sample/Platforms/iOS/Info.plist) file.

### MacCatalyst

This change is identical to the iOS section but for explicitness:

The **Info.plist** file will need to be modified to include the following 2 entries inside the `dict` tag.

```xml
<key>NSMicrophoneUsageDescription</key>
<string>The [app name] wants to use your microphone to record audio.</string>
```

> [!NOTE]
> If you distribute your app to others, you will need to declare an [entitlement](https://learn.microsoft.com/dotnet/maui/ios/entitlements) in order to be able to access the microphone. Add a key to the `Entitlements.plist` file like show below. \
> \
> `<key>com.apple.security.device.audio-input</key>` \
> `<true/>` \
> \
> For a full example of this change check out our [**Entitlements.plist**](../samples/Plugin.Maui.Audio.Sample/Platforms/MacCatalyst/Entitlements.plist) file.

**Replacing [app name] with your application name.**

For a full example of this change check out our [**Info.plist**](../samples/Plugin.Maui.Audio.Sample/Platforms/MacCatalyst/Info.plist) file.

### Windows

The **Package.appxmanifest** file will need to be modified to include the following entry inside the `Capabilities` tag.

```xml
<DeviceCapability Name="microphone"/>
```

For a full example of this change check out our [**Package.appxmanifest**](../samples/Plugin.Maui.Audio.Sample/Platforms/Windows/Package.appxmanifest) file.

## Sample

For a concrete example of recording audio in a .NET MAUI application check out our sample application and specifically the [`AudioRecorderPageViewModel`](../samples/Plugin.Maui.Audio.Sample/ViewModels/AudioRecorderPageViewModel.cs) class.
