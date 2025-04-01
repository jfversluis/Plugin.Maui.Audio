# Stream audio

The `AudioStreamer` class provides you with the ability to stream audio from a microphone in your .NET MAUI application through an eventhandler. The `AudioStreamer` works pretty much the same as the recorder `AudioRecorder`, except that it provides raw audio data instead of a file. In order to create an `AudioStreamer` instance you can make use of the `CreateStreamer` method on the [`AudioManager`](../readme.md#audiomanager) class.

> [!NOTE]
> If you want to stream in the background on iOS, you will need to add a key to the `Info.plist` file like show below.
> `<key>UIBackgroundModes</key>`
> `<array>`
> `  <string>audio</string>`
> `</array>`

```csharp
public class AudioStreamerViewModel
{
    readonly IAudioManager audioManager;
    readonly IAudioStreamer audioStreamer;

    public AudioStreamerViewModel(IAudioManager audioManager)
    {
        this.audioManager = audioManager;
        this.audioStreamer = audioManager.CreateStreamer();
        this.audioStreamer.OnAudioCaptured += OnAudioStreamerCapturedData;
    }

    public async Task StartStreaming()
    {
        await this.sudioStreamer.StartAsync();
    }

    public async Task StopStreaming()
    {
        await this.AudioStreamer.StopAsync();
    }

    private void OnAudioStreamerCapturedData(object sender, AudioStreamEventArgs args)
    {
        // You can use the args.Audio to collect, analyze or manipulate
        // args.Audio is a byte array with linear PCM audio data (like a WAV file without a header)
    }
}
```

## Configure streaming options

When using an `IAudioStreamer` instance it is possible to set parameters of type `AudioStreamerOptions`, this parameters makes it possible to customize the recording settings at the platform level.

The following example shows how to set streaming settings of audio:

```csharp
audioManager.CreateStreamer();
audioStreamer.Options.Channels = ChannelType.Mono;
audioStreamer.Options.BitDepth = BitDepth.Pcm16bit;
audioStreamer.Options.SampleRate = 441000;
```

> [!NOTE]  
> Each platform has its own limitations and capabilities. Like for Android its recommended to use a sample rate of 44100 Hz, for iOS and macOS 48000 Hz is recommended.

### Configure streaming options for whole app

In the app builder your can set the streaming options for the whole app: 

```csharp
builder.UseMauiApp<App>()
       .AddAudio(
         streamerOptions =>
         {
#if IOS || MACCATALYST
             recordingOptions.SampleRate = 44800;             
             streamerOptions.Category = AVFoundation.AVAudioSessionCategory.Record;
             streamerOptions.Mode = AVFoundation.AVAudioSessionMode.Default;
             streamerOptions.CategoryOptions = VFoundation.AVAudioSessionCategoryOptions.MixWithOthers;

#endif
         })....
```

> [!NOTE]  
> Currently iOS and macOS have extra options to be customized

## AudioStreamer API

Once you have created an `AudioStreamer` you can interact with it in the following ways:

### Properties

The `AudioStreamer` class provides the following properties:

#### `CanStreamAudio`

Gets whether the device is capable of streaming audio.

#### `IsStreaming`

Gets whether the streamer is currently streaming audio.

### Methods and Events

The `AudioStreamer` class provides the following methods and events:

#### `StartAsync()`

Start streaming audio.

#### `StopAsync()`

Stop streaming audio.

#### `EventHandler<AudioStreamEventArgs> OnAudioCaptured`

Provides a stream of captured linear PCM audio data (raw WAV audio)

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

For a full example of this change check out our [*AndroidManifest.xml*](https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/samples/Plugin.Maui.Audio.Sample/Platforms/Android/AndroidManifest.xml) file.

### iOS

The *Info.plist* file will need to be modified to include the following 2 entries inside the `dict` tag.

```xml
<key>NSMicrophoneUsageDescription</key>
<string>The [app name] wants to use your microphone to record audio.</string>
```

**Replacing [app name] with your application name.**

For a full example of this change check out our [*Info.plist*](https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/samples/Plugin.Maui.Audio.Sample/Platforms/iOS/Info.plist) file.

### MacCatalyst

This change is identical to the iOS section but for explicitness:

The *Info.plist* file will need to be modified to include the following 2 entries inside the `dict` tag.

```xml
<key>NSMicrophoneUsageDescription</key>
<string>The [app name] wants to use your microphone to record audio.</string>
```

**Replacing [app name] with your application name.**

For a full example of this change check out our [*Info.plist*](https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/samples/Plugin.Maui.Audio.Sample/Platforms/MacCatalyst/Info.plist) file.

### Windows

The *Package.appxmanifest* file will need to be modified to include the following entry inside the `Capabilities` tag.

```xml
<DeviceCapability Name="microphone"/>
```

For a full example of this change check out our [*Package.appxmanifest*](https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/samples/Plugin.Maui.Audio.Sample/Platforms/Windows/Package.appxmanifest) file.

## Sample

For a concrete example of streamed audio in a .NET MAUI application check out our sample application and specifically the [`AudioStreamerPageViewModel`](https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/samples/Plugin.Maui.Audio.Sample/ViewModels/AudioStreamerPageViewModel.cs) class.
