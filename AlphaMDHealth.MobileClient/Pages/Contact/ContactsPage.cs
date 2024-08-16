using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(ContactsPage))]
[QueryProperty("RecordCount", "recordCount")]
[QueryProperty(nameof(ContactID), "id")]
public class ContactsPage : BasePage
{
    private readonly PatientContactsView _patientContactsView;
    private int _recordCount;
    private string _contactID;

    public string RecordCount
    {
        get
        {
            return _recordCount.ToString(CultureInfo.InvariantCulture);
        }
        set
        {
            _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
        }
    }

    public string ContactID
    {
        get
        {
            return _contactID;
        }
        set
        {
            _contactID = Uri.UnescapeDataString(value);
        }
    }

    public ContactsPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientContactsView = new PatientContactsView(this, null) { Margin = new Thickness(0) };
        PageLayout.Add(_patientContactsView);
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

    protected override async void OnDisappearing()
    {
        await _patientContactsView.UnloadUIAsync().ConfigureAwait(true);
        _contactID = string.Empty;
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
            await _patientContactsView.ContactClickAsync(Guid.Empty, true).ConfigureAwait(true);
        }
        else
        {
            if (menuAction == MenuAction.MenuActionSaveKey && MobileConstants.IsTablet && await _patientContactsView.SaveContactsAsync().ConfigureAwait(true))
            {
                _recordCount = Constants.ZERO_VALUE;
                _contactID = string.Empty;
                await LoadDataAsync().ConfigureAwait(true);
                //ShowHideLeftRightHeader(MenuLocation.Right, false);
                //MenuView titleView = new MenuView(MenuLocation.Header, string.Empty, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
                //await OverrideTitleViewAsync(titleView).ConfigureAwait(true);
            }
        }
    }

    private async Task LoadDataAsync()
    {
        _patientContactsView.Parameters = AddParameters(
                 CreateParameter(nameof(BaseDTO.RecordCount), _recordCount.ToString(CultureInfo.InvariantCulture)),
                 CreateParameter(nameof(ContactModel.ContactID), _contactID),
                 CreateParameter(nameof(BaseDTO.SelectedUserID), App._essentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0).ToString(CultureInfo.InvariantCulture)));
        await _patientContactsView.LoadUIAsync(false).ConfigureAwait(true);
    }
}