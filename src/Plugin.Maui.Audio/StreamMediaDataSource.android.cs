namespace Plugin.Maui.Audio;

class StreamMediaDataSource : Android.Media.MediaDataSource
{
	Stream data;

	public StreamMediaDataSource(Stream data)
	{
		this.data = data;
	}

	public override long Size => data.Length;

	public override int ReadAt(long position, byte[]? buffer, int offset, int size)
	{
		ArgumentNullException.ThrowIfNull(buffer);

		if (data.CanSeek)
		{
			data.Seek(position, SeekOrigin.Begin);
		}

		return data.Read(buffer, offset, size);
	}

	public override void Close()
	{
		data.Dispose();
		data = Stream.Null;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		data.Dispose();
		data = Stream.Null;
	}
}