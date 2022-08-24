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

		builder.Services.AddTransient<MusicPlayerPage>();
		builder.Services.AddTransient<MusicPlayerPageViewModel>();

		Routing.RegisterRoute(Routes.MusicPlayer.RouteName, typeof(MusicPlayerPage));

		builder.Services.AddSingleton(AudioManager.Current);

		return builder.Build();
	}
}