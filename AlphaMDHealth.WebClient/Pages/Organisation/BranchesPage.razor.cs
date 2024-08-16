using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class BranchesPage : BasePage
{
    private readonly BranchDTO _branchData = new BranchDTO();
    private long _branchID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    private async Task GetDataAsync()
    {
        _branchData.Branch = new BranchModel { BranchID = 0 };
        await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationBranchesFromServerAsync(_branchData, CancellationToken.None), _branchData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(BranchModel.BranchID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(BranchModel.BranchName),DataHeader=ResourceConstants.R_BRANCH_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(BranchModel.DepartmentsCount),DataHeader=ResourceConstants.R_DEPARTEMENTS_TEXT_KEY},
        };
    }

    private void OnAddEditClick(BranchModel branch)
    {
        _branchID = branch == null ? 0 : branch.BranchID;
        ShowDetailPage = true;
    }

    private async Task OnAddEditClosed(string isDataUpdated)
    {
        ShowDetailPage = false;
        _branchID = 0;
        Success = Error = string.Empty;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await GetDataAsync();
        }
        else
        {
            Error = isDataUpdated;
        }
    }
}