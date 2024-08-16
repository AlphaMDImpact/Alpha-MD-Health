using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientProgramPage))]
[QueryProperty(nameof(IsBeforeLogin), "type")]
public class PatientProgramPage : BasePage
{
    private readonly PatientProgramView _programsView;
    private bool _isBeforeLogin;

    /// <summary>
    /// Flag which represents page is showing in before login flow
    /// </summary>
    public string IsBeforeLogin
    {
        get { return _isBeforeLogin.ToString(CultureInfo.InvariantCulture); }
        set => _isBeforeLogin = Convert.ToBoolean(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
    }

    public PatientProgramPage() : this(false) { }

    public PatientProgramPage(bool isBeforeLogin) : base(PageLayoutType.LoginFlowPageLayout, false)
    {
        _isBeforeLogin = isBeforeLogin;
        _programsView = new PatientProgramView(this, null);
        SetPageContent(true);
    }

    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _programsView.Parameters = AddParameters(CreateParameter(nameof(ProgramDTO.IsActive), _isBeforeLogin.ToString(CultureInfo.InvariantCulture)));
        await _programsView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _programsView.UnloadUIAsync().ConfigureAwait(true);
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
            await _programsView.SaveButtonClickedAsync().ConfigureAwait(true);
        }
    }
}