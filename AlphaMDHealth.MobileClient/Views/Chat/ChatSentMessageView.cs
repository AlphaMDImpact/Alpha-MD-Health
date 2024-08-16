using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class ChatSentMessageView : ChatMessageView
{
    private readonly ChatView _page;
    private static SwipeViewExt PreviousItem { get; set; }

    public ChatSentMessageView(ChatView page)
    {
        _page = page;
        _messageContextView.Style = (Style)App.Current.Resources[StyleConstants.ST_SENT_MESSAGE_STYLE];

        SwipeItem deleteIconSwiped = new SwipeItem
        {
            //todo: BackgroundColor = Color.Transparent,
            IconImageSource = ImageSource.FromResource(AppStyles.NameSpaceImage + ImageConstants.I_CHAT_DELETE_ICON_PNG)//todo: , LibGenericMethods.GetPlatformSpecificValue(25, new OnIdiom<int> { Phone = 25, Tablet = 50 }, 0), LibGenericMethods.GetPlatformSpecificValue(25, new OnIdiom<int> { Phone = 25, Tablet = 50 }, 0), Color.FromArgb("#1C255C"))
        };

        deleteIconSwiped.Invoked += OnLongPressAction;
        SwipeItems items = new SwipeItems
        {
            SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close,
            Mode = SwipeMode.Reveal
        };
        items.Add(deleteIconSwiped);
        SwipeViewExt swipeView = new SwipeViewExt
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            //todo: BackgroundColor = Color.Transparent,
            Content = _mainGrid
        };
        if ((FlowDirection)App.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] == FlowDirection.RightToLeft)
        {
            swipeView.LeftItems = items;
        }
        else
        {
            swipeView.RightItems = items;
        }
        swipeView.CloseRequested += SwipeView_CloseRequested;
        swipeView.SwipeEnded += SwipeView_SwipeEnded;
        swipeView.SwipeStarted += SwipeView_SwipeStarted;
        TapGestureRecognizer swiptabGesture = new TapGestureRecognizer();
        swiptabGesture.Tapped += SwiptabGesture_Tapped;
        swipeView.GestureRecognizers.Add(swiptabGesture);
        swipeView.SetBinding(SwipeViewExt.IsEnabledProperty, new Binding(nameof(CustomAttachmentModel.IsDeleteAllowed)));

        CustomContentView contentView = new CustomContentView
        {
            Content = _mainGrid
        };
        contentView.LongPressed += OnLongPressAction;
        contentView.Pressed += ContentView_Pressed;

        Content = contentView;
    }

    private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        if (PreviousItem != null)
        {
            PreviousItem.Close();
        }
    }

    private void SwiptabGesture_Tapped(object sender, EventArgs e)
    {
        if (!(sender is SwipeViewExt sv))
        {
            return;
        }
        if (sv.IsOpened && PreviousItem != null)
        {
            //todo: PreviousItem.BackgroundColor = Color.Transparent;
            PreviousItem = null;
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is CustomAttachmentModel && PreviousItem != null)
        {
            PreviousItem.Close();
        }
    }

    private void SwipeView_CloseRequested(object sender, EventArgs e)
    {
        //todo:  PreviousItem.BackgroundColor = Color.Transparent;
        PreviousItem = null;
    }

    private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        var item = (sender as SwipeViewExt);

        if (PreviousItem != null)
        {
            PreviousItem.Close();
        }
        PreviousItem = item;
        PreviousItem.BackgroundColor = Color.FromArgb(StyleConstants.TERTIARY_APP_COLOR);
    }

    private void OnLongPressAction(object sender, EventArgs e)
    {
        var item = (sender as CustomContentView).BindingContext as CustomAttachmentModel;
        if (item.IsActive && item.IsRelationNotExpired && item.IsSent)
        {
            AppHelper.ShowBusyIndicator = true;
            _ = _page.DeleteChatDetailAsync(item);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    private void ContentView_Pressed(object sender, EventArgs e)
    {
        //nothing to do
    }
}