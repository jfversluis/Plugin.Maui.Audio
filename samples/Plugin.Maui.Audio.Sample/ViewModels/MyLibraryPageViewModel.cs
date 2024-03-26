using System.Collections.ObjectModel;

namespace Plugin.Maui.Audio.Sample.ViewModels;

public class MyLibraryPageViewModel : BaseViewModel
{
	public Command AddRecordingCommand { get; }
	public Command OpenMusicCommand { get; }
	public ObservableCollection<MusicItemViewModel> Music { get; }


	public MyLibraryPageViewModel()
	{
		Music = new ObservableCollection<MusicItemViewModel>
		{
			new MusicItemViewModel("The Happy Ukelele Song", "Stanislav Fomin", "ukelele.mp3")
		};

		AddRecordingCommand = new Command(async () => await AddRecording());
		OpenMusicCommand = new Command(async (object item) => await OnMusicItemSelected((MusicItemViewModel)item));
	}


	async Task AddRecording()
	{
		await Shell.Current.GoToAsync(Routes.AudioRecorder.RouteName);
	}

	async Task OnMusicItemSelected(MusicItemViewModel musicItem)
	{
		await Shell.Current.GoToAsync(
			Routes.MusicPlayer.RouteName,
			new Dictionary<string, object>
			{
				[Routes.MusicPlayer.Arguments.Music] = musicItem
			});
	}
}