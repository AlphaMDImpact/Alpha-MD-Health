using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class ChatReceivedMessageView : ChatMessageView
{
    public ChatReceivedMessageView()
    {
        _messageContextView.Style = (Style)App.Current.Resources[StyleConstants.ST_RECEIVED_MESSAGE_STYLE];
        Content = _mainGrid;
    }
}