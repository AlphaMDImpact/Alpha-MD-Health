using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Medications Page
/// </summary>
[RouteRegistration(nameof(PatientMedicationsPage))]
[QueryProperty(nameof(RecordCount), "recordCount")]
[QueryProperty(nameof(PatientMedicationModel.PatientMedicationID), "id")]
public class PatientMedicationsPage : BasePage
{
    private readonly PatientMedicationsView _medicationsView;
    private int _recordCount;
    private string _patientMedicationID;
    private bool _isOnAppearingCalled;

    /// <summary>
    /// No of medications to be displayed
    /// </summary>
    public string RecordCount
    {
        get { return _recordCount.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value ?? Constants.NUMBER_ZERO), CultureInfo.InvariantCulture);
            if (_recordCount == -1 && MobileConstants.IsTablet && _isOnAppearingCalled)
            {
                DataMethod();
            }
        }
    }

    /// <summary>
    /// Selected PatientMedicationID
    /// </summary>
    public string PatientMedicationID
    {
        get { return _patientMedicationID; }
        set
        {
            _patientMedicationID = Uri.UnescapeDataString(value.Split(Constants.COMMA_SEPARATOR)[0] ?? Guid.Empty.ToString());
        }
    }

    /// <summary>
    /// Default constructor of PatientMedicationsPage
    /// </summary>
    public PatientMedicationsPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _medicationsView = new PatientMedicationsView(this, null);
        PageLayout.Add(_medicationsView, 0, 0);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    /// <summary>
    ///  Even which will trigger when page is becoming visible
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        await LoadDataAsync(false).ConfigureAwait(true);
        _isOnAppearingCalled = true;
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _medicationsView.UnloadUIAsync().ConfigureAwait(true);
        });
        await Task.CompletedTask;
        _patientMedicationID = string.Empty;
        _isOnAppearingCalled = false;
        base.OnDisappearing();
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
            await _medicationsView.OnMedicationClickAsync(Guid.Empty, true).ConfigureAwait(true);
        }
        else
        {
            if (menuAction == MenuAction.MenuActionSaveKey && MobileConstants.IsTablet && await _medicationsView.SaveMedicationAsync().ConfigureAwait(true))
            {
                _recordCount = Constants.ZERO_VALUE;
                _patientMedicationID = string.Empty;
            }
        }
    }

    private void DataMethod()
    {
        Task.Run(async () => { await LoadDataAsync(false).ConfigureAwait(true); }).ConfigureAwait(true);
    }

    private async Task LoadDataAsync(bool isRefreshRequest)
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        _recordCount = _recordCount == -1 ? 0 : _recordCount;
        _medicationsView.Parameters = AddParameters(
            CreateParameter(nameof(RecordCount), RecordCount),
            CreateParameter(nameof(PatientMedicationModel.PatientMedicationID), _patientMedicationID));
        await _medicationsView.LoadUIAsync(isRefreshRequest).ConfigureAwait(true);
        if (MobileConstants.IsDevicePhone && !string.IsNullOrWhiteSpace(_patientMedicationID) && _patientMedicationID != Guid.Empty.ToString())
        {
            await Task.Delay(1000).ConfigureAwait(true);
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientMedicationPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.id), _patientMedicationID.ToString()).ConfigureAwait(true);
            return;
        }
    }
}