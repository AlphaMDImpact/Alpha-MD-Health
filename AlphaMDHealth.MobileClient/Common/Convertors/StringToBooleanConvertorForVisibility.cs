using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// represents String To Boolean Convertor For Visibility
    /// </summary>
    public class StringToBooleanConvertorForVisibility : IValueConverter
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
            return !string.IsNullOrWhiteSpace((string)value);
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
}
