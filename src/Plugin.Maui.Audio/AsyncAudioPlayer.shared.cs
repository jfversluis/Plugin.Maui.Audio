namespace Plugin.Maui.Audio;

/// <summary>
/// Provides async/await support by wrapping an <see cref="IAudioPlayer"/>.
/// </summary>
public class AsyncAudioPlayer
{
	private readonly IAudioPlayer audioPlayer;

	internal AsyncAudioPlayer(IAudioPlayer audioPlayer)
	{
		this.audioPlayer = audioPlayer;
	}

	/// <summary>
	/// Begin audio playback asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to allow for canceling the audio playback.</param>
	/// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
	public async Task PlayAsync(CancellationToken cancellationToken)
	{
		var taskCompletionSource = new TaskCompletionSource();

		audioPlayer.PlaybackEnded += (o, e) => taskCompletionSource.SetResult();

		audioPlayer.Play();

		await Task.WhenAny(taskCompletionSource.Task, cancellationToken.WhenCanceled());

		audioPlayer.Stop();
	}
}
