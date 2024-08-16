using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class DepartmentsPage : BasePage
{
    private readonly DepartmentDTO _departmentData = new DepartmentDTO { Department = new DepartmentModel() };
    private byte _departmentID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new DepartmentService(AppState.webEssentials).SyncDepartmentsFromServerAsync(_departmentData, CancellationToken.None), _departmentData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(DepartmentModel.DepartmentID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(DepartmentModel.DepartmentName), DataHeader=ResourceConstants.R_DEPARTMENT_NAME_TEXT_KEY },
        };
    }

    private void OnAddEditClick(DepartmentModel department)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _departmentID = department == null ? (byte)0 : department.DepartmentID;

    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _departmentID = 0;
        Success = Error = string.Empty;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }
}