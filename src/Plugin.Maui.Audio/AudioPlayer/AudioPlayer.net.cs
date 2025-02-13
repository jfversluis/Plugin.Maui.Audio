﻿namespace Plugin.Maui.Audio;

partial class AudioPlayer : IAudioPlayer
{
	public AudioPlayer(AudioPlayerOptions audioPlayerOptions) { }

	public AudioPlayer(Stream audioStream, AudioPlayerOptions audioPlayerOptions) { }

	public AudioPlayer(string fileName, AudioPlayerOptions audioPlayerOptions) { }

	public void SetSource(Stream audioStream) { }

	protected virtual void Dispose(bool disposing) { }

	public double Duration { get; }

	public double CurrentPosition { get; }

	public double Volume { get; set; }

	public double Balance { get; set; }

	public bool IsPlaying { get; }

	public bool Loop { get; set; }

	public bool CanSeek { get; }

	public void Play() { }

	public void Pause() { }

	public void Stop() { }

	public void SetSpeed(double speed) { }

	public void Seek(double position) { }

	public double Speed { get; set; }

	public double MinimumSpeed { get; }

	public double MaximumSpeed { get; }

	public bool CanSetSpeed { get; }
}