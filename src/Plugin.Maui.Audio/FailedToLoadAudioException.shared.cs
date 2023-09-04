﻿using System.Runtime.Serialization;

namespace Plugin.Maui.Audio;

/// <summary>
/// When the audio fails to load this exception is thrown with details about the cause.
/// </summary>
public class FailedToLoadAudioException : Exception
{
	/// <summary>
	/// Creates a new instance of this exception.
	/// </summary>
	/// <param name="message">Message which describes the cause of the exception.</param>
	public FailedToLoadAudioException(string message) : base(message)
	{
	}

	protected FailedToLoadAudioException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}

	/// <summary>
	/// Triggers a throw of this exception.
	/// </summary>
	/// <param name="message">Message which describes the cause of the exception.</param>
	/// <exception cref="FailedToLoadAudioException"></exception>
	public static void Throw(string message) => throw new FailedToLoadAudioException(message);
}