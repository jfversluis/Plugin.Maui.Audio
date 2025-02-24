using System.Runtime.Versioning;

namespace Plugin.Maui.Audio;

/// <summary>
/// iOS supports: Wav, ULaw, Alac, Flac, Aac <para />
/// Windows supports: Wav, Alac, Flac, Aac <para />
/// Android supports: Wav, Aac <para />
/// </summary>
public enum Encoding
{
	/// <summary>
	/// Uncompressed WAV File (PCM).
	/// </summary>
	Wav,
	
	/// <summary>
	/// Uncompressed WAV File (PCM).
	/// </summary>
	[Obsolete("Use Wav instead")]
	LinearPCM = Wav,
	
	/// <summary>
	/// Telephony Compression.
	/// </summary>
	[SupportedOSPlatform("iOS")]
	[SupportedOSPlatform("MacCatalyst")]
	ULaw,
	
	/// <summary>
	/// Apple Lossless Audio Compression.
	/// </summary>
	[SupportedOSPlatform("iOS")]
	[SupportedOSPlatform("MacCatalyst")]
	[SupportedOSPlatform("Windows")]
	Alac, 
	
	/// <summary>
	/// Free Lossless Audio Compression.
	/// </summary>
	[SupportedOSPlatform("iOS")]
	[SupportedOSPlatform("MacCatalyst")]
	[SupportedOSPlatform("Windows")]
	Flac,
	
	/// <summary>
	/// Lossy Compression (AAC in MP4/M4A container).
	/// </summary>
	Aac
}

