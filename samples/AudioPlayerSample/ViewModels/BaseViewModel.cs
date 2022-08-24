using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioPlayerSample.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public event PropertyChangedEventHandler PropertyChanged;
}

