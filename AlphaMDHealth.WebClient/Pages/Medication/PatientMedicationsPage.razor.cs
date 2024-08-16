using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientMedicationsPage : BasePage
{
    private PatientMedicationDTO _medicationData = new PatientMedicationDTO();
    private MedicationSevice _service;
    private Guid _patientMedicationID;

    /// <summary>
    /// Patient Medication ID parameter
    /// </summary>
    [Parameter]
    public Guid PatientMedicationID
    {
        get { return _patientMedicationID; }
        set
        {
            if (_patientMedicationID != value)
            {
                if (_medicationData.RecordCount > 0 || _patientMedicationID == Guid.Empty)
                {
                    _medicationData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _patientMedicationID = value;
            }
        }
    }

    /// <summary>
    /// External data to load
    /// </summary>
    [Parameter]
    public object PageData { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (PageData == null)
        {
            if (Parameters?.Count > 0)
            {
                _medicationData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            }
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            _medicationData = PageData as PatientMedicationDTO;
            MapCommonData();
            _isDataFetched = true;
        }
    }

    private async Task GetDataAsync()
    {
        MapCommonData();
        await SendServiceRequestAsync(_service.GetMedicationsAsync(_medicationData, string.Empty), _medicationData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private void MapCommonData()
    {
        _service = new MedicationSevice(AppState.webEssentials);
        _medicationData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
    }

    private async Task OnAddEditClick(PatientMedicationModel patientMedication)
    {
        Success = Error = string.Empty;
        ShowDetailPage = true;
        if (_medicationData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.PatientMedicationsView.ToString(), (patientMedication == null ? Guid.Empty : patientMedication.PatientMedicationID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _patientMedicationID = string.IsNullOrWhiteSpace(patientMedication?.PatientMedicationID.ToString()) ? Guid.Empty : new Guid(patientMedication.PatientMedicationID.ToString());
        }
    }

    private async Task OnAddEditClosedAsync(string message)
    {
        ShowDetailPage = false;
        _patientMedicationID = Guid.Empty;
        Success = Error = string.Empty;
        if (PageData == null)
        {
            await OnViewAllClickedAsync();
        }
        if (message == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = message;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = message;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientMedicationsView.ToString()).ConfigureAwait(false);
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var tableStructure = new List<TableDataStructureModel>()
        {
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.PatientMedicationID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{BorderColorDataField=nameof(PatientMedicationModel.ProgramColor)},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.ShortName),DataHeader=ResourceConstants.R_MEDICINE_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.MedicationDosesString),DataHeader=ResourceConstants.R_DOSES_KEY},
        };
        if (!(_medicationData.RecordCount > 0))
        {
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientMedicationModel.ProgramName), DataHeader = ResourceConstants.R_PROGRAM_TITLE_KEY });
            if (AppState.IsPatient)
            {
                tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientMedicationModel.EndDateString), DataHeader = ResourceConstants.R_END_DATE_KEY, Formatter = _medicationData.Medication?.EndDateString });
            }
            else
            {
                tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientMedicationModel.FromDateString), DataHeader = ResourceConstants.R_START_DATE_KEY, Formatter = _medicationData.Medication?.FromDateString });
                tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientMedicationModel.EndDateString), DataHeader = ResourceConstants.R_END_DATE_KEY, Formatter = _medicationData.Medication?.EndDateString });
            }
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientMedicationModel.MedicationStatusString), DataHeader = ResourceConstants.R_STATUS_KEY, IsBadge = true, BadgeFieldType = nameof(PatientMedicationModel.MedicationStatusColorString) });
            tableStructure.Add(new TableDataStructureModel { DataField = nameof(PatientMedicationModel.IsCritical), DataHeader = ResourceConstants.R_IS_CRITICAL_KEY, });
        }
        return tableStructure;
    }

    private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(PatientMedicationModel.PatientMedicationID),
            BandColor = nameof(PatientEducationModel.ProgramColor),
            LeftHeader = nameof(PatientMedicationModel.ShortName),
            LeftDescription = nameof(PatientMedicationModel.EndDateShortFormat),
            LeftIcon = nameof(PatientMedicationModel.LeftSourceIcon),
            RightDescription = nameof(PatientMedicationModel.MedicationReminderImage),
            RightDescriptionFieldType = FieldTypes.HtmlLightCenterLabelControl
        };
    }

    private string GetPageTitle()
    {
        return LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, AppPermissions.PatientMedicationsView.ToString());
    }
}