using AlphaMDHealth.Model;

namespace AlphaMDHealth.MobileClient;

public class ChatDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate SentMessage { get; set; }
    public DataTemplate ReceivedMessage { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        CustomAttachmentModel customAttachment = (CustomAttachmentModel)item;
        return customAttachment.IsSent ? SentMessage : ReceivedMessage;
    }
}