using Android.Media;

namespace Plugin.Maui.Audio;

partial class BaseOptions
{
    /// <summary>
    /// Gets or sets the preferred audio input device for recording.
    /// <para>
    /// Set this to an <see cref="AudioDeviceInfo"/> obtained from
    /// <see cref="AudioManager.GetDevices(GetDevicesTargets)"/> with
    /// <see cref="GetDevicesTargets.Inputs"/> to record from a specific device
    /// (e.g. a Bluetooth headset or USB microphone). When <see langword="null"/>,
    /// the system default input device is used.
    /// </para>
    /// <para>
    /// Requires Android API 23 (Marshmallow) or higher. On older API levels
    /// this property is ignored.
    /// </para>
    /// </summary>
    public AudioDeviceInfo? PreferredDevice { get; set; }
}
