using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class PaymentModesPage :BasePage
{
    private readonly PaymentModeDTO _paymentModeData = new PaymentModeDTO { PaymentMode = new PaymentModeModel() };
    private byte _paymentModeID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new PaymentModeService(AppState.webEssentials).SyncPaymentModesFromServerAsync(_paymentModeData, CancellationToken.None), _paymentModeData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(PaymentModeModel.PaymentModeID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(PaymentModeModel.Name), DataHeader=ResourceConstants.R_PAYMENT_MODE_NAME_KEY },
        };
    }

    private void OnAddEditClick(PaymentModeModel paymentModeData)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _paymentModeID = paymentModeData == null ? (byte)0 : paymentModeData.PaymentModeID;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _paymentModeID = 0;
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
