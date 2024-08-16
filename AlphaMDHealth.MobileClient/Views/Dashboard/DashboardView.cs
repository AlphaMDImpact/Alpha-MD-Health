using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Default view used to display default content
/// </summary>
public class DashboardView : ViewManager
{
    //private bool _areViewCleared;
    //private readonly DefaultContentView _defaultView;

    ///// <summary>
    ///// Format of day to display in status banner
    ///// </summary>
    //protected string _dayFormatKey = SettingsConstants.S_DATE_DAY_FORMAT_KEY;

    ///// <summary>
    ///// Format of moth to display in status banner
    ///// </summary>
    //protected string _monthFormatKey = SettingsConstants.S_DATE_MONTH_FORMAT_KEY;

    ///// <summary>
    ///// Add reading button to display on end of the page
    ///// </summary>
    //protected SvgImageButtonView _addObservationBtn;

    ///// <summary>
    ///// Add Reading button wrapper to display shadow with button
    ///// </summary>
    //protected ContentView _buttonwrapper; //todo:PancakeView

    ///// <summary>
    ///// Icon to display in no data found view
    ///// </summary>
    //protected string _noDataIcon;

    private DashboardLibDTO _dashboardPageData;
    private TaskCompletionSource<bool> _uiOperationTaskCompletion;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public DashboardView(BasePage page, object parameters) : base(page, parameters)
    {
        _dashboardPageData = new DashboardDTO();
        ParentPage.PageService = new DashboardService(App._essentials);
        ParentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
        ParentPage.PageLayout.Add(ParentPage.MessagePopup, 0, 0);
        SetPageContent(ParentPage.MasterGrid);
        //var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        //_defaultView = new DefaultContentView(ParentPage, MapParameters(string.Empty, MessageViewType.StaticMessageView));
        //_buttonwrapper = new ContentView //todo:PancakeView
        //{
        //    //todo:
        //    //Shadow = new DropShadow
        //    //{
        //    //    Color = (Color)Application.Current.Resources[LibStyleConstants.ST_DROP_SHADOW_COLOR],
        //    //    Opacity = 0.3f,
        //    //    Offset = new Point(new Size(0, 6))
        //    //},
        //    //CornerRadius = Convert.ToSingle(AppImageSize.ImageSizeL) / 2,
        //    Margin = new Thickness(new OnIdiom<double> { Phone = padding, Tablet = ParentPage.PageLayout.Padding.Right + padding }, padding),
        //    HorizontalOptions = LayoutOptions.End,
        //    VerticalOptions = LayoutOptions.End,
        //    IsVisible = false
        //};
        //_addObservationBtn = new SvgImageButtonView(ImageConstants.I_ADD_VITAL_ICON_PNG, AppImageSize.ImageSizeL, AppImageSize.ImageSizeL)
        //{
        //    HorizontalOptions = LayoutOptions.EndAndExpand,
        //    VerticalOptions = LayoutOptions.EndAndExpand
        //};
        //_buttonwrapper.Content = _addObservationBtn;
        //ParentPage.MasterGrid.Add(_buttonwrapper, 0, 1);
        //if (ShellMasterPage.CurrentShell.CurrentPage != null
        //    && !ShellMasterPage.CurrentShell.CurrentPage.ToString().EndsWith(Constants.DASHBOARD_PAGE_CONSTANT))
        //{
        //    SetPageContent(ParentPage.MasterGrid);
        //}
        //_noDataIcon = ImageConstants.I_DASHBOARD_PNG;
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        await Task.Run(async () =>
        {
            if (Parameters?.Count > 0)
            {
                _dashboardPageData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
            }
            if (isRefreshRequest && await _uiOperationTaskCompletion.Task.ConfigureAwait(true))
            {
                AppHelper.ShowBusyIndicator = true;
            }
            _uiOperationTaskCompletion = new TaskCompletionSource<bool>();
            _uiOperationTaskCompletion.SetResult(await LoadUIAsync().ConfigureAwait(true));
        });
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        await Task.CompletedTask;
    }

