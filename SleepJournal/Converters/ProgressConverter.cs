using System.Globalization;

namespace SleepJournal.Converters;

/// <summary>
/// Converts a rating value (1-10) to a progress value (0.0-1.0) for ProgressBar.
/// </summary>
public class ProgressConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            // Convert 0-10 scale to 0.0-1.0 progress
            return doubleValue / 10.0;
        }

        if (value is int intValue)
        {
            return intValue / 10.0;
        }

        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
