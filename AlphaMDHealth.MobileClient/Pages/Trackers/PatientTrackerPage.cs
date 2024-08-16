using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Medication Page
/// </summary>
[RouteRegistration(nameof(PatientTrackerPage))]
[QueryProperty(nameof(PatientTrackerID), "id")]
public class PatientTrackerPage : BasePage
{
    private readonly PatientTrackerAddEdit _patientTrackerView;
    private Guid _patienttrackerID = Guid.Empty;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string PatientTrackerID
    {
        get { return Convert.ToString(_patienttrackerID, CultureInfo.InvariantCulture); }
        set { _patienttrackerID = string.IsNullOrWhiteSpace(value) ? Guid.Empty : new Guid(Convert.ToString(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture)); }
    }

    /// <summary>
    /// Default constructor of PatientTrackerPage
    /// </summary>
    public PatientTrackerPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientTrackerView = new PatientTrackerAddEdit(this, null);
        HideFooter(true);
        SetPageContent(true);
    }

    /// <summary>
    ///  Even which will trigger when page is becoming visible
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientTrackerView.Parameters = AddParameters(CreateParameter(nameof(PatientTrackersModel.PatientTrackerID), Convert.ToString(_patienttrackerID, CultureInfo.InvariantCulture)));
        await _patientTrackerView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _patientTrackerView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    /// <summary>
    /// Method to override to handle header item clicks
    /// </summary>
    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
    /// <param name="menuType">Menu position whether Left, Right</param>
    /// <param name="menuAction">Action type</param>
    public override async Task OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
    {
        if (menuAction == MenuAction.MenuActionSaveKey)
        {
            await _patientTrackerView.SaveTrackerAsync().ConfigureAwait(true);
        }
    }
}