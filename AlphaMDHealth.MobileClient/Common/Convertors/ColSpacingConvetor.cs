using System.Globalization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Column spacing converter
    /// </summary>
    public class ColSpacingConvetor : IValueConverter
    {
        /// <summary>
        /// Converts to column spacing
        /// </summary>
        /// <param name="value">Value to be converted</param>
        /// <param name="targetType">Type of return</param>
        /// <param name="parameter"></param>
        /// <param name="culture">Culture information</param>
        /// <returns>returns the converted value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                string param = parameter.ToString();
                if (param.Equals("ShowUnreadBadge"))
                {
                    return (bool)value ? 15 : 0;
                }
                else if (param.Equals("WidthRequest"))
                {
                    return (bool)value ? 8 : 0;
                }
                else
                {
                    if (param.Equals("RowSpanCell"))
                    {
                        return IsValueEmpty(value) ? 4 : 1;
                    }
                }
            }
            return IsValueEmpty(value) ? 0 : (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
        }

        /// <summary>
        /// Convert value back from targetType by using parameter and culture
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The type to which to convert the value</param>
        /// <param name="parameter">A parameter to use during the conversion</param>
        /// <param name="culture">The culture to use during the conversion</param>
        /// <returns>converted value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static bool IsValueEmpty(object value)
        {
            return value == null || string.IsNullOrWhiteSpace((string)value);
        }
    }
}
