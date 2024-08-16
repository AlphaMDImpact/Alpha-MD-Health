//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using System.Globalization;

//namespace AlphaMDHealth.MobileClient;

//[RouteRegistration("SampleCodesPage")]
//[QueryProperty("RecordCount", "recordCount")]
//public class SampleCodesPage : BasePage
//{
//    private readonly SampleCodesView _sampleListView;
//    private int _recordCount;

//    public string RecordCount
//    {
//        get { return _recordCount.ToString(CultureInfo.InvariantCulture); }
//        set
//        {
//            _recordCount = Convert.ToInt32(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
//        }
//    }

//    public SampleCodesPage()
//    {
//        //IsSplitView = MobileConstants.IsTablet;
//        ////IsChild = true;
//        _sampleListView = new SampleCodesView(this, AddParameters(CreateParameter(nameof(BaseDTO.RecordCount), RecordCount)));
//        Content = _sampleListView;
//    }

//    protected override async void OnAppearing()
//    {
//        base.OnAppearing();
//        await _sampleListView.LoadUIAsync(false).ConfigureAwait(true);
//        AppHelper.ShowBusyIndicator = false;
//    }

//    protected override async void OnDisappearing()
//    {
//        base.OnDisappearing();
//        await _sampleListView.UnloadUIAsync().ConfigureAwait(true);
//    }

//    /// <summary>
//    /// Method to override to handle header item clicks
//    /// </summary>
//    /// <param name="headerLocation">Header location whether Left, Right (in case of tablet)</param>
//    /// <param name="menuType">Menu position whether Left, Right</param>
//    /// <param name="menuAction">Action type</param>
//    public async override void OnMenuActionClick(MenuLocation headerLocation, MenuLocation menuType, MenuAction menuAction)
//    {
//        if (menuAction == MenuAction.MenuActionAddKey)
//        {
//            await _sampleListView.EmployeeClickAsync(Guid.Empty.ToString()).ConfigureAwait(false);
//        }
//        else
//        {
//            if (menuAction == MenuAction.MenuActionSaveKey)
//            {
//                //// Perform save operation here
//            }
//        }
//    }

///// <summary>
///// Refresh current page view after completion of sync
///// </summary>
///// <param name="syncFrom">From which page/view sync is called</param>
//protected override async Task RefreshUIAsync(Pages syncFrom)
//{
//    RefreshUIAsync(Pages syncFrom)
//    {
//        await Task.Run(() =>
//        {
//            //todo: Refresh view/page on completion of sync
//        }).ConfigureAwait(true);
//    }
//}