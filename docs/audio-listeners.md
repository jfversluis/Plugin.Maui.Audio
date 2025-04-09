# Audio Listeners

This library comes with a set of tools that can be used to analyze and manipulate audio data. These tools can be used individually or collectively where they support each other, depending on your needs. 

It mainly consists of:
* `PcmAudioHandler`
* `AudioListeners`
* `PcmAudioHelpers`

Available listeners:
* SilenceListener > provides silence detection
* DecibelListener > provides dBFS output (dBFS = decibels relative to full scale)
* RmsListener > provides RMS output

## How it works collectively

The collective hierarchy: `AudioStreamer` > `PcmAudioHandler` > `AudioListeners` > `PcmAudioHelpers`

Where the `AudioStreamer` provides raw PCM audio that the `PcmAudioHandler` converts into ordered audio samples that the `AudioListeners` listen to and can manipulate and analyse with support of the `PcmAudioHelpers`.

Basic example:
```csharp
// Create PcmAudioHandler
pcmAudioHandler = new PcmAudioHandler(44100, ChannelType.Mono, BitDepth.Pcm16bit);

// Create listener
silenceListener = new SilenceListener
{
   MinimalSilenceTimespanInMilliseconds = 200,
   SilenceThresholdInDb = -40
};
silenceListener.IsSilentChanged += (sender, args) =>
{
   dispatcher.Dispatch(() =>
   {
      IsSilent = args.IsSilent;
   });
};

// Subscribe the SilenceListener to PcmAudioHandler
pcmAudioHandler.Subscribe(silenceListener);

// Connect PcmAudioHandler to AudioStreamer for incoming audio
audioStreamer.OnAudioCaptured += (sender, args) =>
{
   pcmAudioHandler.HandlePcmAudio(args.Audio);
};
```

### PcmAudioHandler usage
The purpose of the `PcmAudioHandler` is to convert raw audio into meaningful samples. For optimization this conversion is done once and can be used by multiple subscribed listeners.

Connecting to the AudioStreamer:
```csharp
void OnAudioStreamerDataCaptured(object sender, AudioStreamEventArgs args)
{
   pcmAudioHandler.HandlePcmAudio(args.Audio);
}
```

> When subscribing listeners to the `PcmAudioHandler` the settings `SampleRate`, `ChannelType` and `BitDepth` ​​are automatically passed on to its listeners.

### AudioListeners usage
An `AudioListener` needs to be subscribed to the `PcmAudioHandler`, the listener will receive audio samples as soon as the `PcmAudioHandler` has converted the audio. 

Custom `AudioListeners` can be created by implementing the `IPcmAudioListener` interface.

## How they work individually
Components can be used individually, but its easier to work with them collectively otherwise you probably need to do some things yourself. 

### PcmAudioHandler

The `PcmAudioHandler` converts RAW audio once for multiple listeners and also broadcasts the converted audio using the `ConvertedAudio` event, for implementation not using listeners. 

#### Properties & Events

##### ConvertedAudio (event)
Provides a stream of ordered audio samples.

##### HandlerOutputMode
When the `PcmAudioHandler` broadcasts converted audio.

* `Sample` > when a sample has been converted
* `ChannelPair` > when a single sample for all channels have been converted
* `Block` > when all samples have been converted (all as a single `AudioStreamer` block)

`Block` is the default value.

##### SampleRate
Samples taken per second (​is automatically passed on to its listeners)

##### Channels
Mono or Stereo audio (​is automatically passed on to its listeners)

##### BitDepth
The size of a single sample (​is automatically passed on to its listeners)


### AudioListeners (IPcmAudioListener)

The `AudioListeners` are designed to handle audio samples for analysis and manipulation. Listeners can work individually as well, when providing audio samples manually.

> Audio data typically consists of large amount of samples, take into account that analyzing or manipulating audio data can be performance prone.

#### Properties & Methods

##### Clear()
Clearing all analytics data and optional caches. 

##### SampleRate
Samples taken per second

##### Channels
Mono or Stereo audio

##### BitDepth
The size of a single sample

### Audio Helpers

PcmAudioHelpers is collective of helper methods that can be freely used for audio data.
It includes, among other things:

* PCM to samples conversion
* WAV file header creation
* Decibel calculations
* RMS (Root Mean Square) calculations
* Channel splitter 

## Sample

For a concrete example of audio listeners in a .NET MAUI application check out our sample application and specifically the [`AudioStreamerPageViewModel`](https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/samples/Plugin.Maui.Audio.Sample/ViewModels/AudioStreamerPageViewModel.cs) class.
