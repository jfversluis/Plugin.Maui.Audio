# Plugin.Maui.Audio

`Plugin.Maui.Audio` provides the ability to play and record audio inside a .NET MAUI application.

## Getting Started

* Available on NuGet: <http://www.nuget.org/packages/Plugin.Maui.Audio> [![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.Audio.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.Audio/)

## API Usage

`Plugin.Maui.Audio` provides the `AudioManager` class that allows for the creation of `AudioPlayer`s and `AudioRecorder`s. The `AudioManager` can be used with or without dependency injection.

### `AudioManager`

#### Dependency Injection

You will first need to register the `AudioManager` with the `MauiAppBuilder` following the same pattern that the .NET MAUI Essentials libraries follow.

```csharp
builder.Services.AddSingleton(AudioManager.Current);
```

You can then enable your classes to depend on `IAudioManager` as per the following example.

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

#### Straight usage

Alternatively if you want to skip using the dependency injection approach you can use the `AudioManager.Current` property.

```csharp
public class AudioPlayerViewModel
{
    public async void PlayAudio()
    {
        var audioPlayer = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ukelele.mp3"));

        audioPlayer.Play();
    }
}
```

### AudioPlayer

Once you have created an `AudioPlayer` you can interact with it in the following ways:

#### Events

##### `PlaybackEnded`

Raised when audio playback completes successfully.

#### Properties

##### `Balance`

Gets or sets the balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right.

##### `CanSeek`

Gets a value indicating whether the position of the loaded audio file can be updated.

##### `CanSetSpeed`

Gets a value indicating whether the playback speed can be changed.

##### `CurrentPosition`

Gets the current position of audio playback in seconds.

##### `Duration`

Gets the length of audio in seconds.

##### `IsPlaying`

Gets a value indicating whether the currently loaded audio file is playing.

##### `MaximumSpeed`

Gets the minumum speed that is supported on the platform the app is running on that can be set for the `Speed` property.

##### `MinimumSpeed`

Gets the maximum speed that is supported on the platform the app is running on that can be set for the `Speed` property.

##### `Speed`

Gets or sets the speed of the playback. Note: the maximum and minimum value that can be set is dependant on the platform you're running on. Setting a value that is not supported on the platform will gracefully fallback, but will not have the desired result.

To determine the supported minimum and maximum speeds at runtime for that platform you can use `MaximumSpeed` and `MinimumSpeed`.

Platform notes:
- Android: between 0 and 6. Setting the value to 0 will pause playback, playback will not be resumed when incrementing the value again.
- iOS: between 0.5 and 2.
- Windows: between 0 and 8. Setting the value to 0 will pause playback, playback will be resumed when incrementing the value again.

##### `Volume`

Gets or sets the playback volume 0 to 1 where 0 is no-sound and 1 is full volume.

##### `Loop`

Gets or sets whether the player will continuously repeat the currently playing sound.

#### Methods

##### `Pause()`

Pause playback if playing (does not resume).

##### `Play()`

Begin playback or resume if paused.

##### `Seek(double position)`

Set the current playback position (in seconds).

##### `Stop()`

Stop playback and set the current position to the beginning.

### AudioRecorder

You can create an `AudioRecorder` through the `AudioManager` and its `CreateRecorder` method.

Once you have created an `AudioRecorder` you can interact with it in the following ways:

#### Properties

##### `CanRecordAudio`

Gets whether the device is capable of recording audio.

##### `IsRecording`

Gets whether the recorder is currently recording audio.

#### Methods

##### `StartAsync()`

Start recording audio to disk in a randomly generated file.

##### `StartAsync(string filePath)`

Start recording audio to disk in the supplied filePath.

##### `StopAsync()`

Stop recording and return the `IAudioSource` instance with the recording data.

## Acknowledgements

This project could not have came to be without these projects and people, thank you! <3

## SimpleAudioPlayer for Xamarin

Basically this plugin, but then for Xamarin. We have been using this in our Xamarin projects with much joy and ease, so thank you so much [Adrian](https://github.com/adrianstevens) (and contributors!) for that. Find the original project [here](https://github.com/adrianstevens/Xamarin-Plugins/tree/main/SimpleAudioPlayer) where we have based our project on and evolved it from there.

## The Happy Ukelele Song

As a little sample song we wanted something Hawaii/Maui themed obviously, and we found The Happy Ukelele Song which seems to fit that description. Thank you [Stanislav Fomin](https://download1.audiohero.com/artist/597084) and [AudioHero](https://download1.audiohero.com/track/40778468) for making it available.
