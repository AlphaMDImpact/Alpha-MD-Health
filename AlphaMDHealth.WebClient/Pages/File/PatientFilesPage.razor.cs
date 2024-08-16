using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
namespace AlphaMDHealth.WebClient;

public partial class PatientFilesPage : BasePage
{
    private FileDTO _fileData = new();
    private Guid _fileID;
    private bool _isDashboardView;
    public object _pageData;
    private IList<ButtonActionModel> _actionButtons = null;

    /// <summary>
    /// FileID parameter
    /// </summary>
    [Parameter]
    public Guid FileID
    {
        get { return _fileID; }
        set
        {
            if (_fileID != value)
            {
                if (_fileData.RecordCount > 0 || _fileID == Guid.Empty)
                {
                    _fileData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _fileID = value;
            }
        }
    }

    /// <summary>
    /// External data to load
    /// </summary>
    [Parameter]
    public object PageData
    {
        get
        {
            return _pageData;
        }
        set
        {
            _pageData = value;
            if (_pageData != null)
            {
                _fileData = _pageData as FileDTO;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (PageData == null)
        {
            if (Parameters?.Count > 0)
            {
                _fileData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(UserDTO.RecordCount)));
            }
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            _fileData = PageData as FileDTO;
            _isDataFetched = true;
        }
        if (IsPatientMobileView)
        {
            _actionButtons ??= new List<ButtonActionModel>();
            _actionButtons.Add(new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                ButtonAction = () => { ShowMoreMenuPage(); },
                Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
            });
        }
    }

    private async Task GetDataAsync()
    {
        _fileData.File = new FileModel { FileID = Guid.Empty };
        _fileData.Files = new List<FileModel>();
        await SendServiceRequestAsync(new FileService(AppState.webEssentials).GetFilesAsync(_fileData), _fileData).ConfigureAwait(true);
        _isDashboardView = _fileData.RecordCount > 0;
        _isDataFetched = true;
    }

    private async Task OnAddEditClickAsync(FileModel file)
    {
        Success = Error = string.Empty;
        if (_fileData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.PatientFilesView.ToString(), (file == null ? Guid.Empty : file.FileID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _fileID = file == null ? Guid.Empty : file.FileID;
            ShowDetailPage = true;
        }
    }

    private async Task OnCloseEventCallbackAsync(string message)
    {
        ShowDetailPage = false;
        _fileID = Guid.Empty;
        Error = Success = string.Empty;
        if (PageData == null) await OnViewAllClickedAsync().ConfigureAwait(false);
        if (message == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = message;
            await GetDataAsync().ConfigureAwait(true);
        }
        else if (!string.IsNullOrWhiteSpace(message))
        {
            Error = message;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientFilesView.ToString()).ConfigureAwait(false);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(FileModel.FileID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false},
            new TableDataStructureModel{ImageSrc=nameof(FileModel.FileImage), HasImage = true, ImageFieldType = FieldTypes.SquareWithBackgroundImageControl, ImageHeight = AppImageSize.ImageSizeM, ImageWidth = AppImageSize.ImageSizeM},
            new TableDataStructureModel{DataField=nameof(FileModel.FileName), DataHeader=ResourceConstants.R_CATEGORY_KEY},
             new TableDataStructureModel{DataField=nameof(FileModel.FormattedNumberOfFiles), DataHeader=ResourceConstants.R_NUMBER_OF_FILES_KEY},
             new TableDataStructureModel{DataField=nameof(FileModel.FormattedDate), DataHeaderValue=string.Format(LibResources.GetResourceValueByKey(_fileData.Resources, ResourceConstants.R_UPDATED_ON_TEXT_KEY),Constants.CHAR_SPACE)}
        };
    }
    private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(FileModel.FileID),
            LeftHeader = nameof(FileModel.FileName),
            LeftHeaderFieldType = FieldTypes.PrimarySmallHStartVCenterBoldLabelControl,
            LeftDescription = nameof(FileModel.FormattedDate),
            LeftDescriptionFieldType = FieldTypes.PrimarySmallHStartVCenterLabelControl,
            LeftIcon = nameof(FileModel.FileImage),
            RightHeader = nameof(FileModel.FormattedNumberOfFiles),
            RightHeaderFieldType = FieldTypes.PrimarySmallHStartVCenterLabelControl,
        };

    }

    private async void ShowMoreMenuPage()
    {
        await NavigateToAsync(AppPermissions.PatientMobileMenusView.ToString());
    }
}