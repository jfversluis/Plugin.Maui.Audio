using System.Globalization;

namespace Plugin.Maui.Audio.Sample.Converters;

public class DecibelToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not double doubleValue)
		{
			return value;
		}

		return $"{value:N0} dBFS";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}