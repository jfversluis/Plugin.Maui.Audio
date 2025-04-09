# Plugin.Maui.Audio

`Plugin.Maui.Audio` provides the ability to play, record and stream audio inside a .NET MAUI application.

## Getting Started

* Available on NuGet: <http://www.nuget.org/packages/Plugin.Maui.Audio> [![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.Audio.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.Audio/)

## API Usage

`Plugin.Maui.Audio` provides the `AudioManager` class that allows for the creation of [`AudioPlayers`](docs/audio-player.md) and [`AudioRecorders`](docs/audio-recorder.md) and [`AudioStreamers`](docs/audio-streamer.md). The `AudioManager` can be used with or without dependency injection.

### `AudioManager`

There are two different ways in which you can interact with the `AudioManager` implementation provided by this plugin, they are:

#### Dependency Injection

You will first need to register the `AudioManager` with the `MauiAppBuilder` based on the following example:

```csharp
builder.AddAudio();
```

> [!NOTE]  
> You can also customize the shared settings for both audio [playback](./docs/audio-player.md#configure-the-playback-options) and [recording](./docs/audio-recorder.md#configure-the-recording-options).

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

Now that you know how to use the `AudioManager` class, please refer to the following sections:

* [Audio playback](docs/audio-player.md)
* [Record audio](docs/audio-recorder.md)
* [Stream audio](docs/audio-streamer.md)
* [Audio listeners](docs/audio-listeners.md)

## Supported Audio Formats

### Audio Playback

The audio formats supported for playback depend on the underlying platform:

| Platform | Supported Formats |
|----------|------------------|
| Android | MP3, WAV, AAC, FLAC, OGG and other formats supported by Android's [MediaPlayer](https://developer.android.com/reference/android/media/MediaPlayer#setdatasource) |
| iOS/MacCatalyst | MP3, WAV, AAC, ALAC, AIFF, and other formats supported by [AVAudioPlayer](https://developer.apple.com/documentation/avfaudio/avaudioplayer) |
| Windows | MP3, WAV, AAC, FLAC, WMA, and other formats supported by [Windows MediaPlayer](https://learn.microsoft.com/windows/win32/wmp/file-format-support-in-windows-media-player) |

The audio file format is determined automatically based on the file extension or stream content.

### Audio Recording

Recording capabilities vary by platform with the following supported encoding formats:

| Platform | Supported Recording Formats |
|----------|----------------------------|
| Android | WAV (PCM), AAC (Android 12+) - [MediaRecorder docs](https://developer.android.com/reference/android/media/MediaRecorder#supported-formats) |
| iOS/MacCatalyst | WAV (PCM), ULaw, ALAC (Apple Lossless), FLAC, AAC - [AVAudioRecorder docs](https://developer.apple.com/documentation/avfoundation/avaudiorecorder) |
| Windows | WAV (PCM), ALAC (Apple Lossless), FLAC, AAC - [Windows.Media.Capture docs](https://learn.microsoft.com/uwp/api/windows.media.capture.mediacapture) |

#### PCM Audio Details

When recording or streaming PCM audio (WAV format):
- **Sample Rates**: 
  - Android: 44100Hz is recommended and most widely supported
  - iOS/macOS: 48000Hz is recommended
  - Customizable in all platforms (8000Hz, 16000Hz, etc.)
- **Channels**:
  - Mono (1 channel) and Stereo (2 channels) supported on all platforms
  - Mono is recommended for maximum compatibility
- **Bit Depth**:
  - 8-bit and 16-bit PCM supported (16-bit recommended)
  - Windows streaming supports only 16-bit PCM

## Acknowledgements

This project could not have came to be without these projects and people, thank you! <3

## SimpleAudioPlayer for Xamarin

Basically this plugin, but then for Xamarin. We have been using this in our Xamarin projects with much joy and ease, so thank you so much [Adrian](https://github.com/adrianstevens) (and contributors!) for that. Find the original project [here](https://github.com/adrianstevens/Xamarin-Plugins/tree/main/SimpleAudioPlayer) where we have based our project on and evolved it from there.

## The Happy Ukelele Song

As a little sample song we wanted something Hawaii/Maui themed obviously, and we found The Happy Ukelele Song which seems to fit that description. Thank you [Stanislav Fomin](https://download1.audiohero.com/artist/597084) and [AudioHero](https://download1.audiohero.com/track/40778468) for making it available.
