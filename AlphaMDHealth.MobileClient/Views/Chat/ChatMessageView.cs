using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class ChatMessageView : ContentView
{
    protected Grid _mainGrid;
    protected Frame _messageContextView;

    /// <summary>
    /// Use to create chat view cell
    /// </summary>
    /// <param name="page">Instance of chat view</param>
    public ChatMessageView()
    {

        CustomLabelControl conversationText = new CustomLabelControl(LabelType.PrimarySmallLeft)
        {
            LineBreakMode = LineBreakMode.WordWrap,
        };
        conversationText.SetBinding(CustomLabelControl.TextProperty, nameof(CustomAttachmentModel.Text));
        conversationText.SetBinding(CustomLabelControl.TextColorProperty, new Binding(nameof(CustomAttachmentModel.TextColor)));
        conversationText.SetBinding(CustomLabelControl.FontAttributesProperty, new Binding(nameof(CustomAttachmentModel.IsActive), converter: new BoolToFontAttributeConvertor(), converterParameter: Constants.CHAT_FONT_ATTRIBUT));
        conversationText.SetBinding(CustomLabelControl.IsVisibleProperty, new Binding(nameof(CustomAttachmentModel.FileType), converter: new ComponentVisibilityConverter(), converterParameter: nameof(CustomAttachmentModel.Text)));

        CustomLabelControl messageDateTime = new CustomLabelControl(LabelType.SecondryExtraSmallRight);
        messageDateTime.SetBinding(CustomLabelControl.TextProperty, nameof(CustomAttachmentModel.AddedOnDate));
        messageDateTime.SetBinding(CustomLabelControl.TextColorProperty, new Binding(nameof(CustomAttachmentModel.DateColor)));
        messageDateTime.SetBinding(CustomLabelControl.IsVisibleProperty, new Binding(nameof(CustomAttachmentModel.FileType), converter: new ComponentVisibilityConverter(), converterParameter: nameof(CustomAttachmentModel.AddedOnDate)));

        ChatAttachmentControl chatAttachmentControl = new ChatAttachmentControl { IsPreview = true };
        chatAttachmentControl.SetBinding(ChatAttachmentControl.ValueProperty, new Binding("."));
        chatAttachmentControl.SetBinding(ChatAttachmentControl.IsVisibleProperty, new Binding(nameof(CustomAttachmentModel.FileType), converter: new ComponentVisibilityConverter(), converterParameter: string.Empty));
        TapGestureRecognizer chatAttachmentTap = new TapGestureRecognizer();
        chatAttachmentTap.Tapped += ChatAttachmentTap_Tapped;
        chatAttachmentControl.GestureRecognizers.Add(chatAttachmentTap);

        Grid conversationGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowSpacing = Constants.CONTENT_PADDING,
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto)},
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star)},
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        conversationGrid.Add(conversationText, 0, 0);
        conversationGrid.Add(messageDateTime, 0, 1);
        conversationGrid.Add(chatAttachmentControl, 0, 0);
        Grid.SetRowSpan(chatAttachmentControl, 2);

        _messageContextView = new Frame
        {
            Content = conversationGrid
        };

        _mainGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            VerticalOptions = LayoutOptions.Center,
            ColumnDefinitions =
            {
                 new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        _mainGrid.Add(_messageContextView, 1, 0);
    }

    private void ChatAttachmentTap_Tapped(object sender, EventArgs e)
    {
        var item = sender as ChatAttachmentControl;
        if (item.BindingContext is CustomAttachmentModel chatDetail && !string.IsNullOrWhiteSpace(chatDetail.FileName))
        {
            string filename;
            if (Uri.IsWellFormedUriString(chatDetail.FileName, UriKind.Absolute))
            {
                Uri uri = new Uri(chatDetail.FileName);
                filename = System.IO.Path.GetFileName(uri.Segments.Last());
            }
            else
            {
                filename = chatDetail.FileName;
            }
            if (!string.IsNullOrWhiteSpace(chatDetail.AttachmentBase64))
            {
                item.ShowAttachment(chatDetail.AttachmentBase64, filename);
            }
        }
    }
}