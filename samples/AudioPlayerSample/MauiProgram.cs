using AudioPlayerSample.Pages;
using AudioPlayerSample.ViewModels;
using Plugin.Maui.Audio;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace AudioPlayerSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddTransient<MyLibraryPage>();
		builder.Services.AddTransient<MyLibraryPageViewModel>();

		RegisterPageRoute<AudioRecorderPage, AudioRecorderPageViewModel>(Routes.AudioRecorder.RouteName, builder.Services);
		RegisterPageRoute<MusicPlayerPage, MusicPlayerPageViewModel>(Routes.MusicPlayer.RouteName, builder.Services);

		builder.Services.AddSingleton(AudioManager.Current);

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