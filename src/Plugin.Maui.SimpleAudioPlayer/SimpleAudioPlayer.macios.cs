using AVFoundation;
using Foundation;

namespace Plugin.Maui.SimpleAudioPlayer;

class SimpleAudioPlayerImplementation : ISimpleAudioPlayer
{
	AVAudioPlayer player;
	bool isDisposed;

	public double Duration => player?.Duration ?? 0;

	public double CurrentPosition => player?.CurrentTime ?? 0;

	public double Volume
	{
		get => player?.Volume ?? 0;
		set
		{
			if (player is not null)
			{
				player.Volume = (float)Math.Clamp(value, 0, 1);
			}
		}
	}

	public double Balance
	{
		get => player?.Pan ?? 0;
		set
		{
			if (player is not null)
			{
				player.Pan = (float)Math.Clamp(value, -1, 1);
			}
		}
	}

	public bool IsPlaying => player?.Playing ?? false;

	public bool Loop
	{
		get => player?.NumberOfLoops != 0;
		set
		{
			if (player is not null)
			{
				player.NumberOfLoops = value ? -1 : 0;
			}
		}
	}

	public bool CanSeek => player is not null;

	public event EventHandler PlaybackEnded;

	~SimpleAudioPlayerImplementation()
	{
		Dispose(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}

		if (disposing)
		{
			DeletePlayer();
		}

		isDisposed = true;
	}

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

	public bool Load(Stream audioStream)
	{
		ArgumentNullException.ThrowIfNull(audioStream);

		DeletePlayer();

		var data = NSData.FromStream(audioStream);
		player = AVAudioPlayer.FromData(data);

		return PreparePlayer();
	}

	public bool Load(string fileName)
	{
		ArgumentNullException.ThrowIfNull(fileName);

		DeletePlayer();

		player = AVAudioPlayer.FromUrl(NSUrl.FromFilename(fileName));

		return PreparePlayer();
	}

	public void Pause()
	{
		player?.Pause();
	}

	public void Play()
	{
		if (player is null)
		{
			return;
		}

		if (player.Playing)
		{
			player.CurrentTime = 0;
		}
		else
		{
			player.Play();
		}
	}

	public void Seek(double position)
	{
		if (player is null)
		{
			return;
		}

		player.CurrentTime = position;
	}

	public void Stop()
	{
		player?.Stop();
		Seek(0);
		PlaybackEnded?.Invoke(this, EventArgs.Empty);
	}

	bool PreparePlayer()
	{
		if (player is null)
		{
			return false;
		}

		player.FinishedPlaying += OnPlayerFinishedPlaying;
		player.PrepareToPlay();

		return true;
	}

	void DeletePlayer()
	{
		Stop();

		if (player is null)
		{
			return;
		}

		player.FinishedPlaying -= OnPlayerFinishedPlaying;
		player.Dispose();
		player = null;

		/* Unmerged change from project 'Plugin.Maui.SimpleAudioPlayer(net6.0-maccatalyst)'
		Before:
			private void OnPlayerFinishedPlaying(object sender, AVStatusEventArgs e)
		After:
			void OnPlayerFinishedPlaying(object sender, AVStatusEventArgs e)
		*/
	}

	void OnPlayerFinishedPlaying(object sender, AVStatusEventArgs e)
	{
		PlaybackEnded?.Invoke(this, e);
	}
}