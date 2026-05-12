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
	/// Force audio output to the built-in speaker, overriding the default routing.
	/// Use this to ensure audio plays through the device speaker even when headphones or Bluetooth devices are connected.
	/// Corresponds to <see cref="AVAudioSessionPortOverride.Speaker"/>.
	/// </summary>
	Speaker = AVAudioSessionPortOverride.Speaker,
}