    private async Task<bool> LoadUIAsync()
    {
        await (ParentPage.PageService as DashboardService).GetDashboardDataAsync(_dashboardPageData as DashboardDTO).ConfigureAwait(true);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ParentPage.PageData = _dashboardPageData;
            ParentPage.ApplyPageResources();
            ParentPage.MessagePopup.FieldType = FieldTypes.MessageControl;
            ParentPage.MessagePopup.ResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
            AppHelper.ShowBusyIndicator = false;
        });
        return true;
    }
}



//using AlphaMDHealth.ClientBusinessLayer;
//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using System.Globalization;

//namespace AlphaMDHealth.MobileClient;

///// <summary>
///// Default view used to display default content
///// </summary>
//public class DashboardView : ViewManager
//{
//    private bool _areViewCleared;
//    private readonly DefaultContentView _defaultView;

//    /// <summary>
//    /// Format of day to display in status banner
//    /// </summary>
//    protected string _dayFormatKey = SettingsConstants.S_DATE_DAY_FORMAT_KEY;

//    /// <summary>
//    /// Format of moth to display in status banner
//    /// </summary>
//    protected string _monthFormatKey = SettingsConstants.S_DATE_MONTH_FORMAT_KEY;

//    /// <summary>
//    /// Add reading button to display on end of the page
//    /// </summary>
//    protected SvgImageButtonView _addObservationBtn;

//    /// <summary>
//    /// Add Reading button wrapper to display shadow with button
//    /// </summary>
//    protected ContentView _buttonwrapper; //todo:PancakeView

//    /// <summary>
//    /// Icon to display in no data found view
//    /// </summary>
//    protected string _noDataIcon;

//    /// <summary>
//    /// page data object
//    /// </summary>
//    protected DashboardLibDTO _dashboardPageData;

//    /// <summary>
//    /// ui update task progress indicator
//    /// </summary>
//    protected TaskCompletionSource<bool> _uiOperationTaskCompletion;

//    /// <summary>
//    /// Parameterized constructor containing page instance and Parameters
//    /// </summary>
//    /// <param name="page">page instance on which view is rendering</param>
//    /// <param name="parameters">Featue parameters to render view</param>
//    public DashboardView(BasePage page, object parameters) : base(page, parameters)
//    {
//        var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
//        _defaultView = new DefaultContentView(ParentPage, MapParameters(string.Empty, MessageViewType.StaticMessageView));
//        _buttonwrapper = new ContentView //todo:PancakeView
//        {
//            //todo:
//            //Shadow = new DropShadow
//            //{
//            //    Color = (Color)Application.Current.Resources[LibStyleConstants.ST_DROP_SHADOW_COLOR],
//            //    Opacity = 0.3f,
//            //    Offset = new Point(new Size(0, 6))
//            //},
//            //CornerRadius = Convert.ToSingle(AppImageSize.ImageSizeL) / 2,
//            Margin = new Thickness(new OnIdiom<double> { Phone = padding, Tablet = ParentPage.PageLayout.Padding.Right + padding }, padding),
//            HorizontalOptions = LayoutOptions.End,
//            VerticalOptions = LayoutOptions.End,
//            IsVisible = false
//        };
//        _addObservationBtn = new SvgImageButtonView(ImageConstants.I_ADD_VITAL_ICON_PNG, AppImageSize.ImageSizeL, AppImageSize.ImageSizeL)
//        {
//            HorizontalOptions = LayoutOptions.EndAndExpand,
//            VerticalOptions = LayoutOptions.EndAndExpand
//        };
//        _buttonwrapper.Content = _addObservationBtn;
//        ParentPage.MasterGrid.Add(_buttonwrapper, 0, 1);
//        if (ShellMasterPage.CurrentShell.CurrentPage != null
//            && !ShellMasterPage.CurrentShell.CurrentPage.ToString().EndsWith(Constants.DASHBOARD_PAGE_CONSTANT))
//        {
//            SetPageContent(ParentPage.MasterGrid);
//        }
//        _noDataIcon = ImageConstants.I_DASHBOARD_PNG;
//        _dashboardPageData = new DashboardDTO();
//        ParentPage.PageService = new DashboardService(App._essentials);
//    }

