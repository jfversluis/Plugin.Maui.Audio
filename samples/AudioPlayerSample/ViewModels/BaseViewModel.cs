using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioPlayerSample.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public async Task<bool> HavePermissionMicrophoneAsync()
	{
		var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

		return (status == PermissionStatus.Unknown) || (status == PermissionStatus.Granted);
	}
}