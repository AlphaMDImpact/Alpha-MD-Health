using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Chat View
/// </summary>
public class ChatView : ViewManager
{
    public event EventHandler<EventArgs> OnChatUpdate; 

    //private CustomAttachmentPopupPage _attachmentPopupPage;
    private readonly ChatDTO _chatData = new ChatDTO
    {
        Chat = new ChatModel(),
        ChatDetail = new ChatDetailModel(),
        SelectedUserID = App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0)
    };
    private ObservableCollectionExtension<CustomAttachmentModel> _messagesToBeDisplayed;

    private readonly AmhEntryControl _replyEditor;
    private readonly AmhImageControl _sendButton;
    private readonly AmhUploadControl _attachmentButton;
    //private readonly CustomCollectionView _collections;
    private readonly AmhListViewCell _listViewCell;
    private readonly AmhLabelControl _chatInstructionLabel;
    private readonly Grid _replyGrid;
    private readonly ContentView _status; //todo: PancakeView
    private readonly bool _isPatientPage;
    private long _selectedUserID;
    private string _selectedChatID;

    /// <summary>
    /// Instance used to pass resource data in Attachment popup page
    /// </summary>
    public BasePage BasePage { private set; get; }

    /// <summary>
    /// Patient readings list
    /// </summary>
    /// <param name="page">Instance of base page</param>
    /// <param name="parameters">View parameters</param>
    public ChatView(BasePage page, object parameters) : base(page, parameters)
    {
        _isPatientPage = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(BaseDTO.IsActive)));
        BasePage = page;
        BasePage.PageService = new ChatService(App._essentials);
        var padding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        _messagesToBeDisplayed = new ObservableCollectionExtension<CustomAttachmentModel>();
        //_collections = new CustomCollectionView
        //{
        //    ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
        //    ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
        //    ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
        //    ItemsSource = _messagesToBeDisplayed,
        //    Margin = new Thickness(0, padding, 0, 0),
        //    ItemTemplate = new ChatDataTemplateSelector
        //    {
        //        SentMessage = new DataTemplate(() =>
        //        {
        //            return new ChatSentMessageView(this);
        //        }),
        //        ReceivedMessage = new DataTemplate(() => { return new ChatReceivedMessageView(); })
        //    },
        //    SelectionMode = SelectionMode.None,
        //};
        _replyEditor = new AmhEntryControl(FieldTypes.MultiLineEntryControl)
        {
            ResourceKey = ResourceConstants.R_CAPTION_KEY,
            //EditorHeightRequest = EditorHeight.Chat,
            //IsUnderLine = false,
            //ShowHeader = false,
        };
        _sendButton = new AmhImageControl() 
        { 
            //Margin = new Thickness(0, 0, 0, padding),
            Icon= ImageConstants.I_CHAT_SEND_ICON_PNG,
        };
        //_attachmentButton = new SvgImageButtonView(ImageConstants.I_ADD_PNG, AppImageSize.ImageSizeS, AppImageSize.ImageSizeS) { Margin = new Thickness(0, 0, 0, padding) };
        _attachmentButton = new AmhUploadControl();
        _replyGrid = new Grid
        {
            Style = new OnIdiom<Style>
            {
                Tablet = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                Phone = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE]
            },
            VerticalOptions = LayoutOptions.EndAndExpand,
            ColumnSpacing = padding,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star }
            },
        };
        _replyGrid.Add(_attachmentButton, 0, 0);
        _replyGrid.Add(_replyEditor, 1, 0);
        _replyGrid.Add(_sendButton, 4, 0);
        //_chatInstructionLabel = new CustomLabelControl(LabelType.SecondryAppSmallLeft)
        //{
        //    LineBreakMode = LineBreakMode.WordWrap,
        //    TextColor = Color.FromArgb(StyleConstants.ACCENT_COLOR),
        //    HorizontalOptions = LayoutOptions.CenterAndExpand
        //};
        _status = new ContentView //todo: PancakeView
        {
            //Style = (Style)App.Current.Resources[StyleConstants.ST_INSTRUCTION_PANCAKE_STYLE],
            Margin = new Thickness(padding, 0, padding, padding),
            Content = _chatInstructionLabel
        };
        Grid mainGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
        };

        var seperator = new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE],
            Margin = new Thickness(-padding, padding)
        };
        //mainGrid.Add(_collections, 0, 0);
        mainGrid.Add(seperator, 0, 1);
        mainGrid.Add(_replyGrid, 0, 2);
        mainGrid.Add(_status, 0, 3);
        //mainGrid.Add(_replyEditor, 0, 2);
        Content = mainGrid;
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        AppHelper.ShowBusyIndicator = true;
        _chatData.Chat.ToID = _selectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        await (BasePage.PageService as ChatService).GetChatDetailsAsync(_chatData).ConfigureAwait(true);
        BasePage.PageData = (BasePage.PageService as ChatService).PageData;
        if (_chatData.ErrCode == ErrorCode.OK)
        {
            Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);
            await SetTitleViewAsync().ConfigureAwait(true);
            RenderUiBasedOnPermissions();
            _replyEditor.PageResources = BasePage.PageData;
            (App.Current as App).OnReceiveNotification -= OnReceiveNotification;
            (App.Current as App).OnReceiveNotification += OnReceiveNotification;
            if (GenericMethods.IsListNotEmpty(_chatData.ChatDetails))
            {
                _messagesToBeDisplayed = new ObservableCollectionExtension<CustomAttachmentModel>();
                //todo: _messagesToBeDisplayed.InsertRange(_chatData.ChatAttachments);
                //_collections.ItemsSource = _messagesToBeDisplayed;
                _chatData.Chat.ChatID = _chatData.Chats[0].ChatID;
                if (!_isPatientPage && MobileConstants.IsTablet)
                {
                    OnChatUpdate.Invoke(_selectedUserID, new EventArgs());
                }
                await ScrollToEndAsync().ConfigureAwait(true);
                await BackgroundSyncAsync().ConfigureAwait(true);
            }
        }
        else
        {
            //Content = new CustomMessageControl(false)
            //{
            //    ControlResourceKey = _chatData.ErrCode.ToString(),
            //    PageResources = BasePage.PageData
            //};
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Pan);
        MessagingCenter.Unsubscribe<object, string>(this, Constants.KEYBOARD_DIAPPEARS);
        _sendButton.OnValueChanged -= OnSendButtonClicked;
        _attachmentButton.OnValueChanged -= OnAttachmentButtonClicked;
        (App.Current as App).OnReceiveNotification -= OnReceiveNotification;
        await Task.CompletedTask;
    }

    private void RenderUiBasedOnPermissions()
    {
        //if (!BasePage.CheckFeaturePermissionByCode(Utility.AppPermissions.ChatDelete.ToString()))
        //{
        //    _chatData.ChatAttachments.ForEach(x => x.IsDeleteAllowed = false);
        //}
        //var addEditPermission = BasePage.CheckFeaturePermissionByCode(Utility.AppPermissions.ChatAddEdit.ToString());

        //_status.IsVisible = !(addEditPermission && _chatData.Chat.IsRelationExists);
        //_replyGrid.IsVisible = addEditPermission && _chatData.Chat.IsRelationExists;
        //if (!addEditPermission)
        //{
        //    _chatInstructionLabel.Text = BasePage.GetResourceValueByKey(ResourceConstants.R_DISABLE_CHAT_TEXT_KEY);
        //}
        //else if (!_chatData.Chat.IsRelationExists)
        //{
        //    _chatInstructionLabel.Text = BasePage.GetResourceValueByKey(ResourceConstants.R_RELATION_EXPIRED_KEY);
        //}
        //else
        //{
            RegisterEvents();
        //}
    }

    private void RegisterEvents()
    {
        // Detach event is called as in case of tablet the same view is updated without onappearing being invoked
        _sendButton.OnValueChanged -= OnSendButtonClicked;
        _sendButton.OnValueChanged += OnSendButtonClicked;
        _attachmentButton.OnValueChanged -= OnAttachmentButtonClicked;
        _attachmentButton.OnValueChanged += OnAttachmentButtonClicked;
        //MessagingCenter.Subscribe<object, string>(this, Constants.KEYBOARD_DIAPPEARS, (sender, eargs) =>
        //{
        //    if (_attachmentPopupPage == null && App._essentials.GetPreferenceValue(StorageConstants.PR_IS_DONE_CLICK_KEY, false))
        //    {
        //        OnSendButtonClicked(sender, new EventArgs());
        //        App._essentials.SetPreferenceValue(StorageConstants.PR_IS_DONE_CLICK_KEY, false);
        //    }
        //});
    }

    private async void OnReceiveNotification(object sender, SignalRNotificationEventArgs e)
    {
        if (e.NotificationMessageType == NotificationMessageType.NotificationChat.ToString() && (_chatData.Chat.ChatID == Guid.Empty || _chatData.Chat.ChatID == Guid.Parse(e.NotificationID)))
        {
            _chatData.Chat.ToID = _selectedUserID;
            await (BasePage.PageService as ChatService).GetChatDetailsAsync(_chatData).ConfigureAwait(true);
            if (_chatData.ErrCode == ErrorCode.OK)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    _messagesToBeDisplayed = new ObservableCollectionExtension<CustomAttachmentModel>();
                    //todo: _messagesToBeDisplayed.InsertRange(_chatData.ChatAttachments);
                    ////_collections.ItemsSource = _messagesToBeDisplayed;
                    ////_chatData.Chat.ChatID = _chatData.Chats[0].ChatID;
                    ////(ShellMasterPage.CurrentShell as ShellMasterPage).UpdateBadgeCount(nameof(ChatsPage), _chatData.BadgeCount);
                    await ScrollToEndAsync().ConfigureAwait(true);
                });
                await BackgroundSyncAsync().ConfigureAwait(true);
            }
        }
    }

    private async void OnAttachmentButtonClicked(object sender, EventArgs e)
    {
        //MaxFileUploadSize = BasePage.GetSettingsValueByKey(SettingsConstants.S_LARGE_IMAGE_RESOLUTION_KEY);
        //SupportedFileTypes = BasePage.GetSettingsValueByKey(SettingsConstants.S_UPLOAD_SUPPORTED_FILE_TYPE_KEY);
        //List<string> actions = Actionlist(FieldTypes.UploadControl);
        //await ImageSourceSelectionAsync(actions).ConfigureAwait(true);
        //if (!string.IsNullOrWhiteSpace(_base64String) && _base64String != ImageConstants.I_UPLOAD_ICON_PNG)
        //{
        //    if (GenericMethods.IsExtensionSupported(SupportedFileTypes, _uploadedFileExtension))
        //    {
        //        CustomAttachmentModel attachmentModel = new CustomAttachmentModel
        //        {
        //            AttachmentBase64 = GetBase64WithPrefix(),
        //            FileName = FileNameWithExtention,
        //            TextColor = StyleConstants.GENERIC_BACKGROUND_COLOR,
        //            DateColor = StyleConstants.GENERIC_BACKGROUND_COLOR,
        //            ControlResourceKey = ResourceConstants.R_CAPTION_KEY,
        //            FileType = _uploadedFileExtension.ToEnum<AppFileExtensions>()
        //        };
        //        _attachmentPopupPage = new CustomAttachmentPopupPage(attachmentModel, BasePage);
        //        _attachmentPopupPage.OnSendButtonClicked += OnAttachmentSendButtonClicked;
        //        //todo:await Navigation.PushPopupAsync(_attachmentPopupPage).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        await BasePage.DisplayMessagePopupAsync(
        //            BasePage.GetResourceValueByKey(ResourceConstants.R_SUPPORTED_UPLOAD_FILE_TYPE_KEY).Replace("{0}", SupportedFileTypes),
        //            BasePage.OnPupupActionClicked, true, false, true).ConfigureAwait(true);
        //    }
        //    _base64String = ImageConstants.I_UPLOAD_ICON_PNG;
        //}
    }

    private async void OnAttachmentSendButtonClicked(object sender, EventArgs e)
    {
        CustomAttachmentModel attachmentData = sender as CustomAttachmentModel;
        _chatData.ChatDetail.ChatText = attachmentData.Text;
        _chatData.ChatDetail.AttachmentBase64 = attachmentData.AttachmentBase64;
        _chatData.ChatDetail.FileName = attachmentData.FileName;
        _chatData.ChatDetail.FileType = attachmentData.FileType;
        if (attachmentData.FileType == AppFileExtensions.jpeg || attachmentData.FileType == AppFileExtensions.jpg || attachmentData.FileType == AppFileExtensions.png)
        {
            var maxUploadSize = BasePage.GetSettingsValueByKey(SettingsConstants.S_IMAGE_COMPRESSED_RESOLUTION_KEY).Split('x');
            //todo:  _chatData.ChatDetail.CompressedAttachment = Convert.ToBase64String(DependencyService.Get<ICompression>().ResetImage(Convert.FromBase64String(attachmentData.AttachmentBase64.Split(Constants.SYMBOL_COMMA)[1]),
            //Convert.ToSingle(maxUploadSize[0], CultureInfo.InvariantCulture), Convert.ToSingle(maxUploadSize[1], CultureInfo.InvariantCulture)));
        }
        //_attachmentPopupPage = null;
        await SaveChatDetailAsync(_chatData.ChatDetail).ConfigureAwait(true);
    }

    private async void OnSendButtonClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_replyEditor.Value))
        {
            _chatData.ChatDetail.ChatText = _replyEditor.Value;
            _replyEditor.Value = string.Empty;
            await SaveChatDetailAsync(_chatData.ChatDetail).ConfigureAwait(true);
        }
    }

    public async Task SaveChatDetailAsync(ChatDetailModel chatDetail)
    {
        if (!string.IsNullOrWhiteSpace(chatDetail.ChatText) || !string.IsNullOrWhiteSpace(chatDetail.AttachmentBase64))
        {
            _chatData.ChatDetail = chatDetail;
            _chatData.ChatDetail.ChatDetailID = Guid.Empty;
            _chatData.ChatDetail.IsActive = true;
            _chatData.ChatDetail.FromID = App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
            _chatData.ChatDetail.ToID = _selectedUserID;
            await SaveAndRefreshChatDetailsAsync().ConfigureAwait(true);
            await ScrollToEndAsync().ConfigureAwait(true);
            if (!_isPatientPage)
            {
                OnChatUpdate?.Invoke(_selectedUserID, new EventArgs());
            }
        }
    }

    private async Task SaveAndRefreshChatDetailsAsync()
    {
        ChatDTO chatData = new ChatDTO { ChatDetail = _chatData.ChatDetail, Chat = _chatData.Chat };
        await (BasePage.PageService as ChatService).SaveChatDetailAsync(chatData).ConfigureAwait(true);
        if (chatData.ErrCode == ErrorCode.OK)
        {
            if (chatData.ChatDetail.IsActive)
            {
                chatData.ChatDetail.IsSent = true;
                //chatData.ChatDetail.AddedOnDate = string.Concat(BasePage.GetResourceValueByKey(ResourceConstants.R_TODAY_TEXT_KEY), Constants.STRING_SPACE, GenericMethods.GetDateTimeBasedOnCulture(App._essentials.ConvertToLocalTime(chatData.ChatDetail.AddedOn), DateTimeType.Time, string.Empty, string.Empty, string.Empty));
                CustomAttachmentModel newChat = new CustomAttachmentModel
                {
                    ID = chatData.ChatDetail.ChatDetailID,
                    Text = chatData.ChatDetail.ChatText,
                    AddedOnDate = chatData.ChatDetail.AddedOnDate,
                    AttachmentBase64 = chatData.ChatDetail.AttachmentBase64,
                    FileName = chatData.ChatDetail.FileName,
                    IsActive = chatData.ChatDetail.IsActive,
                    FileType = chatData.ChatDetail.FileType,
                    IsSent = chatData.ChatDetail.IsSent,
                    IsRelationNotExpired = true,
                    TextColor = chatData.ChatDetail.IsSent ? StyleConstants.GENERIC_BACKGROUND_COLOR : StyleConstants.PRIMARY_TEXT_COLOR,
                    DateColor = chatData.ChatDetail.IsSent ? StyleConstants.GENERIC_BACKGROUND_COLOR : StyleConstants.SECONDARY_TEXT_COLOR,
                    //IsDeleteAllowed = BasePage.CheckFeaturePermissionByCode(Utility.AppPermissions.ChatDelete.ToString())
                };
                //todo:(BasePage.PageService as ChatService).GetFileIcon(newChat, chatData.ChatDetail);
                _messagesToBeDisplayed.Add(newChat);
                //_collections.ItemsSource = _messagesToBeDisplayed;
                _chatData.ChatDetails.Add(chatData.ChatDetail);
            }
            _chatData.ChatDetail = new ChatDetailModel();
            _replyEditor.Value = string.Empty;
            await BackgroundSyncAsync().ConfigureAwait(true);
        }
    }

    internal async Task SetMainGridSizeAsync()
    {
        HeightRequest = 200;
        if (_messagesToBeDisplayed.Count > 0)
        {
            await ScrollToEndAsync().ConfigureAwait(true);
        }
    }

    public async Task DeleteChatDetailAsync(CustomAttachmentModel chat)
    {
        _selectedChatID = Convert.ToString(chat.ID, CultureInfo.InvariantCulture);
        await BasePage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, OnMessgeViewActionClicked, true, true, false).ConfigureAwait(true);
    }

    private async Task DeleteAndSaveChatDetailAsync(string chatDetailID)
    {
        CustomAttachmentModel chat = _messagesToBeDisplayed.FirstOrDefault(x => x.ID == Guid.Parse(chatDetailID));
        chat.IsActive = false;
        chat.IsDeleteAllowed = false;
        _chatData.ChatDetail = _chatData.ChatDetails.FirstOrDefault(x => x.ChatDetailID == chat.ID);
        _chatData.ChatDetail.IsActive = chat.IsActive;
        _chatData.ChatDetail.IsSynced = false;
        _chatData.ChatDetail.FromID = App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0);
        _chatData.ChatDetail.ToID = _selectedUserID;
        await SaveAndRefreshChatDetailsAsync().ConfigureAwait(true);
        //chat.Text = BasePage.GetResourceValueByKey(ResourceConstants.R_DELETED_ITEM_KEY);
        chat.FileType = AppFileExtensions.none;
        await Task.Delay(GenericMethods.GetPlatformSpecificValue(100, 0, 0)).ConfigureAwait(false);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var index = _messagesToBeDisplayed.IndexOf(chat);
            _messagesToBeDisplayed.Remove(chat);
            _messagesToBeDisplayed.Insert(index, chat);
            //_collections.ItemsSource = _messagesToBeDisplayed;
            ScrollToItemAtIndex(index);
            if (!_isPatientPage && DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                OnChatUpdate.Invoke(_selectedUserID, new EventArgs());
            }
        });
    }

    private async void OnMessgeViewActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 0: break;
            case 1:
                BasePage.OnClosePupupAction(sender, e);
                await DeleteAndSaveChatDetailAsync(_selectedChatID).ConfigureAwait(true);
                break;
            case 2:
                BasePage.OnClosePupupAction(sender, e);
                break;
            default:// to do
                break;
        }
        _selectedChatID = string.Empty;
    }

    private async Task SetTitleViewAsync()
    {
        MenuView titleView = new MenuView(MenuLocation.Header, _chatData.Chat.FirstName, false);
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            //await BasePage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
        }
        else
        {
            //await BasePage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
        }
        //(ShellMasterPage.CurrentShell as ShellMasterPage).UpdateBadgeCount(nameof(ChatsPage), _chatData.BadgeCount);
    }

    private async Task ScrollToEndAsync()
    {
        await Task.Delay(GenericMethods.GetPlatformSpecificValue(500, 10, 0)).ConfigureAwait(true);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            //_collections.ScrollTo(_messagesToBeDisplayed.Last(), position: ScrollToPosition.End, animate: false);
            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    await Task.Delay(5).ConfigureAwait(true);
            //    _collections.ScrollTo(_messagesToBeDisplayed.Last(), position: ScrollToPosition.End, animate: false);
            //}
        });
    }

    private void ScrollToItemAtIndex(int index)
    {
        ///Use this when reassigning single element (delete chat and/or downloading attchment)
        MainThread.BeginInvokeOnMainThread(() =>
        {
            //if (Device.RuntimePlatform == Device.Android && index != _collections.LastVisibleItemIndex)
            //{
            //    _collections.ScrollTo(index, position: ScrollToPosition.Center, animate: false);
            //}
            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    _collections.ScrollTo(index, position: ScrollToPosition.Center, animate: false);
            //}
        });
    }

    private async Task BackgroundSyncAsync()
    {
        await BasePage.SyncDataWithServerAsync(Pages.ChatPage, false, default).ConfigureAwait(true);
    }

    //protected override void DeleteUploads()
    //{
    //    // For Further implementation
    //}
}