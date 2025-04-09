namespace Plugin.Maui.Audio.Sample.Pages;

public partial class AudioStreamerPage : ContentPage
{
	public AudioStreamerPage(ViewModels.AudioStreamerPageViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		((ViewModels.AudioStreamerPageViewModel)BindingContext).OnNavigatedFrom();
	}
}