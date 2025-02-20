namespace Plugin.Maui.Audio;


/// <summary>
/// iOS supports: Wav, ULaw, Alac, Flac, Aac <para />
/// Windows supports: Wav, Alac, Flac, Aac <para />
/// Android supports: Wav, Aac <para />
/// </summary>
public enum Encoding
{
	Wav,    // Uncompressed WAV File (PCM)
	ULaw,   // Telephony Compression
	Alac,   // Apple Lossless Audio Compression
	Flac,   // Free Lossless Audio Compression
	Aac     // Lossy Compression (AAC in MP4/M4A container)
}