//    /// <summary>
//    /// Load UI data of view
//    /// </summary>
//    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
//    /// <returns>Returns true if required view is found, else return false</returns>
//    public override async Task LoadUIAsync(bool isRefreshRequest)
//    {
//        await Task.Run(async () =>
//        {
//            if (Parameters?.Count > 0)
//            {
//                _dashboardPageData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
//            }
//            if (isRefreshRequest && await _uiOperationTaskCompletion.Task.ConfigureAwait(true))
//            {
//                AppHelper.ShowBusyIndicator = true;
//            }
//            _uiOperationTaskCompletion = new TaskCompletionSource<bool>();
//            _uiOperationTaskCompletion.SetResult(await LoadUIAsync().ConfigureAwait(true));
//        });
//    }

//    /// <summary>
//    /// Unregister events of Views
//    /// </summary>
//    public override async Task UnloadUIAsync()
//    {
//        _addObservationBtn.Clicked -= OnAddObservationButtonClick;
//        await MainThread.InvokeOnMainThreadAsync(async () =>
//        {
//            try
//            {
//                if (ParentPage.PageLayout?.Children?.Count > 0)
//                {
//                    foreach (var children in ParentPage.PageLayout.Children)
//                    {
//                        await CheckChildViewType(children.GetType().Name, (View)children).UnloadUIAsync().ConfigureAwait(true);
//                        //Check to Avoid continuing iteration if the View are cleared from Children,
//                        //hence stop the Invalid Operation Exception Error from Occuring
//                        if (_areViewCleared)
//                        {
//                            _areViewCleared = false;
//                            break;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                ParentPage.LogErrors($"{nameof(DashboardView)}.UnloadUIAsync()", ex);
//            }
//        }).ConfigureAwait(true);
//    }

//    /// <summary>
//    /// Map parameter values into object
//    /// </summary>
//    /// <param name="key">message key</param>
//    /// <param name="viewType">type of message view</param>
//    /// <returns>Parameters object</returns>
//    public List<SystemFeatureParameterModel> MapParameters(string key, MessageViewType viewType)
//    {
//        return ParentPage.AddParameters(
//            ParentPage.CreateParameter(nameof(MessageViewType), ((int)viewType).ToString()),
//            ParentPage.CreateParameter(nameof(ResourceModel.ResourceKey), string.IsNullOrWhiteSpace(key) 
//                ? ResourceConstants.R_NO_DATA_FOUND_KEY 
//                : key
//            ));
//    }

//    /// <summary>
//    /// Renders all views parallely in given layout
//    /// </summary>
//    /// <param name="sequenceNo">Sequence in which child needs to add</param>
//    /// <param name="feature">Feature which will be added</param>
//    /// <param name="parameters">Featue parameters to render view</param>
//    protected async Task RenderViewsAsync(int sequenceNo, ConfigureDashboardModel feature, List<SystemFeatureParameterModel> parameters)
//    {
//        await MainThread.InvokeOnMainThreadAsync(async () =>
//        {
//            ViewManager viewManager = MapFeaturesCodeToView(feature.FeatureCode, parameters);
//            viewManager.Title = feature.FeatureText;
//            viewManager.NodeID = feature.NodeID;
//            viewManager.ShowTitle = true;
//            await viewManager.LoadUIAsync(false).ConfigureAwait(true);
//            ParentPage.PageLayout?.Add(viewManager, 0, sequenceNo);
//        }).ConfigureAwait(true);
//    }

