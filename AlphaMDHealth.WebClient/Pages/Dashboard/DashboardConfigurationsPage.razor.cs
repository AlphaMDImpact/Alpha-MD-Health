using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class DashboardConfigurationsPage : BasePage
{
    private readonly DashboardDTO _dashboardData = new DashboardDTO();
    private long _dashboardSettingID;
    private string _selectedRoleID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    private async Task GetDataAsync()
    {
        _dashboardData.ConfigurationRecord = new ConfigureDashboardModel { DashboardSettingID = 0 };
        await SendServiceRequestAsync(new DashboardService(AppState.webEssentials).GetDashboardDataAsync(_dashboardData), _dashboardData).ConfigureAwait(true);
        if (_dashboardData.ErrCode == ErrorCode.OK)
        {
            _selectedRoleID = _dashboardData.ConfigurationRecord?.RoleID.ToString() ?? string.Empty;
        }
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(ConfigureDashboardModel.DashboardSettingID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(ConfigureDashboardModel.FeatureText),DataHeader=ResourceConstants.R_FEATURE_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(ConfigureDashboardModel.SequenceNo),DataHeader=ResourceConstants.R_SEQUENCE_NO_KEY},
            new TableDataStructureModel{DataField=nameof(ConfigureDashboardModel.WidgetSizeInWebPage),DataHeader=ResourceConstants.R_COLUMN_NAME_KEY},
        };
    }

    private void OnAddEditClick(ConfigureDashboardModel dashboardSetting)
    {
        ShowDetailPage = true;
        _dashboardSettingID = dashboardSetting == null ? 0 : dashboardSetting.DashboardSettingID;
    }

    private async Task OnCancelClickedAsync()
    {
        await NavigateToAsync(AppPermissions.DashboardView.ToString()).ConfigureAwait(false);
    }

    private async Task OnAddEditClosedAsync(string isDataUpdated)
    {
        ShowDetailPage = false;
        _dashboardSettingID = 0;
        Success = Error = string.Empty;
        string[] msg = isDataUpdated?.Split(Constants.SYMBOL_PIPE_SEPERATOR);
        if (msg?.Length > 0 && msg[0] == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = msg[0];
            await GetDataAsync();
            _selectedRoleID = msg?.Length > 1 ? msg[1] : string.Empty;
        }
        else
        {
            Error = isDataUpdated;
        }
    }
}