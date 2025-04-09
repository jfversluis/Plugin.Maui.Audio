using System.Diagnostics;

namespace Plugin.Maui.Audio;

/// <summary>
/// A specialized stopwatch that handles audio playback timing with support for speed adjustments and start offsets.
/// </summary>
/// <param name="startOffset">The initial time offset to start from.</param>
/// <param name="speed">The playback speed factor.</param>
public class AudioStopwatch(TimeSpan startOffset, double speed) : Stopwatch
{
	/// <summary>
	/// Gets the initial time offset used when calculating elapsed time.
	/// </summary>
	public TimeSpan StartOffset { get; private set; } = startOffset;
	readonly double currentSpeed = speed;

	/// <summary>
	/// Restarts the stopwatch and resets the start offset to zero.
	/// </summary>
	public new void Restart()
	{
		StartOffset = TimeSpan.Zero;
		base.Restart();
	}

	/// <summary>
	/// Gets the total elapsed milliseconds, adjusted by the speed factor and including the start offset.
	/// </summary>
	public new long ElapsedMilliseconds
	{
		get
		{
			return (long)(StartOffset.TotalMilliseconds + (base.ElapsedMilliseconds * currentSpeed));
		}
	}

	/// <summary>
	/// Gets the total elapsed ticks, adjusted by the speed factor and including the start offset.
	/// </summary>
	public new long ElapsedTicks
	{
		get
		{
			return (long)(StartOffset.Ticks + (base.ElapsedTicks * currentSpeed));
		}
	}
}
