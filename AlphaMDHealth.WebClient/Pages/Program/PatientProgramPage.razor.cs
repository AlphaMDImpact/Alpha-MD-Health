using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientProgramPage : BasePage
{
    private readonly PatientProgramDTO _patientProgramData = new PatientProgramDTO { RecordCount = -1, PatientProgram = new PatientProgramModel() };
    private List<OptionModel> _trackerTypeOptionList = new List<OptionModel>();
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmationPopup = true;
    private bool _isSaveButtonClicked;
    private double? _days;
    private bool _isEditable;

    /// <summary>
    /// Patient ID parameter
    /// </summary>
    [Parameter]
    public long PatientID { get; set; }

    /// <summary>
    /// Program ID parameter
    /// </summary>
    [Parameter]
    public long PatientProgramID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _patientProgramData.PatientPrograms = new List<PatientProgramModel>();
        _patientProgramData.PatientProgram.PatientProgramID = PatientProgramID;
        await SendServiceRequestAsync(new PatientProgramService(AppState.webEssentials).GetPatientProgramsAsync(_patientProgramData), _patientProgramData).ConfigureAwait(true);
        if (_patientProgramData.ErrCode == ErrorCode.OK)
        {
            if (PatientProgramID > 0)
            {
                FilterTrackerOptions(_patientProgramData?.PatientProgram?.ProgramID);
            }
            else
            {
                _days = Convert.ToDouble(Constants.CONSTANT_ZERO);
            }
            _isEditable = LibPermissions.HasPermission(_patientProgramData.FeaturePermissions, AppPermissions.PatientProgramAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_patientProgramData.ErrCode.ToString());
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            _isSaveButtonClicked = true;
            if (PatientProgramID > 0)
            {
                IntializeActionButtons();
                _hideConfirmationPopup = false;
            }
            else
            {
                _patientProgramData.PatientProgram.IsActive = true;
                await SavePatientProgramAsync();
            }          
        }
    }

    private void OnProgramSelectionChange(object option)
    {
        if (!string.IsNullOrEmpty(option as string))
        {
            long optionId = Convert.ToInt64(option);
            _patientProgramData.PatientProgram.ProgramID = optionId;
            FilterTrackerOptions(optionId);
        }
    }

    private void FilterTrackerOptions(long? optionId)
    {
        _trackerTypeOptionList = (from type in _patientProgramData?.TrackerTypes
                                  where type.ParentOptionID == optionId
                                  select GetTrackerType(type)).ToList();
    }

    private OptionModel GetTrackerType(OptionModel type)
    {
        type.IsSelected = type.OptionID == _patientProgramData?.PatientProgram?.TrackerID;
        return type;
    }

    private void OnEndPointSelectionChange(object option)
    {
        if (option != null)
        {
            if (string.IsNullOrWhiteSpace(option.ToString()))
            {
                RemoveControlContainsKey(ResourceConstants.R_DAY_OPTION_KEY);
                RemoveControlContainsKey(ResourceConstants.R_DATE_OPTION_KEY);
                RemoveControlContainsKey(ResourceConstants.R_TRACKER_OPTION_KEY);                
                _patientProgramData.PatientProgram.EntryTypeID = 0;
                _patientProgramData.PatientProgram.EntryDate = DateTimeOffset.Now.Date;
                _trackerTypeOptionList.ForEach(x => x.IsSelected = false);
            }
            else
            {
                switch(Convert.ToInt64(option))
                {
                    case ResourceConstants.R_TRACKER_OPTION_ID:
                        RemoveControlContainsKey(ResourceConstants.R_PATIENT_PROGRAM_DAYS_KEY);
                        break;
                    case ResourceConstants.R_DATE_OPTION_ID:
                        RemoveControlContainsKey(ResourceConstants.R_PATIENT_PROGRAM_TRACKER_KEY);
                        RemoveControlContainsKey(ResourceConstants.R_PATIENT_PROGRAM_DAYS_KEY);
                        break;
                    case ResourceConstants.R_DAY_OPTION_ID:
                        RemoveControlContainsKey(ResourceConstants.R_PATIENT_PROGRAM_TRACKER_KEY);
                        RemoveControlContainsKey(ResourceConstants.R_PATIENT_PROGRAM_DATE_KEY);
                        break;
                }

                _patientProgramData.PatientProgram.EntryTypeID = Convert.ToInt32(option);
                if (_patientProgramData.PatientProgram?.EntryTypeID == ResourceConstants.R_DAY_OPTION_ID)
                {
                    _days = Convert.ToDouble(_patientProgramData.PatientProgram.EntryPoint);
                }
                else if (_patientProgramData.PatientProgram.EntryDate == DateTimeOffset.MinValue)
                {
                    _patientProgramData.PatientProgram.EntryDate = DateTimeOffset.Now.Date;
                }
            }          
        }
    }

    private void OnDeleteButtonClicked()
    {
        _isSaveButtonClicked = false;
        IntializeActionButtons();
        _hideConfirmationPopup = false;
    }

    private void IntializeActionButtons()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
    }

    private async Task OnConfirmationPopUpClickedAsync(object e)
    {
        _hideConfirmationPopup = true;
        Success = Error = string.Empty;
        if (e != null)
        {
            if (Convert.ToInt64(e) == 1)
            {
                _patientProgramData.PatientProgram.IsActive = _isSaveButtonClicked;
                await SavePatientProgramAsync();
            }
        }
    }

    private async Task SavePatientProgramAsync()
    {
        _patientProgramData.PatientProgram = new PatientProgramModel
        {
            PatientProgramID = PatientProgramID,
            ProgramID = _patientProgramData.PatientProgram.ProgramID,
            PatientID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
            IsActive = _patientProgramData.PatientProgram.IsActive,
            EntryDate = _patientProgramData.PatientProgram.EntryDate,
            EntryTypeID = (int)(_patientProgramData.EndPointTypes.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0),
            ProgramEntryPoint = _days.ToString(),
            TrackerID = (short)(_trackerTypeOptionList.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0),
            AddedON = GenericMethods.GetUtcDateTime,
        };
        await SendServiceRequestAsync(new PatientProgramService(AppState.webEssentials).SavePatientProgramsToServerAsync(_patientProgramData, CancellationToken.None), _patientProgramData).ConfigureAwait(true);
        if (_patientProgramData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_patientProgramData.ErrCode.ToString());
        }
        else
        {
            Error = _patientProgramData.ErrCode.ToString();
        }
    }
}