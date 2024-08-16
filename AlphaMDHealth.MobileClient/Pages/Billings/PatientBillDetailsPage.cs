using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientBillDetailsPage))]
[QueryProperty(nameof(PatientBillID), "id")]
[QueryProperty(nameof(SelectedUserID), "identifier")]
public class PatientBillDetailsPage : BasePage
{
    private readonly PatientBillDetailsView _patientBillingsView;
    private string _patientBillID;
    private string _selectedUserID;

    /// <summary>
    /// Patient BillID of the selected conversation
    /// </summary>
    public string PatientBillID
    {
        get
        {
            return _patientBillID;
        }
        set => _patientBillID = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Selected UserID of the selected conversation
    /// </summary>
    public string SelectedUserID
    {
        get
        {
            return _selectedUserID;
        }
        set => _selectedUserID = Uri.UnescapeDataString(value);
    }

    public PatientBillDetailsPage()
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientBillingsView = new PatientBillDetailsView(this, null);
        HideFooter(true);
        ScrollView content = new ScrollView { Content = _patientBillingsView, Orientation = ScrollOrientation.Vertical };
        PageLayout.Add(content);
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientBillingsView.Parameters = AddParameters(CreateParameter(nameof(PatientBillModel.PatientBillID), PatientBillID)
            ,CreateParameter(nameof(BaseDTO.SelectedUserID), SelectedUserID));
        await _patientBillingsView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _patientBillingsView.UnloadUIAsync().ConfigureAwait(true);
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
            await _patientBillingsView.ShareButtonClicked().ConfigureAwait(true);
        }
    }
}