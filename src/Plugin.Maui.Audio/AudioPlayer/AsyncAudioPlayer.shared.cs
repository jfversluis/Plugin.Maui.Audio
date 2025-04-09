namespace Plugin.Maui.Audio;

/// <summary>
/// Provides async/await support by wrapping an <see cref="IAudioPlayer"/>.
/// </summary>
public class AsyncAudioPlayer : IAudio
{
	readonly IAudioPlayer audioPlayer;
	CancellationTokenSource? stopCancellationToken;
	bool isDisposed;

#pragma warning disable CS0067

	/// <summary>
	/// Raised when an error occurred while loading media or playing.
	/// </summary>
	public event EventHandler? Error;

#pragma warning restore CS0067

	/// <summary>
	/// Creates a new instance of <see cref="AsyncAudioPlayer"/>.
	/// This is particularly useful if you want to customise the audio playback settings before playback.
	/// </summary>
	/// <param name="audioPlayer">An <see cref="IAudioPlayer"/> implementation to act as the underlying mechanism of playing audio.</param>
	public AsyncAudioPlayer(IAudioPlayer audioPlayer)
	{
		this.audioPlayer = audioPlayer;
		audioPlayer.Error += OnErrorInternal;
	}

	/// <inheritdoc cref="IAudio.Duration" />
	public double Duration => audioPlayer.Duration;

	/// <inheritdoc cref="IAudio.CurrentPosition" />
	public double CurrentPosition => audioPlayer.CurrentPosition;

	/// <inheritdoc cref="IAudio.Volume" />
	public double Volume
	{
		get => audioPlayer.Volume;
		set => audioPlayer.Volume = value;
	}

	/// <inheritdoc cref="IAudio.Balance" />
	public double Balance
	{
		get => audioPlayer.Balance;
		set => audioPlayer.Balance = value;
	}

	/// <inheritdoc cref="IAudio.Speed" />
	public double Speed
	{
		get => audioPlayer.Speed;
		set => audioPlayer.Speed = value;
	}

	/// <inheritdoc cref="IAudio.MinimumSpeed" />
	public double MinimumSpeed => audioPlayer.MinimumSpeed;

	/// <inheritdoc cref="IAudio.MaximumSpeed" />
	public double MaximumSpeed => audioPlayer.MaximumSpeed;

	/// <inheritdoc cref="IAudio.CanSetSpeed" />
	public bool CanSetSpeed => audioPlayer.CanSetSpeed;

	/// <inheritdoc cref="IAudio.IsPlaying" />
	public bool IsPlaying { get; protected set; }

	/// <inheritdoc cref="IAudio.Loop" />
	public bool Loop
	{
		get => audioPlayer.Loop;
		set => audioPlayer.Loop = value;
	}

	/// <inheritdoc cref="IAudio.CanSeek" />
	public bool CanSeek => audioPlayer.CanSeek;

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

	/// <inheritdoc cref="IAudio.Seek(double)" />
	public void Seek(double position) => audioPlayer.Seek(position);

	/// <summary>
	/// Stops the currently playing audio.
	/// </summary>
	public void Stop()
	{
		stopCancellationToken?.Cancel();
	}

	/// <summary>
	/// Something bad happened while loading media or playing.
	/// </summary>
	/// <param name="error">Native platform error</param>
	protected virtual void OnError(EventArgs error)
	{
		Error?.Invoke(this, error);
	}

	/// <summary>
	/// Releases the unmanaged resources used by the <see cref="AsyncAudioPlayer"/> and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			if (disposing)
			{
				audioPlayer.Error -= OnErrorInternal;
				audioPlayer.Dispose();
			}

			isDisposed = true;
		}
	}

	void OnErrorInternal(object? sender, EventArgs e)
	{
		OnError(e);
	}

	/// <summary>
	/// Finalizer that ensures unmanaged resources are freed.
	/// </summary>
	~AsyncAudioPlayer()
	{
		Dispose(disposing: false);
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
