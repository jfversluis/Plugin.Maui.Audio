namespace Plugin.Maui.Audio;


/// <summary>
/// iOS supports: LinearPCM, ULaw, Alac and Flac <para />
/// Windows supports: LinearPCM, Alac and Flac <para />
/// Android supports: LinearPCM <para />
/// </summary>
public enum Encoding
{
	LinearPCM,
	ULaw,
	Alac,
	Flac
}

