using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientTrackersView : BaseLibCollectionView
{
    private readonly TrackerDTO _patientTrackersDTO;
    private Grid _mainLayout;
    private readonly bool _isDashboard;
    private CustomTabsControl _customTabs;
    private PatientTrackerAddEdit _patientTrackerView;
    private PatientTrackerDetailView _patientTrackerDetailView;
    private readonly CustomMessageControl _emptyMessageView;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientTrackersView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new PatientTrackerService(App._essentials);
        _patientTrackersDTO = new TrackerDTO { PatientTrackers = new List<PatientTrackersModel>(), PatientTracker = new PatientTrackersModel() };
        MapParameters();
        _isDashboard = !string.IsNullOrWhiteSpace(_patientTrackersDTO.PatientTracker.TrackerName) || IsDashboardView(_patientTrackersDTO.RecordCount);
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        CustomCellModel customCellModel = isPatientData
           ? new CustomCellModel
           {
               CellID = nameof(PatientTrackersModel.PatientTrackerID),
               CellHeader = nameof(PatientTrackersModel.TrackerName),
               BandColor = nameof(PatientTrackersModel.ProgramColor),
               CellLeftDefaultIcon = nameof(PatientTrackersModel.LeftDefaultIcon),
               //todo:CellLeftSourceIcon = nameof(PatientTrackersModel.LeftSourceIcon),
               CellDescription = nameof(PatientTrackersModel.CurrentValueDisplayFormatString),
               IconSize = AppImageSize.ImageSizeM,
               ArrangeHorizontal = false
           }
           : new CustomCellModel
           {
               CellID = nameof(PatientTrackersModel.PatientTrackerID),
               CellHeader = nameof(PatientTrackersModel.TrackerName),
               BandColor = nameof(PatientTrackersModel.ProgramColor),
               CellLeftDefaultIcon = nameof(PatientTrackersModel.LeftDefaultIcon),
               //todo:CellLeftSourceIcon = nameof(PatientTrackersModel.LeftSourceIcon),
               CellDescription = nameof(PatientTrackersModel.CurrentValueDisplayFormatString),
               CellRightContentHeader = nameof(PatientTrackersModel.FromDateDisplayFormatString),
               CellRightContentDescription = nameof(PatientTrackersModel.ToDateDisplayFormatString),
               ErrCode = nameof(PatientTrackersModel.ErrCode),
               CellRightContentDescriptionInPancakeView = false,
               IconSize = AppImageSize.ImageSizeM,
               ArrangeHorizontal = false
           };
        IsTabletListHeaderDisplay = IsPatientPage() && !_isDashboard;
        CreateGridAndAssignList(customCellModel);
        SetPageContent(_mainLayout);
        _emptyMessageView = new CustomMessageControl(false);
    }

    private void CreateGridAndAssignList(CustomCellModel customCellData)
    {
        _mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = !_isDashboard && DeviceInfo.Idiom == DeviceIdiom.Tablet
                ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)
                : Constants.ZERO_PADDING,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = IsPatientPage() || _isDashboard ? new GridLength(1, GridUnitType.Star) : GridLength.Auto },
            },
            ColumnDefinitions = CreateTabletViewColumn(_isDashboard || DeviceInfo.Idiom == DeviceIdiom.Phone)
        };
        if (_isDashboard)
        {
            AddCollectionView(_mainLayout, customCellData, 0, 1);
        }
        else
        {
            if (IsPatientPage())
            {
                AddCollectionViewWithTabletHeader(_mainLayout, customCellData);
            }
            else
            {
                _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                AddSearchView(_mainLayout, false);
                _customTabs = new CustomTabsControl
                {
                    Margin = new Thickness(0, (double)App.Current.Resources[StyleConstants.ST_APP_PADDING], 0, 0),
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
                bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
                if (isPatientData)
                {
                    _mainLayout.Add(_customTabs, 0, 1);
                }
                AddCollectionView(_mainLayout, customCellData, 0, 2);
                if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                {
                    AddSeparatorView(_mainLayout, 1, 0);
                    Grid.SetRowSpan(Separator, 3);
                }
            }
        }
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            _patientTrackersDTO.SelectedUserID = (ParentPage.PageService as PatientTrackerService).GetUserID(IsPatientPage());
        }
        MapParameters();
        if (_patientTrackersDTO.ErrCode == ErrorCode.OK)
        {
            OnListItemSelection(Tracker_SelectionChanged, !IsPatientOverview(
                _patientTrackersDTO.RecordCount > 0
                    ? _patientTrackersDTO.RecordCount
                : !string.IsNullOrWhiteSpace(_patientTrackersDTO.PatientTracker.TrackerName)
                    ? 1 : 0));
            await (ParentPage.PageService as PatientTrackerService).GetPatientTrackersAsync(_patientTrackersDTO).ConfigureAwait(true);
            ParentPage.PageData = ParentPage.PageService.PageData;
            _emptyListView.PageResources = ParentPage.PageData;
            _emptyMessageView.PageResources = ParentPage.PageData;
            await RenderUIDataAsync(isRefreshRequest).ConfigureAwait(true);
        }
    }

    private void MapParameters()
    {
        if (Parameters?.Count > 0)
        {
            _patientTrackersDTO.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(TrackerDTO.RecordCount)));
            _patientTrackersDTO.PatientTracker.TrackerName = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientTrackersModel.TrackerName)));
            string patientTrackerId = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientTrackersModel.PatientTrackerID)));
            _patientTrackersDTO.PatientTracker.PatientTrackerID = string.IsNullOrWhiteSpace(patientTrackerId) ? Guid.Empty : new Guid(patientTrackerId);
        }
    }

    /// <summary>
    /// Unregister events of View
    /// </summary>
    public async override Task UnloadUIAsync()
    {
        OnListItemSelection(Tracker_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        if (_patientTrackerView != null)
        {
            await _patientTrackerView.UnloadUIAsync().ConfigureAwait(true);
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    private async Task RenderUIDataAsync(bool isRefreshRequest)
    {
        if (!_isDashboard)
        {
            SearchField.PageResources = ParentPage.PageData;
            SearchField.Value = string.Empty;
            SearchField.IsVisible = true;
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !IsPatientPage())
            {
                Separator.IsVisible = true;
            }
        }
        if (_patientTrackersDTO?.ErrCode == ErrorCode.OK)
        {
            await RenderTrackerDataAsync(isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            RenderErrorView(_mainLayout, _patientTrackersDTO?.ErrCode.ToString() ?? ErrorCode.ErrorWhileRetrievingRecords.ToString(), _isDashboard, _isDashboard ? (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] : 0, false, true);
        }
    }


    private async Task RenderTrackerDataAsync(bool isRefreshRequest)
    {
        if (IsPatientPage() && !_isDashboard)
        {
            if (HasAddPermission())
            {
                TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                if (!isRefreshRequest)
                {
                    OnActionButtonClicked += OnAddButtonClicked;
                }
            }
            else
            {
                HideAddButton(_mainLayout, true);
            }
            if (!_isDashboard)
            {
                TabletHeader.Text = GetHeaderText(_patientTrackersDTO.PatientTrackers.Count);
            }
        }
        CollectionViewField.ItemsSource = _patientTrackersDTO?.PatientTrackers;
        if (GenericMethods.IsListNotEmpty(_patientTrackersDTO.PatientTrackers))
        {
            await RenderViewsAsync().ConfigureAwait(true);
        }
        else
        {
            RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, _isDashboard, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            if (!_isDashboard)
            {
                await RenderDetailViewAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task RenderViewsAsync()
    {
        CollectionViewField.ItemsSource = _patientTrackersDTO.PatientTrackers;
        if (!_isDashboard)
        {
            await RenderDetailViewAsync().ConfigureAwait(true);
        }
        else
        {
            if (MobileConstants.IsDevicePhone)
            {
                _mainLayout.HeightRequest = DeviceInfo.Platform == DevicePlatform.iOS ? (CellRowHeight * _patientTrackersDTO.PatientTrackers.Count) + 3 : (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _patientTrackersDTO.PatientTrackers.Count + new OnIdiom<int> { Phone = 5, Tablet = 0 };
            }
            else
            {
                _mainLayout.HeightRequest = _margin + (((IsPatientOverview(_patientTrackersDTO.RecordCount) ? GenericMethods.GetPlatformSpecificValue(5, 7, 0) : GenericMethods.GetPlatformSpecificValue(2, 6, 0)) * _patientTrackersDTO.PatientTrackers.Count) + GenericMethods.GetPlatformSpecificValue(2, 5, 0)) + (CellRowHeight * _patientTrackersDTO.PatientTrackers.Count);
            }
        }
    }

    private void View_Clicked(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private async Task RenderDetailViewAsync()
    {
        if (MobileConstants.IsTablet && !IsPatientPage())
        {
            if (_patientTrackersDTO.PatientTracker?.PatientTrackerID != Guid.Empty)
            {
                await OnTrackerClickAsync(_patientTrackersDTO.PatientTracker.PatientTrackerID, _patientTrackersDTO.PatientTracker.TrackerName, false).ConfigureAwait(true);
                CollectionViewField.SelectedItem = _patientTrackersDTO.PatientTrackers.FirstOrDefault(x => x.PatientTrackerID == _patientTrackersDTO.PatientTracker.PatientTrackerID);
            }
            else
            {
                if (_patientTrackerDetailView != null && _mainLayout.Children.Contains(_patientTrackerDetailView))
                {
                    _mainLayout.Children.Remove(_patientTrackerDetailView);
                }
                _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                _mainLayout.Add(_emptyMessageView, 2, 0);
                Grid.SetRowSpan(_emptyMessageView, 3);
            }
        }
    }

    private async Task OnTrackerClickAsync(Guid patientTrackerId, string patientTrackerName, bool isAddButtonClicked)
    {
        if (isAddButtonClicked)
        {
            CollectionViewField.SelectedItem = null;
        }
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData && Device.Idiom != TargetIdiom.Phone && !_isDashboard)
        {
            //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientTrackerDetailsPage)).ConfigureAwait(true);
            if (_patientTrackerDetailView != null && _mainLayout.Children.Contains(_patientTrackerDetailView))
            {
                _mainLayout.Children.Remove(_patientTrackerDetailView);
            }
            if (_mainLayout.Children.Contains(_emptyMessageView))
            {
                _mainLayout.Children.Remove(_emptyMessageView);
            }
            _patientTrackerDetailView = new PatientTrackerDetailView(new BasePage(), ParentPage.AddParameters(
                ParentPage.CreateParameter(nameof(PatientTrackersModel.PatientTrackerID), Convert.ToString(patientTrackerId, CultureInfo.InvariantCulture)),
                ParentPage.CreateParameter(nameof(BaseDTO.IsActive), Convert.ToString(false, CultureInfo.InvariantCulture))));
            _mainLayout.Add(_patientTrackerDetailView, 2, 0);
            Grid.SetRowSpan(_patientTrackerDetailView, 3);
            _patientTrackerDetailView.OnSaveSuccess -= RefreshPatientTrackersList;
            _patientTrackerDetailView.OnSaveSuccess += RefreshPatientTrackersList;
            await SetRightPageHeaderAsync(patientTrackerName, true).ConfigureAwait(true);
            await _patientTrackerDetailView.LoadUIAsync(false).ConfigureAwait(true);
            //to handle kill state notification click
            CollectionViewField.ItemsSource = new List<PatientTrackersModel>();
            CollectionViewField.ItemsSource = _patientTrackersDTO.PatientTrackers;
        }
        else if (MobileConstants.IsTablet)
        {
            if (MobileConstants.IsTablet)
            {
                if (HasAddPermission())
                {
                    if (ShellMasterPage.CurrentShell.CurrentPage is PatientTrackersPage && !isPatientData)
                    {
                        //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientTrackerPage)).ConfigureAwait(true);
                        if (_patientTrackerView != null && _mainLayout.Children.Contains(_patientTrackerView))
                        {
                            _mainLayout.Children.Remove(_patientTrackerView);
                        }
                        if (_mainLayout.Children.Contains(_emptyMessageView))
                        {
                            _mainLayout.Children.Remove(_emptyMessageView);
                        }
                        _patientTrackerView = new PatientTrackerAddEdit(new BasePage(), ParentPage.AddParameters(
                            ParentPage.CreateParameter(nameof(PatientTrackersModel.PatientTrackerID), Convert.ToString(patientTrackerId, CultureInfo.InvariantCulture)),
                            ParentPage.CreateParameter(nameof(BaseDTO.IsActive), Convert.ToString(false, CultureInfo.InvariantCulture))));
                        _mainLayout.Add(_patientTrackerView, 2, 0);
                        Grid.SetRowSpan(_patientTrackerView, 3);
                        _patientTrackerView.OnSaveSuccess -= RefreshPatientTrackersList;
                        _patientTrackerView.OnSaveSuccess += RefreshPatientTrackersList;
                        await _patientTrackerView.LoadUIAsync(false).ConfigureAwait(true);
                        //to handle kill state notification click
                        CollectionViewField.ItemsSource = new List<PatientTrackersModel>();
                        CollectionViewField.ItemsSource = _patientTrackersDTO.PatientTrackers;
                    }
                    else if (IsPatientPage())
                    {
                        await OpenTrackerPopupAsync(patientTrackerId).ConfigureAwait(true);
                    }
                    else
                    {
                        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientTrackersPage.ToString(),
                            GenericMethods.GenerateParamsWithPlaceholder(Param.id), patientTrackerId.ToString()).ConfigureAwait(true);
                    }
                }
            }
            else
            {
                await OpenTrackerPopupAsync(patientTrackerId).ConfigureAwait(true);
            }
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientTrackerDetailsPage.ToString(),
                GenericMethods.GenerateParamsWithPlaceholder(Param.id), patientTrackerId.ToString()).ConfigureAwait(true);
        }
    }

    private bool HasAddPermission()
    {
        return ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientTrackerAddEdit.ToString());
    }

    private async void Tracker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (isPatientData)
        {
            await TrackerSelectedAsync(sender).ConfigureAwait(true);
        }
        else
        {
            if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
            {
                CollectionViewField.SelectionChanged -= Tracker_SelectionChanged;
                await TrackerSelectedAsync(sender).ConfigureAwait(true);
                CollectionViewField.SelectionChanged += Tracker_SelectionChanged;
            }
        }
    }

    private async Task TrackerSelectedAsync(object sender)
    {
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            await SetRightPageHeaderAsync(null, isPatientData).ConfigureAwait(true);
            await OnTrackerClickAsync((item.SelectedItem as PatientTrackersModel).PatientTrackerID, (item.SelectedItem as PatientTrackersModel).TrackerName, false).ConfigureAwait(true);
        }
    }

    private async Task SetRightPageHeaderAsync(string title, bool isLogin)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !(ShellMasterPage.CurrentShell.CurrentPage is DashboardPage))
        {
            //if (title == null && isLogin)
            //{
            //    await ParentPage.SetRightHeaderItemsAsync(nameof(PatientTrackerDetailsPage)).ConfigureAwait(true);
            //}
            //else
            //{
                await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, title, true)).ConfigureAwait(true);
            //}
        }
    }
    private void UpdateHeaderView(bool isRefreshRequest)
    {
        if (!IsDashboardView(_patientTrackersDTO.RecordCount) && IsPatientPage())
        {
            SearchField.PageResources = ParentPage.PageData;
            SearchField.Value = string.Empty;
            if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientTrackerAddEdit.ToString()))
            {
                TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                if (!isRefreshRequest && !IsPatientOverview(_patientTrackersDTO.RecordCount))
                {
                    OnActionButtonClicked += OnAddButtonClicked;
                }
            }
            else
            {
                HideAddButton(_mainLayout, true);
            }
            TabletHeader.Text = GetHeaderText(_patientTrackersDTO.PatientTrackers.Count);
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
        }
    }

    private async Task OpenTrackerPopupAsync(Guid patientTrackerId)
    {
        PatientTrackerPopUpPage trackerAddEditPage = new PatientTrackerPopUpPage(patientTrackerId);
        trackerAddEditPage.OnSaveButtonClicked += RefreshPatientTrackersList;
        //todo:await Navigation.PushPopupAsync(trackerAddEditPage).ConfigureAwait(true);
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            var programPage = new PatientTrackerPopUpPage(Guid.Empty);
            programPage.OnSaveButtonClicked += RefreshPatientTrackersList; ;
            //todo:await Navigation.PushPopupAsync(programPage).ConfigureAwait(true);
        }
    }

    private async void RefreshPatientTrackersList(object sender, EventArgs e)
    {
        // CollectionViewField.SelectedItem = null;
        if (sender != default)
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey((string)sender), (string)sender == ErrorCode.OK.ToString());
            if ((string)sender == ErrorCode.OK.ToString())
            {
                AppHelper.ShowBusyIndicator = true;
                await LoadUIAsync(true).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
        }
        if (sender == default)
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey((string)sender), (string)sender == default);
            if ((string)sender == default)
            {
                AppHelper.ShowBusyIndicator = true;
                await LoadUIAsync(true).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
        }
    }

    private async void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = new List<PatientTrackersModel>();
            await Task.Delay(2);
            CollectionViewField.ItemsSource = _patientTrackersDTO.PatientTrackers;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(_patientTrackersDTO.PatientTrackers.Count);
            }
        }
        else
        {
            var searchedUsers = _patientTrackersDTO.PatientTrackers.FindAll(y =>
            {
                return y.TrackerName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            CollectionViewField.ItemsSource = searchedUsers;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(searchedUsers.Count);
            }
            if (searchedUsers.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
    }

    private string GetHeaderText(int count)
    {
        return $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientTrackersView.ToString())} ({count})";
    }

    public async Task<bool> SavePatientTrackerAsync()
    {
        return _patientTrackerDetailView != null && _mainLayout.Children.Contains(_patientTrackerDetailView) && await _patientTrackerDetailView.SaveTrackerAsync().ConfigureAwait(true);
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_patientTrackersDTO.PatientTrackers))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _patientTrackersDTO.PatientTrackers.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }
}