using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientTrackerDetailsPage))]
[QueryProperty(nameof(PatientTrackerID), "id")]
public class PatientTrackerDetailsPage : BasePage
{
    private readonly PatientTrackerDetailView _patientTrackerDetailsView;
    private Guid _patienttrackerID = Guid.Empty;
    public event EventHandler<EventArgs> OnSaveButtonClicked;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string PatientTrackerID
    {
        get { return Convert.ToString(_patienttrackerID, CultureInfo.InvariantCulture); }
        set { _patienttrackerID = string.IsNullOrWhiteSpace(value) ? Guid.Empty : new Guid(Convert.ToString(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture)); }
    }

    public PatientTrackerDetailsPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientTrackerDetailsView = new PatientTrackerDetailView(this, null);
        HideFooter(true);
        SetPageContent(true);
    }

    /// <summary>
    ///  Even which will trigger when page is becoming visible
    /// </summary>
    protected override async void OnAppearing()
    {
        //AppHelper.ShowBusyIndicator = false;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientTrackerDetailsView.Parameters = AddParameters(
            CreateParameter(nameof(PatientTrackersModel.PatientTrackerID), PatientTrackerID));
        await _patientTrackerDetailsView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    private void _patientTrackerDetailView_OnSaveSuccess(object sender, EventArgs e)
    {
        OnSaveButtonClicked?.Invoke((string)sender, new EventArgs());
    }


    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _patientTrackerDetailsView.UnloadUIAsync().ConfigureAwait(true);
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
            await _patientTrackerDetailsView.SaveTrackerAsync().ConfigureAwait(true);
        }
    }
}