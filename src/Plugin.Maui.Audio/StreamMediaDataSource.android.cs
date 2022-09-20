using Android.Media;

namespace Plugin.Maui.Audio;

class StreamMediaDataSource : MediaDataSource
{
	System.IO.Stream data;
	public StreamMediaDataSource(System.IO.Stream Data)
	{
		data = Data;
	}
	public override long Size
	{
		get
		{
			return data.Length;
		}
	}
	public override int ReadAt(long position, byte[] buffer, int offset, int size)
	{
		data.Seek(position, System.IO.SeekOrigin.Begin);
		return data.Read(buffer, offset, size);
	}
	public override void Close()
	{
		if (data != null)
		{
			data.Dispose();
			data = null;
		}
	}
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (data != null)
		{
			data.Dispose();
			data = null;
		}
	}
}