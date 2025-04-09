namespace Plugin.Maui.Audio;

/// <summary>
/// Provides extension methods for registering Plugin.Maui.Audio services with the MAUI dependency injection container.
/// </summary>
public static class MauiAppBuilderExtensions
{
	/// <summary>
	/// Adds the <see cref="IAudioManager"/> as a singleton to the dependency injection container and provides the ability to configure
	/// the shared options for both audio playback and recording.
	/// </summary>
	/// <param name="mauiAppBuilder">The <see cref="MauiAppBuilder"/> to register the package with.</param>
	/// <param name="configurePlaybackOptions">
	/// The mechanism to define the shared options for use with the <see cref="IAudioPlayer"/> implementations. Note this is optional.
	/// An example of configuring the audio playback options for iOS/macOS and Android, is as follows:
	/// <code>
	/// builder
	///     .AddAudio(
	///			configurePlaybackOptions: playbackOptions =>
	///			{
	///#if IOS || MACCATALYST
	///				playbackOptions.Category = AVFoundation.AVAudioSessionCategory.Playback;
	///				playbackOptions.Mode = AVFoundation.AVAudioSessionMode.Default;
	///				playbackOptions.CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.DefaultToSpeaker;
	///#endif
	///#if ANDROID
	///				playbackOptions.AudioContentType = Android.Media.AudioContentType.Music;
	///				playbackOptions.AudioUsageKind = Android.Media.AudioUsageKind.Media;
	/// #endif
	///			});
	/// </code>
	/// </param>
	/// <param name="configureRecordingOptions">
	/// The mechanism to define the shared options for use with the <see cref="IAudioRecorder"/> implementations. Note this is optional.
	/// An example of configuring the audio playback options for iOS and macOS, is as follows:
	/// <code>
	/// builder
	///     .AddAudio(
	///			configureRecordingOptions: recordingOptions =>
	///			{
	///#if IOS || MACCATALYST
	///				recordingOptions.Category = AVFoundation.AVAudioSessionCategory.Record;
	///				recordingOptions.Mode = AVFoundation.AVAudioSessionMode.Default;
	///				recordingOptions.CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.DefaultToSpeaker;
	///#endif
	///			});
	/// </code>
	/// </param>
  /// <param name="configureStreamerOptions">Configures options for streaming much alike the playback and recording options.</param>
	/// <returns>The <paramref name="mauiAppBuilder"/> supplied in order to allow for chaining of method calls.</returns>
	public static MauiAppBuilder AddAudio(
		this MauiAppBuilder mauiAppBuilder,
		Action<AudioPlayerOptions>? configurePlaybackOptions = null,
		Action<AudioRecorderOptions>? configureRecordingOptions = null,
		Action<AudioStreamOptions>? configureStreamerOptions = null)
	{
		var playbackOptions = new AudioPlayerOptions();
		configurePlaybackOptions?.Invoke(playbackOptions);
		AudioManager.Current.DefaultPlayerOptions = playbackOptions;

		var recordingOptions = new AudioRecorderOptions();
		configureRecordingOptions?.Invoke(recordingOptions);
		AudioManager.Current.DefaultRecorderOptions = recordingOptions;

		var streamerOptions = new AudioStreamOptions();
		configureStreamerOptions?.Invoke(streamerOptions);
		AudioManager.Current.DefaultStreamerOptions = streamerOptions;

		mauiAppBuilder.Services.AddSingleton(AudioManager.Current);

		return mauiAppBuilder;
	}
}