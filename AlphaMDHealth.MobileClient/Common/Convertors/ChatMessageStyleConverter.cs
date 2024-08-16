using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class ChatMessageStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value != null && (bool)value ? (Style)App.Current.Resources[StyleConstants.ST_SENT_MESSAGE_STYLE] : (Style)App.Current.Resources[StyleConstants.ST_RECEIVED_MESSAGE_STYLE];
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // For future implementation
        return null;
    }
}
