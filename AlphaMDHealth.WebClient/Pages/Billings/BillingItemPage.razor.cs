using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class BillingItemPage :BasePage
{
    private readonly BillingItemDTO _billingItemData = new BillingItemDTO { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Billing Item ID Parameter
    /// </summary>
    [Parameter]
    public short BillingItemID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _billingItemData.BillingItem = new BillingItemModel
        {
            BillingItemID = BillingItemID
        };
        await SendServiceRequestAsync(new BillingItemService(AppState.webEssentials).SyncBillingItemsFromServerAsync(_billingItemData, CancellationToken.None), _billingItemData).ConfigureAwait(true);
        if (_billingItemData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_billingItemData.FeaturePermissions, AppPermissions.BillingItemAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_billingItemData.ErrCode.ToString());
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(BillingItemModel.Name),
            ResourceKey= ResourceConstants.R_BILLING_ITEM_NAME_KEY,
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
                _billingItemData.IsActive = _billingItemData.BillingItem.IsActive= false;
                await SaveBillingItemAsync(_billingItemData).ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            BillingItemDTO billingItemData = new BillingItemDTO
            {
                BillingItem = new BillingItemModel { BillingItemID = BillingItemID },
                BillingItems = _billingItemData.BillingItems
            };
            await SaveBillingItemAsync(billingItemData).ConfigureAwait(true);
        }
    }

    private async Task SaveBillingItemAsync(BillingItemDTO billingItemData)
    {
        await SendServiceRequestAsync(new BillingItemService(AppState.webEssentials).SyncBillingItemToServerAsync(billingItemData, CancellationToken.None), billingItemData).ConfigureAwait(true);
        if (billingItemData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(billingItemData.ErrCode.ToString());
        }
        else
        {
            Error = billingItemData.ErrCode.ToString();
        }
    }
}

