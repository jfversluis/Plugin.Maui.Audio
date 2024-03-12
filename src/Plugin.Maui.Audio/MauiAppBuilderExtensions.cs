namespace Plugin.Maui.Audio;

public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="IAudioManager"/> as a singleton to the dependency injection container.
    /// </summary>
    /// <param name="mauiAppBuilder"></param>
    /// <param name="configurePlaybackOptions"></param>
    /// <param name="configureRecordingOptions"></param>
    /// <returns></returns>
    public static MauiAppBuilder AddAudio(
        this MauiAppBuilder mauiAppBuilder,
        Action<AudioPlayerOptions> configurePlaybackOptions,
        Action<AudioRecorderOptions> configureRecordingOptions)
    {
        var playbackOptions = new AudioPlayerOptions();
        configurePlaybackOptions?.Invoke(playbackOptions);
        AudioManager.Current.DefaultPlayerOptions = playbackOptions;

        var recordingOptions = new AudioRecorderOptions();
        configureRecordingOptions?.Invoke(recordingOptions);
        AudioManager.Current.DefaultRecorderOptions = recordingOptions;

        mauiAppBuilder.Services.AddSingleton(AudioManager.Current);

        return mauiAppBuilder;
    }
}