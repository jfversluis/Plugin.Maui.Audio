namespace Plugin.Maui.Audio;

/// <summary>
/// Provides async/await support by wrapping an <see cref="IAudioPlayer"/>.
/// </summary>
public class AsyncAudioPlayer
{
	readonly IAudioPlayer audioPlayer;
	CancellationTokenSource? stopCancellationToken;

	/// <summary>
	/// Creates a new instance of <see cref="AsyncAudioPlayer"/>.
	/// This is particularly useful if you want to customise the audio playback settings before playback.
	/// </summary>
	/// <param name="audioPlayer">An <see cref="IAudioPlayer"/> implementation to act as the underlying mechanism of playing audio.</param>
	public AsyncAudioPlayer(IAudioPlayer audioPlayer)
	{
		this.audioPlayer = audioPlayer;
	}

	/// <summary>
	/// Begin audio playback asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow for canceling the audio playback.</param>
	/// <returns>A <see cref="Task"/> that represents the asynchronous operation, awaiting this method will wait until the audio has finished playing before continuing.</returns>
	public async Task PlayAsync(CancellationToken cancellationToken)
	{
		var taskCompletionSource = new TaskCompletionSource();

		stopCancellationToken = new();

		audioPlayer.PlaybackEnded += (o, e) => taskCompletionSource.TrySetResult();

		audioPlayer.Play();

		await Task.WhenAny(
			taskCompletionSource.Task,
			cancellationToken.WhenCanceled(),
			stopCancellationToken.Token.WhenCanceled());

		audioPlayer.Stop();
	}

	/// <summary>
	/// Stops the currently playing audio.
	/// </summary>
	public void Stop()
	{
		stopCancellationToken?.Cancel();
	}
}