//    /// <summary>
//    /// Refreshes all views of given layout
//    /// </summary>
//    protected async Task ReloadAllViewsAsync()
//    {
//        await MainThread.InvokeOnMainThreadAsync(async () =>
//        {
//            if (ParentPage.PageLayout?.Children?.Count > 0)
//            {
//                foreach (var children in ParentPage.PageLayout.Children)
//                {
//                    await CheckChildViewType(children.GetType().Name, (View)children).LoadUIAsync(true).ConfigureAwait(true);
//                }
//            }
//        }).ConfigureAwait(true);
//    }

//    private async Task<bool> LoadUIAsync()
//    {
//        await GetDataAsync().ConfigureAwait(true);
//        await MainThread.InvokeOnMainThreadAsync(async () =>
//        {
//            AppHelper.ShowBusyIndicator = true;
//            InitializeViews();
//            DisplaySyncStatus();
//            if (_dashboardPageData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(_dashboardPageData.ConfigurationRecords))
//            {
//                ParentPage.SetPageLayoutOption(LayoutOptions.StartAndExpand, false);
//                ParentPage.AddRowColumnDefinition(GridLength.Auto, _dashboardPageData.ConfigurationRecords.Count, true);
//                //Create IEnumerable<Task> for each listViews to render them parallely.When all task complete their work, continue to next line
//                await Task.WhenAll(
//                    from viewSetting in _dashboardPageData.ConfigurationRecords
//                    orderby viewSetting.SequenceNo
//                    select RenderViewsAsync(Array.IndexOf(_dashboardPageData.ConfigurationRecords.ToArray(), viewSetting), viewSetting,
//                    _dashboardPageData.ConfigurationRecordParameters?.Where(parameter => 
//                        parameter.DashboardSettingID == viewSetting.DashboardSettingID 
//                        && parameter.FeatureID == viewSetting.FeatureID
//                    )?.OrderBy(x => x.Sequence).ToList())
//                ).ConfigureAwait(true);
//            }
//            else
//            {
//                ParentPage.SetPageLayoutOption(LayoutOptions.CenterAndExpand, false);
//                ParentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
//                var key = _dashboardPageData.ErrCode == ErrorCode.OK 
//                    ? ResourceConstants.R_NO_DATA_FOUND_KEY 
//                    : _dashboardPageData.ErrCode.ToString();
//                if (!string.IsNullOrWhiteSpace(_noDataIcon))
//                {
//                    ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == key).KeyDescription = _noDataIcon;
//                }
//                _defaultView.Parameters = MapParameters(key, MessageViewType.StaticMessageView);
//                ParentPage.PageLayout?.Add(_defaultView, 0, 0);
//                await _defaultView.LoadUIAsync(false).ConfigureAwait(true);
//            }
//            AppHelper.ShowBusyIndicator = false;
//        });
//        return true;
//    }

//    private void DisplaySyncStatus()
//    {
//        if (ShellMasterPage.CurrentShell.CurrentPage != null 
//            && ShellMasterPage.CurrentShell.CurrentPage.ToString().EndsWith(Constants.DASHBOARD_PAGE_CONSTANT, StringComparison.InvariantCultureIgnoreCase))
//        {
//            if (MobileConstants.CheckInternet)
//            {
//                ParentPage.SyncStatusView.UpdateShowBackgroundSyncField(true, LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, ErrorCode.SyncInProgress.ToString()), false);
//            }
//            else
//            {
//                ParentPage.SyncStatusView.UpdateShowBackgroundSyncField(false
//                    , string.Format(CultureInfo.InvariantCulture
//                    , LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, ResourceConstants.R_LAST_SYNCED_DATE_TEXT_KEY)
//                    , GenericMethods.GetLocalDateTimeBasedOnCulture(DateTimeOffset.Parse(
//                        App._essentials.GetPreferenceValue(
//                            StorageConstants.PR_LAST_SYNCED_DATE_KEY, 
//                            GenericMethods.GetUtcDateTime.ToString(CultureInfo.InvariantCulture)
//                        ), CultureInfo.InvariantCulture)
//                    , DateTimeType.DateTime
//                    , ParentPage.GetSettingsValueByKey(_dayFormatKey)
//                    , ParentPage.GetSettingsValueByKey(_monthFormatKey)
//                    , string.Empty
//                    )
//                ), false);
//            }
//        }
//    }

