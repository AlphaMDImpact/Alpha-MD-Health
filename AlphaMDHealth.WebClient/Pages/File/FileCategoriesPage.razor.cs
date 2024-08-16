using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class FileCategoriesPage : BasePage
{
    private readonly FileCategoryDTO _fileCategoryData = new FileCategoryDTO { FileCatergory = new FileCategoryModel() };
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
                if (_fileCategoryData.RecordCount > 0 || _fileCategoryID == 0)
                {
                    _fileCategoryData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _fileCategoryID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await RenderPageDataAsync().ConfigureAwait(true);
    }

    private async Task RenderPageDataAsync()
    {
        if (Parameters?.Count > 0)
        {
            _fileCategoryData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(FileCategoryDTO.RecordCount)));
        }
        await SendServiceRequestAsync(new FileCategoryService(AppState.webEssentials).SyncFileCategoriesFromServerAsync(_fileCategoryData, GenericMethods.GetDefaultDateTime, CancellationToken.None), _fileCategoryData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField = nameof(FileCategoryModel.FileCategoryID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false},
            new TableDataStructureModel{DataField = nameof(FileCategoryModel.Name), DataHeader = ResourceConstants.R_FILE_CATEGORY_NAME_KEY},
        };
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.FilesCategoriesView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClick(FileCategoryModel fileCategory)
    {
        Success = Error = string.Empty;
        if (_fileCategoryData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.FilesCategoriesView.ToString(), (fileCategory == null ? 0 : fileCategory.FileCategoryID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _fileCategoryID = fileCategory == null ? 0 : fileCategory.FileCategoryID;
            ShowDetailPage = true;
        }
    }

    private async Task OnAddEditClosedAsync(string isDataUpdated)
    {
        ShowDetailPage = false;
        _fileCategoryID = 0;
        Success = Error = string.Empty;
        if (isDataUpdated == ResourceConstants.R_SAVE_ACTION_KEY)
        {
            _isDataFetched = false;
            Success = ErrorCode.OK.ToString();
            await RenderPageDataAsync().ConfigureAwait(true);
            StateHasChanged();
        }
        else
        {
            if (isDataUpdated != ResourceConstants.R_CANCEL_ACTION_KEY)
            {
                Error = isDataUpdated;
            }
        }
    }
}