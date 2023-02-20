using System.Collections.ObjectModel;

namespace AudioPlayerSample.ViewModels;

public class MyLibraryPageViewModel : BaseViewModel
{
	MusicItemViewModel selectedMusicItem;
	public Command AddRecordingCommand { get; }
	public ObservableCollection<MusicItemViewModel> Music { get; }


	public MyLibraryPageViewModel()
	{
		Music = new ObservableCollection<MusicItemViewModel>
		{
			new MusicItemViewModel("The Happy Ukelele Song", "Stanislav Fomin", "ukelele.mp3")
		};

		AddRecordingCommand = new Command(AddRecording);
	}

	public MusicItemViewModel SelectedMusicItem
	{
		get => selectedMusicItem;
		set
		{
			selectedMusicItem = value;
			NotifyPropertyChanged();

			OnMusicItemSelected();
		}
	}

	async void AddRecording()
	{
		await Shell.Current.GoToAsync(Routes.AudioRecorder.RouteName);
	}

	async void OnMusicItemSelected()
	{
		await Shell.Current.GoToAsync(
			Routes.MusicPlayer.RouteName,
			new Dictionary<string, object>
			{
				[Routes.MusicPlayer.Arguments.Music] = SelectedMusicItem
			});
	}
}