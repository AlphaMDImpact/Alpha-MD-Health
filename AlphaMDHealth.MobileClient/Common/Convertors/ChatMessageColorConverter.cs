using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// represents Chat Message Color Converter
    /// </summary>
    public class ChatMessageColorConverter : IValueConverter
    {
        /// <summary>
        /// implement this method to convert value to target type
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="targetType">target type for value</param>
        /// <param name="parameter">Parametres required for conversion</param>
        /// <param name="culture">culter for coversion</param>
        /// <returns>Returns converted value</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string parameterType = parameter.ToString();
                switch (parameterType)
                {
                    case nameof(CustomAttachmentModel.Text):
                        return (bool)value ? (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR];
                    case nameof(CustomAttachmentModel.AddedOnDate):
                        return (bool)value ? (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR] : (Color)Application.Current.Resources[StyleConstants.ST_SECONDARY_TEXT_COLOR];
                    default:
                        return Colors.Transparent;
                }
            }
            return Colors.Transparent;
        }

        /// <summary>
        /// Reverse Conversion
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="targetType">target type for value</param>
        /// <param name="parameter">Parametres required for conversion</param>
        /// <param name="culture">culter for coversion</param>
        /// <returns>Returns converted value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // For futute implementation
            return null;
        }
    }
}
