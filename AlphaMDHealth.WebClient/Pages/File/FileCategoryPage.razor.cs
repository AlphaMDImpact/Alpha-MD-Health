using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class FileCategoryPage : BasePage
{
    private readonly FileCategoryDTO _categoryData = new FileCategoryDTO { RecordCount = -1, FileCatergory = new FileCategoryModel(), FileCategoryDetails = new List<FileCategoryDetailModel>() };
    private List<ButtonActionModel> _messageButtonActions;
    private bool _showDeletedConfirmationPopup = true;
    private bool _isEditable = false;

    private long _fileCategoryID;

    /// <summary>
    /// FileCategoryID parameter
    /// </summary>
    [Parameter]
    public long FileCategoryID
    {
        get { return _fileCategoryID; }
        set
        {
            if (_fileCategoryID != value)
            {
                _fileCategoryID = value;
                //Task.Run(async () => { await GetDataAsync(); });
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _categoryData.FileCatergory.FileCategoryID = FileCategoryID;
        await SendServiceRequestAsync(new FileCategoryService(AppState.webEssentials).SyncFileCategoriesFromServerAsync(_categoryData, GenericMethods.GetDefaultDateTime, CancellationToken.None), _categoryData).ConfigureAwait(true);
        if (_categoryData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_categoryData.FeaturePermissions, AppPermissions.FileCategoryAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_categoryData.ErrCode.ToString());
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _categoryData.IsActive = true;
            FileCategoryDTO categoryDTO = new FileCategoryDTO
            {
                FileCatergory = new FileCategoryModel { FileCategoryID = FileCategoryID, IsActive = _categoryData.IsActive, ImageName = _categoryData.FileCatergory.ImageName },
                FileCategoryDetails = _categoryData.FileCategoryDetails
            };
            await SaveFileCategoryAsync(categoryDTO).ConfigureAwait(false);
        }
    }

    private async Task OnDeleteConfirmationClickedAsync(object sequenceNo)
    {
        _showDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (Convert.ToInt64(sequenceNo) == 1)
        {
            _categoryData.IsActive = _categoryData.FileCatergory.IsActive = false;
            await SaveFileCategoryAsync(_categoryData).ConfigureAwait(true);
        }
    }

    private async Task SaveFileCategoryAsync(FileCategoryDTO categoryDTO)
    {
        await SendServiceRequestAsync(new FileCategoryService(AppState.webEssentials).SyncFileCategoryToServerAsync(categoryDTO, CancellationToken.None), categoryDTO).ConfigureAwait(true);
        if (categoryDTO.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(ResourceConstants.R_SAVE_ACTION_KEY);
        }
        else
        {
            Error = categoryDTO.ErrCode.ToString();
        }
    }

    private void OnImageChanged(object file)
    {
        _categoryData.FileCatergory.ImageName = file != null && file is AttachmentModel && (file as AttachmentModel).IsActive
            ? (file as AttachmentModel).FileValue
            : string.Empty;
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField = nameof(FileCategoryDetailModel.Name),
            ResourceKey = ResourceConstants.R_FILE_CATEGORY_NAME_KEY,
            IsRequired = true
        },
        new TabDataStructureModel
        {
            DataField = nameof(FileCategoryDetailModel.Description),
            ResourceKey = ResourceConstants.R_FILE_CATEGORY_DESCRIPTION_KEY,
            IsRequired = true
        },
    };

    private async Task OnCancelClickAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _messageButtonActions = new List<ButtonActionModel>
        {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey =ResourceConstants.R_OK_ACTION_KEY  },
            new ButtonActionModel{ ButtonID = Constants.NUMBER_TWO, ButtonResourceKey =ResourceConstants.R_CANCEL_ACTION_KEY  },
        };
        _showDeletedConfirmationPopup = false;
    }
}
