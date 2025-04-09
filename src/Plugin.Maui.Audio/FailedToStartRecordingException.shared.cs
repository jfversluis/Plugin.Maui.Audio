namespace Plugin.Maui.Audio;

/// <summary>
/// When the recording fails to start this exception is thrown with details about the cause.
/// </summary>
public class FailedToStartRecordingException : Exception
{
	/// <summary>
	/// Creates a new instance of this exception.
	/// </summary>
	/// <param name="message">Message which describes the cause of the exception.</param>
	public FailedToStartRecordingException(string message) : base(message)
	{
	}

	/// <summary>
	/// Triggers a throw of this exception.
	/// </summary>
	/// <param name="message">Message which describes the cause of the exception.</param>
	/// <exception cref="FailedToLoadAudioException"></exception>
	public static void Throw(string message) => throw new FailedToStartRecordingException(message);
}