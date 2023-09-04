using AudioPlayerSample.ViewModels;

namespace AudioPlayerSample.Pages;

public partial class MyLibraryPage : ContentPage
{
	public MyLibraryPage(MyLibraryPageViewModel myLibraryPageViewModel)
	{
		InitializeComponent();

		BindingContext = myLibraryPageViewModel;
	}
}