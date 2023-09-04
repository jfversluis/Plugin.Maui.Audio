using System.Globalization;
using System.Text;

namespace AudioPlayerSample.Converters;

public class SecondsToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not double doubleValue)
		{
			return value;
		}

		StringBuilder formatBuilder = new();
		var timeSpan = TimeSpan.FromSeconds(doubleValue);

		if (timeSpan.Hours > 0)
		{
			formatBuilder.Append(@"hh\:");
		}

		formatBuilder.Append(@"mm\:ss");

		return timeSpan.ToString(formatBuilder.ToString());
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}