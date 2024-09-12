using Plugin.Maui.Audio.Sample.ViewModels;

namespace Plugin.Maui.Audio.Sample.Pages;

public partial class SilenceDetectionPage : ContentPage
{
	public SilenceDetectionPage(SilenceDetectionPageViewModel silenceDetectionPageViewModel)
	{
		InitializeComponent();

		BindingContext = silenceDetectionPageViewModel;
	}
}