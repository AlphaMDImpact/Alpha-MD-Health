using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class DashboardPage : BasePage
{
    private readonly DashboardDTO _pageData = new DashboardDTO() { RecordCount = -2, ConfigurationRecord = new ConfigureDashboardModel() };
    private bool _isDashboardPage = true;

    /// <summary>
    /// Resource key to display message into page
    /// </summary>
    [Parameter]
    public string Key{ get;set; }

    protected override async Task OnInitializedAsync()
    {
        await GetPageDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await GetPageDataAsync();
        await base.OnParametersSetAsync();
    }

    private async Task GetPageDataAsync()
    {
        _isDashboardPage = string.IsNullOrWhiteSpace(Key) && AppState.RouterData.SelectedRoute.Page == AppPermissions.DashboardView.ToString();
        _pageData.SelectedUserID = _isDashboardPage ? 0 : AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await SendServiceRequestAsync(new DashboardService(AppState.webEssentials).GetDashboardDataAsync(_pageData), _pageData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private async Task OnAddButtonClickedAsync(object o)
    {
        await NavigateToAsync(AppPermissions.DashboardConfigurationView.ToString()).ConfigureAwait(true);
    }

}