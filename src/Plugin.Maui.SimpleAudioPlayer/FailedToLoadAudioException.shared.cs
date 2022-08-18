using System.Runtime.Serialization;

namespace Plugin.Maui.SimpleAudioPlayer;

public class FailedToLoadAudioException : Exception
{
	public FailedToLoadAudioException(string message) : base(message)
	{
	}

    protected FailedToLoadAudioException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public static void Throw(string message) => throw new FailedToLoadAudioException(message);
}
