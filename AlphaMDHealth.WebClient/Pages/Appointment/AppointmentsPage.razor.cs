using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class AppointmentsPage : BasePage
{
    private readonly AppointmentDTO _appointmentData = new();
    private bool _showAppointmentViewPage;
    private bool _showVideoPopup;
    private long _appointmentID;
    private bool _isPatientAppointments;
    private bool _isDashboardView;
    private string _currentRoute;

    /// <summary>
    /// AppointmentID parameter
    /// </summary>
    [Parameter]
    public long AppointmentID
    {
        get { return _appointmentID; }
        set
        {
            if (_appointmentID != value)
            {
                _appointmentID = value;
                _appointmentData.RecordCount = default;
                SetDetailPage();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_currentRoute != AppState.RouterData.SelectedRoute.Page)
        {
            await GetDataAsync().ConfigureAwait(true);
        }
        await base.OnParametersSetAsync();
    }

    private async Task OnAddEditClosedAsync(string message)
    {
        ShowDetailPage = false;
        _showAppointmentViewPage = false;
        _showVideoPopup = false;
        Success = Error = string.Empty;
        _appointmentID = 0;
        if (message.Split(Constants.SYMBOL_PIPE_SEPERATOR).Length > 1)
        {
            _appointmentID = Convert.ToInt64(message.Split(Constants.SYMBOL_PIPE_SEPERATOR)[1], CultureInfo.InvariantCulture);
            _isDataFetched = true;
            ShowDetailPage = true;
        }
        else if (message == ErrorCode.OK.ToString())
        {
            _isDataFetched = false;
            Success = message;
            await GetDataAsync();
        }
        else
        {
            Error = message;
        }
    }

    private void OnAppointmentClicked(AppointmentModel appointment)
    {
        OnAppointmentSelect(appointment == null ? 0 : appointment.AppointmentID);
    }

    protected void OnAppointmentSelected(OptionModel appointment) 
    {
        OnAppointmentSelect(appointment == null ? 0 : appointment.OptionID);
    }

    private void OnAppointmentSelect(long id)
    {
        Success = Error = string.Empty;
        if (_isDashboardView)
        {
            NavigateToAsync(_isPatientAppointments ? AppPermissions.PatientAppointmentsView.ToString() : AppPermissions.AppointmentsView.ToString(), id.ToString()).ConfigureAwait(true);
        }
        else
        {
            _appointmentID = id;
            SetDetailPage();
        }
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(_isPatientAppointments ? AppPermissions.PatientAppointmentsView.ToString() : AppPermissions.AppointmentsView.ToString()).ConfigureAwait(true);
    }

    private void SetDetailPage()
    {
        if (_appointmentData.RecordCount == 0 && _appointmentID != 0)
        {
            _showAppointmentViewPage = true;
            _showVideoPopup = false;
        }
        ShowDetailPage = true;
    }

    private bool ShowTitle()
    {
        if (_appointmentData.ErrCode == ErrorCode.OK && (_isDashboardView || _isPatientAppointments || ShowDetailPage))
        {
            return false;
        }
        return true;
    }

    private string GetPageTitle()
    {
        return LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures
            , _isPatientAppointments ? AppPermissions.PatientAppointmentsView.ToString() : AppPermissions.AppointmentsView.ToString());
    }

    private bool HasAddEditPermission()
    {
        return !_isDashboardView && LibPermissions.HasPermission(_appointmentData.FeaturePermissions, AppPermissions.AppointmentAddEdit.ToString());
    }

    private async Task GetDataAsync()
    {
        _currentRoute = AppState.RouterData.SelectedRoute.Page;
        _appointmentData.Appointment = new AppointmentModel { AppointmentID = 0 };
        if (Parameters?.Count > 0)
        {
            _appointmentData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(AppointmentDTO.RecordCount)));
        }
        _isDashboardView = _appointmentData.RecordCount > 0;
        GetSelectedUserID();
        if (!ShowDetailPage)
        {
            await SendServiceRequestAsync(new AppointmentService(AppState.webEssentials).GetAppointmentsAsync(_appointmentData), _appointmentData).ConfigureAwait(true);
        }
        _isDataFetched = true;
    }

    private void GetSelectedUserID()
    {
        _isPatientAppointments = AppState.RouterData.SelectedRoute.Page == AppPermissions.PatientDetailView.ToString() || AppState.RouterData.SelectedRoute.Page == AppPermissions.PatientAppointmentsView.ToString();
        if(_isDashboardView && _isPatientAppointments)
        {
            _appointmentData.SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
        }
        else 
        {
            if(!_isPatientAppointments)
            {
                _appointmentData.SelectedUserID = 0;
            }
            else
            {
                _appointmentData.SelectedUserID = _isDashboardView ? 0 : AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
            }
        }
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        List<TableDataStructureModel> tableColumns = new()
        {
            new TableDataStructureModel{DataField=nameof(AppointmentModel.AppointmentStatusID),IsKey=true,IsSearchable=false,IsHidden=true,IsSortable=false},
            new TableDataStructureModel{DataField=nameof(AppointmentModel.PageHeading),DataHeader=ResourceConstants.R_APPOINTMENT_SUBJECT_TEXT_KEY},
            new TableDataStructureModel{DataField=nameof(AppointmentModel.FromDateString),DataHeader=ResourceConstants.R_STARTS_TEXT_KEY},
            new TableDataStructureModel{DataField=nameof(AppointmentModel.ToDateString),DataHeader=ResourceConstants.R_ENDS_TEXT_KEY}
        };
        if (!_isDashboardView)
        {
            tableColumns.Add(new TableDataStructureModel { DataField = nameof(AppointmentModel.AppointmentTypeName), DataHeader = ResourceConstants.R_APPOINTMENT_TYPE_TEXT_KEY });
            tableColumns.Add(new TableDataStructureModel { DataField = nameof(AppointmentModel.AppointmentStatusName), DataHeader = ResourceConstants.R_STATUS_KEY , IsBadge = true, BadgeFieldType = nameof(AppointmentModel.AppointmentStatusColor) });
        }
        return tableColumns;
    }

}