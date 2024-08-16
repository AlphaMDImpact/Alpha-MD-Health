using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ChatPage : BasePage
{
    private readonly ChatDTO _chatData = new ChatDTO { Chat = new ChatModel(), ChatDetail = new ChatDetailModel(), RecordCount = -1 };
    private AttachmentModel _attachmentData = new AttachmentModel();
    private AttachmentModel _currentFile;
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private bool _attachmentSelected;
    private bool _noChatFound;
    private bool _showAttachmentFile;
    private bool _isSelectedChatRoute;
    private int _refreshRequired;
    private bool _showPreview;

    /// <summary>
    /// Chat ID of the recipient user
    /// </summary>
    [Parameter]
    public Guid ChatID { get; set; }

    /// <summary>
    /// To ID of the recipient user
    /// </summary>
    [Parameter]
    public long ToID { get; set; }

    /// <summary>
    /// To determine whether ChatDetail is called from Chats menu or Patient tab
    /// </summary>
    [Parameter]
    public bool IsFromChat { get; set; }

    /// <summary>
    /// Callback method to set value after a message is sent.
    /// </summary>
    [Parameter]
    public EventCallback<Tuple<string, string>> OnSendClicked { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _refreshRequired = IsFromChat ? 1 : 0;
        await GetChatDetailsDataAsync().ConfigureAwait(true);
        AppState.OnReceiveNotification += OnReceiveNotification;
    }

    private async Task GetChatDetailsDataAsync()
    {
        _isSelectedChatRoute = AppState.RouterData.SelectedRoute.Page == AppPermissions.ChatAddEdit.ToString();
        _chatData.Chat.ChatID = ChatID;
        _chatData.Chat.ToID = IsFromChat ? ToID : AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        _chatData.ChatDetail.ChatText = string.Empty;
        _chatData.SelectedUserID = AppState.MasterData.Users[0].UserID;
        await SendServiceRequestAsync(new ChatService(AppState.webEssentials).GetChatDetailsAsync(_chatData), _chatData).ConfigureAwait(true);
        _noChatFound = IsFromChat && ToID == 0;
        _isDataFetched = true;
    }

    private bool CheckIfImage(AttachmentModel attachmentData)
    {
        return (attachmentData.FileExtension == nameof(AppFileExtensions.doc) 
            || attachmentData.FileExtension == nameof(AppFileExtensions.docx) 
            || attachmentData.FileExtension == nameof(AppFileExtensions.pdf) 
            || attachmentData.FileExtension == nameof(AppFileExtensions.xlsx) 
            || attachmentData.FileExtension == nameof(AppFileExtensions.xls))
                ? false : true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (ToID != 0 || !IsFromChat)
        {
            long newToID = _chatData.Chat.ToID == _chatData.SelectedUserID ? _chatData.Chat.FromID : _chatData.Chat.ToID;
            if (newToID != ToID || _refreshRequired > 0)
            {
                bool scrollToEnd = _refreshRequired != 2;
                _refreshRequired = 0;
                await GetChatDetailsDataAsync().ConfigureAwait(true);
                if (_isDataFetched)
                {
                    if (scrollToEnd)
                    {
                        await JSRuntime.InvokeVoidAsync(Constants.SCROLL_TO_BOTTOM, Constants.SCROLL_BAR);
                    }
                }
                if (newToID != ToID)
                {
                    _chatData.ChatDetail.ChatText = string.Empty;
                }
            }
        }
        await base.OnParametersSetAsync();
    }



    private void OnAttachmentClick()
    {
        _attachmentSelected = true;
    }

    private void OnFileClicked(AttachmentModel file)
    {
        if (file.FileExtension != AppFileExtensions.none.ToString())
        {
            _showPreview = true;
            _currentFile = file;
        }
    }

    private void OnPreviewActionClicked(AttachmentModel attachmentData)
    {
        _showPreview = false;
    }

    private void OnImageChanged(object image)
    {
        var imageFile = image as AttachmentModel;
        _attachmentSelected = true;
        _showAttachmentFile = true;
        _currentFile = imageFile;
    }

    private string GetFileIcon(string fileTypeString)
    {
        var fileType = Enum.TryParse<AppFileExtensions>(fileTypeString, out var parsedFileType)
                        ? parsedFileType
                        : AppFileExtensions.none; // Assuming 'Other' is a default enum case

        string fileIcon = fileType switch
        {
            AppFileExtensions.pdf => ImageConstants.I_PDF_UPLOAD_ICON,
            AppFileExtensions.doc => ImageConstants.I_DOC_UPLOAD_ICON,
            AppFileExtensions.docx => ImageConstants.I_DOC_UPLOAD_ICON,
            AppFileExtensions.xlsx => ImageConstants.I_XSL_UPLOAD_MOBILE_SVG,
            AppFileExtensions.xls => ImageConstants.I_XSL_UPLOAD_MOBILE_SVG,
            _ => ImageConstants.I_FILES_UPLOAD_ICON,
        };

        return fileIcon;
    }

    private void OnAttachmentCancel()
    {
        //ResourcePropertyChange(false);
        _chatData.ChatDetail.AttachmentBase64 = string.Empty;
        _attachmentData.FileName = _chatData.ChatDetail.FileName = string.Empty;
        _chatData.ChatDetail.FileType = AppFileExtensions.none;
        _attachmentData.Text = _chatData.ChatDetail.ChatText = string.Empty;
    }

    private async Task OnAttachmentSendAsync(AttachmentModel attachData)
    {
        _attachmentSelected = false;
        _showAttachmentFile = false;
        if (attachData != null)
        {
            _chatData.ChatDetail.ChatText = attachData.FileDescription;
            _chatData.ChatDetail.AttachmentBase64 = attachData.FileValue;
            _chatData.ChatDetail.FileType = attachData.FileExtension.ToEnum<AppFileExtensions>();
            _chatData.ChatDetail.FileName = attachData.FileName;
            await OnSendButtonClickedAsync().ConfigureAwait(true);
        }
    }

    private async Task OnFileClickAsync(CustomAttachmentModel attachmentData)
    {
        if (attachmentData.IsActive)
        {
            FieldTypes type = FieldTypes.AlphaEntryControl;
            if (type == FieldTypes.UploadControl)
            {
                await JSRuntime.InvokeVoidAsync(Constants.FILE_SAVE_AS, attachmentData.FileName);
            }
            else
            {
                if (type == FieldTypes.UploadControl && !string.IsNullOrWhiteSpace(attachmentData.FileName))
                {
                    _attachmentSelected = true;
                    _attachmentData.FileName = attachmentData.FileName;
                    _attachmentData.Text = attachmentData.Text;
                    _showAttachmentFile = true;
                    _isDataFetched = true;
                }
            }
        }
    }
    private void OnDeleteButtonClick(AttachmentModel attachmentData)
    {
        if (attachmentData.IsSent)
        {
            _chatData.ChatDetail = _chatData.ChatDetails.FirstOrDefault(x => x.ChatDetailID == attachmentData.FileID);
            _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
                            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },};
            _hideConfirmationPopup = false;
        }
    }
    private async Task DeletePatientTrackerPopUpCallbackAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _chatData.ChatDetail.IsActive = false;
                _chatData.ChatDetail.FromID = _chatData.ChatDetail.AddedById;
                await SendChatMessageAsync().ConfigureAwait(true);
            }
            else
            {
                _chatData.ChatDetail = new ChatDetailModel();
            }
        }
    }

    private async Task OnSendButtonClickedAsync()
    {
        if (!string.IsNullOrWhiteSpace(_chatData.ChatDetail.ChatText) || !string.IsNullOrWhiteSpace(_chatData.ChatDetail.AttachmentBase64))
        {
            _chatData.Chats.Clear();
            _chatData.ChatDetail.ChatDetailID = Guid.Empty;
            _chatData.ChatDetail.IsActive = true;
            _chatData.ChatDetail.IsSent = true;
            _chatData.ChatDetail.FromID = AppState.MasterData.Users[0].UserID;
            _chatData.ChatDetail.ToID = _chatData.Chat.ToID == _chatData.ChatDetail.FromID ? _chatData.Chat.FromID : _chatData.Chat.ToID;
            await SendChatMessageAsync().ConfigureAwait(true);
        }
    }

    private async Task SendChatMessageAsync()
    {
        //1 indicates scroll position has to be reset else 2 
        _refreshRequired = _chatData.ChatDetail.IsActive ? 1 : 2;
        await SendServiceRequestAsync(new ChatService(AppState.webEssentials).
            SaveChatDetailAsync(new ChatDTO { Chat = _chatData.Chat, ChatDetail = _chatData.ChatDetail }), _chatData).ConfigureAwait(true);
        if (IsFromChat)
        {
            await OnSendClicked.InvokeAsync(new Tuple<string, string>(ChatID.ToString(), ToID.ToString(CultureInfo.InvariantCulture)));
        }
        else
        {
            await GetChatDetailsDataAsync().ConfigureAwait(true);
        }
        OnAttachmentCancel();
    }

    private async void OnReceiveNotification(object sender, SignalRNotificationEventArgs e)
    {
        var receiverID = Convert.ToInt64(e.NotificationFromID.Replace(Constants.USER_TAG_PREFIX, string.Empty, StringComparison.OrdinalIgnoreCase), CultureInfo.InvariantCulture);
        if ((receiverID == _chatData.Chat.ToID) || (receiverID == _chatData.Chat.FromID))
        {
            _refreshRequired = 1;
            await GetChatDetailsDataAsync().ConfigureAwait(true);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        ToID = 0;
        if (AppState != null)
        {
            AppState.OnReceiveNotification -= OnReceiveNotification;
        }
    }
}