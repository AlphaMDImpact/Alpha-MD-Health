using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient readings page
/// </summary>
[RouteRegistration(nameof(PatientReadingsPage))]
[QueryProperty(nameof(RecordCount), "recordCount")]
[QueryProperty(nameof(ReadingCategoryID), "id")]
[QueryProperty(nameof(ReadingID), "type")]
[QueryProperty(nameof(IsAdd), "identifier")]
public class PatientReadingsPage : BasePage
{
    private short _readingCategoryID = 0;
    private short _readingID = 0;
    private int _recordCount;
    private bool _isAdd;
    private readonly PatientReadingsView _patientReadingsView;

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
    /// Number of records to be displayed
    /// </summary>
    public string RecordCount
    {
        get { return Convert.ToString(_recordCount, CultureInfo.InvariantCulture); }
        set { _recordCount = string.IsNullOrWhiteSpace(value) ? (int)0 : Convert.ToInt16(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture); }
    }

    /// <summary>
    /// Number of records to be displayed
    /// </summary>
    public string IsAdd
    {
        get { return Convert.ToString(_isAdd, CultureInfo.InvariantCulture); }
        set { _isAdd = Convert.ToBoolean(Uri.UnescapeDataString(value ?? false.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture); }
    }

    /// <summary>
    /// Patient readings page
    /// </summary>
    public PatientReadingsPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        _patientReadingsView = new PatientReadingsView(this, null);
        PageLayout.Add(_patientReadingsView);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    /// <summary>
    /// Invoked when page appears on screen
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _patientReadingsView.Parameters = AddParameters(
            CreateParameter(nameof(ReadingModel.ReadingCategoryID), Convert.ToString(_readingCategoryID, CultureInfo.InvariantCulture)),
            CreateParameter(nameof(ReadingModel.ReadingID), Convert.ToString(_readingID, CultureInfo.InvariantCulture)),
            CreateParameter(nameof(BaseDTO.RecordCount), Convert.ToString(_recordCount, CultureInfo.InvariantCulture)),
            CreateParameter(nameof(IsAdd), Convert.ToString(_isAdd, CultureInfo.InvariantCulture)),
            CreateParameter(Constants.VIEW_TYPE_STRING, ListStyleType.SeperatorView.ToString()),
            CreateParameter(Constants.DISPLAY_CATEGORY_FILTER_STRING, true.ToString(CultureInfo.InvariantCulture)),
            CreateParameter(nameof(BaseDTO.SelectedUserID),App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0).ToString(CultureInfo.InvariantCulture)));
        _uiOperationTaskCompletion = new TaskCompletionSource<bool>();
        _uiOperationTaskCompletion.SetResult(await LoadUIAsync(false).ConfigureAwait(true));
    }

    /// <summary>
    /// Invoked when page is unloaded from screen
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _patientReadingsView.UnloadUIAsync().ConfigureAwait(true);
        _readingID = 0;
        base.OnDisappearing();
    }

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
        if (await _uiOperationTaskCompletion.Task.ConfigureAwait(true))
        {
            if (_patientReadingsView != null)
            {
                AppHelper.ShowBusyIndicator = true;
                _uiOperationTaskCompletion = new TaskCompletionSource<bool>();
                _uiOperationTaskCompletion.SetResult(await _patientReadingsView.OnActionClickedAsync(menuAction, headerLocation == MenuLocation.Right).ConfigureAwait(true));
                AppHelper.ShowBusyIndicator = false;
            }
        }
    }

    private async Task<bool> LoadUIAsync(bool isRefreshRequest)
    {
        await _patientReadingsView.LoadUIAsync(isRefreshRequest).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        return true;
    }
}