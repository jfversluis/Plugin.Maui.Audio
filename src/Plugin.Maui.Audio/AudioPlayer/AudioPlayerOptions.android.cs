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
}
