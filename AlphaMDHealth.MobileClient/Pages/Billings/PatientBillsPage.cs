using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientBillsPage))]
[QueryProperty(nameof(RecordCount), "recordCount")]
[QueryProperty(nameof(SelectedUserID), "id")]
public class PatientBillsPage : BasePage
{
    private readonly PatientBillingsView _patientBillingsView;
    private int _recordCount;
    private long _patientID;
    private bool _isOnAppearingCalled;

    public string RecordCount
    {
        get { return _recordCount.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value ?? Constants.NUMBER_ZERO), CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Patinet Id of the selected patient
    /// </summary>
    public string SelectedUserID
    {
        get { return _patientID.ToString(CultureInfo.InvariantCulture); }
        set
        {
            _patientID = string.IsNullOrWhiteSpace(value) ? 0 : Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public PatientBillsPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientBillingsView = new PatientBillingsView(this, null);
        PageLayout.Add(_patientBillingsView, 0, 0);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        AppHelper.ShowBusyIndicator = true;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientBillingsView.Parameters = AddParameters(
            CreateParameter(nameof(BaseDTO.RecordCount), _recordCount.ToString(CultureInfo.InvariantCulture)),
            CreateParameter(nameof(BaseDTO.SelectedUserID), SelectedUserID)
        );
        await _patientBillingsView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _patientBillingsView.UnloadUIAsync().ConfigureAwait(true);
        });
        await Task.CompletedTask;
        SelectedUserID = string.Empty;
        _isOnAppearingCalled = false;
        base.OnDisappearing();
    }

    public async override Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionShareKey)
        {
            await _patientBillingsView.ShareButtonClicked().ConfigureAwait(true);
        }
    }
}