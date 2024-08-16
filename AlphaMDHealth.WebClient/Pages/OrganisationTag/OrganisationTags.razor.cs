using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;
public partial class OrganisationTags : BasePage
{
    private readonly OrganisationTagDTO _organisationTagData = new OrganisationTagDTO { OrganisationTag = new OrganisationTagModel() };
    private long _organisationTagId; 
    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new OrganisationTagService(AppState.webEssentials).SyncOrganisationTagsFromServerAsync(_organisationTagData, CancellationToken.None), _organisationTagData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(OrganisationTagModel.OrganisationTagID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(OrganisationTagModel.TagText), DataHeader=ResourceConstants.R_ORGANISATION_TAG_TEXT_KEY },
        };
    }

    private void OnAddEditClick(OrganisationTagModel organisationTagData)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _organisationTagId = organisationTagData == null ? 0 : organisationTagData.OrganisationTagID;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _organisationTagId = 0;
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
