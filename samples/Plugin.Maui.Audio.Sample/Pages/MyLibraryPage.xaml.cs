using Plugin.Maui.Audio.Sample.ViewModels;

namespace Plugin.Maui.Audio.Sample.Pages;

public partial class MyLibraryPage : ContentPage
{
	public MyLibraryPage(MyLibraryPageViewModel myLibraryPageViewModel)
	{
		InitializeComponent();

		BindingContext = myLibraryPageViewModel;
	}
}