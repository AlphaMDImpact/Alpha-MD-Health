using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class ComponentVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value != null)
        {
            return value.ToString() == AppFileExtensions.none.ToString() ? ((string)parameter) != string.Empty : ((string)parameter) == string.Empty;
        }
        else
        {
            return false;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // For futute implementation
        return null;
    }
}