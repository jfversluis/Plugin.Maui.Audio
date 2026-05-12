using AVFoundation;

namespace Plugin.Maui.Audio;

/// <summary>
/// Specifies the preferred audio output port override for iOS/macOS.
/// </summary>
/// <remarks>
/// This enum controls audio routing on iOS/macOS platforms using AVAudioSession.
/// Unlike Android's device-specific routing, iOS uses a session-wide port override that affects all audio.
/// </remarks>
public enum AudioOutputPort : ulong
{
	/// <summary>
	/// Use the default audio routing behavior. The system will route audio based on connected devices.
	/// Corresponds to <see cref="AVAudioSessionPortOverride.None"/>.
	/// </summary>
	Default = AVAudioSessionPortOverride.None,

	/// <summary>
	/// Force audio output to the built-in speaker, overriding the default earpiece routing.
	/// This is primarily useful when using the PlayAndRecord category, where the default output is the earpiece.
	/// Note: This does not override wired headphones or Bluetooth devices — when those are connected, audio routes to them regardless.
	/// Corresponds to <see cref="AVAudioSessionPortOverride.Speaker"/>.
	/// </summary>
	Speaker = AVAudioSessionPortOverride.Speaker,
}
