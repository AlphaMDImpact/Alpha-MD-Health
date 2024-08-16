using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ChatsPage : BasePage, IDisposable
{
    private readonly ChatDTO _chatData = new ChatDTO { Chat = new ChatModel(), RecordCount = 0 };
    private ChatPage _chatPage = new ChatPage();
    private bool _isDashBoardView;

    /// <summary>
    /// To ID of the recipient user
    /// </summary>
    [Parameter]
    public long ToID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _chatData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ChatDTO.RecordCount)));
        _chatData.Chat.ToID = ToID;
        if (_chatData.RecordCount > 0 && AppState.RouterData.SelectedRoute.Page.ToString(CultureInfo.InvariantCulture) == AppPermissions.PatientDetailView.ToString())
        {
            _chatData.RecordCount = -2;
            _chatData.Chat.ToID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        }
        await GetChatsDataAsync().ConfigureAwait(true);
        AppState.OnReceiveNotification += OnReceiveNotification;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(ChatModel.ToID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{HasImage=true,ImageFieldType=FieldTypes.SquareWithBackgroundImageControl,ImageHeight=AppImageSize.ImageSizeM,ImageWidth=AppImageSize.ImageSizeM,IsSortable=false, ImageSrc=nameof(ChatModel.ImageName),ShowRowColumnHeader=false,MaxColumnWidthSize="10vh"},
            new TableDataStructureModel{DataField=nameof(ChatModel.FirstName),DataHeader=ResourceConstants.R_FIRST_NAME_KEY,IsSortable=false,ShowRowColumnHeader=false},
        };
    }

    private async Task GetChatsDataAsync()
    {
        await SendServiceRequestAsync(new ChatService(AppState.webEssentials).GetChatsAsync(_chatData, false), _chatData).ConfigureAwait(true);
        if (_chatData.ErrCode == ErrorCode.OK && _chatData.Chats?.Count > 0 && _chatData.Chat.ChatID == Guid.Empty && _chatData.Chat.ToID > 0)
        {
            // Display selected conversation on Chats list
            var newChat = _chatData.Chats.FirstOrDefault(x => x.ToID == _chatData.Chat.ToID || x.FromID == _chatData.Chat.ToID);
            _chatData.Chat.ChatID = newChat != null ? newChat.ChatID : _chatData.Chat.ChatID;
        }
        _isDashBoardView = _chatData.RecordCount > 0 || _chatData.RecordCount == -2;
        _isDataFetched = true;
    }

    private async void OnReceiveNotification(object sender, SignalRNotificationEventArgs e)
    {
        await GetChatsDataAsync().ConfigureAwait(true);
    }

    private async Task OnClosedEventCallbackAsync(string value)
    {
        ShowDetailPage = false;
        if (!string.IsNullOrEmpty(value))
        {
            _isDataFetched = false;
            _chatData.Chat.ToID = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            await GetChatsDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = value;
        }
    }

    private async Task OpenChatEventCallbackAsync(Tuple<string, string> selectedValues)
    {
        if (selectedValues != null)
        {
            ToID = Convert.ToInt64(selectedValues.Item2, CultureInfo.InvariantCulture);
            _chatData.Chat.ChatID = string.IsNullOrWhiteSpace(selectedValues.Item1) ? Guid.Empty : Guid.Parse(selectedValues.Item1);
            _chatData.Chat.ToID = Convert.ToInt64(selectedValues.Item2, CultureInfo.InvariantCulture);
            
            await GetChatsDataAsync().ConfigureAwait(true);
        }
    }

    private void OnAddButtonClick()
    {
        ShowDetailPage = true;
    }

    private async void OnChatViewClickAsync(ChatModel chatModel)
    {
        var chatID = chatModel.ChatID;
        ChatModel selectedChat = _chatData.Chats.FirstOrDefault(x => x.ChatID == chatID);
        _chatData.Chat.ToID = selectedChat.ToID == AppState.MasterData.Users[0].UserID ? selectedChat.FromID : selectedChat.ToID;
        _chatData.Chat.ChatID = selectedChat.ChatID;
        _chatData.UserChatCards.Find(x => x.CardId == chatID.ToString()).BadgeText = string.Empty;
        if (_chatData.RecordCount == -2)
        {
            await NavigateToAsync(AppPermissions.ChatView.ToString()).ConfigureAwait(false);
        }
        if (_chatData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.ChatsView.ToString(), Convert.ToString(_chatData.Chat.ToID, CultureInfo.InvariantCulture)).ConfigureAwait(false);
        }
        else
        {
            await GetChatsDataAsync().ConfigureAwait(true);
        }
    }

    private async Task OnViewAllClickedAsync(object e)
    {
        if (_chatData.RecordCount == -2)
        {
            await NavigateToAsync(AppPermissions.ChatView.ToString()).ConfigureAwait(false);
        }
        else
        {
            await NavigateToAsync(AppPermissions.ChatsView.ToString()).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _chatData.Chat = new ChatModel();
        ToID = 0;
        _chatPage.Dispose();
        AppState.OnReceiveNotification -= OnReceiveNotification;
    }
}