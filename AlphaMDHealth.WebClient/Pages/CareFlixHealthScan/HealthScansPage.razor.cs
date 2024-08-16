using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class HealthScansPage : BasePage
{
    private readonly HealthScanDTO _healthScanData = new HealthScanDTO { ExternalServiceTransaction = new HealthScanModel()};
    private List<CardModel> _healthScanCards;
    private long _transactionID;
    
    protected override async Task OnInitializedAsync()
    {
        await GetHealthScansDataAsync().ConfigureAwait(true);
    }

    private async Task GetHealthScansDataAsync()
    {
        await SendServiceRequestAsync(new HealthScanService(AppState.webEssentials).GetHealthScansAsync(_healthScanData), _healthScanData).ConfigureAwait(true);
        if (_healthScanData.ErrCode == ErrorCode.OK)
        {
            _healthScanCards = new List<CardModel>();
            AddInCards(ResourceConstants.R_ORGANISATION_CREDITS_KEY, _healthScanData.OrganisationCredits.ToString());
            AddInCards(ResourceConstants.R_ASSIGNED_CREDITS_KEY, _healthScanData.CreditsAssigned.ToString());
            AddInCards(ResourceConstants.R_AVAILABLE_CREDITS_KEY, _healthScanData.CreditsAvailable.ToString());
            AddInCards(ResourceConstants.R_TOTAL_PATIENTS_KEY, _healthScanData.NumberOfPatient.ToString());
            _isDataFetched = true;
        }
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(HealthScanModel.TransactionID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(HealthScanModel.TrasactionDateTimeValue),DataHeader=ResourceConstants.R_DATE_TIME_KEY, MaxColumnWidthSize="60vh"},
            new TableDataStructureModel{DataField=nameof(HealthScanModel.Quantity),DataHeader=ResourceConstants.R_QUANTITY_KEY, MaxColumnWidthSize="15vh"},
            new TableDataStructureModel{DataField=nameof(HealthScanModel.UnitPrice),DataHeader=ResourceConstants.R_UNIT_PRICE_KEY, MaxColumnWidthSize = "15vh"},
            new TableDataStructureModel{DataField=nameof(HealthScanModel.Discount),DataHeader=ResourceConstants.R_DISCOUNT_PERCENTAGE_KEY, MaxColumnWidthSize="15vh"},
            new TableDataStructureModel{DataField=nameof(HealthScanModel.TotalPrice),DataHeader=ResourceConstants.R_TOTAL_AMOUNT_KEY, MaxColumnWidthSize = "15vh"},
        };
    }

    private void OnAddEditClicked(HealthScanModel healthScan)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _transactionID = healthScan == null ? 0 : healthScan.TransactionID;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _transactionID = 0;
        Success = Error = string.Empty;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await GetHealthScansDataAsync();
        }
        else
        {
            Error = errorMessage;
        }
    }

    private void AddInCards(string header, string subHeader)
    {
        _healthScanCards.Add(new CardModel
        {
            Header = LibResources.GetResourceValueByKey(_healthScanData.Resources, header),
            ImageBase64 = ImageConstants.I_BARCODE_ICON,
            SubHeader = subHeader
        });
    }
}