using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class BillingItemsPage :BasePage
{
    private readonly BillingItemDTO _billingItemData= new BillingItemDTO { BillingItem = new BillingItemModel() };
    private short _billingItemID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new BillingItemService(AppState.webEssentials).SyncBillingItemsFromServerAsync(_billingItemData, CancellationToken.None), _billingItemData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(BillingItemModel.BillingItemID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(BillingItemModel.Name), DataHeader=ResourceConstants.R_BILLING_ITEM_NAME_KEY },
        };
    }

    private void OnAddEditClick(BillingItemModel billingItemData)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _billingItemID = billingItemData == null ? (short)0 : billingItemData.BillingItemID;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _billingItemID = 0;
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