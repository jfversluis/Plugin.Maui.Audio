﻿using Plugin.Maui.SimpleAudioPlayer;

namespace AudioPlayerSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddTransient<MainPage>();

        builder.Services.AddTransient(SimpleAudioPlayer.ImplementationFactory);
        //builder.Services.AddScoped(SimpleAudioPlayer.ImplementationFactory);
        //builder.Services.AddSingleton(SimpleAudioPlayer.Current);

		return builder.Build();
	}
}

