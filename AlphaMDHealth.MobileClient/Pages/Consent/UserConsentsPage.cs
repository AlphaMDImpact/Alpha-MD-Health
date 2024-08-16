using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(UserConsentsPage))]
[QueryProperty(nameof(IsBeforeLogin), "type")]
public class UserConsentsPage : BasePage
{
    private readonly UserConsentsView _consentsView;
    private bool _isBeforeLogin;

    /// <summary>
    /// Flag which represents page is showing in before login flow
    /// </summary>
    public string IsBeforeLogin
    {
        get { return _isBeforeLogin.ToString(CultureInfo.InvariantCulture); }
        set => _isBeforeLogin = Convert.ToBoolean(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
    }

    public UserConsentsPage() : this(false) { }

    public UserConsentsPage(bool isBeforeLogin) : base(PageLayoutType.LoginFlowPageLayout, false)
    {
        _isBeforeLogin = isBeforeLogin;
        if (!_isBeforeLogin)
        {
            HideFooter(true);
        }
        _consentsView = new UserConsentsView(this, null);
        SetPageContent(false);
        //todo:Content.Effects.Add(new CustomSafeAreaInsetEffect());
    }

    protected override async void OnAppearing()
    {
        if (!_isBeforeLogin)
        {
            HideFooter(true);
        }
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _consentsView.Parameters = AddParameters(CreateParameter(nameof(BaseDTO.IsActive), _isBeforeLogin.ToString(CultureInfo.InvariantCulture)));
        await _consentsView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    protected override async void OnDisappearing()
    {
        await _consentsView.UnloadUIAsync().ConfigureAwait(true);
        HideFooter(false);
        base.OnDisappearing();
    }
}