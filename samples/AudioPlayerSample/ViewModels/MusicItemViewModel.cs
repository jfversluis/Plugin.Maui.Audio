using System;
namespace AudioPlayerSample.ViewModels;

public class MusicItemViewModel : BaseViewModel
{
	public MusicItemViewModel(string title, string artist, string filename)
	{
		Title = title;
		Artist = artist;
		Filename = filename;
	}

	public string Title { get; }
	public string Artist { get; }
	public string Filename { get; }
}