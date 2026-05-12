using Android.Media;

namespace Plugin.Maui.Audio;

partial class AudioPlayerOptions : BaseOptions
{
	/// <summary>
	/// Gets or sets the audio content type for Android. Default value: <see cref="AudioContentType.Unknown"/>.
	/// </summary>
	/// <remarks>
	/// See https://developer.android.com/reference/android/media/AudioAttributes for more information.
	/// </remarks>
	public AudioContentType AudioContentType { get; set; } = AudioContentType.Unknown;

	/// <summary>
	/// Gets or sets the audio usage kind for Android. Default value: <see cref="AudioUsageKind.Unknown"/>.
	/// </summary>
	/// <remarks>
	/// See https://developer.android.com/reference/android/media/AudioAttributes for more information.
	/// On Android API26 and below, this is used to set the audio stream type. Where the following values are used:
	/// <list type="bullet">
	/// <item><see cref="AudioUsageKind.Media"/> - <see cref="Android.Media.Stream.Music"/></item>
	/// <item><see cref="AudioUsageKind.Alarm"/> - <see cref="Android.Media.Stream.Alarm"/></item>
	/// <item><see cref="AudioUsageKind.Notification"/> - <see cref="Android.Media.Stream.Notification"/></item>
	/// <item><see cref="AudioUsageKind.VoiceCommunication"/> - <see cref="Android.Media.Stream.VoiceCall"/></item>
	/// <item><see cref="AudioUsageKind.Unknown"/> - <see cref="Android.Media.Stream.System"/></item>
	/// </list>
	/// If any other value is used, the default value of <see cref="Android.Media.Stream.System"/> is used.
	/// </remarks>
	public AudioUsageKind AudioUsageKind { get; set; } = AudioUsageKind.Unknown;

	/// <summary>
	/// Gets or sets whether audio focus should be automatically managed. Default value: <see langword="true"/>.
	/// </summary>
	/// <remarks>
	/// When enabled (default), the player will automatically request audio focus when playing and abandon it when paused or stopped.
	/// This ensures proper interaction with other audio sources like phone calls and other apps.
	/// When disabled, the player will not request or abandon audio focus, giving you full control over audio focus management.
	/// See https://developer.android.com/media/optimize/audio-focus for more information.
	/// </remarks>
	public bool ManageAudioFocus { get; set; } = true;

	/// <summary>
	/// Gets or sets the preferred audio output device for Android. Default value: <see cref="AudioOutputDevice.Default"/>.
	/// </summary>
	/// <remarks>
	/// This property allows you to control which audio output device is used for playback.
	/// For example, you can force audio to play through the device speaker even when Bluetooth is connected.
	/// <para>
	/// This feature requires Android API 28 (Android 9.0 Pie) or higher.
	/// On older versions, this setting will be ignored and the system default routing will be used.
	/// </para>
	/// <para>
	/// Note: The system may override this preference based on user actions or system policies.
	/// For example, if the user is in a phone call, the system may route audio differently.
	/// </para>
	/// </remarks>
#pragma warning disable CA1416 // This enum is only used on API 23+ contexts, initialized as default on all API levels
	public AudioOutputDevice PreferredOutputDevice { get; set; } = AudioOutputDevice.Default;
#pragma warning restore CA1416
}
