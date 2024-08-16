using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Reading Details page
/// </summary>
[RouteRegistration(nameof(PatientReadingDetailsPage))]
[QueryProperty(nameof(ReadingID), "id")]
[QueryProperty(nameof(ReadingCategoryID), "name")] 
public class PatientReadingDetailsPage : BasePage
{
    private short _readingCategoryID;
    private short _readingID;
    private readonly PatientReadingDetailView _readingView;

    /// <summary>
    /// Task completion source to make sure UI Load operation task happens 1 at a time
    /// </summary>
    protected TaskCompletionSource<bool> _uiOperationTaskCompletion;

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
    /// Patient Reading Details page
    /// </summary>
    public PatientReadingDetailsPage() : base(PageLayoutType.MastersContentPageLayout, true)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _readingView = new PatientReadingDetailView(this, null);
        SetPageContent(false);
    }

    /// <summary>
    /// Invoked when page appears on screen
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        _readingView.Parameters = AddParameters(
            CreateParameter(nameof(PatientReadingDTO.ReadingCategoryID), ReadingCategoryID),
            CreateParameter(nameof(PatientReadingDTO.ReadingID), ReadingID),
            CreateParameter(nameof(BaseDTO.SelectedUserID),App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0).ToString(CultureInfo.InvariantCulture)));
        _uiOperationTaskCompletion = new TaskCompletionSource<bool>();
        _uiOperationTaskCompletion.SetResult(await LoadUIAsync(false).ConfigureAwait(true));
    }

    /// <summary>
    /// Invoked when page is unloaded from screen
    /// </summary>
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _readingView.UnloadUIAsync();
    }

    ///
    /// <summary>
    /// Refresh current page view after completion of sync
    /// </summary>
    /// <param name="syncFrom">From which page/view sync is called</param>
    protected override async Task RefreshUIAsync(Pages syncFrom)
    {
        if (await _uiOperationTaskCompletion.Task.ConfigureAwait(true))
        {
            AppHelper.ShowBusyIndicator = true;
            _uiOperationTaskCompletion = new TaskCompletionSource<bool>();
            _uiOperationTaskCompletion.SetResult(await LoadUIAsync(true).ConfigureAwait(true));
        }
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
            await ShellMasterPage.CurrentShell.CurrentPage
                .PushPageByTargetAsync(nameof(PatientReadingPage),
                GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.type, Param.id),
                ReadingCategoryID, ReadingID, string.Empty).ConfigureAwait(true);
        }
    }

    private async Task<bool> LoadUIAsync(bool isRefreshRequest)
    {
        await _readingView.LoadUIAsync(isRefreshRequest).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        return true;
    }
}