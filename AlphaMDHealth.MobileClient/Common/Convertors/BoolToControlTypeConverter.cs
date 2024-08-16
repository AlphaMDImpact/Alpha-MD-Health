using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// represents Control type converter based on bool value of is selected
/// </summary>
public class BoolToControlTypeConverter : IValueConverter
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
        if (value != null && value is bool && (bool)value == true)
        {
            return FieldTypes.PrimaryAppSmallHVCenterBoldLabelControl;
        }
        return FieldTypes.PrimarySmallHVCenterLabelControl;
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
        if (value != null && value is FieldTypes && (FieldTypes)value == FieldTypes.PrimaryAppSmallHVCenterBoldLabelControl)
        {
            return true;
        }
        return false;
    }
}