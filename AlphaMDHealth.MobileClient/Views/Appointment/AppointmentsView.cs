using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class AppointmentsView : BaseLibCollectionView
{
    private AppointmentDTO _appointmentData = new AppointmentDTO { Appointments = new List<AppointmentModel>() };
    private AppointmentView _appointmentView;
    //private readonly CustomCalendarControl _calender;
    private readonly Grid _mainLayout;
    private readonly CustomMessageControl _emptyMessageView;
    private readonly bool _isDashboard;
    private bool _isFirstTimeButtonClick;
    private bool _isPatientData;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public AppointmentsView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new AppointmentService(App._essentials);
        MapParameters();
        _isDashboard = IsDashboardView(_appointmentData.RecordCount);
        var roleID = App._essentials.GetPreferenceValue(StorageConstants.PR_ROLE_ID_KEY, 0);
        _isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (_isDashboard || _isPatientData || _appointmentData.SelectedUserID > 0)
        {
            CustomCellModel customCellModel = new CustomCellModel
            {
                CellHeader = nameof(AppointmentModel.AppointmentTypeName),
                CellDescription = nameof(AppointmentModel.PageHeading),
                CellLeftDefaultIcon = nameof(AppointmentModel.AppointmentTypeImage),
                CellRightContentHeader = nameof(AppointmentModel.FromDateString),
                CellRightContentDescription = nameof(AppointmentModel.AppointmentStatusName),
                CellDescriptionColor = nameof(AppointmentModel.AppointmentStatusColor),
                IconSize = AppImageSize.ImageSizeM,
            };
            IsTabletListHeaderDisplay = IsPatientPage() && !_isDashboard;
            _mainLayout = new Grid
            {
                Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
                ColumnSpacing = !_isDashboard && MobileConstants.IsTablet ? Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture) : Constants.ZERO_PADDING,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions = CreateTabletViewColumn(_isDashboard || DeviceInfo.Idiom == DeviceIdiom.Phone)
            };
            LoadPatientView(customCellModel);
            _emptyMessageView = new CustomMessageControl(false);
        }
        else
        {
            _mainLayout = new Grid
            {
                Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                ColumnSpacing = ParentPage is AppointmentsPage ? Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture) : Constants.ZERO_PADDING,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                },
            };
            //todo:
            //_calender = new CustomCalendarControl
            //{
            //    ControlResourceKey = ResourceConstants.R_DAY_TEXT_KEY
            //};
            //_mainLayout.Add(_calender, 0, 0);
        }
        _isFirstTimeButtonClick = true;
        SetPageContent(_mainLayout);
    }

    private void LoadPatientView(CustomCellModel customCellModel)
    {
        if (IsTabletListHeaderDisplay)
        {
            AddCollectionViewWithTabletHeader(_mainLayout, customCellModel);
        }
        else
        {
            if (!_isDashboard)
            {
                AddSearchView(_mainLayout, false);
                if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                {
                    AddSeparatorView(_mainLayout, 1, 0);
                    Grid.SetRowSpan(Separator, 2);
                }
            }
            AddCollectionView(_mainLayout, customCellModel, 0, 1);
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
            MapParameters();
        }
        await (ParentPage.PageService as AppointmentService).GetAppointmentsAsync(_appointmentData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_isDashboard || _appointmentData.SelectedUserID > 0 && IsPatientPage() || _isPatientData)
        {
            await LoadUIForPatientAsync(_appointmentData, isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            if (!ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.AppointmentAddEdit.ToString()))
            {
                ParentPage.ShowHideLeftRightHeader(MenuLocation.Left, false);
            }
            await LoadUIForDoctorAsync(isRefreshRequest).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        OnListItemSelection(Appointments_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        if (TabletActionButton != null && ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.AppointmentAddEdit.ToString()))
        {
            OnActionButtonClicked -= OnAddButtonClicked;
        }
        _appointmentView?.UnLoadUIData();

        //todo:
        //if (_calender != null)
        //{
        //    _calender.OnAppointmentCliked -= Calender_OnAppointmentCliked;
        //    _calender.UnLoadUIData();
        //}
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Opens appointment add edit popup page
    /// </summary>
    /// <returns>Opens appointment add edit popup page</returns>
    public async Task OnAddButtonClickAsync()
    {
        if (_isFirstTimeButtonClick)
        {
            _isFirstTimeButtonClick = false;
            if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
            {
                await NavigateToAdddEditAppoinmentAsync(string.Empty).ConfigureAwait(false);
            }
        }
    }

    private async Task LoadUIForPatientAsync(AppointmentDTO appointments, bool isRefreshRequest)
    {
        _appointmentData = appointments;
        _emptyListView.PageResources = ParentPage.PageData;
        if (_emptyMessageView != null)
        {
            _emptyMessageView.PageResources = ParentPage.PageData;
        }
        ApplyPageHeader(isRefreshRequest);
        if (_appointmentData?.ErrCode == ErrorCode.OK)
        {
            await RenderAppointmentDataAsync(isRefreshRequest).ConfigureAwait(false);
        }
        else
        {
            RenderErrorView(_mainLayout, _appointmentData?.ErrCode.ToString() ?? ErrorCode.ErrorWhileRetrievingRecords.ToString(), _isDashboard, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }

    private void ApplyPageHeader(bool isRefreshRequest)
    {
        if (TabletHeader != null && ParentPage is PatientsPage)
        {
            ApplyTableHeaderText(_appointmentData.Appointments?.Count ?? 0);
        }
        if (!isRefreshRequest && !(ShellMasterPage.CurrentShell.CurrentPage is DashboardPage))
        {
            if (SearchField != null)
            {
                SearchField.Value = string.Empty;
                SearchField.PageResources = ParentPage.PageData;
                SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
            }
            if (TabletActionButton != null && ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.AppointmentAddEdit.ToString()))
            {
                TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                OnActionButtonClicked += OnAddButtonClicked;
            }
        }
    }

    private void ApplyTableHeaderText(int count)
    {
        if (IsPatientPage())
        {
            TabletHeader.Text = $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientAppointmentsView.ToString())} ({count})";
        }
        else
        {
            TabletHeader.Text = $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.AppointmentsView.ToString())} ({count})";
        }
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        OnActionButtonClicked -= OnAddButtonClicked;
        await OnAddButtonClickAsync().ConfigureAwait(true);
        OnActionButtonClicked += OnAddButtonClicked;
    }

    private async Task RenderAppointmentDataAsync(bool isRefreshRequest)
    {
        if (GenericMethods.IsListNotEmpty(_appointmentData.Appointments))
        {
            OnListItemSelection(Appointments_SelectionChanged, true && !IsPatientOverview(_appointmentData.RecordCount));
            CollectionViewField.ItemsSource = _appointmentData.Appointments;
            if (!_isDashboard)
            {
                if (MobileConstants.IsTablet && _appointmentData.SelectedUserID == 0)
                {
                    if (_appointmentData.Appointment.AppointmentID > 0)
                    {
                        await AppointmentClickAsync(_appointmentData.Appointment).ConfigureAwait(true);
                        CollectionViewField.SelectedItem = _appointmentData.Appointments.FirstOrDefault(x => x.AppointmentID == _appointmentData.Appointment.AppointmentID);
                    }
                    else
                    {
                        RenderEmptyDetailView();
                    }
                }
            }
            else
            {
                _mainLayout.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _appointmentData.Appointments.Count + new OnIdiom<int> { Phone = 10, Tablet = 0 };
            }
        }
        else
        {
            RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, _isDashboard, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            if (MobileConstants.IsTablet && !_isDashboard && _appointmentData.SelectedUserID == 0)
            {
                RenderEmptyDetailView();
            }
        }
    }

    private void RenderEmptyDetailView()
    {
        CollectionViewField.SelectedItem = null;
        if (_appointmentView != null && _mainLayout.Children.Contains(_appointmentView))
        {
            _mainLayout.Children.Remove(_appointmentView);
        }
        _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
        _mainLayout.Add(_emptyMessageView, 2, 0);
        Grid.SetRowSpan(_emptyMessageView, 2);
    }

    private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = _appointmentData.Appointments;
        }
        else
        {
            var searchedUsers = _appointmentData.Appointments.FindAll(y =>
            {
                return y.PageHeading.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                || y.AppointmentTypeName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });

            CollectionViewField.ItemsSource = searchedUsers;
            if (searchedUsers.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
        if (TabletHeader != null)
        {
            ApplyTableHeaderText(CollectionViewField.ItemsSource?.Cast<object>().Count() ?? 0);
        }
    }

    private async void Appointments_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CollectionViewField.SelectionChanged -= Appointments_SelectionChanged;
        var item = sender as CollectionView;
        if (item.SelectedItem != null && ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.AppointmentView.ToString()))
        {
            await SetRightPageHeaderAsync(null).ConfigureAwait(true);
            await AppointmentClickAsync(item.SelectedItem as AppointmentModel).ConfigureAwait(true);
        }
        CollectionViewField.SelectionChanged += Appointments_SelectionChanged;
    }

    private async Task AppointmentClickAsync(AppointmentModel Appointment)
    {
        if (MobileConstants.IsTablet)
        {
            if (ShellMasterPage.CurrentShell.CurrentPage is AppointmentsPage)
            {
                if (_appointmentView != null && _mainLayout.Children.Contains(_appointmentView))
                {
                    _mainLayout.Children.Remove(_appointmentView);
                }
                if (_mainLayout.Children.Contains(_emptyMessageView))
                {
                    _mainLayout.Children.Remove(_emptyMessageView);
                }
                _appointmentView = new AppointmentView(ParentPage, null);
                _mainLayout.Add(_appointmentView, 2, 0);
                Grid.SetRowSpan(_appointmentView, 2);
                await SetRightPageHeaderAsync(Appointment.AppointmentTypeName).ConfigureAwait(true);
                await _appointmentView.LoadUIDataAsync(false, (Appointment.AppointmentID).ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);

            }
            else if (_appointmentData.SelectedUserID > 0)
            {
                CollectionViewField.SelectedItem = null;
                if (ShellMasterPage.CurrentShell.CurrentPage is PatientsPage)
                {
                    if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
                    {
                        await DisplayAppointmentViewPopupAsync(Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                    }
                }
                else
                {
                    await DisplayAppointmentViewPopupAsync(Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                }
            }
            else
            {
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.AppointmentsPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.appointmentID), "0", Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            }
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.AppointmentViewPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.appointmentID), Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
        }
    }

    private async Task SetRightPageHeaderAsync(string title)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !(ShellMasterPage.CurrentShell.CurrentPage is DashboardPage))
        {
            //if (title == null)
            //{
            //    await ParentPage.SetRightHeaderItemsAsync(nameof(AppointmentViewPage)).ConfigureAwait(true);
            //}
            //else
            //{
                await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, title, true)).ConfigureAwait(true);
            //}
        }
    }

    private async void Calender_OnAppointmentCliked(object sender, EventArgs e)
    {
        //todo:
        //_calender.OnAppointmentCliked -= Calender_OnAppointmentCliked;
        //var item = sender as Grid;
        //if (!string.IsNullOrWhiteSpace(item.StyleId) && ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.AppointmentView.ToString()))
        //{
        //    await DisplayAppointmentViewPopupAsync(item.StyleId).ConfigureAwait(false);
        //}
        //_calender.OnAppointmentCliked += Calender_OnAppointmentCliked;
    }

    private async Task DisplayAppointmentViewPopupAsync(string appointmentID)
    {
        AppointmentViewPopupPage appointmentViewPage = new AppointmentViewPopupPage(appointmentID);
        appointmentViewPage.OnEditButtonClicked += OnEditAppointmentClicked;
        //todo:await Navigation.PushPopupAsync(appointmentViewPage).ConfigureAwait(false);
    }

    private async void OnEditAppointmentClicked(object sender, EventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            await NavigateToAdddEditAppoinmentAsync(sender as string).ConfigureAwait(false);
        }
    }

    private async Task NavigateToAdddEditAppoinmentAsync(string appointmentID)
    {
        //todo:
        //DateTime selectedDate = _calender == null
        //    ? DateTime.Now.Date
        //    : DateTime.Now.Date;//todo:_calender._calendar.SelectedDate;
        //AppointmentPopupPage appointmentAddEditPage = new AppointmentPopupPage(appointmentID, _appointmentData.SelectedUserID, selectedDate);
        //appointmentAddEditPage.OnSaveButtonClicked += OnAppointmentChanged;
        //appointmentAddEditPage.OnCloseButtonClickedEvent += OnAppointmentClosed;
        //appointmentAddEditPage.OnLeftHeaderClickedEvent += OnAppointmentClosed;
        //appointmentAddEditPage.OnRightHeaderClickedEvent += OnAppointmentClosed;
        ////todo:await Navigation.PushPopupAsync(appointmentAddEditPage).ConfigureAwait(false);
    }

    private void OnAppointmentClosed(object sender, EventArgs e)
    {
        _isFirstTimeButtonClick = true;
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_appointmentData.Appointments))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _appointmentData.Appointments.Count + 60;//// + new OnIdiom<int> { Phone = 10, Tablet = 0 };
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }

    private async void OnAppointmentChanged(object sender, EventArgs e)
    {
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
    }

    private async Task LoadUIForDoctorAsync(bool isRefreshRequest)
    {
        //todo:
        //if (!isRefreshRequest && !IsPatientOverview(_appointmentData.RecordCount))
        //{
        //    _calender.OnAppointmentCliked += Calender_OnAppointmentCliked;
        //}
        //_appointmentData.SheduledAppoinments = _appointmentData.Appointments.OrderBy(x => x.FromDateTime.TimeOfDay).GroupBy(x => x.FromDateTime);
        //_calender.PageResources = ParentPage.PageData;
        //_calender.Appointments = _appointmentData.SheduledAppoinments;
        //await MainThread.InvokeOnMainThreadAsync(async () =>
        // {
        //     _calender.DayViewLoad(DateTime.Today);
        //     if (_appointmentData.Appointment.AppointmentID > 0)
        //     {
        //         await DisplayAppointmentViewPopupAsync(_appointmentData.Appointment.AppointmentID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
        //     }
        // }).ConfigureAwait(false);
    }

    private void MapParameters()
    {
        if (Parameters?.Count > 0)
        {
            _appointmentData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _appointmentData.Appointment = new AppointmentModel
            {
                AppointmentID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(AppointmentModel.AppointmentID)))
            };
            _appointmentData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        }
    }
}