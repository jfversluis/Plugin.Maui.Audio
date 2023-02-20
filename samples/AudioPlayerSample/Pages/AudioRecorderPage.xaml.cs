namespace AudioPlayerSample.Pages;

public partial class AudioRecorderPage : ContentPage
{
	public AudioRecorderPage(ViewModels.AudioRecorderPageViewModel audioRecorderPageViewModel)
	{
		InitializeComponent();

		BindingContext = audioRecorderPageViewModel;
		(BindingContext as ViewModels.AudioRecorderPageViewModel).setPage(this);
	}
}