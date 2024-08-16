using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientTrackersPage))]
[QueryProperty(nameof(PatientTrackerName), nameof(Param.name))]
[QueryProperty(nameof(PatientTrackerID), nameof(Param.id))]
public class PatientTrackersPage : BasePage
{
    private readonly PatientTrackersView _patientTrackersView;
    private int _recordCount;
    private string _patientTrackerName;
    private Guid _patienttrackerID = Guid.Empty;

    public string PatientTrackerName
    {
        get => _patientTrackerName;
        set => _patientTrackerName = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Tracker Id of the selected conversation
    /// </summary>
    public string PatientTrackerID
    {
        get { return Convert.ToString(_patienttrackerID, CultureInfo.InvariantCulture); }
        set { _patienttrackerID = string.IsNullOrWhiteSpace(value) ? Guid.Empty : new Guid(Convert.ToString(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture)); }
    }

    public PatientTrackersPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientTrackersView = new PatientTrackersView(this, null);
        PageLayout.Add(_patientTrackersView, 0, 0);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        await LoadDataAsync().ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task LoadDataAsync()
    {
        _patientTrackersView.Parameters = AddParameters(
            CreateParameter(nameof(PatientTrackersModel.TrackerName), PatientTrackerName),
            CreateParameter(nameof(PatientTrackersModel.PatientTrackerID), PatientTrackerID)
        );
        await _patientTrackersView.LoadUIAsync(false).ConfigureAwait(true);
    }

    public override async Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionSaveKey && MobileConstants.IsTablet && await _patientTrackersView.SavePatientTrackerAsync().ConfigureAwait(true))
        {
            _recordCount = Constants.ZERO_VALUE;
            _patientTrackerName = string.Empty;
            await LoadDataAsync().ConfigureAwait(true);
            ShowHideLeftRightHeader(MenuLocation.Right, false);
            MenuView titleView = new MenuView(MenuLocation.Header, string.Empty, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
            await OverrideTitleViewAsync(titleView).ConfigureAwait(true);
        }
    }

    private async Task LoadDataAsync(bool isRefreshRequest)
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        _recordCount = _recordCount == -1 ? 0 : _recordCount;
        _patientTrackersView.Parameters = AddParameters(
            CreateParameter(nameof(PatientTrackersModel.TrackerName), _patientTrackerName));
        await _patientTrackersView.LoadUIAsync(isRefreshRequest).ConfigureAwait(true);
    }

    protected override async void OnDisappearing()
    {
        await _patientTrackersView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }
}
