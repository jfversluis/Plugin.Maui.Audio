namespace Plugin.Maui.Audio;


/// <summary>
/// iOS supports: Wav, ULaw, Alac, Flac, Aac <para />
/// Windows supports: Wav, Alac, Flac, Aac <para />
/// Android supports: Wav, Aac <para />
/// </summary>
public enum Encoding
{
	Wav,	// pcm WAV file
	ULaw,	// telephony compression
	Alac,	// apple lossless compression
	Flac,	// lossless compression
	Aac		// lossy compression (AAC in MP4/M4A container)
}

