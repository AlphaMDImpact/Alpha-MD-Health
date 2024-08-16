using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class PatientProgramsPage : BasePage
{
    private PatientProgramDTO _programData = new PatientProgramDTO() { PatientProgram = new PatientProgramModel() };
    private long _patientProgramID;

    /// <summary>
    /// Program ID parameter
    /// </summary>
    [Parameter]
    public long PatientProgramID
    {
        get { return _patientProgramID; }
        set
        {
            if (_patientProgramID != value)
            {
                if (_programData.RecordCount > 0 || _patientProgramID == 0)
                {
                    _programData.RecordCount = default;
                    ShowDetailPage = true;
                }
                _patientProgramID = value;
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (Parameters?.Count > 0)
        {
            _programData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(PatientProgramDTO.RecordCount)));
        }
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _programData.Programs = new List<ProgramModel>();
        await SendServiceRequestAsync(new PatientProgramService(AppState.webEssentials).GetPatientProgramsAsync(_programData), _programData).ConfigureAwait(true);
        _isDataFetched = true;
    }

    private async Task OnProgramClickedAsync(ProgramModel program)
    {
        var patientProgramID = Convert.ToInt64(program?.PatientProgramID ?? 0, CultureInfo.InvariantCulture);
        if (_programData.RecordCount > 0)
        {
            await NavigateToAsync(AppPermissions.PatientProgramsView.ToString(), patientProgramID.ToString()).ConfigureAwait(false);
        }
        else
        {
            _patientProgramID = patientProgramID;
            ShowDetailPage = true;
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(AppPermissions.PatientProgramsView.ToString()).ConfigureAwait(false);
    }

    private async Task OnProgramAddEditClosedAsync(string errorMessage)
    {
        Success = Error = string.Empty;
        ShowDetailPage = false;
        _patientProgramID = 0;
        if (errorMessage == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = errorMessage;
            await GetDataAsync().ConfigureAwait(true);
        }
        else
        {
            Error = errorMessage;
        }
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        var columns = new List<TableDataStructureModel>
        {
            new TableDataStructureModel{DataField=nameof(ProgramModel.PatientProgramID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{HasImage=true,ImageFieldType=FieldTypes.CircleWithBackgroundImageControl,ImageHeight=AppImageSize.ImageSizeM,ImageWidth=AppImageSize.ImageSizeM, MaxColumnWidthSize="10vh",ImageBackgroundColor=nameof(ProgramModel.ProgramGroupIdentifier) ,ImageIcon=nameof(ProgramModel.ProgramImage), },
            new TableDataStructureModel{DataField=nameof(ProgramModel.Name),DataHeader=ResourceConstants.R_PROGRAM_NAME_KEY},
            new TableDataStructureModel{DataField=nameof(ProgramModel.AddedOnString),DataHeader=ResourceConstants.R_ENROLLED_ON_TEXT_KEY},
        };
        if (_programData.RecordCount < 1)
        {
            columns.Add(new TableDataStructureModel { DataField = nameof(ProgramModel.ProgramEntryPoint), DataHeader = ResourceConstants.R_ENTRY_POINT_KEY });
        }
        return columns;
    }
}