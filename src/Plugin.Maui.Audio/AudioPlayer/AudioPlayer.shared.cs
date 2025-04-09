namespace Plugin.Maui.Audio;

public partial class AudioPlayer : IAudioPlayer
{
#pragma warning disable CS0067

	/// <summary>
	/// Raised when an error occurred while loading media or playing.
	/// </summary>
	public event EventHandler? Error;

	/// <summary>
	/// Raised when audio playback completes successfully.
	/// </summary>
	public event EventHandler? PlaybackEnded;

#pragma warning restore CS0067
	/// <summary>
	/// Finalizer that ensures unmanaged resources are freed.
	/// </summary>
	~AudioPlayer()
	{
		Dispose(false);
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
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}
}
