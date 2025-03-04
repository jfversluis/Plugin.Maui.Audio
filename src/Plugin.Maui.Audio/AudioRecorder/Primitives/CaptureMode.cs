namespace Plugin.Maui.Audio;

/// <summary>
/// Availability timing of captured audio
/// Bundling: buffers data and delivers when finished
/// Streaming: passthrough data byt eventhandler as soon as possible
/// </summary>
public enum CaptureMode
{
	Bundling,
	Streaming
}