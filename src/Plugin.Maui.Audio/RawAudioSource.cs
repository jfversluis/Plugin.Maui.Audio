using System;
using System.IO;

namespace Plugin.Maui.Audio;

/// <summary>
/// Enumeration for supported bits per sample in PCM audio.
/// </summary>
public enum BitsPerSample
{
	/// <summary>
	/// 8-bit sample depth.
	/// </summary>
	Bit8 = 8,
	
	/// <summary>
	/// 16-bit sample depth.
	/// </summary>
	Bit16 = 16
}

/// <summary>
/// WAV audio clip created from raw PCM sound data without header.
/// The <see cref="Bytes"/> property provides the complete WAV file with header, usable inside a player.
/// Supports both 8-bit and 16-bit PCM audio.
/// </summary>
public class RawAudioSource : IAudioSource
{
	readonly byte[] soundData;
	readonly int sampleRate;
	readonly int nbOfChannels;
	readonly BitsPerSample bitsPerSample;

	byte[]? withHeader;

	/// <summary>
	/// Initializes a new instance of the <see cref="RawAudioSource"/> class with 8-bit samples.
	/// </summary>
	/// <param name="soundData">Raw PCM sound data as a span of bytes.</param>
	/// <param name="sampleRate">Sample rate in Hz (e.g., 44100).</param>
	/// <param name="nbOfChannels">Number of audio channels (e.g., 1 for mono, 2 for stereo).</param>
	/// <param name="bitsPerSample">Bits per sample (e.g., 8 or 16).</param>
	public RawAudioSource(ReadOnlySpan<byte> soundData, int sampleRate, int nbOfChannels = 1, BitsPerSample bitsPerSample = BitsPerSample.Bit8)
		: this(soundData.ToArray(), sampleRate, nbOfChannels, bitsPerSample)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RawAudioSource"/> class with specified bits per sample.
	/// </summary>
	/// <param name="soundData">Raw PCM sound data as a byte array.</param>
	/// <param name="sampleRate">Sample rate in Hz (e.g., 44100).</param>
	/// <param name="nbOfChannels">Number of audio channels (e.g., 1 for mono, 2 for stereo).</param>
	/// <param name="bitsPerSample">Bits per sample (e.g., 8 or 16).</param>
	public RawAudioSource(byte[] soundData, int sampleRate, int nbOfChannels = 1, BitsPerSample bitsPerSample = BitsPerSample.Bit8)
	{
		ArgumentNullException.ThrowIfNull(soundData);

		if (sampleRate <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be positive.");
		}

		if (nbOfChannels <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(nbOfChannels), "Number of channels must be positive.");
		}

		if (bitsPerSample != BitsPerSample.Bit8 && bitsPerSample != BitsPerSample.Bit16)
		{
			throw new NotSupportedException($"Unsupported BitsPerSample: {bitsPerSample}. Only 8-bit and 16-bit are supported.");
		}

		// Validate sound data length based on bits per sample and number of channels
		int bytesPerSample = bitsPerSample == BitsPerSample.Bit8 ? 1 : 2;
		if (soundData.Length % (nbOfChannels * bytesPerSample) != 0)
		{
			throw new ArgumentException("Sound data length is not aligned with the specified number of channels and bits per sample.", nameof(soundData));
		}

		this.soundData = soundData;
		this.sampleRate = sampleRate;
		this.nbOfChannels = nbOfChannels;
		this.bitsPerSample = bitsPerSample;
	}

	/// <summary>
	/// Gets a stream providing access to the audio data with WAV header.
	/// </summary>
	/// <returns>A memory stream containing the complete WAV file data.</returns>
	public Stream GetAudioStream()
	{
		return new MemoryStream(Bytes, false);
	}

	/// <summary>
	/// Gets the complete WAV file data, including the header.
	/// </summary>
	public byte[] Bytes
	{
		get
		{
			withHeader ??= BuildWavFile();

			return withHeader;
		}
	}

	/// <summary>
	/// Constructs the WAV file by prepending the appropriate header to the raw PCM data.
	/// </summary>
	/// <returns>Byte array containing the complete WAV file.</returns>
	byte[] BuildWavFile()
	{
		int dataSize = soundData.Length;
		int bytesPerSample = this.bitsPerSample == BitsPerSample.Bit8 ? 1 : 2;
		int byteRate = sampleRate * nbOfChannels * bytesPerSample;
		short blockAlign = (short)(nbOfChannels * bytesPerSample);
		short audioFormat = 1; // PCM
		short bitsPerSample = (short)this.bitsPerSample;
		int fileSize = dataSize + 44 - 8; // Total file size minus "RIFF" and size field itself
		byte[] wavHeader = new byte[44];

		// RIFF header
		Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, wavHeader, 0, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(fileSize), 0, wavHeader, 4, 4);
		Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, wavHeader, 8, 4);

		Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, wavHeader, 12, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(16), 0, wavHeader, 16, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(audioFormat), 0, wavHeader, 20, 2);
		Buffer.BlockCopy(BitConverter.GetBytes((short)nbOfChannels), 0, wavHeader, 22, 2);
		Buffer.BlockCopy(BitConverter.GetBytes(sampleRate), 0, wavHeader, 24, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(byteRate), 0, wavHeader, 28, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(blockAlign), 0, wavHeader, 32, 2);
		Buffer.BlockCopy(BitConverter.GetBytes(bitsPerSample), 0, wavHeader, 34, 2);

		Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("data"), 0, wavHeader, 36, 4);
		Buffer.BlockCopy(BitConverter.GetBytes(dataSize), 0, wavHeader, 40, 4);

		byte[] wavSoundData = new byte[wavHeader.Length + dataSize];
		Buffer.BlockCopy(wavHeader, 0, wavSoundData, 0, wavHeader.Length);
		Buffer.BlockCopy(soundData, 0, wavSoundData, wavHeader.Length, dataSize);

		return wavSoundData;
	}
}
