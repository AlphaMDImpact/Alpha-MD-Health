using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(PatientTasksPage))]
[QueryProperty(nameof(RecordCount), "recordCount")]
[QueryProperty(nameof(TaskModel.PatientTaskID), "id")]
[QueryProperty(nameof(IsComingAfterLogin), "identifier")]
public class PatientTasksPage : BasePage
{
    //private readonly PatientTasksView _tasksView;
    private int _recordCount;
    private long _patientTaskID;
    private bool _isComingAfterLogin;
    private bool _pageLoaded;

    /// <summary>
    /// No of tasks to be displayed
    /// </summary>
    public string RecordCount
    {
        get => _recordCount.ToString(CultureInfo.InvariantCulture);
        set => _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value ?? Constants.NUMBER_ZERO), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// IS double blood pressure task
    /// </summary>
    public string IsComingAfterLogin
    {
        get => _isComingAfterLogin.ToString(CultureInfo.InvariantCulture);
        set => _isComingAfterLogin = Convert.ToBoolean(Uri.UnescapeDataString(value ?? false.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Selected PatientTaskID
    /// </summary>
    public string PatientTaskID
    {
        get => _patientTaskID.ToString(CultureInfo.InvariantCulture);
        set
        {
            _patientTaskID = Convert.ToInt64(Uri.UnescapeDataString(value ?? Constants.NUMBER_ZERO), CultureInfo.InvariantCulture);
            if (_pageLoaded && _patientTaskID > 0)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadTaskUIAsync().ConfigureAwait(true);
                });
            }
        }
    }

    public PatientTasksPage() : base(PageLayoutType.EndToEndPageLayout, false)
    {
        //IsSplitView = MobileConstants.IsTablet;
        //ShowHideFooter();
        //_tasksView = new PatientTasksView(this, null) { Margin = 0 };
        //PageLayout.Add(_tasksView, 0, 0);
        PageLayout.Padding = 0;
        SetPageContent(false);
    }

    protected override async void OnAppearing()
    {
        if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            return;
        }
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        Shell.SetTabBarIsVisible(this, !_isComingAfterLogin);
        base.OnAppearing();
        await LoadTaskUIAsync().ConfigureAwait(true);
        _pageLoaded = true;
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task LoadTaskUIAsync()
    {
        //_tasksView.Parameters = AddParameters(
        //                CreateParameter(nameof(RecordCount), RecordCount),
        //                CreateParameter(nameof(TaskModel.PatientTaskID), _patientTaskID.ToString()),
        //                CreateParameter(nameof(TaskModel.IsActive), _isComingAfterLogin.ToString()));
        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        //    Shell.SetTabBarIsVisible(this, !_isComingAfterLogin);
        //});
        //await _tasksView.LoadUIAsync(false).ConfigureAwait(true);
    }

    protected override async void OnDisappearing()
    {
        if (App._essentials.GetPreferenceValue(StorageConstants.PR_IS_WORKING_ON_BACKGROUND_MODE_KEY, false))
        {
            return;
        }
        _pageLoaded = false;
        //await _tasksView.UnloadUIAsync().ConfigureAwait(true);
        PatientTaskID = Constants.CONSTANT_ZERO;
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
        //if (_tasksView != null)
        //{
        //    await _tasksView.OnActionClickedAsync(menuAction, headerLocation == MenuLocation.Right).ConfigureAwait(false);
        //}
    }

    /// <summary>
    /// Refresh current page view after completion of sync
    /// </summary>
    /// <param name="syncFrom">From which page/view sync is called</param>
    protected override async Task RefreshUIAsync(Pages syncFrom)
    {
        await Task.Run(async () =>
        {
            //await _tasksView.LoadUIAsync(true).ConfigureAwait(true);
        }).ConfigureAwait(true);
    }

    private void ShowHideFooter()
    {
        Shell.SetTabBarIsVisible(this, false);
    }
}