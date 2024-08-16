using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Prescription Page
/// </summary>
[RouteRegistration(nameof(PrescriptionPage))]
[QueryProperty(nameof(PatientMedicationID), "id")]
public class PrescriptionPage : BasePage
{
    private Guid _patientmedicationID = Guid.Empty;
    private readonly PrescriptionView _prescriptionView;

    /// <summary>
    /// Patient MedicationID
    /// </summary>
    public string PatientMedicationID
    {
        get { return Convert.ToString(_patientmedicationID, CultureInfo.InvariantCulture); }
        set { _patientmedicationID = string.IsNullOrWhiteSpace(value) ? Guid.Empty : new Guid(Convert.ToString(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture)); }
    }

    /// <summary>
    /// Default constructor of PrescriptionPage
    /// </summary>
    public PrescriptionPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _prescriptionView = new PrescriptionView(this, null);
        HideFooter(true);
        ScrollView content = new ScrollView { Content = _prescriptionView, Orientation = ScrollOrientation.Vertical };
        PageLayout.Add(content);
        SetPageContent(false);
    }

    /// <summary>
    /// Even which will trigger when page is becoming visible
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _prescriptionView.Parameters = AddParameters(
            CreateParameter(nameof(PatientMedicationModel.PatientMedicationID), PatientMedicationID));
        await _prescriptionView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _prescriptionView.UnloadUIAsync().ConfigureAwait(true);
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
        if (menuAction == MenuAction.MenuActionShareKey)
        {
            await _prescriptionView.ShareButtonClicked().ConfigureAwait(true);
        }
    }
}