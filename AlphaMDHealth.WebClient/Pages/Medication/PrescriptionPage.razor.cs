using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Reflection.Metadata;

namespace AlphaMDHealth.WebClient;

public partial class PrescriptionPage : BasePage
{
    private PatientMedicationDTO _prescriptionData;
    private Guid _clickedMedicationID;
    private MedicationSevice _medicationService;
    public object _pageData;

    /// <summary>
    /// External data to load
    /// </summary>
    [Parameter]
    public object PageData
    {
        get
        {
            return _pageData;
        }
        set
        {
            _pageData = value;
            if(_pageData != null) 
            { 
                _prescriptionData = _pageData as PatientMedicationDTO;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (PageData == null)
        {
            _prescriptionData = new PatientMedicationDTO
            {
                Medication = new PatientMedicationModel(),
                Medications = new List<PatientMedicationModel>(),
                IsPrescriptionView = true,
                RecordCount = -2
            };
            if (Parameters?.Count > 0)
            {
                _prescriptionData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            }
            if (_prescriptionData.RecordCount < 1)
            {
                _prescriptionData.RecordCount = -2;
            }
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            _prescriptionData = PageData as PatientMedicationDTO;
            MapCommonData();
            _isDataFetched = true;
        }
    }

    private async Task GetDataAsync()
    {
        MapCommonData();
        await SendServiceRequestAsync(_medicationService.GetMedicationsAsync(_prescriptionData, string.Empty), _prescriptionData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private void MapCommonData()
    {
        _medicationService = new MedicationSevice(AppState.webEssentials);
        _prescriptionData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
    }

    private void OnPrescriptionAddEdit(PatientMedicationModel patientMedicationModel)
    {
        ShowDetailPage = true;
        _clickedMedicationID = patientMedicationModel == null ? Guid.Empty : patientMedicationModel.PatientMedicationID;
    }

    private async Task PopUpClosedEventCallbackAsync(string isDataUpdated)
    {
        ShowDetailPage = false;
        _clickedMedicationID = Guid.Empty;
        Success = string.Empty;
        Error = string.Empty;
        if (isDataUpdated == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = isDataUpdated;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = isDataUpdated;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientMedicationsView.ToString()).ConfigureAwait(false);
    }
    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.PatientMedicationID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{BorderColorDataField=nameof(PatientMedicationModel.ProgramColor)},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.ShortName),DataHeader=ResourceConstants.R_MEDICINE_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.FormattedDate),DataHeader=ResourceConstants.R_START_DATE_KEY},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.Frequency),DataHeader=ResourceConstants.R_FREQUENCY_KEY,IsHtmlTag=true},
            new TableDataStructureModel{DataField=nameof(PatientMedicationModel.Notes),DataHeader=ResourceConstants.R_ADDITIONAL_NOTE_TEXT_KEY,IsHtmlTag=true},
        };
    }
}