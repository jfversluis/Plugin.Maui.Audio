using System;
using System.Threading.Tasks;

namespace Plugin.Maui.Audio;

partial class AudioRecorder : IAudioRecorder
{
	public AudioRecorder()
	{
	}

	public bool CanRecordAudio => false;

	public bool IsRecording => false;

	public Task StartAsync() => Task.CompletedTask;

	public Task StartAsync(string filePath) => Task.CompletedTask;

	public Task<IAudioSource> StopAsync() => Task.FromResult<IAudioSource>(new EmptyAudioSource());
}