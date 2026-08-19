using System.Diagnostics;
using AVFoundation;

namespace Plugin.Maui.Audio;

internal class ActiveSessionHelper
{
    internal static void InitializeSession(BaseOptions options)
    {		
        var audioSession = AVAudioSession.SharedInstance();

		// Determine the effective category to set.
		// Don't downgrade from PlayAndRecord to Playback — PlayAndRecord is a superset
		// that supports both recording and playback. Downgrading would break any active
		// streamer/recorder sharing this process-wide session.
		var categoryToSet = options.Category;
		var categoryOptionsToSet = options.CategoryOptions;

		if (categoryToSet == AVAudioSessionCategory.Playback &&
			audioSession.Category == AVAudioSession.CategoryPlayAndRecord)
		{
			categoryToSet = AVAudioSessionCategory.PlayAndRecord;
			// Preserve the current session's category options (e.g. MixWithOthers, AllowBluetooth
			// set by streamer/recorder) and merge with player's options, plus DefaultToSpeaker
			// to maintain correct speaker routing for playback.
			categoryOptionsToSet = audioSession.CategoryOptions | options.CategoryOptions | AVAudioSessionCategoryOptions.DefaultToSpeaker;
		}

		if (!TrySetCategoryAndActivate(audioSession, categoryToSet, options.Mode, categoryOptionsToSet, options))
		{
			// If activation fails, it may be because a conflicting category is already active.
			// Deactivate the current session first, then retry with the new category.
			Trace.WriteLine("Audio session activation failed. Attempting to deactivate and reactivate with new category.");
			
			var deactivateError = audioSession.SetActive(false);
			if (deactivateError is not null)
			{
				Trace.WriteLine($"Warning: Failed to deactivate existing audio session: {deactivateError}");
			}
			
			if (!TrySetCategoryAndActivate(audioSession, categoryToSet, options.Mode, categoryOptionsToSet, options))
			{
				Trace.TraceError("Failed to activate audio session after retry.");
				return;
			}
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

	private static bool TrySetCategoryAndActivate(
		AVAudioSession audioSession,
		AVAudioSessionCategory category,
		AVAudioSessionMode mode,
		AVAudioSessionCategoryOptions categoryOptions,
		BaseOptions options)
	{
		var error = audioSession.SetCategory(category, mode, categoryOptions);
		if (error is not null)
		{
			Trace.TraceError($"Failed to set audio session category: {error}");
			return false;
		}

		error = audioSession.SetActive(true, GetSessionSetActiveOptions(options));
		if (error is not null)
		{
			Trace.TraceError($"Failed to activate audio session: {error}");
			return false;
		}

		return true;
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