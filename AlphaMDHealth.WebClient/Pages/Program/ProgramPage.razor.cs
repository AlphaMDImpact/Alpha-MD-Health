using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ProgramPage : BasePage
{
    private readonly ProgramDTO _programData = new ProgramDTO { RecordCount = -1 };
    private IList<TabDataStructureModel> _dataFormatter;
    private List<ButtonActionModel> _messageButtonActions;
    private List<OptionModel> _tabsData;
    private AppPermissions _selectedTab;
    private bool _hidePublishUnpublishConfirmation = true;
    private bool _shouldShowConfigureRange;
    private bool _shouldShowConfigureReading;
    private bool _isDataUpdated;
    private string _string;
    private double? _programDuration;
    private long _selectedID;
    private bool _hasPermission;
    private bool _isEditable;
    private bool _isButtonClicked = true;
    private AmhDropdownControl _inputRef;

    [Parameter]
    public bool IsShowAddEditPage { get; set; }

    /// <summary>
    /// Program ID parameter
    /// </summary>
    [Parameter]
    public long ProgramID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _programData.Program = new ProgramModel { ProgramID = ProgramID };
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramsFromServerAsync(_programData, CancellationToken.None), _programData).ConfigureAwait(true);
        if (_programData.ErrCode == ErrorCode.OK)
        {
            _tabsData = new List<OptionModel>();
            AddTabAsPerPermission();
            OnProgramTabSelectionValueChanged(_tabsData?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 1);
            _programData.Tasks ??= new List<TaskModel>();
            _programData.SubFlows ??= new List<SubFlowModel>();
            _programData.ProgramCareGivers ??= new List<CaregiverModel>();
            _programData.ProgramReadings ??= new List<ReadingModel>();
            _programData.ProgramEducations ??= new List<PatientEducationModel>();
            _programData.ProgramMedications ??= new List<PatientMedicationModel>();
            _programData.ProgramBillingItems ??= new List<PatientBillModel>();
            _programData.ProgramTrackers ??= new List<ProgramTrackerModel>();
            _programData.ProgramNotes ??= new List<ProgramNoteModel>();
            _programData.ProgramConfigurations ??= new List<ProgramConfigurationModel>();
            _programData.ProgramServices ??= new List<ProgramServiceModel>();
            _programData.Program.ProgramGroupIdentifier = string.IsNullOrEmpty(_programData.Program.ProgramGroupIdentifier) ? StyleConstants.DEFAULT_COLOR : _programData.Program.ProgramGroupIdentifier;
            OnProgramTypeSelectionChange(Convert.ToInt64(_programData.Program.ProgramTypeID));
            _programDuration = _programData.Program.ProgramDuration;
            _dataFormatter = GetContentDataFormatter();
            _hasPermission = LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramAddEdit.ToString());
            _isEditable = _hasPermission && (ProgramID == 0 || _programData.Program.IsSynced);
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_isDataUpdated.ToString()).ConfigureAwait(true);
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        if (_programData.Program.ProgramTypeID != _programData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_TIME_BOUNDED_PROGRAM_KEY).ResourceKeyID)
        {
            _programData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PROGRAM_DURATION_KEY).IsRequired = false;
        }

        ValidateControl(_isButtonClicked);
        RemoveControlByKey(ResourceConstants.R_SUPPORTED_CODE_SYSTEM_KEY);
        if (IsValid())
        {
            var selectedKey = _programData.Resources.Find(x => x.ResourceKeyID == _programData.ProgramTypes.Find(x=>x.IsSelected).OptionID).ResourceKey;
            _programData.Program.ProgramDuration = (selectedKey == ResourceConstants.R_OPEN_ENDED_KEY) ? 0 : Convert.ToInt32(_programDuration, CultureInfo.InvariantCulture);
            ProgramDTO programData = new ProgramDTO
            {
                Program = _programData.Program,
                LanguageDetails = _programData.LanguageDetails,
                IsActive = true
            };
            await PostDataAsync(programData);
        }       
        StateHasChanged();
    }

    public bool ValidateControl(bool IsButtonClick)
    {
        return _inputRef.ValidateControl(IsButtonClick);
    }

    protected void OnProgramTabSelectionValueChanged(object? value)
    {
        _selectedTab = _tabsData?.FirstOrDefault(x => x.OptionID == Convert.ToInt64(value)).GroupName.ToEnum<AppPermissions>() ?? default;
    }

    private async Task OnSaveProgramReasonConfigClickedAsync()
    {
        ProgramDTO programData = new ProgramDTO
        {
            OrganisationID = _programData.OrganisationID,
            CreatedByID = _programData.Program.ProgramID,
            ProgramConfigurations = _programData.ProgramConfigurations
        };
        programData.ErrCode = ErrorCode.OK;
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramReasonConfigToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        if (programData.ErrCode == ErrorCode.OK)
        {
            Success = programData.ErrCode.ToString();
        }
        else
        {
            Error = _programData.ErrCode.ToString();
        }
    }

    private async Task PostDataAsync(ProgramDTO programData)
    {
        await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncProgramToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
        _programData.ErrCode = programData.ErrCode;
        if (programData.ErrCode == ErrorCode.OK)
        {
            _isDataUpdated = true;
            await OnClose.InvokeAsync(ResourceConstants.R_SAVE_ACTION_KEY.ToString());
        }
        else
        {
            Error = programData.ErrCode.ToString();
        }
    }

    private void OnPublishButtonClicked()
    {
        if (IsValid())
        {
            if (_programData.Tasks.Any(x => x.IsActive))
            {
                _messageButtonActions = new List<ButtonActionModel>
            {
                new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
                new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
            };
                _hidePublishUnpublishConfirmation = false;
            }
            else
            {
                Error = ResourceConstants.R_PROGRAM_MANDATORY_TASK_ERROR_KEY;
            }
        } 
    }

    private async Task OnPopupOkClickAsync(object sequenceNo)
    {
        _hidePublishUnpublishConfirmation = true;
        if (Convert.ToInt64(sequenceNo) == 1)
        {
            ProgramDTO programData = new ProgramDTO
            {
                Program = _programData.Program,
                LanguageDetails = _programData.LanguageDetails,
                IsActive = true
            };
            programData.Program.IsPublished = !_programData.Program.IsPublished;
            await SendServiceRequestAsync(new ProgramService(AppState.webEssentials).SyncPublishProgramToServerAsync(programData, CancellationToken.None), programData).ConfigureAwait(true);
            _programData.ErrCode = programData.ErrCode;
            if (programData.ErrCode == ErrorCode.OK)
            {
                _programData.Program.IsPublished = programData.Program.IsPublished;
                await OnClose.InvokeAsync(_isDataUpdated.ToString());
            }
            else
            {
                Error = programData.ErrCode.ToString();
            }
        }
    }

    private void OnSelectionChange(object option)
    {
        if (!string.IsNullOrWhiteSpace(option.ToString()))
        {
            long optionId = Convert.ToInt64(option);
            _programData.Program.CodeSystemID = optionId;
        }
    }

    private void OnProgramTypeSelectionChange(object e)
    {
        if (!string.IsNullOrWhiteSpace(e.ToString()))
        {
            long optionId = Convert.ToInt64(e);
            _programData.Program.ProgramTypeID = (short)Convert.ToInt32(optionId);
        }
        //ClearControls();
    }

    private void GetSelfRegistrationClicked(string val)
    {
        _programData.Program.AllowSelfRegistration = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private void GetProviderToScanClicked(string val)
    {
        _programData.Program.AllowProviderToScan = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private void GetPatientToScanClicked(string val)
    {
        _programData.Program.AllowPatientToScan = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private void GetPatientToBuyCreditsClicked(string val)
    {
        _programData.Program.AllowPatientToBuyCredits = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private void GetProgramToBuyCreditsClicked(string val)
    {
        _programData.Program.AllowProgramToBuyCredits = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private List<OptionModel> GetSelfRegistrationOption()
    {
        return new List<OptionModel> {
         new OptionModel { OptionID = 1,
             OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_ALLOW_SELF_REGISTRATION_KEY),
             IsSelected = _programData.Program.AllowSelfRegistration }
     };
    }

    private List<OptionModel> GetProviderToScanOption()
    {
        return new List<OptionModel> {
         new OptionModel { OptionID = 1,
             OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_ALLOW_PROVIDER_TO_SCAN_KEY),
             IsSelected = _programData.Program.AllowProviderToScan }
     };
    }

    private List<OptionModel> GetPatientToScanOption()
    {
        return new List<OptionModel> {
         new OptionModel { OptionID = 1,
             OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_ALLOW_PATIENT_TO_SCAN_KEY),
             IsSelected = _programData.Program.AllowPatientToScan }
     };
    }
    private List<OptionModel> GetPatientToBuyCreditsOption()
    {
        return new List<OptionModel> {
         new OptionModel { OptionID = 1,
             OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_ALLOW_PATIENTS_TO_BUY_CREDITS_DIRECTLY_KEY),
             IsSelected = _programData.Program.AllowPatientToBuyCredits }
     };
    }
    private List<OptionModel> GetProgramToBuyCreditsOption()
    {
        return new List<OptionModel> {
         new OptionModel { OptionID = 1,
             OptionText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_ALLOW_PROGRAM_TO_BUY_CREDICTS_DIRECTLY_KEY),
             IsSelected = _programData.Program.AllowProgramToBuyCredits }
     };
    }

    private void OnConfigureLinkClicked(Tuple<string, ReadingModel> cellData)
    {
        _string = cellData.Item1;
        _selectedID = (cellData.Item2 as ReadingModel).ProgramReadingID;
        IsShowAddEditPage = true;
        if (_string == LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_CONFIGURE_READINGS))
        {
            _shouldShowConfigureReading = true;
            _shouldShowConfigureRange = false;
        }
        else if (_string == LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.ProgramReadingRangesView.ToString()))
        {
            _shouldShowConfigureReading = false;
            _shouldShowConfigureRange = true;
        }
    }

    private void OnProgramReadingAddEditClick(ReadingModel programReading)
    {
        Success = Error = string.Empty;
        if (!_shouldShowConfigureRange && !_shouldShowConfigureReading)
        {
            IsShowAddEditPage = true;
            _selectedID = programReading == null ? 0 : programReading.ProgramReadingID;
        }
    }

    private void OnProgramCareGiverAddEditClick(CaregiverModel programCaregiver)
    {
        OnAddEditClicked(programCaregiver == null ? 0 : programCaregiver.ProgramCareGiverID);
    }

    private void OnProgramMedicationAddEditClick(PatientMedicationModel programMedication)
    {
        OnAddEditClicked(programMedication == null ? 0 : programMedication.ProgramMedicationID);
    }

    private void OnProgramSubFlowAddEditClick(SubFlowModel programSubFlow)
    {
        OnAddEditClicked(programSubFlow == null ? 0 : programSubFlow.ProgramSubFlowID);
    }

    private void OnProgramTaskAddEditClick(TaskModel programTask)
    {
        OnAddEditClicked(programTask == null ? 0 : programTask.ProgramTaskID);
    }

    private void OnProgramEducationAddEditClick(PatientEducationModel programEducation)
    {
        OnAddEditClicked(programEducation == null ? 0 : programEducation.ProgramEducationID);
    }

    private void OnProgramBillingItemAddEditClick(PatientBillModel programBilling)
    {
        OnAddEditClicked(programBilling == null ? 0 : programBilling.ProgramBillingItemID);
    }

    private void OnProgramReasonAddEditClick(ReasonModel programReason)
    {
        OnAddEditClicked(programReason == null ? 0 : programReason.ProgramReasonID);
    }
    private void OnProgramServiceAddEditClick(ProgramServiceModel programReason)
    {
        OnAddEditClicked(programReason == null ? 0 : programReason.ProgramExternalServiceID);
    }
    private void OnProgramTrackerAddEditClick(ProgramTrackerModel programTracker)
    {
        OnAddEditClicked(programTracker == null ? 0 : programTracker.ProgramTrackerID);
    }

    private void OnProgramNoteAddEditClick(ProgramNoteModel programNote)
    {
        OnAddEditClicked(programNote == null ? 0 : programNote.ProgramNoteID);
    }

    private void OnAddEditClicked(long id)
    {
        Success = Error = string.Empty;
        IsShowAddEditPage = true;
        _selectedID = id;
    }

    private async void ProgramBillingItemPageClosedEventCallback(string errorCode)
    {
        await OnAddEditPageClosed(errorCode, ProgramBillingItemSaved);
    }

    private async void ProgramNoteTypeClosedEventCallback(string errorCode)
    {
        await OnAddEditPageClosed(errorCode, ProgramNoteTypeSaved);
    }

    private async void ProgramTrackerClosedEventCallback(string errorCode)
    {
        await OnAddEditPageClosed(errorCode, ProgramTrackerSaved);
    }

    private async void OnAddEditPageClosed(string errorCode)
    {
        await OnAddEditPageClosed(errorCode, null);
    }

    private async Task OnAddEditPageClosed(string errorCode, Action action)
    {
        IsShowAddEditPage = false;
        Success = Error = string.Empty;
        _selectedID = 0;
        if (errorCode == ErrorCode.OK.ToString() || errorCode == string.Empty)
        {
            _isButtonClicked =false;
            _isDataUpdated = true;
            _isDataFetched = false;
            action?.Invoke();
            await OnInitializedAsync();
        }
        StateHasChanged();
    }

    private void OnCancelClick()
    {
        OnClose.InvokeAsync(ResourceConstants.R_CANCEL_ACTION_KEY.ToString());
    }

    private void ProgramBillingItemSaved()
    {
        if (_programData.Tasks.Count == 0 && _programData.Program.IsPublished)
        {
            _programData.Program.IsPublished = false;
        }
    }

    private void ProgramNoteTypeSaved()
    {
        if (_programData.ProgramNotes.Count == 0 && _programData.Program.IsPublished)
        {
            _programData.Program.IsPublished = false;
        }
    }

    private void ProgramTrackerSaved()
    {
        if (_programData.ProgramTrackers.Count == 0 && _programData.Program.IsPublished)
        {
            _programData.Program.IsPublished = false;
        }
    }

    private void AddTabAsPerPermission()
    {
        if (_tabsData != null)
        {
            GenerateTabBasedOnPermission(AppPermissions.ProgramTasksView, 1);
            GenerateTabBasedOnPermission(AppPermissions.SubflowsView, 2);
            GenerateTabBasedOnPermission(AppPermissions.ProgramCaregiversView, 3);
            GenerateTabBasedOnPermission(AppPermissions.ProgramReadingsView, 4);
            GenerateTabBasedOnPermission(AppPermissions.ProgramEducationsView, 5);
            GenerateTabBasedOnPermission(AppPermissions.ProgramMedicationsView, 6);
            GenerateTabBasedOnPermission(AppPermissions.ProgramTrackersView, 7);
            GenerateTabBasedOnPermission(AppPermissions.PatientProviderNoteTypesView, 8);
            GenerateTabBasedOnPermission(AppPermissions.ProgramBillingItemsView, 9);
            GenerateTabBasedOnPermission(AppPermissions.ProgramReasonsView, 10);
            GenerateTabBasedOnPermission(AppPermissions.ProgramConfigurationView, 11);
            GenerateTabBasedOnPermission(AppPermissions.ProgramServicesView, 12);
        }
    }

    private void GenerateTabBasedOnPermission(AppPermissions permission, long sequence)
    {
        if (LibPermissions.HasPermission(_programData.FeaturePermissions, permission.ToString()))
        {
            _tabsData.Add(new OptionModel
            {
                GroupName = permission.ToString(),
                OptionID = sequence,
                OptionText = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, permission.ToString()),
                IsSelected = _selectedTab == permission
            });
        }
    }

    private List<TabDataStructureModel> GetContentDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
            new TabDataStructureModel { DataField = nameof(ProgramDetails.Name), ResourceKey = ResourceConstants.R_PROGRAM_NAME_KEY },
            new TabDataStructureModel { DataField = nameof(ProgramDetails.Description), ResourceKey = ResourceConstants.R_PROGRAM_DESCRIPTION_KEY, IsRequired = false }
        };
    }

    private List<TableDataStructureModel> GenerateTableStructure(string nameOfList)
    {
        switch (nameOfList)
        {
            case nameof(CaregiverModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(CaregiverModel.PatientCareGiverID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(CaregiverModel.FullName), DataHeader = ResourceConstants.R_SELECT_PROVIDER_KEY },
                    new TableDataStructureModel { DataField = nameof(CaregiverModel.RoleName), DataHeader = ResourceConstants.R_PROFESSION_KEY },
                    new TableDataStructureModel { DataField = nameof(CaregiverModel.AssignAfterDays), DataHeader = ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY },
                    new TableDataStructureModel { DataField = nameof(CaregiverModel.AssignForDays), DataHeader = ResourceConstants.R_SHOW_FOR_DAYS_KEY }
                };
            case nameof(TaskModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(TaskModel.ProgramTaskID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(TaskModel.Name), DataHeader = ResourceConstants.R_TASK_NAME_KEY },
                    new TableDataStructureModel { DataField = nameof(TaskModel.TaskType), DataHeader = ResourceConstants.R_TASK_TYPE_KEY },
                    new TableDataStructureModel { DataField = nameof(TaskModel.SelectedItemName), DataHeader = ResourceConstants.R_ITEM_KEY },
                    new TableDataStructureModel { DataField = nameof(TaskModel.AssignAfterDays), DataHeader = ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY },
                    new TableDataStructureModel { DataField = nameof(TaskModel.AssignForDays), DataHeader = ResourceConstants.R_SHOW_FOR_DAYS_KEY }
                };
            case nameof(SubFlowModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(SubFlowModel.ProgramSubFlowID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(SubFlowModel.Name), DataHeader = ResourceConstants.R_SUBFLOW_NAME_KEY },
                    new TableDataStructureModel { DataField = nameof(SubFlowModel.OperationType), DataHeader = ResourceConstants.R_OPERATION_TYPE_KEY },
                    new TableDataStructureModel { DataField = nameof(SubFlowModel.AssignAfterDays), DataHeader = ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY },
                    new TableDataStructureModel { DataField = nameof(SubFlowModel.AssignForDays), DataHeader = ResourceConstants.R_SHOW_FOR_DAYS_KEY }
                };
            case nameof(PatientEducationModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(PatientEducationModel.ProgramEducationID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false},
                    new TableDataStructureModel { DataField = nameof(PatientEducationModel.PageHeading), DataHeader = ResourceConstants.R_EDUCATION_KEY},
                    new TableDataStructureModel { DataField = nameof(PatientEducationModel.ForProviders), DataHeader = ResourceConstants.R_FOR_PROVIDERS_KEY},
                    new TableDataStructureModel { DataField = nameof(PatientEducationModel.AssignAfterDays), DataHeader = ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY},
                    new TableDataStructureModel { DataField = nameof(PatientEducationModel.AssignForDays), DataHeader = ResourceConstants.R_SHOW_FOR_DAYS_KEY},
                };
            case nameof(PatientMedicationModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(PatientMedicationModel.ProgramMedicationID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false},
                    new TableDataStructureModel { DataField = nameof(PatientMedicationModel.ShortName), DataHeader = ResourceConstants.R_MEDICINE_NAME_KEY},
                    new TableDataStructureModel { DataField = nameof(PatientMedicationModel.MedicationDosesString), DataHeader = ResourceConstants.R_DOSES_KEY},
                    new TableDataStructureModel { DataField = nameof(PatientMedicationModel.AssignAfterDays), DataHeader = ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY},
                    new TableDataStructureModel { DataField = nameof(PatientMedicationModel.AssignForDays), DataHeader = ResourceConstants.R_SHOW_FOR_DAYS_KEY},
                     new TableDataStructureModel { DataField = nameof(PatientMedicationModel.IsCritical), DataHeader = ResourceConstants.R_IS_CRITICAL_KEY},
                };
            case nameof(ProgramTrackerModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(ProgramTrackerModel.ProgramTrackerID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(ProgramTrackerModel.TrackerName), DataHeader = ResourceConstants.R_TRACKER_NAME_TEXT_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramTrackerModel.AssignAfterDays), DataHeader = ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramTrackerModel.AssignForDays), DataHeader = ResourceConstants.R_SHOW_FOR_DAYS_KEY }
                };
            case nameof(ProgramNoteModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(ProgramNoteModel.ProgramNoteID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(ProgramNoteModel.NoteText), DataHeader = ResourceConstants.R_NOTE_TYPE_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramNoteModel.Questionnaire), DataHeader = ResourceConstants.R_QUESTIONNAIRE_KEY },
                };
            case nameof(PatientBillModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(PatientBillModel.ProgramBillingItemID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(PatientBillModel.Item), DataHeader = ResourceConstants.R_ITEM_KEY },
                    new TableDataStructureModel { DataField = nameof(PatientBillModel.Amount), DataHeader = ResourceConstants.R_AMOUNT_KEY },
                };
            case nameof(ProgramConfigurationModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(ProgramConfigurationModel.FeatureID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(ProgramConfigurationModel.FeatureText), DataHeader = ResourceConstants.R_FEATURE_LABEL_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramConfigurationModel.FeatureCode), DataHeader = ResourceConstants.R_OPERATION_LABEL_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramConfigurationModel.IsReasonRequired), DataHeader=ResourceConstants.R_REASON_REQUIRED_LABEL_KEY, IsCheckBox = true,LinkText=string.Concat(nameof(ProgramConfigurationModel.FeatureID),nameof(ProgramConfigurationModel.IsReasonRequired))},
                    new TableDataStructureModel { DataField = nameof(ProgramConfigurationModel.IsSignatureRequired), DataHeader=ResourceConstants.R_THUMB_SIGNATURE_LABEL_KEY, IsCheckBox = true,LinkText=string.Concat(nameof(ProgramConfigurationModel.FeatureID),nameof(ProgramConfigurationModel.IsSignatureRequired)) },
                };
            case nameof(ProgramServiceModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(ProgramServiceModel.ProgramExternalServiceID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(ProgramServiceModel.ServiceName), DataHeader = ResourceConstants.R_SERVICE_NAME_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramServiceModel.Quantity), DataHeader = ResourceConstants.R_NO_OF_SCANS_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramServiceModel.AssignAfterDays), DataHeader = ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY },
                    new TableDataStructureModel { DataField = nameof(ProgramServiceModel.AssignForDays), DataHeader = ResourceConstants.R_SHOW_FOR_DAYS_KEY }
                };
            case nameof(ReasonModel):
                return new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(ReasonModel.ProgramReasonID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(ReasonModel.Reason), DataHeader = ResourceConstants.R_REASON_NAME_TEXT_KEY },
                };
            default:
                var tblFields = new List<TableDataStructureModel>
                {
                    new TableDataStructureModel { DataField = nameof(ReadingModel.ProgramReadingID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
                    new TableDataStructureModel { DataField = nameof(ReadingModel.ReadingCategory), DataHeader = ResourceConstants.R_CATEGORY_KEY },
                    new TableDataStructureModel { DataField = nameof(ReadingModel.Reading), DataHeader = ResourceConstants.R_SELECT_READING_KEY },
                    new TableDataStructureModel { DataField = nameof(ReadingModel.SequenceNo), DataHeader = ResourceConstants.R_SEQUENCE_NO_KEY },
                    new TableDataStructureModel { DataField = nameof(ReadingModel.IsCritical), DataHeader = ResourceConstants.R_ADD_TO_MEDICAL_HISTORY },
                };
                if (LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramReadingAddEdit.ToString()) && _programData.Program.IsSynced)
                {
                    tblFields.Add(new TableDataStructureModel { LinkText = LibResources.GetResourceValueByKey(_programData.Resources, ResourceConstants.R_CONFIGURE_READINGS), IsLink = true,LinkFieldType=FieldTypes.LinkHStartVCenterLabelControl, IsSearchable = false ,IsSortable = false});
                }
                if (LibPermissions.HasPermission(_programData.FeaturePermissions, AppPermissions.ProgramReadingRangesView.ToString()) && _programData.Program.IsSynced)
                {
                    tblFields.Add(new TableDataStructureModel { LinkText = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.ProgramReadingRangesView.ToString()), IsLink = true, LinkFieldType = FieldTypes.LinkHStartVCenterLabelControl, IsSearchable = false , IsSortable = false});
                }
                return tblFields;
        }
    }
}