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
> Currently you can only customize options for iOS, macOS and Android.

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
