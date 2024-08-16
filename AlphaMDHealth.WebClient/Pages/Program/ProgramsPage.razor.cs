using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProgramsPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO();
    private bool _shouldShowSubscribedPopUp;
    private List<ButtonActionModel> _actionButtons;
    private long _programID;

    /// <summary>
    /// Program ID parameter
    /// </summary>
    [Parameter]
    public long ProgramID
    {
        get { return _programID; }
        set
        {
            if (_programID != value)
            {
                if (_programData.RecordCount > 0 || _programID == 0)
                {
                    _programData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _programID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _programData.Program = new ProgramModel { ProgramID = 0 };
        await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
    }

    private async Task RetrieveDataAndDisplayAsync()
    {
        if (Parameters?.Count > 0)
        {
            _programData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ProgramDTO.RecordCount)));
        }
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        _actionButtons = new List<ButtonActionModel>();
        if (AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0) > 1
            && LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramSubscribeView.ToString()))
        {
            _actionButtons.Add(new ButtonActionModel
            {
                ButtonResourceKey = ResourceConstants.R_SUBSCRIBE_PROGRAM_KEY,
                ButtonAction = OnSubscriptionProgramClickedAsync
            });
        }
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        if (AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0) > 1)
        {
            return new List<TableDataStructureModel>
            {
                new TableDataStructureModel{DataField=nameof(ProgramModel.ProgramID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
                new TableDataStructureModel{DataField=nameof(ProgramModel.Name),DataHeader=ResourceConstants.R_PROGRAM_NAME_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.AddedOnString),DataHeader=ResourceConstants.R_PROGRAM_TYPE_KEY,IsHtmlTag=true, IsSortable = true},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfTasks),DataHeader=ResourceConstants.R_NUMBER_OF_TASKS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfSubflows),DataHeader=ResourceConstants.R_NUMBER_OF_SUBFLOWS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfDefaultCareGivers),DataHeader=ResourceConstants.R_NUMBER_OF_CAREGIVERS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfReadings),DataHeader=ResourceConstants.R_NUMBER_OF_READINGS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfEducations),DataHeader=ResourceConstants.R_NUMBER_OF_EDUCATIONS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfMedications),DataHeader=ResourceConstants.R_NUMBER_OF_MEDICATIONS_KEY},
            };
        }
        else
        {
            return new List<TableDataStructureModel>
            {
                new TableDataStructureModel{DataField=nameof(ProgramModel.ProgramID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
                new TableDataStructureModel{DataField=nameof(ProgramModel.Name),DataHeader=ResourceConstants.R_PROGRAM_NAME_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfTasks),DataHeader=ResourceConstants.R_NUMBER_OF_TASKS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfSubflows),DataHeader=ResourceConstants.R_NUMBER_OF_SUBFLOWS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfDefaultCareGivers),DataHeader=ResourceConstants.R_NUMBER_OF_CAREGIVERS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfReadings),DataHeader=ResourceConstants.R_NUMBER_OF_READINGS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfEducations),DataHeader=ResourceConstants.R_NUMBER_OF_EDUCATIONS_KEY},
                new TableDataStructureModel{DataField=nameof(ProgramModel.NoOfMedications),DataHeader=ResourceConstants.R_NUMBER_OF_MEDICATIONS_KEY},
            };
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.ProgramsView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClickedAsync(ProgramModel programData)
    {
        Success = Error = string.Empty;
        if (_programData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.ProgramsView.ToString(), (programData == null ? 0 : programData.ProgramID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _programID = programData == null ? 0 : programData.ProgramID;
            ShowDetailPage = true;
        }
    }

    private async Task OnAddEditPageClosedAsync(string actionKey)
    {
        ShowDetailPage = false;
        _programID = 0;
        Success = Error = string.Empty;
        _isDataFetched = false;
        await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
    }

    private void OnSubscriptionProgramClickedAsync()
    {
        ShowDetailPage = _shouldShowSubscribedPopUp = true;
    }

    private async Task OnProgramSubscribePageClosedAsync(string actionKey)
    {
        ShowDetailPage = _shouldShowSubscribedPopUp = false;
        Success = Error = string.Empty;
        if (actionKey == ResourceConstants.R_SAVE_ACTION_KEY)
        {
            Success = ErrorCode.OK.ToString();
        }
        else
        {
            _isDataFetched = false;
            await RetrieveDataAndDisplayAsync().ConfigureAwait(true);
        }
    }
}