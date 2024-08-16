using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.MobileClient;

public class PatientBulkUploadView : BaseLibCollectionView
{
    public readonly UserDTO _userData = new UserDTO
    {
        User = new UserModel(),
        Languages = new List<OptionModel>(),
        Genders = new List<OptionModel>()
    };

    private readonly Grid _mainLayout;
    private readonly CustomUploadControl _uploadComponent;
    private readonly CustomButtonControl _downloadSampleExcel;
    private readonly CustomButtonControl _downloadExcel;
    internal readonly BasePage _parentPage;

    /// <summary>
    /// OnDateSelectedValueChanged Event
    /// </summary>
    public event EventHandler<EventArgs> OnSaveSuccess;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientBulkUploadView(BasePage page, object parameters) : base(page, parameters)
    {
        _parentPage = page;
        _uploadComponent = new CustomUploadControl(page, parameters)
        {
            ControlType = FieldTypes.UploadControl,
            IsStreamSave = true,
        };
        ParentPage.PageService = new UserService(App._essentials);
        _downloadSampleExcel = new CustomButtonControl(ButtonType.DeleteWithoutMargin);
        _downloadExcel = new CustomButtonControl(ButtonType.PrimaryWithoutMargin) { IsVisible = false };
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions = CreateTabletViewColumn(true),
        };
        _mainLayout.Add(_uploadComponent, 0, 0);
        _mainLayout.Add(_downloadSampleExcel, 0, 1);
        _mainLayout.Add(_downloadExcel, 0, 2);
        SetPageContent(_mainLayout);
    }

    internal async Task OnSaveButtonClickedAsync()
    {
        if (_uploadComponent.FileStream == null)
        {
            _userData.ErrCode = ErrorCode.InvalidData;
            _userData.ErrorDescription = _parentPage.GetResourceValueByKey(_userData.ErrCode.ToString());
            OnSaveSuccess?.Invoke(_userData, new EventArgs());
            return;
        }
        UserDTO users = new UserDTO
        {
            User = new UserModel
            {
                IsUser = false,
                IsMobile = true,
                ExcelPath = _uploadComponent.Value,
                RoleID = (int)RoleName.Patient,
                OrganisationID = App._essentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0)
            },
            IsActive = false
        };
        await (ParentPage.PageService as UserService).SyncBulkUserToServerAsync(users, CancellationToken.None).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        if (users.ErrCode == ErrorCode.OK)
        {
            _downloadExcel.IsVisible = true;
            users.ErrorDescription = _parentPage.GetResourceValueByKey(users.ErrCode.ToString());
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_userData.User.ExcelName))
            {
                _downloadExcel.IsVisible = false;
            }
            if (users.ErrCode == ErrorCode.BulkUploadDataEntryStatus)
            {
                var userObj = JObject.Parse(users.Response);
                var successCount = Convert.ToInt32(userObj[nameof(users.CreatedByID)]);
                var totalRecordCount = Convert.ToInt32(userObj[nameof(users.RecordCount)]);
                _uploadComponent.Value = Convert.ToString(userObj[nameof(users.AddedBy)]);
                if (!string.IsNullOrWhiteSpace(_uploadComponent.Value))
                {
                    _downloadExcel.IsVisible = true;
                }
                users.ErrorDescription = string.Format(_parentPage.GetResourceValueByKey(users.ErrCode.ToString()), totalRecordCount, _userData.PhoneNumber, successCount, _userData.PhoneNumber,
                    (totalRecordCount - successCount), _userData.PhoneNumber);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(_uploadComponent.Value))
                {
                    _downloadExcel.IsVisible = true;
                }
                users.ErrorDescription = _parentPage.GetResourceValueByKey(users.ErrCode.ToString());
            }
        }
        OnSaveSuccess?.Invoke(users, new EventArgs());

    }

    /// <summary>
    /// refresh list
    /// </summary>
    public void RefreshList()
    {
        InvokeListRefresh(Guid.Empty, new EventArgs());
    }
    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            //refersh one
        }
        if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            return;
        }
        if (_userData != null)
        {
            _userData.User.UserID = 0;
        }

        await Task.WhenAll(
             ParentPage.GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_USER_PROFILE_PAGE_GROUP),
             ParentPage.GetSettingsAsync(GroupConstants.RS_COMMON_GROUP)
             //,(ParentPage.PageService as UserService).GetUserAsync(_userData, false, false) //todo:
         ).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_userData.ErrCode == ErrorCode.OK)
        {
            _uploadComponent.SetEmptyViewText();
            _downloadSampleExcel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DOWNLOAD_EXCEL_SAMPLE_TEXT_KEY);
            _downloadExcel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_DOWNLOAD_TEXT_KEY);
            _downloadSampleExcel.Clicked += DownloadSampleExcel_Clicked;
            _downloadExcel.Clicked += DownloadExcel_Clicked;
            _uploadComponent.OnFileDelete += _uploadComponent_OnFileDelete;
            _uploadComponent.SupportedFileTypes = ParentPage.GetSettingsValueByKey(SettingsConstants.S_UPLOAD_SUPPORTED_FILE_TYPE_KEY);
        }
    }

    private void _uploadComponent_OnFileDelete(object sender, EventArgs e)
    {
        if (sender.ToString() == ErrorCode.OK.ToString() && string.IsNullOrWhiteSpace(_uploadComponent.FileNameWithExtention))
        {
            _downloadExcel.IsVisible = false;
        }
    }

    private async void DownloadExcel_Clicked(object sender, EventArgs e)
    {
        if (await PermissionHelper.CheckPermissionStatusAsync(Permission.Storage, ParentPage).ConfigureAwait(true))
        {
            //todo:DependencyService.Get<IDataViewer>().ShowAttachment(new AttachmentModel { OriginalFile = Convert.FromBase64String(_uploadComponent.Value), FileName = _uploadComponent.FileNameWithExtention });
        }
    }

    private async void DownloadSampleExcel_Clicked(object sender, EventArgs e)
    {
        if (await PermissionHelper.CheckPermissionStatusAsync(Permission.Storage, ParentPage).ConfigureAwait(true))
        {
            //todo:await DependencyService.Get<IDataViewer>().DownloadFileFromAsset(Constants.BULK_PATIENT_SAMPLE_FILE, Constants.SAMPLE_EXCEL_FOLDER);
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _downloadSampleExcel.Clicked -= DownloadSampleExcel_Clicked;
        _downloadExcel.Clicked -= DownloadExcel_Clicked;
        _uploadComponent.OnFileDelete -= _uploadComponent_OnFileDelete;
    }
}