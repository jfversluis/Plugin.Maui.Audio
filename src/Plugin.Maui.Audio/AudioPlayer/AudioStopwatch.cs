using System.Diagnostics;

namespace Plugin.Maui.Audio;

public class AudioStopwatch(TimeSpan startOffset, double speed) : Stopwatch
{
	public TimeSpan StartOffset { get; private set; } = startOffset;
	readonly double currentSpeed = speed;

	public new void Restart()
	{
		StartOffset = TimeSpan.Zero;
		base.Restart();
	}

	public new long ElapsedMilliseconds
	{
		get
		{
			return (long)(StartOffset.TotalMilliseconds + (base.ElapsedMilliseconds * currentSpeed));
		}
	}

	public new long ElapsedTicks
	{
		get
		{
			return (long)(StartOffset.Ticks + (base.ElapsedTicks * currentSpeed));
		}
	}
}
