using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class EducationCategoriesPage : BasePage
{
    private readonly EducationCategoryDTO _educationCategoryData = new EducationCategoryDTO { EductaionCatergory = new EductaionCatergoryModel() };
    private long _educationCategoryID;

    protected override async Task OnInitializedAsync()
    {
        await RenderPageDataAsync().ConfigureAwait(true);
    }

    private async Task RenderPageDataAsync()
    {
        await SendServiceRequestAsync(new EducationCategoryService(AppState.webEssentials).SyncEducationCategoriesFromServerAsync(_educationCategoryData, GenericMethods.GetDefaultDateTime, CancellationToken.None), _educationCategoryData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField = nameof(EductaionCatergoryModel.EducationCategoryID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false},
            new TableDataStructureModel{DataField = nameof(EductaionCatergoryModel.Name), DataHeader = ResourceConstants.R_EDUCATION_CATEGORY_NAME_KEY},
        };
    }

    private void OnAddEditClick(EductaionCatergoryModel educationCategory)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _educationCategoryID = educationCategory == null ? 0 : educationCategory.EducationCategoryID;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _educationCategoryID = 0;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await RenderPageDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }
}