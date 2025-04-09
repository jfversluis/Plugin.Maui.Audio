using System.Buffers.Binary;

namespace Plugin.Maui.Audio.AudioListeners;

/// <summary>
/// Provides utility methods for working with PCM audio data.
/// </summary>
public static class PcmAudioHelpers
{
	/// <summary>
	/// RMS to decibel value (negative decibel value, also named dBFS)
	/// </summary>
	/// <param name="rms">The RMS value to convert to decibel.</param>
	/// <returns>The given RMS value converted to the equivalent decibel value.</returns>
	public static double RmsToDecibel(double rms)
	{
		double decibel = 20 * Math.Log10(Math.Abs(rms));
		return decibel;
	}

	/// <summary>
	/// Decibel to (positive) RMS value.
	/// </summary>
	/// <param name="decibel">Negative decibel value (also named dBFS).</param>
	/// <returns>RMS (always positive; sign information not preserved)</returns>
	public static double DecibelToAbsoluteRms(double decibel)
	{
		var rms = Math.Pow(10.0, decibel / 20.0);
		return rms;
	}

	/// <summary>
	/// Calculate RMS (Root Mean Square) for each audio sample
	/// </summary>
	/// <param name="audioSamples"></param>
	/// <param name="bitDepth"></param>
	/// <returns>Calculated RMS values</returns>
	public static double CalculateRms(int[] audioSamples, BitDepth bitDepth)
	{
		var valueCount = audioSamples.Length;
		var sampleDevider = bitDepth switch
		{
			BitDepth.Pcm16bit => short.MaxValue,
			BitDepth.Pcm8bit => byte.MaxValue,
			_ => 1
		};

		double square = 0;
		for (var i = 0; i < valueCount; i++)
		{
			double value = (double)audioSamples[i] / sampleDevider;
			square += value * value;
		}

		double mean = square / (float)valueCount;
		var root = (float)Math.Sqrt(mean);
		return root;
	}

	/// <summary>
	/// Convert Samples to Samples per channel
	/// </summary>
	/// <param name="samples"></param>
	/// <param name="channels"></param>
	/// <returns>Array with the amount of channels, each containing its samples</returns>
	public static int[][] ConvertToSamplesPerChannel(int[] samples, ChannelType channels)
	{
		if (channels is ChannelType.Mono)
		{
			return new int[1][] { samples };
		}

		var samplesByChannel = new int[2][];
		samplesByChannel[0] = new int[samples.Length / 2];
		samplesByChannel[1] = new int[samples.Length / 2];

		for (var n = 0; n < samples.Length / 2; n++)
		{
			samplesByChannel[0][n] = samples[n * 2];
			samplesByChannel[1][n] = samples[n * 2 + 1];
		}

		return samplesByChannel;
	}

	/// <summary>
	/// Convert raw PCM audio bytes to ordered audio samples.
	/// </summary>
	/// <param name="pcmAudio"></param>
	/// <param name="bitDepth"></param>
	/// <returns></returns>
	public static int[] ConvertRawPcmAudioBytesToOrderedAudioSamples(byte[] pcmAudio, BitDepth bitDepth)
	{
		var samples = new List<int>();
		var bytesPerSample = (uint)bitDepth / 8;

		for (var n = 0; n < pcmAudio.Length / bytesPerSample; n++)
		{
			int sample = 0;

			if (bytesPerSample == 1)
			{
				var sampleBytes = new[] { pcmAudio[n] };
				sample = sampleBytes[0] - 128;
			}
			else if (bytesPerSample == 2)
			{
				var sampleBytes = new byte[2] { pcmAudio[n * bytesPerSample], pcmAudio[n * bytesPerSample + 1] };
				sample = BinaryPrimitives.ReadInt16LittleEndian(sampleBytes);
			}
			else if (bytesPerSample == 4)
			{
				var sampleBytes = new byte[4]
				{
					pcmAudio[n * bytesPerSample],
					pcmAudio[n * bytesPerSample + 1],
					pcmAudio[n * bytesPerSample + 2],
					pcmAudio[n * bytesPerSample + 3]
				};
				sample = BinaryPrimitives.ReadInt32LittleEndian(sampleBytes);
			}

			samples.Add(sample);
		}

		return samples.ToArray();
	}

