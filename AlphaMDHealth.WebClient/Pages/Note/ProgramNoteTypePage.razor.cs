using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProgramNoteTypePage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { ProgramNote = new ProgramNoteModel() };
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    /// <summary>
    /// selected Program Note ID
    /// </summary>
    [Parameter]
    public long ProgramNoteID { get; set; }

    /// <summary>
    /// is Program Synced
    /// </summary>
    [Parameter]
    public bool IsSynced { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programData.ProgramNote.ProgramID = ProgramID;
        _programData.ProgramNote.ProgramNoteID = ProgramNoteID;
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramNotesFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            if (ProgramNoteID == 0 || AppState.MasterData.OrganisationID == _programData.ProgramNotes.FirstOrDefault().OrganisationID)
            {
                IsSynced = true;
            }
            _isEditable = IsSynced && LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.PatientProviderNoteTypeAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(ProgramNoteModel.NoteText),
            ResourceKey= ResourceConstants.R_NOTE_TYPE_KEY,
        },
        new TabDataStructureModel
        {
            DataField=nameof(ProgramNoteModel.NoteDescription),
            ResourceKey= ResourceConstants.R_NOTE_DESCRIPTION_KEY,
        }
    };

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
             new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
             new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
         };
        _hideConfirmationPopup = false;
    }
    
    private async Task DeletePopUpCallbackAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _programData.ProgramNote.IsActive = false;
                await SaveProgramNoteAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            ProgramNoteModel programNoteData = _programData.ProgramNotes.FirstOrDefault(x => x.LanguageID == AppState.SelectedLanguageID);
            _programData.ProgramNote.ProgramNoteID = ProgramNoteID;
            _programData.ProgramNote.QuestionnaireID = _programData.Items.FirstOrDefault(x => x.IsSelected == true).OptionID;
            _programData.ProgramNote.IsActive = true;
            _programData.ProgramNote.NoteText = programNoteData?.NoteText;
            await SaveProgramNoteAsync().ConfigureAwait(true);
        }
    }

    private async Task SaveProgramNoteAsync()
    {
        _programData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync( new ProgramService(AppState.webEssentials).SyncProgramNoteToServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_programData.ErrCode.ToString());
        }
        else
        {
            Error = _programData.ErrCode.ToString();
        }
    }
}