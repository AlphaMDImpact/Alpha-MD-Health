using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class ProfessionsPage : BasePage
{
    private readonly ProfessionDTO _professionData = new ProfessionDTO { Profession = new ProfessionModel() };
    private byte _professionID;

    protected override async Task OnInitializedAsync()
    {
        await RenderPageDataAsync().ConfigureAwait(true);
    }

    private async Task RenderPageDataAsync()
    {
        await SendServiceRequestAsync(new ProfessionService(AppState.webEssentials).SyncProfessionsFromServerAsync(_professionData), _professionData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField = nameof(ProfessionModel.ProfessionID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false},
            new TableDataStructureModel{DataField = nameof(ProfessionModel.Profession), DataHeader = ResourceConstants.R_PROFESSION_NAME_TEXT_KEY},
        };
    }

    private void OnAddEditClick(ProfessionModel profession)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        _professionID = profession == null ? (byte)0 : profession.ProfessionID;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _professionID = 0;
        Success = Error = string.Empty;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await RenderPageDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }
}
