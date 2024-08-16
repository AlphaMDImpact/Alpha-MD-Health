using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;
public partial class ConsentsPage : BasePage
{
    private readonly ConsentDTO _consentData = new ConsentDTO();
    private long _consentID; 

    protected override async Task OnInitializedAsync()
    {
        await GetConsentsDataAsync().ConfigureAwait(true);
    }

    private async Task GetConsentsDataAsync()
    {
        _consentData.Consent = new ConsentModel();
        await SendServiceRequestAsync(new ConsentService(AppState.webEssentials).GetConsentsAsync(_consentData), _consentData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel { DataField=nameof(ConsentModel.ConsentID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel { DataField=nameof(ConsentModel.RoleName), DataHeader=ResourceConstants.R_ROLES_KEY },
            new TableDataStructureModel { DataField=nameof(ConsentModel.ConsentName), DataHeader=ResourceConstants.R_CONSENT_PAGE_KEY },
            new TableDataStructureModel { DataField=nameof(ConsentModel.IsRequired), DataHeader=ResourceConstants.R_IS_REQUIRED_KEY },
            new TableDataStructureModel { DataField=nameof(ConsentModel.SequenceNo), DataHeader=ResourceConstants.R_SEQUENCE_NO_KEY },
        };
    }

    private void OnAddEditClick(ConsentModel consent)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _consentID = consent == null ? 0 : consent.ConsentID;
    }

    private async Task OnAddEditClosedAsync(string isDataUpdated)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _consentID = 0;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await GetConsentsDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = isDataUpdated;
        }
    }
}