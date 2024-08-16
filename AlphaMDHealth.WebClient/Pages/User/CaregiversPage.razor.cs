using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class CaregiversPage : BasePage
{
    private readonly CaregiverDTO _caregiverData = new CaregiverDTO { Caregiver = new CaregiverModel() };
    private long _patientCaregiverID;

    /// <summary>
    /// Patient Caregiver ID parameter
    /// </summary>
    [Parameter]
    public long PatientCaregiverID
    {
        get { return _patientCaregiverID; }
        set
        {
            if (_patientCaregiverID != value)
            {
                if (_caregiverData.RecordCount > 0 || _patientCaregiverID == 0)
                {
                    _caregiverData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _patientCaregiverID = value;
            }
        }
    }
    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        if (Parameters?.Count > 0)
        {
            _caregiverData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(CaregiverDTO.RecordCount)));
        }
        await SendServiceRequestAsync(new UserService(AppState.webEssentials).GetPatientCareGiversAsync(_caregiverData), _caregiverData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var tableDataStructure = new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField= nameof(CaregiverModel.PatientCareGiverID), IsKey= true, IsSearchable= false, IsHidden= true, IsSortable= false},
            new TableDataStructureModel{BorderColorDataField=nameof(CaregiverModel.ProgramColor)},
            new TableDataStructureModel{DataField= nameof(CaregiverModel.FirstName), DataHeader= ResourceConstants.R_NAME_TEXT_KEY },
            new TableDataStructureModel{DataField= nameof(CaregiverModel.Department), DataHeader= ResourceConstants.R_POSITION_KEY },
            new TableDataStructureModel{DataField= nameof(CaregiverModel.ProgramName), DataHeader= ResourceConstants.R_PROGRAM_TITLE_KEY},
        };
        if (_caregiverData.RecordCount < 1)
        {
            tableDataStructure.Add(new TableDataStructureModel { DataField = nameof(CaregiverModel.FromDateValue), DataHeader = ResourceConstants.R_START_DATE_KEY, Formatter = _caregiverData.Caregiver.FromDateValue });
            tableDataStructure.Add(new TableDataStructureModel { DataField = nameof(CaregiverModel.ToDateValue), DataHeader = ResourceConstants.R_END_DATE_KEY, Formatter = _caregiverData.Caregiver.ToDateValue });
        }
        return tableDataStructure;
    }

    private async Task OnAddEditClick(CaregiverModel caregiver)
    {
        Success = Error = string.Empty;        
        if (_caregiverData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.CaregiversView.ToString(), (caregiver == null ? 0 : caregiver.PatientCareGiverID).ToString()).ConfigureAwait(false);
        }
        else
        {
            _patientCaregiverID = caregiver == null ? 0 : caregiver.PatientCareGiverID;
            ShowDetailPage = true;
        }       
    }

    private async Task OnViewAllClickedAsync(object e)
    {
        await NavigateToAsync(AppPermissions.CaregiversView.ToString()).ConfigureAwait(true);
    }

    private async Task OnAddEditClosedAsync(string actionResult)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _patientCaregiverID = 0;
        if (actionResult == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = actionResult;
            await GetDataAsync();
            StateHasChanged();
        }
        else
        {
            if (actionResult != ResourceConstants.R_CANCEL_ACTION_KEY)
            {
                Error = actionResult;
            }
        }
    }
}