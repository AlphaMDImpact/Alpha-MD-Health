using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(AppointmentsPage))]
[QueryProperty("RecordCount", "recordCount")]
[QueryProperty("AppointmentID", "appointmentID")]
[QueryProperty("SelectedUserID", "id")]
public class AppointmentsPage : BasePage
{
    private readonly AppointmentsView _appointmentsView;
    private int _recordCount;
    private long _appointmentID;
    private long _selectedUserID;
    private bool _isPatientData;

    /// <summary>
    /// RecordCount Parameter
    /// </summary>
    public string RecordCount
    {
        get { return _recordCount.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// AppointmentID Parameter
    /// </summary>
    public string AppointmentID
    {
        get
        {
            return _appointmentID.ToString(CultureInfo.InvariantCulture);
        }
        set
        {
            _appointmentID = Convert.ToInt64(Uri.UnescapeDataString(value.Split(Constants.COMMA_SEPARATOR)[0]), CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// AppointmentID Parameter
    /// </summary>
    public string SelectedUserID
    {
        get
        {
            return _selectedUserID.ToString(CultureInfo.InvariantCulture);
        }
        set
        {
            _selectedUserID = Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public AppointmentsPage()
    {
        var roleID = App._essentials.GetPreferenceValue(StorageConstants.PR_ROLE_ID_KEY, 0);
        _isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;

        //IsSplitView = _isPatientData && MobileConstants.IsTablet;
        _appointmentsView = new AppointmentsView(this, null) { Margin = new Thickness(0) };
        PageLayout.Add(_appointmentsView);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _appointmentsView.Parameters = AddParameters(
           CreateParameter(nameof(RecordCount), RecordCount),
           CreateParameter(nameof(SelectedUserID), SelectedUserID),
           CreateParameter(nameof(AppointmentModel.AppointmentID), _appointmentID.ToString(CultureInfo.InvariantCulture)));
        if (_isPatientData)
        {
            await _setHeaderCompletionSource.Task.ConfigureAwait(true);
            //ShowHideLeftRightHeader(MenuLocation.Left, false);
        }
        await _appointmentsView.LoadUIAsync(false).ConfigureAwait(true);
        if (MobileConstants.IsDevicePhone && _appointmentID > 0)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.AppointmentViewPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.appointmentID), _appointmentID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            return;
        }
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        _appointmentID = 0;
        if ((_isPatientData || MobileConstants.IsTablet) && _appointmentsView != null)
        {
            await _appointmentsView.UnloadUIAsync().ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionAddKey)
        {
            await _appointmentsView.OnAddButtonClickAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Refresh view/page based on current service sync
    /// </summary>
    /// <param name="syncFrom">From which page sync from is called</param>
    protected override async Task RefreshUIAsync(Pages syncFrom)
    {
        await Task.Run(async () =>
        {
            await _appointmentsView.LoadUIAsync(true).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}