namespace Plugin.Maui.Audio;

partial class AudioPlayerOptions
{
	/// <summary>
	/// Gets or sets the preferred audio output device name for Windows. Default value: <see langword="null"/> (system default).
	/// </summary>
	/// <remarks>
	/// This property allows you to control which audio output device is used for playback on Windows.
	/// Set this to the name (or partial name) of the audio render device, for example "Speakers" or "Headphones".
	/// The first device whose name contains this value (case-insensitive) will be selected.
	/// <para>
	/// If the specified device is not found, the system default audio device will be used.
	/// Set to <see langword="null"/> or empty string to use the system default.
	/// </para>
	/// </remarks>
	public string? PreferredOutputDeviceName { get; set; }
}