	/// <summary>
	/// Convert ordered audio samples to raw PCM audio bytes.
	/// </summary>
	/// <param name="samples"></param>
	/// <param name="bitDepth"></param>
	/// <returns></returns>
	/// <exception cref="Exception">when conversion fails</exception>
	public static byte[] ConvertOrderedAudioSamplesToRawPcmAudioBytes(int[] samples, BitDepth bitDepth)
	{
		var bytesPerSample = (uint)bitDepth / 8;
		var totalBytes = samples.Length * bytesPerSample;
		var pcmAudio = new byte[totalBytes];
		var pcmAudioSpan = new Span<byte>(pcmAudio);

		for (var n = 0; n < samples.Length; n++)
		{
			if (bytesPerSample == 1)
			{
				var sample = Convert.ToByte(samples[n]);
				var pcmValue = sample + 128;
				if (pcmValue > byte.MaxValue)
				{
					throw new Exception($"Failed to write sample '{samples[n]}' as little endian");
				}

				pcmAudioSpan[n] = (byte)pcmValue;
			}
			else if (bytesPerSample == 2)
			{
				if (!BinaryPrimitives.TryWriteInt16LittleEndian(pcmAudioSpan.Slice(n * 2, 2), (short)samples[n]))
				{
					throw new Exception($"Failed to write sample '{samples[n]}' as little endian");
				}
			}
			else if (bytesPerSample == 4)
			{
				if (!BinaryPrimitives.TryWriteInt32LittleEndian(pcmAudioSpan.Slice(n * 4, 4), samples[n]))
				{
					throw new Exception($"Failed to write sample '{samples[n]}' as little endian");
				}
			}
		}

		return pcmAudio;
	}

	public static byte[] CreateWavFileHeader(long audioLength, long sampleRate, int channels, int bitDepth)
	{
		long dataLength = audioLength + 36;
		int blockAlign = (int)(channels * (bitDepth / 8));
		long byteRate = sampleRate * blockAlign;

		byte[] header = new byte[44];

		header[0] = Convert.ToByte('R'); // RIFF/WAVE header
		header[1] = Convert.ToByte('I'); // (byte)'I'
		header[2] = Convert.ToByte('F');
		header[3] = Convert.ToByte('F');
		header[4] = (byte)(dataLength & 0xff);
		header[5] = (byte)((dataLength >> 8) & 0xff);
		header[6] = (byte)((dataLength >> 16) & 0xff);
		header[7] = (byte)((dataLength >> 24) & 0xff);
		header[8] = Convert.ToByte('W');
		header[9] = Convert.ToByte('A');
		header[10] = Convert.ToByte('V');
		header[11] = Convert.ToByte('E');
		header[12] = Convert.ToByte('f'); // fmt chunk
		header[13] = Convert.ToByte('m');
		header[14] = Convert.ToByte('t');
		header[15] = (byte)' ';
		header[16] = 16; // 4 bytes - size of fmt chunk
		header[17] = 0;
		header[18] = 0;
		header[19] = 0;
		header[20] = 1; // format = 1
		header[21] = 0;
		header[22] = Convert.ToByte(channels);
		header[23] = 0;
		header[24] = (byte)(sampleRate & 0xff);
		header[25] = (byte)((sampleRate >> 8) & 0xff);
		header[26] = (byte)((sampleRate >> 16) & 0xff);
		header[27] = (byte)((sampleRate >> 24) & 0xff);
		header[28] = (byte)(byteRate & 0xff);
		header[29] = (byte)((byteRate >> 8) & 0xff);
		header[30] = (byte)((byteRate >> 16) & 0xff);
		header[31] = (byte)((byteRate >> 24) & 0xff);
		header[32] = (byte)(blockAlign); // block align
		header[33] = 0;
		header[34] = Convert.ToByte(bitDepth); // bits per sample
		header[35] = 0;
		header[36] = Convert.ToByte('d');
		header[37] = Convert.ToByte('a');
		header[38] = Convert.ToByte('t');
		header[39] = Convert.ToByte('a');
		header[40] = (byte)(audioLength & 0xff);
		header[41] = (byte)((audioLength >> 8) & 0xff);
		header[42] = (byte)((audioLength >> 16) & 0xff);
		header[43] = (byte)((audioLength >> 24) & 0xff);

		return header;
	}
}