//    /// <summary>
//    /// Initialize viewes
//    /// </summary>
//    private void InitializeViews()
//    {
//        ParentPage.PageLayout.Children?.Clear();
//        _areViewCleared = true;
//        ParentPage.PageLayout.RowDefinitions = new RowDefinitionCollection();
//        ParentPage.CreateSyncStatusView();
//    }

//    /// <summary>
//    /// Gets pagedata from db
//    /// </summary>
//    /// <returns>page data</returns>
//    public async Task GetDataAsync()
//    {
//        _addObservationBtn.Clicked -= OnAddObservationButtonClick;
//        await (ParentPage.PageService as DashboardService).GetDashboardDataAsync(_dashboardPageData as DashboardDTO).ConfigureAwait(true);
//        await MainThread.InvokeOnMainThreadAsync(() =>
//        {
//            if (IsPatientPage())
//            {
//                ParentPage.PageLayout.Padding = new Thickness(-Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_TOP_PADDING], CultureInfo.InvariantCulture), 0);
//            }
//            ParentPage.PageData = ParentPage.PageService.PageData;
//            _dayFormatKey = SettingsConstants.S_EXTENDED_DAY_FORMAT_KEY;
//            _monthFormatKey = SettingsConstants.S_MONTH_FORMAT_KEY;
//            _buttonwrapper.IsVisible = _dashboardPageData.IsActive;
//            if (_buttonwrapper.IsVisible)
//            {
//                _addObservationBtn.Clicked += OnAddObservationButtonClick;
//            }
//        });
//    }

//    private async void OnAddObservationButtonClick(object sender, EventArgs e)
//    {
//        AppHelper.ShowBusyIndicator = true;
//        if (MobileConstants.IsTablet)
//        {
//            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingsPage)
//                , GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.type, Param.id, Param.identifier)
//                , string.Empty, string.Empty, string.Empty, true.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
//        }
//        else
//        {
//            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingPage)
//                , GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.type, Param.id)
//                , string.Empty, string.Empty, string.Empty).ConfigureAwait(false);
//        }
//    }

//    /// <summary>
//    /// Map permission codes with features
//    /// </summary>
//    /// <param name="featureCode">feature to map with view</param>
//    /// <param name="parameters">Featue parameters to render view</param>
//    /// <returns>View to display in page</returns>
//    public ViewManager MapFeaturesCodeToView(string featureCode, List<SystemFeatureParameterModel> parameters)
//    {
//        //return new DefaultContentView(null, MapParameters(ErrorCode.NotImplemented.ToString(), MessageViewType.PinacleView));
//        if (_dashboardPageData.SelectedUserID > 0)
//        {
//            parameters = parameters ?? new List<SystemFeatureParameterModel>();
//            parameters.Add(ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID)
//                , _dashboardPageData.SelectedUserID.ToString(CultureInfo.InvariantCulture)));
//        }
//        var viewManager = MapFeature(featureCode, parameters);
//        viewManager.NavigationText = LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, ResourceConstants.R_SHOW_MORE_KEY);
//        if (IsPatientPage())
//        {
//            viewManager.FeatureCode = featureCode;
//            viewManager.OnMoreOptionClicked += OnSeeMoreOptionClicked;
//        }
//        return viewManager;
//    }

//    private async void OnSeeMoreOptionClicked(object sender, EventArgs e)
//    {
//        if (IsPatientPage())
//        {
//            AppHelper.ShowBusyIndicator = true;
//            var view = sender.ToString();
//            if (view.Equals(AppPermissions.ChatsView.ToString()))
//            {
//                view = AppPermissions.ChatView.ToString();
//            }
//            await (ShellMasterPage.CurrentShell.CurrentPage as PatientsPage).LoadTragetViewAsync(view).ConfigureAwait(true);
//            AppHelper.ShowBusyIndicator = false;
//        }
//    }

