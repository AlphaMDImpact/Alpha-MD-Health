using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.WebClient;

public partial class AddBulkUserPage : BasePage
{
    private readonly UserDTO _userData = new UserDTO() { RecordCount = -1 };
    private bool _isDownloadVisible;

    /// <summary>
    /// CurrentView parameter
    /// </summary>
    [Parameter]
    public AppPermissions CurrentView { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _userData.ViewFor = CurrentView;
        _userData.User = new UserModel();
        await SendServiceRequestAsync(new UserService(AppState.webEssentials).GetUserAsync(_userData), _userData).ConfigureAwait(true);
        if (_userData.ErrCode == ErrorCode.OK)
        {
            PageTitle = string.Format(
                LibResources.GetResourceValueByKey(_userData.Resources, ResourceConstants.R_BULK_UPLOAD_TEXT_KEY),
                LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppState.RouterData.SelectedRoute.Page));
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_userData.ErrCode.ToString());
        }
    }

    private void OnAttachmentClick(object fileModel)
    {
        _isDownloadVisible = false;
        if (fileModel != null)
        {
            var file = fileModel as AttachmentModel;
            if (file?.IsActive == true)
            {
                _userData.User.ExcelName = file.FileName;
                _userData.AttachmentBase64 = file.FileValue.Substring(file.FileValue.LastIndexOf(Constants.COMMA_SEPARATOR) + 1);
            }
            else
            {
                _userData.User.ExcelName = string.Empty;
                _userData.AttachmentBase64 = string.Empty;
            }
        }
    }

    /// <summary>
    /// it download sample Excel file for reference
    /// </summary>
    /// <returns> download the excel</returns>
    private void OnDownloadSampleClick()
    {
        // Navigate to the URL to trigger the download
        NavigationManager.NavigateTo(_userData.SampleFilePath, true);
    }

    /// <summary>
    /// it download Excel file uploaded by user
    /// </summary>
    /// <returns> download the excel</returns>
    private void OnDownloadButtonClicked()
    {
        var base64String = Constants.EXCEL_PREFIX + _userData.AttachmentBase64;
        NavigationManager.NavigateTo(base64String, true);
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveButtonClickAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            UserDTO users = new UserDTO
            {
                ViewFor = _userData.ViewFor,
                AttachmentBase64 = _userData.AttachmentBase64,
                User = new UserModel
                {
                    ExcelPath = _userData.User.ExcelPath,
                    OrganisationDomain = AppState.MasterData.OrganisationDomain,
                    OrganisationID = Convert.ToInt64(AppState.webEssentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0))
                },
                IsActive = false
            };
            await SendServiceRequestAsync(new UserService(AppState.webEssentials).SyncBulkUserToServerAsync(users, CancellationToken.None), users).ConfigureAwait(true);
            if (users.ErrCode == ErrorCode.OK)
            {
                _isDownloadVisible = true;
                await OnClose.InvokeAsync(ErrorCode.AllExcelDataSuccess.ToString());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_userData.User.ExcelName))
                {
                    _isDownloadVisible = false;
                }
                else if (_userData.User.ExcelName.Contains(Constants.EXCEL_EXTESTION))
                {
                    _isDownloadVisible = true;
                }
                if (users.ErrCode == ErrorCode.BulkUploadDataEntryStatus)
                {
                    var userObj = JObject.Parse(users.Response);
                    var successCount = Convert.ToInt32(userObj[nameof(users.CreatedByID)]);
                    var totalRecordCount = Convert.ToInt32(userObj[nameof(users.RecordCount)]);
                    Error = string.Format(LibResources.GetResourceValueByKey(_userData.Resources, users.ErrCode.ToString())
                        , totalRecordCount, PageTitle, Constants.COMMA_SEPARATOR, successCount, PageTitle, (totalRecordCount - successCount), PageTitle);
                    _userData.AttachmentBase64 = users.AttachmentBase64;
                }
                else
                {
                    _userData.AttachmentBase64 = users.AttachmentBase64;
                    Error = users.ErrCode.ToString();
                }
            }
        }
    }
}