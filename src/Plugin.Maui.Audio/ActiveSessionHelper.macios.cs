using System.Diagnostics;
using AVFoundation;

namespace Plugin.Maui.Audio;

internal class ActiveSessionHelper
{
    internal static void InitializeSession(BaseOptions options)
    {		
        var audioSession = AVAudioSession.SharedInstance();

		var error = audioSession.SetCategory(options.Category, options.Mode, options.CategoryOptions);
		if (error is not null)
		{
			Trace.TraceError("failed to set category");
			Trace.TraceError(error.ToString());
		}

		error = audioSession.SetActive(true, GetSessionSetActiveOptions(options));
		if (error is not null)
		{
			Trace.TraceError("failed activate audio session");
			Trace.TraceError(error.ToString());
		}

		if (options.PreferredInput is not null)
		{
			audioSession.SetPreferredInput(options.PreferredInput, out var inputError);
			if (inputError is not null)
			{
				Trace.TraceError($"failed to set preferred input: {inputError}");
			}
		}
    }

    public static void FinishSession(BaseOptions options)
    {
        if (options.SessionLifetime is not SessionLifetime.KeepSessionAlive)
		{
			var audioSession = AVAudioSession.SharedInstance();

			var error = audioSession.SetActive(false, GetSessionSetActiveOptions(options));
			if (error is not null)
			{
				Trace.WriteLine($"Failed to deactivate the audio session: {error}");
			}
		}
    }

    private static AVAudioSessionSetActiveOptions GetSessionSetActiveOptions(BaseOptions options)
    {
        if (options.SessionLifetime is SessionLifetime.EndSessionAndNotifyOthers)
        {
            return AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation;
        }
        else
        {
            return 0;
        }
    }
}