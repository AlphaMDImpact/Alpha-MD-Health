using System.Globalization;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class ManageOrganisationsPage : BasePage
{
    private readonly OrganisationDTO _organizationData = new OrganisationDTO { OrganisationProfile = new OrganisationModel(), PageDetails = new List<ContentDetailModel>() };

    protected override async Task OnInitializedAsync()
    {
        _organizationData.OrganisationProfile = new OrganisationModel { OrganisationID = 0 };
        _organizationData.RecordCount = -2;
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        if (Parameters?.Count > 0)
        {
            _organizationData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ProgramDTO.RecordCount)));
        }
        await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationsFromServerAsync(_organizationData, CancellationToken.None), _organizationData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(OrganisationModel.OrganisationID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(OrganisationModel.OrganisationName),DataHeader=ResourceConstants.R_ORGANISATION_NAME_KEY, IsSearchable = true},
            new TableDataStructureModel{DataField=nameof(OrganisationModel.OrganisationDomain),DataHeader=ResourceConstants.R_DOMAIN_KEY, IsSearchable = true},
            new TableDataStructureModel{DataField=nameof(OrganisationModel.TaxNumber),DataHeader=ResourceConstants.R_TAX_NUMBER_KEY, IsSearchable = true},
            new TableDataStructureModel{DataField=nameof(OrganisationModel.AddedOnString),DataHeader=ResourceConstants.R_REGISTERED_ON_KEY,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(OrganisationModel.NoOfEmployee),DataHeader=ResourceConstants.R_NUMBER_OF_EMPLOYEES_KEY, IsSearchable = false},
            new TableDataStructureModel{DataField=nameof(OrganisationModel.NoOfPatient),DataHeader=ResourceConstants.R_NUMBER_OF_PATIENTS_KEY, IsSearchable = false},
            new TableDataStructureModel{DataField=nameof(OrganisationModel.CurrentStatus),DataHeader=ResourceConstants.R_STATUS_KEY, IsSearchable = true ,IsHtmlTag=true},
        };
    }

    private async Task OnAddEditClickAsync(OrganisationModel program)
    {
        Success = Error = string.Empty;
        await NavigateToAsync(AppPermissions.ManageOrganisationView.ToString(), Convert.ToString(program == null ? -1 : program.OrganisationID, CultureInfo.InvariantCulture)).ConfigureAwait(false);
    }
}