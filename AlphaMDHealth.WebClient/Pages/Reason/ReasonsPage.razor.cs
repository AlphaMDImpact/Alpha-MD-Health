using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class ReasonsPage : BasePage
{
    private readonly ReasonDTO _reasonData = new ReasonDTO { Reason = new ReasonModel() };
    private byte _reasonID;

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new ReasonService(AppState.webEssentials).SyncReasonsFromServerAsync(_reasonData, CancellationToken.None), _reasonData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(ReasonModel.ReasonID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(ReasonModel.Reason), DataHeader=ResourceConstants.R_REASON_TEXT_KEY },
        };
    }

    private void OnAddEditClick(ReasonModel reasonData)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _reasonID = reasonData == null ? (byte)0 : reasonData.ReasonID;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _reasonID = 0;
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