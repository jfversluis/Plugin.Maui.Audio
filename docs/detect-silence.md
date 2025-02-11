# Detect silence

Silence detection helps stop recording automatically when microphone doesn't capture any sound besides the background noise. To use silence detection simply add `DetectSilenceAsync` after starting to record.

```csharp
await audioRecorder.StartAsync(tempRecordFilePath);
await audioRecorder.DetectSilenceAsync();
await audioRecorder.StopAsync();
```

## Properties

`SoundDetected` is true when during actual recording there was a sound detected. It is set to false at start of `DetectSilenceAsync` method. This property can be use to prevent from saving empty recordings.

```csharp
public async Task<IAudioSource> GetRecordingAsync()
{
    IAudioSource audioSource = await audioRecorder.StopAsync();

    if (audioRecorder.SoundDetected)
    {
        return audioSource;
    }
    else
    {
        return null;
    }
}
```

## Methods

`DetectSilenceAsync` has three optional parameters.

### Silence treshold

`SilenceTreshold` defines how quiet it must be for the algorithm to qualify it as silence. When `audioLevel <= silenceThreshold * noiseLevel` then it is considered as silence and silence duration time is starting to be measured.

> [!NOTE]
> The background noise is dynamically adjusted enabling using this feature in noisy environment with changing amplitude of background sounds.

> [!NOTE]
> Default value of 'SilenceTreshold' is 2.

### Silence duration

`SilenceDuration` defines how long silence must persist to complete the `DetectSilenceAsync` task. The value is given in milliseconds.

> [!NOTE]
> Default value of 'SilenceDuration' is 1000 ms.

### Cancellation token

If there will be available another way to stop recording other than `DetectSilenceAsync` method (e.g. stop record button), remember to use cancellation token to stop silence detection also. Example of usage:

```csharp
async Task StartStopRecordToggleAsync()
{
    if (!IsRecording)
    {
        await RecordAsync();
    }
    else
    {
        StopRecording();
    }
}

async Task RecordAsync()
{
    await RecordUntilSilenceDetectedAsync();
    StopRecording();
    audioSource = await GetRecordingAsync();
}

public async Task RecordUntilSilenceDetectedAsync()
{
    cancelDetectSilenceTokenSource = new();
	
    try
    {
        if (!audioRecorder.IsRecording)
        {
            await audioRecorder.StartAsync(tempRecordFilePath);
            await audioRecorder.DetectSilenceAsync(SilenceTreshold, SilenceDuration, cancelDetectSilenceTokenSource.Token);
        }
    }
    catch (OperationCanceledException)
    {
        return;
    }
}

void StopRecording() => cancelDetectSilenceTokenSource?.Cancel();

public async Task<IAudioSource> GetRecordingAsync()
{
    IAudioSource audioSource = await audioRecorder.StopAsync();
		
    ...
}
```
