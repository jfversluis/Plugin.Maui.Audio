# Plugin.Maui.Audio

`Plugin.Maui.Audio` provides the ability to play audio inside a .NET MAUI application.

## Getting Started

* Available on NuGet: <http://www.nuget.org/packages/Plugin.Maui.Audio> [![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.Audio.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.Audio/)

## API Usage

`Plugin.Maui.Audio` provides the `AudioManager` class that allows for the creation of `AudioPlayer`s. The `AudioManager` can be used with or without dependency injection.

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

##### `CurrentPosition`

Gets the current position of audio playback in seconds.

##### `Duration`

Gets the length of audio in seconds.

##### `IsPlaying`

Gets a value indicating whether the currently loaded audio file is playing.

##### `Loop`

Gets or sets the playback volume 0 to 1 where 0 is no-sound and 1 is full volume.

##### `Volume`

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
