# Audio playback

The `AudioPlayer` class provides you with the ability to play audio files/streams in your .NET MAUI application. In order to create an `AudioPlayer` instance you can make use of the `CreatePlayer` method on the [`AudioManager`](../readme.md#audiomanager) class.

```csharp
public class AudioPlayerViewModel
{
    readonly IAudioManager audioManager;

    public AudioPlayerViewModel(IAudioManager audioManager)
    {
        this.audioManager = audioManager;
    }

    public async void PlayAudio()
    {
        var audioPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"));

        audioPlayer.Play();
    }
}
```

## Configure the playback options

When calling `CreatePlayer` it is possible to provide an optional parameter of type `AudioPlayerOptions`, this parameter makes it possible to customize the playback settings at the platform level. 

> [!NOTE]
> Currently you can customize options for iOS, macOS, Android, and Windows.

The following example shows how to configure your audio to blend in with existing audio being played on device on iOS and macOS:

```csharp
audioManager.CreatePlayer(
    await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"),
    new AudioPlayerOptions
    {
#if IOS || MACCATALYST
        CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.MixWithOthers
#endif
    });
```

For more information, please refer to the iOS documentation: https://developer.apple.com/documentation/avfaudio/avaudiosession/categoryoptions-swift.struct?language=objc

This next example shows how to configure some of the attributes to describe your audio stream. This can, for example, influence which volume setting applies to your played audio.

```csharp
audioManager.CreatePlayer(
    await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"),
    new AudioPlayerOptions
    {
#if ANDROID
        AudioContentType = Android.Media.AudioContentType.Music,
        AudioUsageKind = Android.Media.AudioUsageKind.Media,
#endif
    });
```

For more information, please refer to the Android documentation: https://developer.android.com/reference/android/media/AudioAttributes

## Audio Focus and Interruption Handling

The `AudioPlayer` automatically handles audio focus on Android and audio interruptions on iOS/macOS by default. This ensures proper behavior when your app interacts with other audio sources, such as phone calls, notifications, or other media apps.

### Android Audio Focus

On Android, the plugin automatically:
- **Requests audio focus** when you call `Play()`, notifying the system that your app wants to play audio
- **Abandons audio focus** when you call `Pause()` or `Stop()`, allowing other apps to take control
- **Responds to focus changes** from other apps:
  - **Permanent loss**: Stops playback (e.g., user starts music in another app)
  - **Temporary loss**: Pauses playback and resumes when focus returns (e.g., phone call)
  - **Audio ducking**: Temporarily lowers volume to 20% while other audio plays (e.g., navigation prompts), then restores full volume

For more information, see the [Android Audio Focus documentation](https://developer.android.com/media/optimize/audio-focus).

#### Configuring Audio Focus (Android)

You can control audio focus behavior through the `AudioPlayerOptions`:

```csharp
var audioPlayer = audioManager.CreatePlayer(
    await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"),
    new AudioPlayerOptions
    {
#if ANDROID
        ManageAudioFocus = false  // Disable automatic audio focus management
#endif
    });
```

When `ManageAudioFocus` is set to `false`, the player will not request or respond to audio focus changes, giving you full manual control.

### iOS/macOS Audio Interruptions

On iOS and macOS, the plugin automatically:
- **Registers for interruption notifications** when the player is created
- **Responds to interruptions**:
  - **Interruption began**: Pauses playback (e.g., incoming phone call, alarm)
  - **Interruption ended**: Resumes playback if the system indicates it should resume
- **Unregisters** interruption observers when the player is disposed

For more information, see the [iOS Audio Interruptions documentation](https://developer.apple.com/documentation/avfaudio/handling-audio-interruptions).

#### Configuring Interruption Handling (iOS/macOS)

You can control interruption handling behavior through the `AudioPlayerOptions`:

```csharp
var audioPlayer = audioManager.CreatePlayer(
    await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"),
    new AudioPlayerOptions
    {
#if IOS || MACCATALYST
        HandleAudioInterruptions = false  // Disable automatic interruption handling
#endif
    });
```

When `HandleAudioInterruptions` is set to `false`, the player will not automatically pause or resume during interruptions, giving you full manual control.

> [!NOTE]
> By default, both `ManageAudioFocus` (Android) and `HandleAudioInterruptions` (iOS/macOS) are enabled (`true`). Your app will properly interact with system audio and other apps out of the box. The audio focus management is handled transparently - you can still control playback manually using `Play()`, `Pause()`, and `Stop()` methods. For backward compatibility, playback will continue even if audio focus cannot be acquired, though this is rare.

## Controlling Audio Output Device/Port

You can control which audio output device or port is used for playback on all platforms.

### Android - Output Device Selection

On Android, you can specify which audio output device should be used for playback. This is useful when you want to ensure audio plays through a specific output, such as the device speaker, even when other outputs like Bluetooth are connected.

```csharp
audioManager.CreatePlayer(
    await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"),
    new AudioPlayerOptions
    {
#if ANDROID
        PreferredOutputDevice = Plugin.Maui.Audio.AudioOutputDevice.Speaker
#endif
    });
```

This feature requires Android API 28 (Android 9.0 Pie) or higher. On older versions, the setting will be ignored and the system default routing will be used.

Available output device options include:
- `AudioOutputDevice.Default` - Use system default routing
- `AudioOutputDevice.Speaker` - Built-in device speaker (loudspeaker)
- `AudioOutputDevice.Earpiece` - Built-in earpiece (typically used for phone calls)
- `AudioOutputDevice.WiredHeadset` - Wired headset or headphones with microphone
- `AudioOutputDevice.WiredHeadphones` - Wired headphones without microphone
- `AudioOutputDevice.BluetoothA2dp` - Bluetooth device with A2DP profile (e.g., Bluetooth headphones, car audio)
- `AudioOutputDevice.BluetoothSco` - Bluetooth SCO device (typically used for phone calls)
- `AudioOutputDevice.UsbDevice` - USB audio device
- `AudioOutputDevice.UsbAccessory` - USB accessory
- `AudioOutputDevice.AuxLine` - Auxiliary line connection (e.g., 3.5mm aux cable)

**Note:** The system treats this as a preference, not a guarantee. If the requested device is not available or connected, the system will fall back to its default routing behavior.

### iOS/macOS - Output Port Override

On iOS, you can override the audio output port to force audio to play through the built-in speaker instead of the earpiece when no external audio devices are connected. This is primarily useful for `PlayAndRecord` sessions where the default output is the earpiece.

> [!IMPORTANT]
> The speaker override requires the audio session category to be set to `PlayAndRecord`. If your session uses the default `Playback` category, the plugin will automatically upgrade to `PlayAndRecord` when a speaker override is requested. Note that speaker override does **not** override wired headphones or Bluetooth — when those are connected, audio will route to them regardless of this setting. On Mac Catalyst, speaker override has no effect since Macs do not distinguish between speaker and earpiece routing.

```csharp
audioManager.CreatePlayer(
    await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"),
    new AudioPlayerOptions
    {
#if IOS || MACCATALYST
        Category = AVFoundation.AVAudioSessionCategory.PlayAndRecord,
        PreferredOutputPort = Plugin.Maui.Audio.AudioOutputPort.Speaker
#endif
    });
```

Available output port options include:
- `AudioOutputPort.Default` - Use system default routing
- `AudioOutputPort.Speaker` - Force output to built-in speaker

**Note:** Unlike Android's per-player device selection, iOS uses a session-wide port override that affects all audio output on the device. The override remains in effect until explicitly changed back to `AudioOutputPort.Default` or all players using the override are disposed.

### Windows - Output Device Selection

On Windows, you can specify which audio output device should be used for playback by providing the device name (or a partial name match).

```csharp
audioManager.CreatePlayer(
    await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"),
    new AudioPlayerOptions
    {
#if WINDOWS
        PreferredOutputDeviceName = "Speakers"
#endif
    });
```

The first audio render device whose name contains the specified value (case-insensitive) will be selected. If the specified device is not found, the system default audio device will be used.

### Cross-Platform Example

```csharp
var options = new AudioPlayerOptions
{
#if ANDROID
    PreferredOutputDevice = Plugin.Maui.Audio.AudioOutputDevice.Speaker,
#elif IOS || MACCATALYST
    Category = AVFoundation.AVAudioSessionCategory.PlayAndRecord,
    PreferredOutputPort = Plugin.Maui.Audio.AudioOutputPort.Speaker,
#elif WINDOWS
    PreferredOutputDeviceName = "Speakers",
#endif
};
var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"), options);
player.Play(); // Audio plays through device speaker on all platforms
```

## AudioPlayer API

Once you have created an `AudioPlayer` you can interact with it in the following ways:

### Events

The `AudioPlayer` class provides the following events:

#### `PlaybackEnded`

Raised when audio playback completes successfully.

### Properties

The `AudioPlayer` class provides the following properties:

#### `Balance`

Gets or sets the balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right.

#### `CanSeek`

Gets a value indicating whether the position of the loaded audio file can be updated.

#### `CanSetSpeed`

Gets a value indicating whether the playback speed can be changed.

#### `CurrentPosition`

Gets the current position of audio playback in seconds.

#### `Duration`

Gets the length of audio in seconds.

#### `IsPlaying`

Gets a value indicating whether the currently loaded audio file is playing.

#### `MaximumSpeed`

Gets the maximum speed that is supported on the platform the app is running on that can be set for the `Speed` property.

#### `MinimumSpeed`

Gets the minimum speed that is supported on the platform the app is running on that can be set for the `Speed` property.

#### `Speed`

Gets or sets the speed of the playback. Note: the maximum and minimum value that can be set is dependant on the platform you're running on. Setting a value that is not supported on the platform will gracefully fallback, but will not have the desired result.

To determine the supported minimum and maximum speeds at runtime for that platform you can use `MaximumSpeed` and `MinimumSpeed`.

Platform notes:
- Android: between 0 and 2.5. Setting the value to 0 will pause playback, playback will not be resumed when incrementing the value again.
- iOS: between 0.5 and 2.
- Windows: between 0 and 8. Setting the value to 0 will pause playback, playback will be resumed when incrementing the value again.

#### `Volume`

Gets or sets the playback volume 0 to 1 where 0 is no-sound and 1 is full volume.

#### `Loop`

Gets or sets whether the player will continuously repeat the currently playing sound.

### Methods

The `AudioPlayer` class provides the following methods:

#### `Pause()`

Pause playback if playing (does not resume).

#### `Play()`

Begin playback or resume if paused.

#### `Seek(double position)`

Set the current playback position (in seconds).

#### `Stop()`

Stop playback and set the current position to the beginning.

## Sample

For a concrete example of playing audio in a .NET MAUI application check out our sample application and specifically the [`MusicPlayerPageViewModel`](https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/samples/Plugin.Maui.Audio.Sample/ViewModels/MusicPlayerPageViewModel.cs) class.
