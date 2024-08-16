using System.Globalization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Error Code To String Covertor
    /// </summary>
    public class ErrorCodeToStringCovertor : IValueConverter
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
            return value != null && (ErrorCode)value == ErrorCode.OK ?
                  //((Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR]).ToHex() :
                  Colors.Transparent.ToHex():
                  (StyleConstants.ERROR_BACKGROUND_COLOR);
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
