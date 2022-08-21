using System.Globalization;

namespace AudioPlayerSample.Converters;

public class SecondsToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not double doubleValue)
		{
			return value;
		}

		return TimeSpan.FromSeconds(doubleValue).ToString(@"hh\:mm\:ss");
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}

