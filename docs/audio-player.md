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
