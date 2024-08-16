using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class InstructionsPage
{
    private readonly ProgramDTO _programData = new ProgramDTO();
    private long _instructionID;

    protected override async Task OnInitializedAsync()
    {
        _programData.Instruction = new InstructionModel { InstructionID = 0 };
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncInstructionsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(InstructionModel.InstructionID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(InstructionModel.Name),DataHeader=ResourceConstants.R_INSTRUCTION_NAME_KEY},
        };
    }

    private void OnAddEditClick(InstructionModel instructionData)
    {
        Success = Error = string.Empty;
        _instructionID = instructionData == null ? 0 : instructionData.InstructionID;
        ShowDetailPage = true;
    }

    private async Task OnAddEditClosedAsync(string errorMessage)
    {
        ShowDetailPage = false;
        _instructionID = 0;
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