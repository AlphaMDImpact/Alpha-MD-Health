using System.Globalization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// represents Status To Background Color Convertor
    /// </summary>
    public class StatusToBackgroundColorConvertor : IValueConverter
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
            return value != null ? StyleConstants.PERCENT20_ALPHA_COLOR + ((string)value).Replace(Constants.SYMBOL_HASH, ' ').Trim() : ((Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR]).ToHex();
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
