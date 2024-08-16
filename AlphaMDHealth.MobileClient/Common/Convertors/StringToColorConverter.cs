using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// represents Status To Background Color Convertor
/// </summary>
public class StringToColorConverter : IValueConverter
{
    /// <summary>
    /// implement this method to convert value to target type
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="targetType">target type for value</param>
    /// <param name="parameter">Parametres required for conversion</param>
    /// <param name="culture">culter for coversion</param>
    /// <returns>Returns converted value</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && !string.IsNullOrWhiteSpace((string)value) ? Color.FromArgb((string)value) : default;
    }

    /// <summary>
    /// Reverse Conversion
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="targetType">target type for value</param>
    /// <param name="parameter">Parametres required for conversion</param>
    /// <param name="culture">culter for coversion</param>
    /// <returns>Returns converted value</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
