namespace Plugin.Maui.Audio;

public class AsyncAudioPlayer
{
	private readonly IAudioPlayer audioPlayer;

	public AsyncAudioPlayer(IAudioPlayer audioPlayer)
	{
		this.audioPlayer = audioPlayer;
	}

	public Task PlayAsync(CancellationToken cancellationToken)
	{
		var taskCompletionSource = new TaskCompletionSource<bool>();

		try
		{
			audioPlayer.PlaybackEnded += (o, e) => taskCompletionSource.SetResult(true);

			audioPlayer.Play();

			return taskCompletionSource.Task;
		}
		catch (OperationCanceledException)
		{
			audioPlayer.Stop();
		}

		return Task.CompletedTask;
	}
}
