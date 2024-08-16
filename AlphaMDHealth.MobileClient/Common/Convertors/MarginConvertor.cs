using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// margin convertor
    /// </summary>
    public class MarginConvertor : IValueConverter
    {
        /// <summary>
        /// convert margin from bool
        /// </summary>
        /// <param name="value">bool value </param>
        /// <param name="targetType"></param>
        /// <param name="parameter">is show unread badge</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = parameter.ToString();
            if (!string.IsNullOrWhiteSpace(param))
            {
                Thickness leftMargin;
                leftMargin = (bool)value ? new Thickness(-5, 0, 0, 0) : new Thickness(-15, 0, 0, 0);
                return leftMargin;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
