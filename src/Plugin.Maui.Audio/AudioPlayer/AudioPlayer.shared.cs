namespace Plugin.Maui.Audio;

public partial class AudioPlayer : IAudioPlayer
{
#pragma warning disable CS0067

	/// <summary>
	/// Something bad happened while loading media or playing.
	/// </summary>
	public event EventHandler? Error;

	public event EventHandler? PlaybackEnded;

#pragma warning restore CS0067
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

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}
}
