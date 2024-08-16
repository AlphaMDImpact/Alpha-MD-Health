using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Medication Page
/// </summary>
[RouteRegistration(nameof(PatientMedicationPage))]
[QueryProperty(nameof(PatientMedicationID), "id")]
public class PatientMedicationPage : BasePage
{
    private readonly PatientMedicationView _medicationView;
    private string _medicationId;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string PatientMedicationID
    {
        get
        {
            return _medicationId;
        }
        set => _medicationId = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Default constructor of PatientMedicationPage
    /// </summary>
    public PatientMedicationPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _medicationView = new PatientMedicationView(this, null);
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
        _medicationView.Parameters = AddParameters(CreateParameter(nameof(PatientMedicationModel.PatientMedicationID), Convert.ToString(_medicationId, CultureInfo.InvariantCulture)));
        await _medicationView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Even which will trigger when page disappears
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _medicationView.UnloadUIAsync().ConfigureAwait(true);
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
        if (menuAction == MenuAction.MenuActionSaveKey)
        {
            await _medicationView.SaveMedicationAsync().ConfigureAwait(true);
        }
    }
}
