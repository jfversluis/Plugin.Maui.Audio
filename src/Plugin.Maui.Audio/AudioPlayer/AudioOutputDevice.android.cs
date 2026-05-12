using Android.Media;

namespace Plugin.Maui.Audio;

/// <summary>
/// Specifies the preferred audio output device type for Android.
/// </summary>
/// <remarks>
/// This enum is used to specify which audio output device should be preferred for playback.
/// Requires Android API 28 (Android 9.0 Pie) or higher for setting the preferred device.
/// On older versions, this setting will be ignored and the system default routing will be used.
/// </remarks>
public enum AudioOutputDevice
{
	/// <summary>
	/// Use the system default audio output device. No preferred device will be set.
	/// </summary>
	Default = 0,

	/// <summary>
	/// Route audio to the built-in device speaker (typically the loudspeaker).
	/// Corresponds to <see cref="AudioDeviceType.BuiltinSpeaker"/>.
	/// </summary>
	Speaker = AudioDeviceType.BuiltinSpeaker,

	/// <summary>
	/// Route audio to the built-in earpiece (typically used for phone calls).
	/// Corresponds to <see cref="AudioDeviceType.BuiltinEarpiece"/>.
	/// </summary>
	Earpiece = AudioDeviceType.BuiltinEarpiece,

	/// <summary>
	/// Route audio to a wired headset or headphones.
	/// Corresponds to <see cref="AudioDeviceType.WiredHeadset"/>.
	/// </summary>
	WiredHeadset = AudioDeviceType.WiredHeadset,

	/// <summary>
	/// Route audio to a wired headphone device.
	/// Corresponds to <see cref="AudioDeviceType.WiredHeadphones"/>.
	/// </summary>
	WiredHeadphones = AudioDeviceType.WiredHeadphones,

	/// <summary>
	/// Route audio to a Bluetooth device with A2DP profile (e.g., Bluetooth headphones, car audio).
	/// Corresponds to <see cref="AudioDeviceType.BluetoothA2dp"/>.
	/// </summary>
	BluetoothA2dp = AudioDeviceType.BluetoothA2dp,

	/// <summary>
	/// Route audio to a Bluetooth SCO (Synchronous Connection Oriented) device (typically used for phone calls).
	/// Corresponds to <see cref="AudioDeviceType.BluetoothSco"/>.
	/// </summary>
	BluetoothSco = AudioDeviceType.BluetoothSco,

	/// <summary>
	/// Route audio to an auxiliary line connection (e.g., 3.5mm aux cable).
	/// Corresponds to <see cref="AudioDeviceType.AuxLine"/>.
	/// </summary>
	AuxLine = AudioDeviceType.AuxLine,

	/// <summary>
	/// Route audio to a USB audio device.
	/// Corresponds to <see cref="AudioDeviceType.UsbDevice"/>.
	/// </summary>
	UsbDevice = AudioDeviceType.UsbDevice,

	/// <summary>
	/// Route audio to a USB accessory.
	/// Corresponds to <see cref="AudioDeviceType.UsbAccessory"/>.
	/// </summary>
	UsbAccessory = AudioDeviceType.UsbAccessory,
}
