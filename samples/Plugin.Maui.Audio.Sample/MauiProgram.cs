using Plugin.Maui.Audio.Sample.Pages;
using Plugin.Maui.Audio.Sample.ViewModels;
using Plugin.Maui.Audio;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Plugin.Maui.Audio.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.AddAudio(
				playbackOptions =>
				{
#if IOS || MACCATALYST
					playbackOptions.Category = AVFoundation.AVAudioSessionCategory.Playback;
#endif
#if ANDROID
					playbackOptions.AudioContentType = Android.Media.AudioContentType.Music;
					playbackOptions.AudioUsageKind = Android.Media.AudioUsageKind.Media;
#endif
				},
				recordingOptions =>
				{
#if IOS || MACCATALYST
					recordingOptions.Category = AVFoundation.AVAudioSessionCategory.Record;
					recordingOptions.Mode = AVFoundation.AVAudioSessionMode.Default;
					recordingOptions.CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.MixWithOthers;
#endif
				},
				streamerOptions =>
				{
#if IOS || MACCATALYST
					streamerOptions.Category = AVFoundation.AVAudioSessionCategory.Record;
					streamerOptions.Mode = AVFoundation.AVAudioSessionMode.Default;
					streamerOptions.CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.MixWithOthers;
#endif
				})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddTransient<MyLibraryPage>();
		builder.Services.AddTransient<MyLibraryPageViewModel>();

		RegisterPageRoute<AudioRecorderPage, AudioRecorderPageViewModel>(Routes.AudioRecorder.RouteName, builder.Services);
		RegisterPageRoute<AudioStreamerPage, AudioStreamerPageViewModel>(Routes.AudioStreamer.RouteName, builder.Services);
		RegisterPageRoute<MusicPlayerPage, MusicPlayerPageViewModel>(Routes.MusicPlayer.RouteName, builder.Services);

		return builder.Build();
	}

	static void RegisterPageRoute<TPage, TPageViewModel>(
		string route,
		IServiceCollection services)
		where TPage : ContentPage
		where TPageViewModel : BaseViewModel
	{
		Routing.RegisterRoute(route, typeof(TPage));

		services.AddTransient(typeof(TPage));
		services.AddTransient(typeof(TPageViewModel));
	}
}