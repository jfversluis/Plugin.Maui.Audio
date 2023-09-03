using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace AudioPlayerSample.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public static async Task<bool> CheckPermissionIsGrantedAsync<TPermission>() where TPermission : BasePermission, new()
	{
		if (DeviceInfo.Platform == DevicePlatform.Android)
		{
			var status = await Permissions.CheckStatusAsync<TPermission>();

			return (status == PermissionStatus.Unknown) || (status == PermissionStatus.Granted);
		}

		return true;
	}
}