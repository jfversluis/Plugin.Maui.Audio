using Plugin.Maui.Audio;

namespace AudioPlayerSample;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel mainPageModel)
	{
		InitializeComponent();
        BindingContext = mainPageModel;

	}
}
