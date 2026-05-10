namespace Plugin.Maui.Audio;

partial class AudioRecorderOptions
{
    /// <summary>
    /// Gets or sets the ID of the preferred audio capture device.
    /// <para>
    /// Set this to a device ID obtained from
    /// <c>Windows.Devices.Enumeration.DeviceInformation.FindAllAsync</c> using
    /// <c>Windows.Media.Devices.MediaDevice.GetAudioCaptureSelector()</c> to record
    /// from a specific device (e.g. a Bluetooth headset or USB microphone).
    /// When <see langword="null"/>, the system default input device is used.
    /// </para>
    /// <para>
    /// <b>Note:</b> If an invalid or disconnected device ID is provided,
    /// <c>MediaCapture.InitializeAsync</c> will throw an exception. Unlike Android,
    /// Windows does not silently fall back to the default device.
    /// </para>
    /// </summary>
    public string? AudioDeviceId { get; set; }
}
