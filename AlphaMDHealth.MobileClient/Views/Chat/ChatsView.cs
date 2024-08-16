using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Internal;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class ChatsView : ViewManager
{
    private readonly ChatDTO _chatData = new ChatDTO { Chat = new ChatModel() };
    private readonly Grid _mainLayout;
    //private readonly AmhMessageControl _emptyMessageView;
    private bool _isProviderList;
    //private ChatView _chatDetailView;
    private long _selectedUserID;
    private readonly CustomCellModel _customCellData;
    private AmhListViewControl<ChatModel> _chatsListView;


    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public ChatsView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new ChatService(App._essentials);
        if (Parameters?.Count > 0)
        {
            _chatData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _customCellData = new CustomCellModel
        {
            CellHeader = nameof(ChatModel.FirstName),
            //todo: CellLeftSourceIcon = nameof(ChatModel.LeftSourceIcon),
            CellLeftDefaultIcon = nameof(ChatModel.LeftDefaultIcon),
            CellDescription = nameof(ChatModel.LatestMessages),
            CellRightContentDescription = nameof(ChatModel.UnreadMessages),
            //todo: RightDesciptionStyle = (Style)Application.Current.Resources[StyleConstants.ST_BADGE_STYLE],
            ArrangeHorizontal = false
        };
        _mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            //ColumnDefinitions = CreateTabletViewColumn(IsDashboardView(_chatData.RecordCount) || DeviceInfo.Idiom == DeviceIdiom.Phone),
        };
        _chatsListView = new AmhListViewControl<ChatModel>(FieldTypes.OneRowListViewControl)
        {
            ResourceKey = AppPermissions.ChatsView.ToString(),
            SourceFields = new AmhViewCellModel
            {
                ID = nameof(ChatModel.ChatID),
                LeftImage = nameof(ChatModel.ImageName),
                LeftFieldType = FieldTypes.SquareImageControl,
                LeftHeader = nameof(ChatModel.FirstName),
                RightIcon = nameof(ChatModel.LastName),
                RightFieldType = FieldTypes.CircleImageControl
            },
            ShowSearchBar = true,
            ShowAddButton = false,
        };
        ParentPage.PageLayout.Add(_chatsListView, 0, 0);
        //AddCollectionView(_mainLayout, _customCellData, 0, 1);
        //if (!IsDashboardView(_chatData.RecordCount))
        //{
        //    AddSearchView(_mainLayout, false);
        //    //SearchField.IsVisible = false;
        //    if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        //    {
        //        AddSeparatorView(_mainLayout, 1, 0);
        //        Grid.SetRowSpan(Separator, 2);
        //        //Separator.IsVisible = false;
        //    }
        //}
        //_emptyMessageView = new AmhMessageControl();
        SetPageContent(_mainLayout);

    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
        {
        LoadPageUI(isRefreshRequest);
        if (!IsDashboardView(_chatData.RecordCount))
        {
            //SearchField.Value = string.Empty;
        }
        await (ParentPage.PageService as ChatService).GetChatsAsync(_chatData, _isProviderList).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        _chatsListView.PageResources = ParentPage.PageService.PageData;
        //_emptyMessageView.PageResources = ParentPage.PageData;
        _chatData.Resources = ParentPage.PageService.PageData.Resources;
        //_emptyListView.PageResources = ParentPage.PageData;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            //if (!IsDashboardView(_chatData.RecordCount))
            //{
            //    SearchField.PageResources = ParentPage.PageData;
            //    SearchField.IsVisible = true;
            //    if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            //    {
            //        Separator.IsVisible = true;
            //    }
            //}
            if (_chatData.ErrCode == ErrorCode.OK)
            {
                await RenderChatsAsync(isRefreshRequest);
            }
            else
            {
                //RenderErrorView(_mainLayout, _chatData.ErrCode.ToString(), IsDashboardView(_chatData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
                await SetChatDetailErrorAsync().ConfigureAwait(true);
            }
        });
    }

    private async Task RenderChatsAsync(bool isRefreshRequest)
    {
        if (_chatData.Chats?.Count == 1 && _isProviderList)
        {
           // await ChatItemClickAsync(_chatData.Chats.First().ToID).ConfigureAwait(true);
            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                return;
            }
        }
        if (GenericMethods.IsListNotEmpty(_chatData.Chats) || (_selectedUserID > 0 && !IsDashboardView(_chatData.RecordCount)))
        {
            await RenderListViewAsync(isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            //RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, IsDashboardView(_chatData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            await SetChatDetailErrorAsync().ConfigureAwait(true);
        }
        //(ShellMasterPage.CurrentShell as ShellMasterPage).UpdateBadgeCount(nameof(ChatsPage), _chatData.BadgeCount);
    }

    private async Task SetChatDetailErrorAsync()
    {
        //if (!IsDashboardView(_chatData.RecordCount))
        //{
        //    await SetDetailContentAsync(0, false).ConfigureAwait(true);
        //}
    }

    private async Task RenderListViewAsync(bool isRefreshRequest)
    {
        //CollectionViewField.ItemsSource = _chatData.Chats;
        _chatsListView.DataSource = _chatData.Chats;
        if (!IsDashboardView(_chatData.RecordCount))
        {
            //CollectionViewField.SelectedItem = _chatData.Chats.FirstOrDefault(x => (x.FromID == _selectedUserID || x.ToID == _selectedUserID) && _selectedUserID != 0);
            //SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            await SetDetailContentAsync(_selectedUserID, isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            //_mainLayout.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _chatData.Chats.Count + new OnIdiom<int> { Phone = 10, Tablet = 0 };
        }
        if (!isRefreshRequest && !IsPatientOverview(_chatData.RecordCount))
        {
            //OnListItemSelection(ChatList_SelectionChanged, true);
            _chatsListView.OnValueChanged += ChatList_SelectionChanged;
            (App.Current as App).OnReceiveNotification += OnReceiveNotification;
        }
    }

    private void LoadPageUI(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            _chatData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _isProviderList = GenericMethods.MapValueType<bool>(GetParameterValue(SPFieldConstants.FIELD_IS_PROVIDER_LIST));
            _selectedUserID = _chatData.Chat.ToID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
            if (_isProviderList)
            {
                _customCellData.CellDescription = nameof(ChatModel.UserProfession);
                _customCellData.CellRightContentHeader = string.Empty;
                _customCellData.CellRightContentDescription = string.Empty;
                _customCellData.RightDesciptionStyle = null;
            }
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        //OnListItemSelection(ChatList_SelectionChanged, false);
        (App.Current as App).OnReceiveNotification -= OnReceiveNotification;
        //if (SearchField != null)
        //{
        //    SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        //}
        if (_chatsListView != null)
        {
            //await _chatDetailView.UnloadUIAsync().ConfigureAwait(true);
            //_chatsListView.OnValueChanged -= OnNewChatReceived;
            if (_mainLayout.Children.Contains(_chatsListView))
            {
                _mainLayout.Children.Remove(_chatsListView); 
                // Reset title view content as chat detail view is unloaded
                //await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, string.Empty, MenuAction.MenuActionProfileKey, 0, null, string.Empty, string.Empty, ShellMasterPage.CurrentShell.CurrentPage?.IsAddEditPage ?? false)).ConfigureAwait(true);
            }
            //_chatsListView = null;
        }
        //_chatsListView = null;
    }

    /// <summary>
    /// Updates chat details view
    /// </summary>
    /// <param name="toID">User id</param>
    /// <param name="isRefreshRequest">true if refresh requested else false</param>
    /// <returns></returns>
    public async Task UpdateChatDetailsViewAsync(long toID, bool isRefreshRequest)
    {
        //if (!isRefreshRequest)
        //{
        //    //await ParentPage.SetRightHeaderItemsAsync(nameof(ChatPage)).ConfigureAwait(true);
        //    if (_mainLayout.Children.Contains(_emptyMessageView))
        //    {
        //        _mainLayout.Children.Remove(_emptyMessageView);
        //    }
        //    if (_chatDetailView != null && _mainLayout.Children.Contains(_chatDetailView))
        //    {
        //        _mainLayout.Children.Remove(_chatDetailView);
        //    }
        //    _chatDetailView = new ChatView(ParentPage, ParentPage.AddParameters(
        //        ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), Convert.ToString(toID, CultureInfo.InvariantCulture)),
        //        ParentPage.CreateParameter(nameof(BaseDTO.IsActive), Convert.ToString(false, CultureInfo.InvariantCulture))));
        //    _chatDetailView.OnChatUpdate -= OnNewChatReceived;
        //    _chatDetailView.OnChatUpdate += OnNewChatReceived;
        //    _mainLayout.Add(_chatDetailView, 2, 0);
        //    Grid.SetRowSpan(_chatDetailView, 2);
        //    AppHelper.ShowBusyIndicator = true;
        //    await _chatDetailView.LoadUIAsync(false).ConfigureAwait(false);
        //    AppHelper.ShowBusyIndicator = false;
        //}
    }

    private async void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
    //    var serchBar = sender as CustomSearchBar;
    //    // CollectionViewField.Footer = null;
    //    if (string.IsNullOrWhiteSpace(serchBar.Text))
    //    {
    //        CollectionViewField.ItemsSource = new List<ChatModel>();
    //        await Task.Delay(10);
    //        CollectionViewField.ItemsSource = _chatData.Chats;
    //    }
    //    else
    //    {
    //        var searchedUsers = _chatData.Chats.FindAll(y =>
    //        {
    //            return !string.IsNullOrWhiteSpace(y.FirstName) && y.FirstName.ToLowerInvariant().Contains(serchBar.Text.ToLowerInvariant().Trim());
    //        });
    //        CollectionViewField.ItemsSource = searchedUsers;
    //        if (searchedUsers.Count <= 0)
    //        {
    //            RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
    //        }
    //    }
    }

    private async void OnReceiveNotification(object sender, SignalRNotificationEventArgs e)
    {
        //    if (e.NotificationMessageType == NotificationMessageType.NotificationChat.ToString())
        //    {
        //        _chatData.Chat.ToID = _selectedUserID;
        //        await (ParentPage.PageService as ChatService).GetChatsAsync(_chatData, _isProviderList).ConfigureAwait(true);
        //        if (_chatData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(_chatData.Chats))
        //        {
        //            MainThread.BeginInvokeOnMainThread(() =>
        //            {
        //                CollectionViewField.ItemsSource = new List<ChatModel>();
        //                CollectionViewField.ItemsSource = _chatData.Chats;
        //                CollectionViewField.SelectedItem = _chatData.Chats.FirstOrDefault(x => (x.FromID == _selectedUserID || x.ToID == _selectedUserID) && _selectedUserID != 0);
        //                (ShellMasterPage.CurrentShell as ShellMasterPage).UpdateBadgeCount(nameof(ChatsPage), _chatData.BadgeCount);
        //            });
        //        }
        //    }
        }

    private async void OnNewChatReceived(object sender, EventArgs e)
    {
        //    _selectedUserID = _chatData.Chat.ToID = Convert.ToInt64(sender, CultureInfo.InvariantCulture);
        //    await LoadUIAsync(true).ConfigureAwait(true);
    }

    private async void ChatList_SelectionChanged(object sender, EventArgs e)
    {
        //var item = sender;
        //if (item != null)
        //{
        //    var chat = item as ChatModel;
        //    if (_selectedUserID == 0 || (chat.FromID != _selectedUserID && chat.ToID != _selectedUserID))
        //    {
        //        AppHelper.ShowBusyIndicator = true;
        //        await ChatItemClickAsync(_chatData.Chat.FromID == chat.ToID ? chat.FromID : chat.ToID).ConfigureAwait(true);
        //    }
        //}
        if (_chatsListView.Value != null)
        {
            var selectedUserID = (_chatsListView.Value as ChatModel).ToID;

            AppHelper.ShowBusyIndicator = true;
            // App._essentials.SetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, selectedUserID);
            //await ((App)Application.Current).SetupSignalRAsync(true).ConfigureAwait(true);
            //await ParentPage.NavigateOnNextPageAsync(false, _chatData.IsActive, LoginFlow.ChatPage).ConfigureAwait(true);
            //await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.ChatPage.ToString()).ConfigureAwait(true);
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.ChatPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(AlphaMDHealth.Utility.Param.id), selectedUserID.ToString()).ConfigureAwait(true);
            await ParentPage.RemovePageAsync().ConfigureAwait(true); 
            AppHelper.ShowBusyIndicator = false;
        }
    }

    private async Task ChatItemClickAsync(long toID)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            //if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.ChatAddEdit.ToString()))
            //{
                if (ShellMasterPage.CurrentShell.CurrentPage is ChatsPage)
                {
                    ChatsPage chatsPage = ShellMasterPage.CurrentShell.CurrentPage as ChatsPage;
                    if (chatsPage.ToID != toID.ToString(CultureInfo.InvariantCulture))
                    {
                        chatsPage.ToID = toID.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    if (_isProviderList && _chatData.Chats?.Count == 1)
                    {
                        _selectedUserID = toID;
                    }
                    //await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.ChatsPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(AlphaMDHealth.Utility.Param.id, AlphaMDHealth.Utility.Param.recordCount, AlphaMDHealth.Utility.Param.isAdd), toID.ToString(CultureInfo.InvariantCulture), "0", "true").ConfigureAwait(true);
                    await ShellMasterPage.CurrentShell.PushMainPageAsync(new ListViewDemoPage()).ConfigureAwait(false);
                }
                //}
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.ChatPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(AlphaMDHealth.Utility.Param.id), toID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            if (_isProviderList)
            {
                await ParentPage.RemovePageAsync().ConfigureAwait(true);
            }
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task SetDetailContentAsync(long toID, bool isRefreshRequest)
    {
        //if (MobileConstants.IsTablet)
        //{
        //    if (toID > 0)
        //    {
        //        await UpdateChatDetailsViewAsync(toID, isRefreshRequest).ConfigureAwait(true);
        //    }
        //    else
        //    {
        //        //if (_chatDetailView != null && _mainLayout.Children.Contains(_chatDetailView))
        //        //{
        //        //    _mainLayout.Children.Remove(_chatDetailView);
        //        //}
        //        _emptyMessageView.ResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
        //        _mainLayout.Add(_emptyMessageView, 2, 0);
        //        Grid.SetRowSpan(_emptyMessageView, 2);
        //    }
        //}
        if (toID > 0)
        {
            await UpdateChatDetailsViewAsync(toID, isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            if (_chatsListView != null && _mainLayout.Children.Contains(_chatsListView))
            {
                _mainLayout.Children.Remove(_chatsListView);
            }
            else
            {
                //_emptyMessageView.ResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                //_mainLayout.Add(_emptyMessageView, 2, 0);
                //Grid.SetRowSpan(_emptyMessageView, 2);
            }
        }
    }
}