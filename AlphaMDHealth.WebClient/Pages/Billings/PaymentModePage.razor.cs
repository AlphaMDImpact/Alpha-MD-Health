using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;
public partial class PaymentModePage :BasePage
{
    private readonly PaymentModeDTO _paymentModeData = new PaymentModeDTO { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// PaymentMode ID Parameter
    /// </summary>
    [Parameter]
    public byte PaymentModeID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _paymentModeData.PaymentMode = new PaymentModeModel
        {
            PaymentModeID = PaymentModeID
        };
        await SendServiceRequestAsync(new PaymentModeService(AppState.webEssentials).SyncPaymentModesFromServerAsync(_paymentModeData, CancellationToken.None), _paymentModeData).ConfigureAwait(true);
        if (_paymentModeData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_paymentModeData.FeaturePermissions, AppPermissions.PaymentModeAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_paymentModeData.ErrCode.ToString());
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(PaymentModeModel.Name),
            ResourceKey= ResourceConstants.R_PAYMENT_MODE_NAME_KEY,
        },
    };

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
                new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
                new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _paymentModeData.IsActive = false;
                await SavePaymentModeAsync(_paymentModeData).ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            PaymentModeDTO paymentModeData = new PaymentModeDTO
            {
                PaymentMode = new PaymentModeModel { PaymentModeID = PaymentModeID },
                PaymentModes = _paymentModeData.PaymentModes
            };
            await SavePaymentModeAsync(paymentModeData).ConfigureAwait(true);
        }
    }

    private async Task SavePaymentModeAsync(PaymentModeDTO paymentModeData)
    {
        await SendServiceRequestAsync(new PaymentModeService(AppState.webEssentials).SyncPaymentModeToServerAsync(paymentModeData, CancellationToken.None), paymentModeData).ConfigureAwait(true);
        if (paymentModeData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(paymentModeData.ErrCode.ToString());
        }
        else
        {
            Error = paymentModeData.ErrCode.ToString();
        }
    }
}

