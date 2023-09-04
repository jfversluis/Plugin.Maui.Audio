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
		PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

		if (status == PermissionStatus.Granted)
		{
			return true;
		}

		if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
		{
			// Prompt the user to turn on in settings
			// On iOS once a permission has been denied it may not be requested again from the application
			return false;
		}

		if (Permissions.ShouldShowRationale<TPermission>())
		{
			// Prompt the user with additional information as to why the permission is needed
		}

		status = await Permissions.RequestAsync<TPermission>();

		return status == PermissionStatus.Granted;
	}
}