using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient reading add/edit page
/// </summary>
[RouteRegistration(nameof(PatientReadingPage))]
[QueryProperty(nameof(ReadingCategoryID), "name")]
[QueryProperty(nameof(ItemID), "recordCount")]
[QueryProperty(nameof(ReadingID), "type")]
[QueryProperty(nameof(PatientReadingID), "id")]
[QueryProperty(nameof(IsComingAfterLogin), "identifier")]
public class PatientReadingPage : BasePage
{
    private readonly PatientReadingView _readingView;
    private long _itemID;
    private short _readingCategoryID;
    private short _readingID;
    private Guid _patientReadingID;
    private bool _isComingAfterLogin;

    /// <summary>
    /// Currently reading category id
    /// </summary>
    public string ReadingCategoryID
    {
        get { return Convert.ToString(_readingCategoryID, CultureInfo.InvariantCulture); }
        set { _readingCategoryID = string.IsNullOrWhiteSpace(value) ? (short)0 : Convert.ToInt16(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture); }
    }

    /// <summary>
    /// Currently reading id
    /// </summary>
    public string ReadingID
    {
        get { return Convert.ToString(_readingID, CultureInfo.InvariantCulture); }
        set { _readingID = string.IsNullOrWhiteSpace(value) ? (short)0 : Convert.ToInt16(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture); }
    }

    /// <summary>
    /// Patient task ID of task type education
    /// </summary>
    public string ItemID
    {
        get { return Convert.ToString(_itemID, CultureInfo.InvariantCulture); }
        set { _itemID = string.IsNullOrWhiteSpace(value) ? (long)0 : Convert.ToInt64(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture); }
    }

    /// <summary>
    /// Patient reading ID 
    /// </summary>
    public string PatientReadingID
    {
        get { return Convert.ToString(_patientReadingID, CultureInfo.InvariantCulture); }
        set { _patientReadingID = string.IsNullOrWhiteSpace(value) ? Guid.Empty : new Guid(Convert.ToString(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture)); }
    }

    /// <summary>
    /// IS coming from after login 
    /// </summary>
    public string IsComingAfterLogin
    {
        get
        {
            return _isComingAfterLogin.ToString();
        }
        set { _isComingAfterLogin = Convert.ToBoolean(Uri.UnescapeDataString(value ?? false.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture); }
    }

    public PatientReadingPage() : this(false)
    { }

    /// <summary>
    /// Patient reading add/edit page
    /// </summary>
    public PatientReadingPage(bool isComingAfterLogin) : base(PageLayoutType.MastersContentPageLayout, false)
    {
        _isComingAfterLogin = isComingAfterLogin;
        Shell.SetTabBarIsVisible(this, false);
        _readingView = new PatientReadingView(this, null)
        {
            Margin = new Thickness(0, -(double)Application.Current.Resources[StyleConstants.ST_APP_PADDING] + 5, 0, 0)
        };
        SetPageContent(false);
    }

    /// <summary>
    /// Invoked when page appears on screen
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _readingView.Parameters = AddParameters(
            CreateParameter(nameof(PatientReadingDTO.ReadingCategoryID), ReadingCategoryID),
            CreateParameter(nameof(PatientReadingUIModel.PatientTaskID), ItemID),
            CreateParameter(nameof(PatientReadingUIModel.ReadingID), ReadingID),
            CreateParameter(nameof(PatientReadingUIModel.PatientReadingID), PatientReadingID),
            CreateParameter(nameof(BaseDTO.SelectedUserID),App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0).ToString(CultureInfo.InvariantCulture)));
        ////HideFooter(_isComingAfterLogin);
        await _readingView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Invoked when page is unloaded from screen
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _readingView.UnloadUIAsync().ConfigureAwait(true);
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
            await _readingView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        }
    }
}