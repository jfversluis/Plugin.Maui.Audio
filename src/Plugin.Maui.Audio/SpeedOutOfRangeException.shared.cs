namespace Plugin.Maui.Audio;

/// <summary>
/// When attempt is done to set playback speed outside the supported range, this exception is thrown with details about the cause.
/// </summary>
public class SpeedOutOfRangeException : Exception
{
	public double? MinValue { get; }
	public double? MaxValue { get; }
	public double? ActualValue { get; }

	/// <summary>
	/// Creates a new instance of this exception.
	/// </summary>
	/// <param name="message">Message which describes the cause of the exception.</param>
	/// <param name="actualValue">The actual value that caused the exception.</param>
	/// <param name="minValue">Minimum value that is supported.</param>
	/// <param name="maxValue">Maximum value that is supported.</param>
	/// <param name="innerException">Inner exception if exists.</param>
	public SpeedOutOfRangeException(string? message = null, double? actualValue = null, double? minValue = null, double? maxValue = null, Exception? innerException = null) : base(message, innerException)
	{
		MinValue = minValue;
		MaxValue = maxValue;
		ActualValue = actualValue;
	}

	/// <summary>
	/// Triggers a throw of this exception.
	/// </summary>
	/// <param name="message">Message which describes the cause of the exception.</param>
	/// <param name="actualValue">The actual value that caused the exception.</param>
	/// <param name="minValue">Minimum value that is supported.</param>
	/// <param name="maxValue">Maximum value that is supported.</param>
	/// <param name="innerException">Inner exception if exists.</param>
	/// <exception cref="SpeedOutOfRangeException"></exception>
	public static void Throw(string? message = null, double? actualValue = null, double? minValue = null, double? maxValue = null, Exception? innerException = null) => throw new SpeedOutOfRangeException(message, actualValue, minValue, maxValue, innerException);
}
