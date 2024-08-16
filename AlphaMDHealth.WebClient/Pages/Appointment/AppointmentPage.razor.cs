using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class AppointmentPage : BasePage
{
    private readonly AppointmentDTO _appointmentData = new() { RecordCount = -1, ExternalParticipant = new ParticipantsModel() };
    private List<TabDataStructureModel> _dataFormatter;
    private List<ButtonActionModel> _actionData;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isRepeatRequest;
    private bool _isTimeInValidError;
    private string _participantsErrorLabel;
    private string _labelTimeInvalidError;
    private List<OptionModel> _externalParticipantCheck;
    private bool _isEditable;
    private bool _isSaveButtonClicked;

    /// <summary>
    /// Appointment ID parameter
    /// </summary>
    [Parameter]
    public long AppointmentID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _appointmentData.Appointment = new AppointmentModel
        {
            AppointmentID = AppointmentID,
        };
        GetSelectedUserID();
        await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).GetAppointmentsAsync(_appointmentData), _appointmentData).ConfigureAwait(true);
        if (_appointmentData.ErrCode == ErrorCode.OK)
        {
            _appointmentData.IsExternalParticipant = _appointmentData.ExternalParticipant?.AppointmentID > 0;
            _dataFormatter = GetLinksDataFormatter();
            _isEditable = LibPermissions.HasPermission(_appointmentData.FeaturePermissions, AppPermissions.AppointmentAddEdit.ToString());

            _externalParticipantCheck = new List<OptionModel> { new OptionModel {
                OptionID = 1, OptionText = LibResources.GetResourceValueByKey(_appointmentData.Resources, ResourceConstants.R_APPOINTMENT_INVITE_EXTERNAL_KEY) ,
                IsSelected = _appointmentData.IsExternalParticipant
            }};
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_appointmentData.ErrCode.ToString());
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        _isSaveButtonClicked = true;
        Success = Error = string.Empty;
        GetParticipantsErrorMessage();
        if(IsValid() && IsDateTimeNotValid() && string.IsNullOrEmpty(_participantsErrorLabel))
        {
            await SaveAndDeleteDataAsync(true, false);
        }
    }

    private int GetMinDoctorCount()
    {
        return _appointmentData.IsExternalParticipant ? 1 : 2;
    }

    private bool IsDateTimeNotValid()
    {
        if(!_appointmentData.Appointment.FromDateTime.HasValue
            || !_appointmentData.Appointment.ToDateTime.HasValue
            || _appointmentData.Appointment.FromDateTime.Value > _appointmentData.Appointment.ToDateTime.Value
            || _appointmentData.Appointment.FromDateTime.Value.Date != _appointmentData.Appointment.ToDateTime.Value.Date
            || _appointmentData.Appointment.FromDateTime == _appointmentData.Appointment.ToDateTime
            || (AppState.webEssentials.ConvertToLocalTime(_appointmentData.Appointment.FromDateTime.Value) - DateTimeOffset.Now).TotalMinutes
                < Convert.ToInt32(LibSettings.GetSettingValueByKey(_appointmentData.Settings, SettingsConstants.S_APPOINTMENT_DEFAULT_TIMEOUT_KEY), CultureInfo.InvariantCulture))
        {
            _labelTimeInvalidError = ResourceConstants.R_INVALID_DATA_KEY;
            return false;
        }
        else
        {
            return true;
        }
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_NO_ACTION_KEY },
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_YES_ACTION_KEY }
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _hideDeletedConfirmationPopup = true;
            Success = Error = string.Empty;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                await SaveAndDeleteDataAsync(_isRepeatRequest, _isRepeatRequest);
            }
            else
            {
                _isRepeatRequest = false;
            }
        }
    }

    private async Task SaveAndDeleteDataAsync(bool isSaveRequest, bool IsRepeatRequest)
    {
        AppointmentDTO appointmentData = new AppointmentDTO { AppointmentParticipants = new List<ParticipantsModel>() };
        appointmentData.LocaltoUtcTimeInSeconds = Convert.ToInt32((await JSRuntime.InvokeAsync<DateTime>("localDate") - DateTime.UtcNow).TotalSeconds);
        if (!isSaveRequest)
        {
            appointmentData.Appointment = new AppointmentModel
            {
                AppointmentID = _appointmentData.Appointment.AppointmentID,
                AppointmentStatusID = ResourceConstants.R_CANCELLED_STATUS_KEY
            };
            await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).UpdateAppointmentAsync(appointmentData, new CancellationToken()), appointmentData).ConfigureAwait(true);
        }
        else
        {
            var dayFormat = LibSettings.GetSettingValueByKey(_appointmentData?.Settings, SettingsConstants.S_DATE_DAY_FORMAT_KEY);
            var monthFormat = LibSettings.GetSettingValueByKey(_appointmentData?.Settings, SettingsConstants.S_MONTH_FORMAT_KEY);
            var yearFormat = LibSettings.GetSettingValueByKey(_appointmentData?.Settings, SettingsConstants.S_YEAR_FORMAT_KEY);
            appointmentData.Appointment = _appointmentData.Appointment;
            appointmentData.Appointment.IsRepeatRequest = IsRepeatRequest;
            appointmentData.Appointment.FromDateString = GenericMethods.GetDateTimeBasedOnCulture(appointmentData.Appointment.FromDateTime, DateTimeType.DateTime, dayFormat, monthFormat, yearFormat);

            appointmentData.Appointment.FromDateTime = appointmentData.Appointment.FromDateTime.Value.ToUniversalTime();
            appointmentData.Appointment.ToDateTime = appointmentData.Appointment.ToDateTime.Value.ToUniversalTime();
            appointmentData.Appointment.AppointmentStatusID = ResourceConstants.R_NEW_STATUS_KEY;
            appointmentData.AppointmentDetails = _appointmentData.AppointmentDetails;
            appointmentData.Appointment.AppointmentTypeID = (short)(_appointmentData.AppointmentTypes.FirstOrDefault(x => x.IsSelected)?.OptionID);
            appointmentData.AppointmentParticipants = _appointmentData.AppointmentParticipantsDropdown?.Where(x => x.IsSelected)?.Select(x => new ParticipantsModel { ParticipantID = x.OptionID })?.ToList();
            appointmentData.ExternalParticipant = _appointmentData.ExternalParticipant;
            appointmentData.IsExternalParticipant = _appointmentData.IsExternalParticipant;
            await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).SyncAppointmentToServerAsync(appointmentData, CancellationToken.None), appointmentData).ConfigureAwait(true);
            if (appointmentData.ErrCode == ErrorCode.DuplicateData)
            {
                _isRepeatRequest = true;
                SetUnavailableStatus(appointmentData);
                OnRemoveClick();
                return;
            }
        }
        if (appointmentData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(appointmentData.ErrCode.ToString());
        }
        else
        {
            Error = appointmentData.ErrCode.ToString();
        }
    }

    private async Task OnCancelButtonClickAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnDateTimeChanged()
    {
        _labelTimeInvalidError = string.Empty;
    }

    private void OnAppointmentParticipantsChanged()
    {
        if(_isSaveButtonClicked)
        {
            GetParticipantsErrorMessage();
        }
    }

    private void OnExternalParticipantAdded(object e)
    {      
        _appointmentData.IsExternalParticipant = e != null && e.ToString() == Constants.NUMBER_ONE;
        if( _isSaveButtonClicked ) { GetParticipantsErrorMessage(); }
        if (!_appointmentData.IsExternalParticipant)
        {
            RemoveControlContainsKey(ResourceConstants.R_NAME_KEY);
            RemoveControlContainsKey(ResourceConstants.R_PHONE_NUMBER_KEY);
            RemoveControlContainsKey(ResourceConstants.R_APPOINTMENT_EMAIL_KEY);
        }
    }

    private void SetUnavailableStatus(AppointmentDTO appointmentData)
    {
        string unavailableText = LibResources.GetResourceValueByKey(_appointmentData.Resources, ResourceConstants.R_UNAVAILABLE_TEXT_KEY);
        string availableText = LibResources.GetResourceValueByKey(_appointmentData.Resources, ResourceConstants.R_AVAILABLE_TEXT_KEY);
        foreach (var item in _appointmentData.AppointmentParticipantsDropdown)
        {
            if (item.OptionText.Contains(Constants.SYMBOL_AMPERSAND))
            {
                item.ParentOptionText = item.OptionText[..item.OptionText.IndexOf(Constants.SYMBOL_AMPERSAND)];
            }
            else if (!item.OptionText.Contains(Constants.SYMBOL_LESS_THAN) && !item.OptionText.Contains(Constants.SYMBOL_AMPERSAND))
            {
                item.ParentOptionText = item.OptionText;
            }
            if (item.OptionText.IndexOf(Constants.SYMBOL_LESS_THAN) > -1)
            {
                item.OptionText = item.OptionText[..item.OptionText.IndexOf(Constants.SYMBOL_LESS_THAN)];
            }
            if (appointmentData.AppointmentParticipants.Exists(x => x.ParticipantID == item.OptionID))
            {
                if (!item.OptionText.Contains(unavailableText, StringComparison.OrdinalIgnoreCase))
                {
                    item.OptionText = item.ParentOptionText;
                }
            }
            else
            {
                if (!item.OptionText.Contains(availableText, StringComparison.OrdinalIgnoreCase))
                {
                    item.OptionText = item.ParentOptionText;
                }
            }
        }
    }

    private List<TabDataStructureModel> GetLinksDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading), FieldType= FieldTypes.TextEntryControl, ResourceKey=ResourceConstants.R_APPOINTMENT_SUBJECT_TEXT_KEY },
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageData), FieldType= FieldTypes.MultiLineEntryControl, ResourceKey=ResourceConstants.R_INFORMATION_TEXT_KEY, IsRequired = LibResources.GetMinLengthValueByKey(_appointmentData.Resources, ResourceConstants.R_INFORMATION_TEXT_KEY, AppState.SelectedLanguageID) > 0 },
        };
    }

    private void GetSelectedUserID()
    {
        _appointmentData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
    }

    private void GetParticipantsErrorMessage()
    {
        var list = _appointmentData.AppointmentParticipantsDropdown.Where(x => x.IsSelected).ToList();
        int count = GetMinDoctorCount();
        if (list.Count > 0 )
        {
            if(list.Count < GetMinDoctorCount() || !(list.Any(x => x.SequenceNo == (byte)RoleName.Doctor && x.IsSelected)))
            {
                _participantsErrorLabel = LibResources.GetResourceValueByKey(_appointmentData.Resources, ResourceConstants.R_CAREGIVER_SELECTION_ERROR_MESSAGE_KEY);
            }
            else
            {
                _participantsErrorLabel = string.Empty;
            }

        }
        else
        {
            _participantsErrorLabel = string.Format(CultureInfo.CurrentCulture,
           LibResources.GetResourceValueByKey(_appointmentData.Resources, ResourceConstants.R_REQUIRED_FIELD_VALIDATION_KEY),
           LibResources.GetResourceValueByKey(_appointmentData.Resources, ResourceConstants.R_PARTICIPANTS_TEXT_KEY));
        }
        
    }
}