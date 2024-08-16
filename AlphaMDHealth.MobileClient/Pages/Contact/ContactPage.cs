using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(ContactPage))]
[QueryProperty(nameof(ContactID), "id")]
public class ContactPage : BasePage
{
    private readonly PatientContactView _patientContactView;
    private string _contactID;

    /// <summary>
    /// User Id of the selected conversation
    /// </summary>
    public string ContactID
    {
        get
        {
            return _contactID;
        }
        set => _contactID = Uri.UnescapeDataString(value);
    }

    public ContactPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientContactView = new PatientContactView(this, null);
        HideFooter(true);
        PageLayout.Add(_patientContactView);
        SetPageContent(false);
    }
    

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientContactView.Parameters = AddParameters(
            CreateParameter(nameof(ContactModel.ContactID), Convert.ToString(_contactID, CultureInfo.InvariantCulture))
        );
        await _patientContactView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _patientContactView.UnloadUIAsync().ConfigureAwait(true);
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
            await _patientContactView.OnSaveActionClicked().ConfigureAwait(true);
        }
    }
}