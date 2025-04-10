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

When calling `CreateRecorder` it is possible to provide an optional parameter of type `AudioRecorderOptions`, this parameter makes it possible to customize the recording settings at the platform level. **Note that currently you can only customize options for iOS and macOS**.

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
