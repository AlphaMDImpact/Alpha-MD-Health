using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class CreditTransacationPage : BasePage
{
    private readonly HealthScanDTO _healthScanData = new HealthScanDTO { RecordCount = -1 };
    private HealthScanService _healthScanService;
    private double? _totalAmount;
    private decimal _charges;
    private bool _isEditable;
    private bool _isFormValid;

    /// <summary>
    /// Transacation ID parameter
    /// </summary>
    [Parameter]
    public long TransactionID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _healthScanService = new HealthScanService(AppState.webEssentials);
        _healthScanData.ExternalServiceTransaction = new HealthScanModel{TransactionID = TransactionID};
        await SendServiceRequestAsync(_healthScanService.GetHealthScansAsync(_healthScanData), _healthScanData).ConfigureAwait(true);
        if (_healthScanData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_healthScanData.FeaturePermissions, AppPermissions.CreditAddEdit.ToString()) && TransactionID == 0;
            _isDataFetched = true;  
        }
        else
        {
            await OnClose.InvokeAsync(_healthScanData.ErrCode.ToString());
        }       
    }

    private void OnSaveButtonClicked()
    {
        _healthScanData.Resources.Find(x => x.ResourceKey == ResourceConstants.R_QUANTITY_TO_BUY_KEY).MinLength = _healthScanData.ExternalServiceTransaction.MinimumQuantityToBuy.Value;
        Success = Error = string.Empty;
        if(IsValid())
        {
            _isFormValid = true;
        };
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void CalculateDiscountAndTotalAmount()
    {
        _totalAmount = _healthScanData.ExternalServiceTransaction.UnitPrice * _healthScanData.ExternalServiceTransaction.Quantity ?? 0;
        _healthScanData.ExternalServiceTransaction.DiscountPrice = (_totalAmount * _healthScanData.ExternalServiceTransaction.DiscountPercentage) / 100;
        _healthScanData.ExternalServiceTransaction.TotalPrice = _totalAmount - _healthScanData.ExternalServiceTransaction.DiscountPrice;
        _charges = Convert.ToDecimal(_healthScanData.ExternalServiceTransaction.TotalPrice);
    }

    private async Task OnPaymentResponseReceivedAsync(RazorpayPaymentModel responseData)
    {    
        if (responseData.Status != null && responseData.Status == "captured")
        {
            HealthScanDTO _careFlixHealthScan = new HealthScanDTO
            {
                ExternalServiceTransaction = new HealthScanModel
                {
                    TransactionDateTime = _healthScanData.ExternalServiceTransaction.TransactionDateTime?.ToUniversalTime(),
                    PaymentID = responseData.PaymentID,
                    IsPatient = false,
                    Quantity = _healthScanData.ExternalServiceTransaction.Quantity,
                    UnitPrice = _healthScanData.ExternalServiceTransaction.UnitPrice,
                    DiscountPercentage = _healthScanData.ExternalServiceTransaction.DiscountPercentage,
                    DiscountPrice = _healthScanData.ExternalServiceTransaction.DiscountPrice,
                    TotalPrice = _healthScanData.ExternalServiceTransaction.TotalPrice,
                },
            };
            await SendServiceRequestAsync(new HealthScanService(AppState.webEssentials).SyncHealthScansToServerAsync(_careFlixHealthScan, CancellationToken.None, false), _careFlixHealthScan).ConfigureAwait(true);
            if (_careFlixHealthScan.ErrCode == ErrorCode.OK)
            {
                await OnClose.InvokeAsync(_careFlixHealthScan.ErrCode.ToString());
            }
            else
            {
                Error = _careFlixHealthScan.ErrCode.ToString();
            }
        }
        else
        {
            Error = ResourceConstants.R_PAYMENT_FAILED_KEY;
        }
        _isFormValid = false;
    }
}
