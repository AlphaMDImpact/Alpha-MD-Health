using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
namespace AlphaMDHealth.WebClient;

public partial class PatientScanHistory : BasePage
{
    private PatientScanHistoryDTO _patientScanHistoryData;
    private List<CardModel> _healthScanCards;
    private bool _showLoadMore;
    private IList<ButtonActionModel> _actionButtons;

    protected override async Task OnInitializedAsync()
    {
        _patientScanHistoryData = new PatientScanHistoryDTO();
        _patientScanHistoryData.RecordCount = 10;
        await GetPatientScanHistoryData();
        if (IsPatientMobileView)
        {
            _actionButtons ??= new List<ButtonActionModel>();
            _actionButtons.Add(new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                ButtonAction = () => { OnBackButtonClicked(); },
                Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
            });
        }
    }

    private async Task GetPatientScanHistoryData()
    {
        await SendServiceRequestAsync(new PatientScanHistoryService(AppState.webEssentials).SyncPatientScanHistoryFromServerAsync(_patientScanHistoryData, CancellationToken.None), _patientScanHistoryData).ConfigureAwait(true);
        if (_patientScanHistoryData.ErrCode == ErrorCode.OK)
        {
            _healthScanCards = new List<CardModel>();
            if (GenericMethods.IsListNotEmpty(_patientScanHistoryData.PatientScanHistoryData))
            {
                AddInCards(ResourceConstants.R_SCANS_KEY, _patientScanHistoryData.PatientScanHistoryData.FirstOrDefault()?.TotalScans.ToString());
                AddInCards(ResourceConstants.R_USED_SCANS_KEY, _patientScanHistoryData.PatientScanHistoryData.FirstOrDefault()?.UsedScans.ToString());
                AddInCards(ResourceConstants.R_REMAINING_SCANS_KEY, (_patientScanHistoryData.PatientScanHistoryData.FirstOrDefault()?.TotalScans - _patientScanHistoryData.PatientScanHistoryData.FirstOrDefault()?.UsedScans).ToString());
            }
            _showLoadMore = true;
            _isDataFetched = true;
        }
    }

    private async void OnBackButtonClicked()
    {
        await NavigateToAsync(AppPermissions.PatientMobileMenusView.ToString()).ConfigureAwait(false);
    }

    private AmhViewCellModel GenerateTableStructure()
    {
        return new AmhViewCellModel
        {
            LeftHeader = nameof(PatientScanHistoryModel.ScanPurchaseDateTime)
        };
    }

    private async void OnLoadMoreClicked()
    {
        _patientScanHistoryData.RecordCount = _patientScanHistoryData.RecordCount + 10;
        await GetPatientScanHistoryData();
        if(_patientScanHistoryData.RecordCount > _patientScanHistoryData.PatientScanHistoryData.Count())
        {
            _showLoadMore = false;
        }
        StateHasChanged();
    }

    private void AddInCards(string header, string subHeader)
    {
        _healthScanCards.Add(new CardModel
        {
            Header = LibResources.GetResourceValueByKey(_patientScanHistoryData.Resources, header),
            ImageBase64 = ImageConstants.I_BARCODE_ICON,
            SubHeader = subHeader
        });
    }
}
