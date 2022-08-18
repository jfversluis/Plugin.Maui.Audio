namespace Plugin.Maui.SimpleAudioPlayer;

public static class MauiAppBuilderExtensions
{
	public static MauiAppBuilder UseSimpleAudioPlayer(
		this MauiAppBuilder mauiAppBuilder,
		Action<SimpleAudioPlayerOptions> optionsProvider = null)
	{
		var options = new SimpleAudioPlayerOptions();

        optionsProvider?.Invoke(options);

		ISimpleAudioPlayerFactory factory = new SimpleAudioPlayerFactory();

        mauiAppBuilder.Services.AddSingleton(factory);

		switch (options.SimpleAudioPlayerLifetime)
		{
			case ServiceLifetime.Singleton:
				mauiAppBuilder.Services.AddSingleton(factory.CreatePlayer());
				break;

            case ServiceLifetime.Scoped:
                mauiAppBuilder.Services.AddScoped(s => factory.CreatePlayer());
                break;

            case ServiceLifetime.Transient:
                mauiAppBuilder.Services.AddTransient(s => factory.CreatePlayer());
                break;
        }

		return mauiAppBuilder;
	}
}

public class SimpleAudioPlayerOptions
{
	public ServiceLifetime SimpleAudioPlayerLifetime { get; set; } = ServiceLifetime.Singleton;
}