namespace Plugin.Maui.Audio;

class StreamMediaDataSource : Android.Media.MediaDataSource
{
	Stream? data;

	public StreamMediaDataSource(Stream data)
	{
		this.data = data;
	}

	public override long Size => data?.Length ?? 0;
	public override int ReadAt(long position, byte[]? buffer, int offset, int size)
	{
		ArgumentNullException.ThrowIfNull(buffer);

		data?.Seek(position, SeekOrigin.Begin);

		return data?.Read(buffer, offset, size) ?? 0;
	}
	public override void Close()
	{
		data?.Dispose();
		data = null;
	}
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		data?.Dispose();
		data = null;
	}
}