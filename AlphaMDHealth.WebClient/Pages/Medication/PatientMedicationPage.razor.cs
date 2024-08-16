using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class PatientMedicationPage : BasePage
{
    private readonly PatientMedicationDTO _medicationData = new PatientMedicationDTO { Medication = new PatientMedicationModel(), Medications = new List<PatientMedicationModel>(), Reminders = new List<MedicationReminderModel>(), RecordCount = -1 };
    private List<ButtonActionModel> _actionData;
    private MedicationSevice _service;
    private double? _dosesValue;
    private double? _alternateDays;
    private double? _assignAfterDays;
    private double? _assignForDays;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;
    private bool _noMedicationFound;
    private string _selectedDosesUnitValue;
    private bool showMedicinesPopup;
    private bool _isToggeled;
    private bool _isMedicationDateExpired;
    private List<MedicationReminderModel> _medicationReminders;
    private List<ResourceModel> _reminderListOptions;
    private IList<ButtonActionModel> _actionButtons = null;
    /// <summary>
    /// IsSynced
    /// </summary>
    [Parameter]
    public bool IsSynced { get; set; }

    /// <summary>
    /// Medication ID parameter
    /// </summary>
    [Parameter]
    public Guid PatientMedicationID { get; set; }

    /// <summary>
    /// Program ID
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    /// <summary>
    /// Program Duration
    /// </summary>
    [Parameter]
    public int ProgramDuration { get; set; }

    /// <summary>
    /// Selected ProgramMedication ID
    /// </summary>
    [Parameter]
    public long ProgramMedicationID { get; set; }


    protected override async Task OnInitializedAsync()
    {
        _service = new MedicationSevice(AppState.webEssentials);
        _medicationData.Medication.PatientMedicationID = PatientMedicationID;
        _medicationData.Medication.ProgramID = ProgramID;
        _medicationData.Medication.ProgramMedicationID = ProgramMedicationID;
        _medicationData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        await SendServiceRequestAsync(_service.GetMedicationsAsync(_medicationData), _medicationData).ConfigureAwait(true);
        if (_medicationData.ErrCode == ErrorCode.OK)
        {
            if (ProgramID > 0 || _medicationData.Medication.ProgramMedicationID > 0)
            {
                _isEditable = IsSynced && LibPermissions.HasPermission(_medicationData.FeaturePermissions, AppPermissions.ProgramMedicationAddEdit.ToString());
            }
            else
            {
                _isEditable = LibPermissions.HasPermission(_medicationData.FeaturePermissions, AppPermissions.PatientMedicationAddEdit.ToString());
            }

            if (_medicationData.Medication.PatientMedicationID != Guid.Empty || ProgramMedicationID != 0)
            {
                _dosesValue = Convert.ToDouble(_medicationData.Medication.Doses, CultureInfo.InvariantCulture);
                _alternateDays = _medicationData.Medication.AfterDays == 0 ? null : Convert.ToDouble(_medicationData.Medication.AfterDays, CultureInfo.InvariantCulture);
                _assignAfterDays = _medicationData.Medication.AssignAfterDays;
                _assignForDays = _medicationData.Medication.AssignForDays;
                SetUnitIdentifier(_medicationData.Medication.UnitIdentifier);
                _isMedicationDateExpired = _medicationData.Medication.EndDate.Value.Date >= DateTime.Now.Date;
            }
            else
            {
                _medicationData.Medication.IsCritical = false;
            }
            if (_medicationData.Medication.PatientMedicationID != Guid.Empty && ProgramID == 0 && _medicationData.Medication.ProgramMedicationID == 0)
            {
                _isEditable = _isEditable && _isMedicationDateExpired;
            }
            if (AppState.IsPatient)
            {
                if (_medicationData.Reminders.Count > 0)
                {
                    _medicationReminders = _medicationData.Reminders;
                }
                _reminderListOptions = new List<ResourceModel>();
                _isToggeled = _medicationData.Medication.Reminder;
                _isEditable = Convert.ToInt64(_medicationData.Medication.AddedByID) == _medicationData.Medication.PatientID;
            }
            _isDataFetched = true;
            if (IsPatientMobileView)
            {
                _actionButtons ??= new List<ButtonActionModel>();
                if (_isEditable || (AppState.IsPatient && _isMedicationDateExpired))
                {
                    _actionButtons.Add(new ButtonActionModel
                    {
                        ButtonResourceKey = ResourceConstants.R_SAVE_ACTION_KEY,
                        ButtonAction = () => { OnSaveClick(); },
                        Icon = ImageConstants.I_SAVE_ICON_RESPONSIVE
                    });
                }
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                    ButtonAction = () => { OnCancelClickedAsync(); },
                    Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
                });
            }
        }
        else
        {
            await OnClose.InvokeAsync(_medicationData.ErrCode.ToString());
        }
    }

    private async void OnSearchValueChanged()
    {
        _noMedicationFound = false;
        if (!string.IsNullOrWhiteSpace(_medicationData.Medication.ShortName)
            && _medicationData.Medication.ShortName.Length > 2)
        {
            await SendServiceRequestAsync(new MedicineService(AppState.webEssentials).SearchMedicineAsync(_medicationData, _medicationData.Medication.ShortName), _medicationData).ConfigureAwait(true);
            if (_medicationData.ErrCode == ErrorCode.OK)
            {
                if (_medicationData.Medicines.Count == 1)
                {
                    AssignMedicineValues(_medicationData.Medicines[0].FullName, _medicationData.Medicines[0].UnitIdentifier);
                }
                else if (_medicationData.Medicines.Count > 0)
                {
                    showMedicinesPopup = true;
                    _noMedicationFound = false;
                    _medicationData.ErrorDescription = _medicationData.Medication.ShortName;
                    StateHasChanged();
                }
                else
                {
                    AssignMedicineValues(_medicationData.Medication.ShortName, string.Empty);
                    _noMedicationFound = true;
                }
            }
            else
            {
                AssignMedicineValues(_medicationData.Medication.ShortName, string.Empty);
                Error = _medicationData.ErrCode.ToString();
            }
        }
        else
        {
            AssignMedicineValues(_medicationData.Medication.ShortName, string.Empty);
        }

    }

    private void OnFrequencyChanged()
    {
        StateHasChanged();
    }

    private async void OnSaveClick()
    {
        if (IsValid())
        {
            _medicationData.Medications.Clear();
            if (_medicationData.Medication.StartDate > _medicationData.Medication.EndDate)
            {
                Error = string.Format(CultureInfo.InvariantCulture,
                    LibResources.GetResourceValueByKey(_medicationData.Resources, ResourceConstants.R_MAX_RANGE_VALIDATION_KEY),
                    LibResources.GetResourceValueByKey(_medicationData.Resources, ResourceConstants.R_START_DATE_KEY),
                    LibResources.GetResourceValueByKey(_medicationData.Resources, ResourceConstants.R_END_DATE_KEY));
                return;
            }
            if (ProgramID > 0)
            {
                _medicationData.Medication.ProgramID = ProgramID;
                _medicationData.Medication.AssignAfterDays = Convert.ToInt16(_assignAfterDays, CultureInfo.InvariantCulture);
                _medicationData.Medication.AssignForDays = Convert.ToInt16(_assignForDays, CultureInfo.InvariantCulture);
                _medicationData.Medication.AddedByID = Convert.ToString(AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0));
                _medicationData.Medication.IsActive = true;

                var result = new ProgramService(AppState.webEssentials).ValidateAssingAfterAndShowForDate(ProgramDuration, _medicationData.Medication.AssignAfterDays, _medicationData.Medication.AssignForDays, _medicationData.Resources);
                if (!(result.isValidAssignAfterDay && result.isValidShowForAfterDay))
                {
                    Error = string.Format(CultureInfo.InvariantCulture, result.errorCode.ToString()).ToString();
                    return;
                }
            }
            else
            {
                _medicationData.Medication.PatientID = _medicationData.SelectedUserID;
            }
            _medicationData.Medication.UnitIdentifier = string.IsNullOrWhiteSpace(_selectedDosesUnitValue) ? string.Empty : _medicationData.UnitOptions.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_selectedDosesUnitValue))?.GroupName ?? string.Empty;
            _medicationData.Medication.Doses = Convert.ToDecimal(_dosesValue, CultureInfo.InvariantCulture);
            _medicationData.Medication.HowOften = Convert.ToInt32(_medicationData.FrequencyOptions.Find(x => x.IsSelected).OptionID);
            _medicationData.Medication.AfterDays = _medicationData.FrequencyOptions.FirstOrDefault(x => x.IsSelected).GroupName == ResourceConstants.R_ALTERNATE_FOR_KEY
                ? Convert.ToByte(_alternateDays, CultureInfo.InvariantCulture)
                : default;
            _medicationData.Medication.Note = _medicationData.Medication.Note?.Trim();
            if (AppState.IsPatient || _medicationData.Medication.IsCritical)
            {
                _medicationData.Medication.Reminder = _isToggeled;
            }
            if (!_isEditable)
            {
                _medicationData.Medication.IsReadOnly = true;
            }
            await SaveMedicationDataAsync(_medicationData).ConfigureAwait(true);
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private async Task SaveMedicationDataAsync(PatientMedicationDTO data)
    {
        if (_service != null)
        {
            await SendServiceRequestAsync(_service.SaveMedicationAsync(data), data).ConfigureAwait(true);
            if (data.ErrCode == ErrorCode.OK)
            {
                if (AppState.IsPatient && data.Medication.Reminder)
                {
                    await JsRuntime.InvokeVoidAsync("invokeWebviewMethod", "reminderSaved", data.ErrCode.ToString());
                }
                await OnClose.InvokeAsync(data.ErrCode.ToString());
            }
            else
            {
                Error = data.ErrCode.ToString();
                StateHasChanged();
            }
        }
    }

    private void MedicineOptionsPopUpClosedEventCallback(PatientMedicationDTO data)
    {
        showMedicinesPopup = false;
        if (data != null)
        {
            AssignMedicineValues(data.Medication.FullName, data.Medication.UnitIdentifier);
        }
    }

    private void AssignMedicineValues(string fullName, string unitIdentifier)
    {
        _medicationData.Medication.FullName = fullName;
        _medicationData.Medication.UnitIdentifier = unitIdentifier;
        if (!string.IsNullOrEmpty(unitIdentifier))
        {
            _noMedicationFound = false;
            SetUnitIdentifier(unitIdentifier);
        }
    }

    private void SetUnitIdentifier(string unitIdentifier)
    {
        _selectedDosesUnitValue = Convert.ToString(_medicationData.UnitOptions.FirstOrDefault(item => item.GroupName == unitIdentifier)?.OptionID ?? -1);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _medicationData.Medication.IsActive = false;
                await SaveMedicationDataAsync(_medicationData).ConfigureAwait(true);
            }
        }
    }

    private List<OptionModel> GetIsCriticalOption()
    {
        return new List<OptionModel> { new OptionModel {
            OptionID = 1,
            OptionText = LibResources.GetResourceValueByKey(_medicationData.Resources, ResourceConstants.R_IS_CRITICAL_KEY),
            IsSelected = _medicationData.Medication.IsCritical } };
    }

    private string GetPageTitle()
    {
        return LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, ProgramID > 0
            ? AppPermissions.ProgramMedicationAddEdit.ToString()
            : AppPermissions.PatientMedicationAddEdit.ToString());
    }

    private void OnCriticalChecked(object isCritical)
    {
        if (!string.IsNullOrWhiteSpace(isCritical as string))
        {
            var isCriticalFlag = Convert.ToInt32(isCritical) == 1;
            _medicationData.Medication.IsCritical = isCriticalFlag;

            if (isCriticalFlag)
            {
                _isToggeled = isCriticalFlag;
                OnReminderSet(isCriticalFlag);
            }
        }
        else
        {
            _medicationData.Medication.IsCritical = false;
        }
    }

    private void OnReminderSet(object isToggeled)
    {
        var frequency = _medicationData.Medication.Frequency;
        if (!string.IsNullOrWhiteSpace(frequency))
        {
            var frequencyList = frequency.Split(Constants.PIPE_SEPERATOR);
            if (frequencyList.Count() > 0)
            {
                CreateReminderOptions(frequencyList);
            }
        }
    }

    private void CreateReminderOptions(string[] frequencyList)
    {
        if (_reminderListOptions != null)
        {
            if (_reminderListOptions.Count() > 0 && AppState.IsPatient)
            {
                foreach (var resource in _reminderListOptions)
                {
                    _medicationData.Resources.Remove(resource);
                }
            }
        }
        var index = 0;
        _reminderListOptions = new List<ResourceModel>();
        _medicationData.Reminders = new List<MedicationReminderModel>();
        foreach (var frequencyID in frequencyList)
        {
            Enum.TryParse<FrequencyType>(frequencyID, out FrequencyType frequencyType);
            if (frequencyType != FrequencyType.MedicationInSOSKey)
            {
                index += 1;
                if (AppState.IsPatient)
                {
                    string doseKey = string.Concat(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(_medicationData.Resources, ResourceConstants.R_DOSE_REMINDER_KEY), Constants.DASH_INDICATOR, frequencyID.ToString());
                    ResourceModel dose = LibResources.GetResourceByKey(_medicationData.Resources, ResourceConstants.R_DOSE_REMINDER_KEY);
                    _reminderListOptions.Add(new ResourceModel
                    {
                        ResourceKey = doseKey,
                        ResourceValue = string.Format(CultureInfo.InvariantCulture, dose.ResourceValue, index),
                        PlaceHolderValue = dose.PlaceHolderValue,
                        MaxLength = 24,
                        MinLength = 0,
                        LanguageID = dose.LanguageID,
                        ResourceID = Convert.ToInt32(frequencyID)
                    });
                }

                if (_medicationReminders != null)
                {
                    if (_medicationReminders.Count > 0)
                    {
                        _medicationReminders[index - 1].ReminderId = frequencyID;
                        _medicationData.Reminders.Add(_medicationReminders[index - 1]);
                    }
                }
                else
                {
                    _medicationData.Reminders.Add(
                    new MedicationReminderModel
                    {
                        ReminderId = frequencyID,
                        ReminderDateTime = SetDefaultReminderTime(frequencyID),
                        IsActive = true
                    });
                }
            };
        }
        _medicationReminders = null;
        _medicationData.Resources.AddRange(_reminderListOptions);
        StateHasChanged();
    }
    private DateTimeOffset SetDefaultReminderTime(string frequencyID)
    {
        string reminderkey = LibSettings.GetSettingValueByKey(_medicationData.Settings, SettingsConstants.MEDICINE_REMINDER_TIME_KEY);
        string[] values = reminderkey.Split(Constants.PIPE_SEPERATOR);
        Enum.TryParse<FrequencyType>(frequencyID, out FrequencyType frequencyType);
        DateTime time = DateTime.Today;
        switch (frequencyType)
        {
            case FrequencyType.MedicationInMorningKey:
                time = time.AddHours(Convert.ToDouble(values[0]));
                break;
            case FrequencyType.MedicationInAfternoonKey:
                time = time.AddHours(Convert.ToDouble(values[1]));
                break;
            case FrequencyType.MedicationInEveningKey:
                time = time.AddHours(Convert.ToDouble(values[2]));
                break;
            case FrequencyType.MedicationInNightKey:
                time = time.AddHours(Convert.ToDouble(values[3]));
                break;
            case FrequencyType.MedicationInSOSKey:
                break;
            default:
                time = DateTime.Now;
                break;
        }
        return new DateTimeOffset(time);
    }

    private DateTimeOffset GetReminderTime(string id)
    {
        var reminder = _medicationData.Reminders.FirstOrDefault(x => x.ReminderId == id);
        if (reminder != null)
        {
            return reminder.ReminderDateTime;
        }
        else
        {
            return new DateTimeOffset(DateTime.Now);
        }
    }

    private void OnReminderTimeChanged(object time, string id)
    {
        if (time != null)
        {
            var reminder = _medicationData.Reminders.FirstOrDefault(x => x.ReminderId == id);
            if (reminder != null)
            {
                var timeToSet = Convert.ToDateTime(time.ToString());
                if (_medicationData.Reminders.Any(x => x.ReminderDateTime.TimeOfDay == timeToSet.TimeOfDay))
                {
                    Error = LibResources.GetResourceValueByKey(_medicationData.Resources, ResourceConstants.R_REMINDER_TIME_ERROR_KEY);
                    return;
                };
                if (timeToSet.TimeOfDay != reminder.ReminderDateTime.TimeOfDay)
                {
                    reminder.ReminderDateTime = timeToSet;
                }
            }
        }
    }
}