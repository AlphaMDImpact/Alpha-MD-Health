using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration("PatientsPage")]
[QueryProperty(nameof(UserID), "id")]
[QueryProperty(nameof(RecordCount), "recordCount")]
public class PatientsPage : BasePage
{
    private readonly PatientsView _patientsView;
    private int _recordCount;
    private long _patientID;

    /// <summary>
    /// No of patients to be displayed
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
    /// Patinet Id of the selected patient
    /// </summary>
    public string UserID
    {
        get { return _patientID.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _patientID = Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public PatientsPage() : base(PageLayoutType.MastersContentPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientsView = new PatientsView(this, null);
        AddRowColumnDefinition(GridLength.Star, 1, true);
        PageLayout.Padding = new Thickness(0);
        PageLayout.Add(_patientsView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
            base.OnAppearing();
            _patientsView.Parameters = AddParameters(CreateParameter(nameof(RecordCount), RecordCount), CreateParameter(nameof(UserID), UserID));
            await _patientsView.LoadUIAsync(false).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }

    protected override async void OnDisappearing()
    {
        if (!App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            await _patientsView.UnloadUIAsync().ConfigureAwait(true);
            App._essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_USER_ID_KEY, (long)0);
            base.OnDisappearing();
        }
    }

    /// <summary>
    /// Refresh current page view after completion of sync
    /// </summary>
    /// <param name="syncFrom">From which page/view sync is called</param>
    protected override async Task RefreshUIAsync(Pages syncFrom)
    {
        await Task.Run(() => { }).ConfigureAwait(true);
    }

    /// <summary>
    /// Open view based on current selection
    /// </summary>
    /// <param name="targetView">View which is selected</param>
    public async Task LoadTragetViewAsync(string targetView)
    {
        await _patientsView.LoadTragetViewAsync(targetView).ConfigureAwait(true);
    }

    public override async Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionAddKey)
        {
            await _patientsView.AddSinglePatientClicked().ConfigureAwait(true);
        }
        else if (menuAction == MenuAction.MenuActionBulkUploadKey)
        {
            await _patientsView.AddBulkPatientClicked().ConfigureAwait(true);
        }
    }
}