//    /// <summary>
//    /// Maps child instance in a perticular view type 
//    /// </summary>
//    /// <param name="syncFrom">View from where sync is called</param>
//    /// <param name="child">Layout child to whome type which is needs to identify</param>
//    /// <returns>Instance of view in perticular type</returns>
//    public ViewManager CheckChildViewType(string syncFrom, View child)
//    {
//        //return child as DefaultContentView;
//        switch (syncFrom.ToEnum<Pages>())
//        {
//            case Pages.AppointmentsView:
//                return child as AppointmentsView;
//            case Pages.CaregiversView:
//                return child as CaregiversView;
//            case Pages.ChatsView:
//                return child as ChatsView;
//            case Pages.EducationsView:
//            case Pages.MyEducationsView:
//            case Pages.PatientEducationsView:
//                return child as EducationsView;
//            case Pages.PatientFilesView:
//                return child as PatientFilesView;
//            case Pages.PatientsView:
//                return child as PatientsView;
//            case Pages.PatientTasksView:
//                return child as PatientTasksView;
//            case Pages.PatientReadingsView:
//                return child as PatientReadingsView;
//            case Pages.PatientMedicationsView:
//                return child as PatientMedicationsView;
//            case Pages.BillingReportsView:
//                return child as BillingReportsView;
//            case Pages.TrackersView:
//            case Pages.PatientTrackersView:
//                return child as PatientTrackersView;
//            case Pages.PatientProviderNotesView:
//                return child as PatientProviderNotesView;
//            default:
//                return child as DefaultContentView;
//        }
//    }

//    private ViewManager MapFeature(string featureCode, List<SystemFeatureParameterModel> parameters)
//    {
//        switch (featureCode.ToEnum<AppPermissions>())
//        {
//            case AppPermissions.PatientAppointmentsView:
//            case AppPermissions.AppointmentsView:
//                return new AppointmentsView(null, parameters);
//            case AppPermissions.CaregiversView:
//                return new CaregiversView(null, parameters);
//            case AppPermissions.ChatsView:
//                return new ChatsView(null, parameters);
//            case AppPermissions.EducationsView:
//            case AppPermissions.MyEducationsView:
//            case AppPermissions.PatientEducationsView:
//                parameters.Add(new SystemFeatureParameterModel
//                {
//                    ParameterName = nameof(ContentPageModel.IsPatientPage),
//                    ParameterValue = IsPatientPage().ToString(CultureInfo.InvariantCulture),
//                });
//                return new EducationsView(ShellMasterPage.CurrentShell.BaseContentPageInstance, parameters);
//            case AppPermissions.PatientFilesView:
//                return new PatientFilesView(null, parameters);
//            case AppPermissions.PatientsView:
//                return new PatientsView(null, parameters);
//            case AppPermissions.PatientTasksView:
//                return new PatientTasksView(null, parameters);
//            ////case AppPermissions.PatientTasksView:
//            ////    return new PatientTasksProviderView(null, parameters);
//            case AppPermissions.PatientReadingsView:
//                return new PatientReadingsView(null, parameters);
//            case AppPermissions.PatientProgramsView:
//                return new PatientProgramsView(null, parameters);
//            case AppPermissions.PatientView:
//                return new PatientDemographicsView(null, parameters);
//            case AppPermissions.ChatView:
//                return new ChatView(null, parameters);
//            case AppPermissions.PatientMedicationsView:
//                return new PatientMedicationsView(null, parameters);
//            case AppPermissions.BillingReportsView:
//                return new BillingReportsView(null, parameters);
//            case AppPermissions.TrackersView:
//            case AppPermissions.PatientTrackersView:
//                return new PatientTrackersView(null, parameters);
//            case AppPermissions.PatientProviderNotesView:
//                return new PatientProviderNotesView(null, parameters);
//            default:
//                return new DefaultContentView(null, MapParameters(ErrorCode.NotImplemented.ToString(), MessageViewType.PinacleView));
//        }
//    }
//}