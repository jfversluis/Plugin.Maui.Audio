namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public AudioRecorder()
	{
	}

	public bool CanRecordAudio => false;

	public bool IsRecording => false;

	public Task DetectSilenceAsync(double silenceThreshold, int silenceDuration)
	{
		throw new NotImplementedException();
	}

	public Task StartAsync() => Task.CompletedTask;

	public Task StartAsync(string filePath) => Task.CompletedTask;

	public Task<IAudioSource> StopAsync() => Task.FromResult<IAudioSource>(new EmptyAudioSource());
}