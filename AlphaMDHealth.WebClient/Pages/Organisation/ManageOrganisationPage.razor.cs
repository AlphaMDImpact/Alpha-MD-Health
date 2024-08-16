using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ManageOrganisationPage :BasePage
{
    /// <summary>
    /// OrganizationID ID
    /// </summary>
    [Parameter]
    public long OrganizationID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isDataFetched = true;
    }

    public void HideOrganisationProfilePage()
    {
        ShowDetailPage = true;
        StateHasChanged();
    }
}