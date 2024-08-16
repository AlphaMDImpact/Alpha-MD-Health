using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;
public partial class ProgramEducationPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { ProgramEducation = new PatientEducationModel() };
    private List<OptionModel> _educations = new List<OptionModel>();
    private double? _assignAfterDays;
    private double? _assignForDays;
    private bool _hideConfirmationPopup = true;
    private List<ButtonActionModel> _actionData;
    private bool _isEditable = false;
    private ProgramService _programService;

    /// <summary>
    /// IsSynced
    /// </summary>
    [Parameter]
    public bool IsSynced { get; set; }

    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    /// <summary>
    ///  Program Duration
    /// </summary>
    [Parameter]
    public int ProgramDuration { get; set; }

    /// <summary>
    /// selected caregiver data
    /// </summary>
    [Parameter]
    public long SelectedProgramEducationID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programService = new ProgramService(AppState.webEssentials);
        _programData.ProgramEducation.ProgramID = ProgramID;
        _programData.ProgramEducation.ProgramEducationID = SelectedProgramEducationID;
        _programData.ProgramEducation.IsSynced = IsSynced;
        await SendServiceRequestAsync(_programService.SyncProgramEducationFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            if (SelectedProgramEducationID > 0)
            {
                _educations = _programData.Items.Where(x => x.ParentOptionID == _programData.ProgramEducation.PatientEducationID || x.OptionID == -1).ToList();
                _assignAfterDays = _programData.ProgramEducation.AssignAfterDays;
                _assignForDays = _programData.ProgramEducation.AssignForDays;
            }
            _isEditable = IsSynced;
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(null).ConfigureAwait(true);
        }
    }

    private async void OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            ProgramDTO programData = new ProgramDTO
            {
                ProgramEducation = new PatientEducationModel
                {
                    ProgramID = ProgramID,
                    ProgramEducationID = SelectedProgramEducationID,
                    IsActive = true,
                    PageID = _programData.ProgramEducation.PageID,
                    AssignAfterDays = Convert.ToInt16(_assignAfterDays, CultureInfo.InvariantCulture),
                    AssignForDays = Convert.ToInt16(_assignForDays, CultureInfo.InvariantCulture),
                    ForProviders = _programData.ProgramEducation.ForProviders
                }
            };
            var result = _programService.ValidateAssingAfterAndShowForDate(ProgramDuration, programData.ProgramEducation.AssignAfterDays, programData.ProgramEducation.AssignForDays, _programData.Resources);
            if (!(result.isValidAssignAfterDay && result.isValidShowForAfterDay))
            {
                Error = string.Format(CultureInfo.InvariantCulture, result.errorCode.ToString()).ToString();
                return;
            }
            await SaveEducationAsync(programData).ConfigureAwait(true);
        }
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task DeletePopUpCallbackAsync(object value)
    {
        Success = Error = string.Empty;
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _programData.ProgramEducation.IsActive = false;
                ProgramDTO programData = new ProgramDTO
                {
                    ProgramEducation = _programData.ProgramEducation
                };
                await SaveEducationAsync(programData).ConfigureAwait(true);
            }
        }
    }

    private void OnCategoryChange(object? e)
    {
        int.TryParse(e as string, out var optionId);
        _educations = _programData.Items.Where(x => x.ParentOptionID == optionId || x.OptionID == -1).ToList();
    }

    private void OnEducationChange(object? e)
    {
        int.TryParse(e as string, out var optionId);
        _programData.ProgramEducation.PageID = optionId;
    }

    private List<OptionModel> GetOptions()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 1, OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_FOR_PROVIDERS_KEY),IsSelected=_programData.ProgramEducation.ForProviders }
        };
    }

    private async Task SaveEducationAsync(ProgramDTO programData)
    {
        await SendServiceRequestAsync(_programService.SyncProgramEducationToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        if (programData.ErrCode == ErrorCode.OK)
        {
            programData.ProgramEducation.PageHeading = _programData.Items.First(x => x.OptionID == _programData.ProgramEducation.PageID).OptionText;
            await OnClose.InvokeAsync(programData.ErrCode.ToString());
        }
        else
        {
            Error = programData.ErrCode.ToString();
            StateHasChanged();
        }
    }
}