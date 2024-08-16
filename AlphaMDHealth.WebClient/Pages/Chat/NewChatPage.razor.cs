using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class NewChatPage : BasePage
{
    private readonly ChatDTO _chatData = new ChatDTO { Chat = new ChatModel { ToID = -1 }, RecordCount = -1 };

    protected override async Task OnInitializedAsync()
    {
        await GetChatDataAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        await GetChatDataAsync().ConfigureAwait(true);
        if (_isDataFetched)
        {
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }

    private async Task GetChatDataAsync()
    {
        await SendServiceRequestAsync(new ChatService(AppState.webEssentials).GetChatsAsync(_chatData, false), _chatData).ConfigureAwait(true);
        if (_chatData.ErrCode == ErrorCode.OK && _chatData.Chats.Count == 1)
        {
            await OnClose.InvokeAsync(Convert.ToString(_chatData.Chats[0].ToID, CultureInfo.InvariantCulture));
        }
        _isDataFetched = true;
    }
    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(ChatModel.ToID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{HasImage=true, ImageSrc = nameof(ChatModel.ImageName), ImageHeight = AppImageSize.ImageSizeM, ImageWidth = AppImageSize.ImageSizeM, ImageFieldType = FieldTypes.SquareWithBackgroundImageControl,ShowRowColumnHeader=false,MaxColumnWidthSize="10vh"},
            new TableDataStructureModel{DataField=nameof(ChatModel.FirstName),DataHeader=ResourceConstants.R_FIRST_NAME_KEY,IsSortable=false,ShowRowColumnHeader=false},
        };
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnCardClickAsync(ChatModel chatModel)
    {
        var chatID = chatModel.ChatID;
        ChatModel selectedChat = _chatData.Chats.FirstOrDefault(x => x.ChatID == chatID);
        _chatData.Chat.ToID = selectedChat.ToID == AppState.MasterData.Users[0].UserID ? selectedChat.FromID : selectedChat.ToID;
        _chatData.Chat.ChatID = selectedChat.ChatID;
        await OnClose.InvokeAsync(Convert.ToString(_chatData.Chat.ToID, CultureInfo.InvariantCulture));
    }
}