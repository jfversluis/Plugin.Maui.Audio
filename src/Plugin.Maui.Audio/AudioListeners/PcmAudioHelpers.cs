namespace Plugin.Maui.Audio.AudioListeners;

public static class PcmAudioHelpers
{
	/// <summary>
	/// RMS to decibel value (negative decibel value, also named dBFS)
	/// </summary>
	/// <param name="rms"></param>
	/// <returns></returns>
	public static double RmsToDecibel(double rms)
	{
		double decibel = 20 * Math.Log10(Math.Abs(rms));
		return decibel;
	}

	/// <summary>
	/// Decibel to (positive) RMS value
	/// </summary>
	/// <param name="decibel">negative decibel value (also named dBFS)</param>
	/// <returns></returns>
	public static double DecibelToAbsoluteRms(double decibel)
	{
		var rms = Math.Pow(10.0, decibel / 20.0);
		return rms;
	}

	/// <summary>
	/// RMS = Root Mean Square
	/// </summary>
	/// <param name="values"></param>
	/// <param name="bitDepth"></param>
	/// <returns></returns>
	public static double CalculateRms(int[] values, BitDepth bitDepth)
	{
		var valueCount = values.Length;
		var sampleDevider = bitDepth switch
		{
			BitDepth.Pcm16bit => short.MaxValue,
			BitDepth.Pcm8bit => byte.MaxValue,
			_ => 1
		};

		double square = 0;
		for (var i = 0; i < valueCount; i++)
		{
			double value = (double)values[i] / sampleDevider;
			square += value * value;
		}

		double mean = square / (float)valueCount;

		var root = (float)Math.Sqrt(mean);

		return root;
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