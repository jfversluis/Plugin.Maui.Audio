using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Plugin.Maui.Audio;

/// <summary>
/// Quality of the compression algorithm applied where applicable (e.g. AAC)
/// </summary>
public enum CompressionQuality
{
	Min,
	Low,
	Medium,
	High,
	Max
}
