using System.Globalization;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// bool to font attribut conversion
    /// </summary>
    public class BoolToFontAttributeConvertor : IValueConverter
    {
        /// <summary>
        ///  bool to font attribut conversion
        /// </summary>
        /// <param name="value">bool value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">input parameter</param>
        /// <param name="culture">culture</param>
        /// <returns>Font attribute</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter.ToString() == Constants.CHAT_FONT_ATTRIBUT)
            {
                if (value != null)
                {
                    return (bool)value ? FontAttributes.None : FontAttributes.Italic;
                }
                return FontAttributes.None;
            }
            else
            {
                return value != null && (bool)value ? FontAttributes.Bold : FontAttributes.None;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
