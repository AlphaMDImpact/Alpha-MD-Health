using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientsView : BaseLibCollectionView
{
    private readonly UserDTO _userData = new UserDTO();
    private readonly Grid _mainLayout;
    private readonly Grid _overlayGrid;
    private readonly CustomMessageControl _emptyDetailView;
    private readonly CustomButtonControl _showMoreButton;
    private readonly CustomCellModel _customCellModel;
    private PatientHeaderView _patinetView;
    private readonly double _padding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
    private readonly double _detailViewSpacing = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_TRIPLE_ROW_HEIGHT], CultureInfo.InvariantCulture);
    private bool _isExpanded = true;
    private long _patientID;
    private bool _isShowButtonClicked;
    private bool _isCollectionRefreshed;
    private string _searchText;
    private bool _showInternetErrorMsg = true;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientsView(BasePage page, object parameters) : base(page, parameters)
    {
        if (Parameters?.Count > 0)
        {
            _userData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _customCellModel = new CustomCellModel
        {
            CellHeader = nameof(UserModel.FirstName),
            ErrCode = nameof(UserModel.ErrorCode),
            CellDescription = nameof(UserModel.LastName),
            //todo:CellLeftSourceIcon = nameof(UserModel.ImageSource),
            CellLeftDefaultIcon = nameof(UserModel.ImageName),
            RowHeight = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] / 2
        };
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
            },
            ColumnDefinitions = CreateTabletViewColumn(true),
        };
        _overlayGrid = new Grid
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR],
            HorizontalOptions = LayoutOptions.Start,
            ColumnSpacing = _padding,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnDefinitions = IsDashboardView(_userData.RecordCount) ? CreateTabletViewColumn(true) : CreateOverlayViewColumn()
        };
        AddCollectionView(_overlayGrid, _customCellModel, 0, 1);
        if (!IsDashboardView(_userData.RecordCount))
        {
            _showMoreButton = new CustomButtonControl(ButtonType.TransparentWithoutMargin)
            {
                ImageSource = ImageSource.FromResource(AppStyles.NameSpaceImage + GetIcon(false)),//todo:, Constants.MORE_BUTTON_WIDTH, Constants.MORE_BUTTON_WIDTH, Color.Default),
                WidthRequest = Constants.MORE_BUTTON_WIDTH,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Margin = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(-(Constants.MORE_BUTTON_WIDTH / 2), 0, 0, 0) : new Thickness(0, 0, -(Constants.MORE_BUTTON_WIDTH / 2), 0),
            };
            InitializePatientView();
            AddSearchView(_overlayGrid, false);
            SearchField.OnSearchFocused += OnSearchBarFocused;
            if (MobileConstants.IsTablet)
            {
                AddSeparatorView(_overlayGrid, 1, 0);
                Grid.SetRowSpan(Separator, 2);
                _overlayGrid.Add(_showMoreButton, 1, 0);
                Grid.SetRowSpan(_showMoreButton, 2);
                _showMoreButton.Clicked += OnShowMoreClicked;
            }
        }
        _mainLayout.Add(_overlayGrid, 0, 0);
        SetPageContent(_mainLayout);
        _emptyDetailView = new CustomMessageControl(false) { IsVisible = false };
        _mainLayout.Add(_emptyDetailView, 0, 0);
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
            _userData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(UserDTO.RecordCount)));
            _patientID = GenericMethods.MapValueType<long>(GetParameterValue(SPFieldConstants.FIELD_USER_ID));
        }
        await Task.WhenAll(
            ParentPage.GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_USER_PROFILE_PAGE_GROUP),
            new UserService(App._essentials).GetUsersAsync(_userData)
        ).ConfigureAwait(true);
        _emptyDetailView.PageResources = ParentPage.PageData;
        _emptyListView.PageResources = ParentPage.PageData;
        _searchText = ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SEARCH_TEXT_KEY).ResourceValue;
        if (_userData.ErrCode == ErrorCode.OK)
        {
            if (GenericMethods.IsListNotEmpty(_userData.Users))
            {

                CollectionViewField.ItemsSource = _userData.Users;
                OnListItemSelection(Patient_SelectionChanged, true);
                if (!IsDashboardView(_userData.RecordCount))
                {
                    if (!_isExpanded)
                    {
                        ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SEARCH_TEXT_KEY).ResourceValue = string.Empty;
                    }
                    SearchField.PageResources = ParentPage.PageData;
                    SearchField.Value = string.Empty;
                    SearchField.OnSearchTextChanged += OnSearchTextChanged;
                    if (_showInternetErrorMsg)
                    {
                        await SetDetailContentAsync(_patientID).ConfigureAwait(true);
                    }
                    if (!_isExpanded && _patientID == 0)
                    {
                        await OnButtonClickedAsync().ConfigureAwait(true);
                    }
                }
                else
                {
                    _mainLayout.HeightRequest = ((double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _userData.Users.Count) - (_margin * (_userData.Users.Count - 1));
                }
            }
            else
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, IsDashboardView(_userData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
                await SetDetailContentAsync(0).ConfigureAwait(true);
            }
        }
        else
        {
            RenderErrorView(_mainLayout, _userData.ErrCode.ToString(), IsDashboardView(_userData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        OnListItemSelection(Patient_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= OnSearchTextChanged;
        }
        if (_patinetView != null)
        {
            _patinetView.OnPatientListRefreshCallBack -= PatientListRefreshAsync;
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    private async void OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = new List<UserModel>();
            await Task.Delay(10);
            CollectionViewField.ItemsSource = _userData.Users;
        }
        else
        {
            var searchedUsers = _userData.Users.FindAll(y =>
            {
                return !string.IsNullOrWhiteSpace(y.FirstName) && y.FirstName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            CollectionViewField.ItemsSource = searchedUsers;
            if (searchedUsers.Count == 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
    }

    private async void OnShowMoreClicked(object sender, EventArgs e)
    {
        await OnButtonClickedAsync().ConfigureAwait(true);
    }

    private async void OnSearchBarFocused(object sender, EventArgs e)
    {
        if (!_isExpanded)
        {
            await OnButtonClickedAsync().ConfigureAwait(true);
        }
    }

    private async Task OnButtonClickedAsync()
    {
        _isShowButtonClicked = true;
        ShowHidePatientList();
        if (GenericMethods.IsListNotEmpty(_userData.Users))
        {
            await Task.Delay(10).ConfigureAwait(true);
            var selectedPatient = _userData.Users.FirstOrDefault(x => x.UserID == _patientID);
            CollectionViewField.SelectedItem = selectedPatient;
            int index = _userData.Users.IndexOf(selectedPatient);
            CollectionViewField.ScrollTo(index, animate: false);
        }
        else
        {
            if (_isExpanded)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, IsDashboardView(_userData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            }
        }
        _isShowButtonClicked = false;
        _isCollectionRefreshed = false;
    }

    private void ShowHidePatientList()
    {
        _showMoreButton.ImageSource = ImageSource.FromResource(AppStyles.NameSpaceImage + GetIcon(_isExpanded));//todo: Constants.MORE_BUTTON_WIDTH, Constants.MORE_BUTTON_WIDTH, Color.Default);
        _overlayGrid.ColumnDefinitions[0].Width = _isExpanded ? AppStyles.GetImageSize(AppImageSize.ImageSizeL) + 2 * _padding : _screenWidth * 0.3 - 1 - _padding;
        _customCellModel.ShowIconOnly = _isExpanded;
        //ParentPage.ExpandCollapseLeftHeader(MenuLocation.Left, !_isExpanded);
        if (_isExpanded)
        {
            SearchField.UnFocusSearchBar();
            ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SEARCH_TEXT_KEY).ResourceValue = string.Empty;
        }
        else
        {
            ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SEARCH_TEXT_KEY).ResourceValue = _searchText;
        }
        SearchField.PageResources = ParentPage.PageData;
        SearchField.Value = string.Empty;
        _isExpanded = !_isExpanded;
        OnListItemSelection(Patient_SelectionChanged, false);
        _overlayGrid.Children.Remove(CollectionViewField);
        AddCollectionView(_overlayGrid, _customCellModel, 0, 1);
        _isCollectionRefreshed = true;
        OnListItemSelection(Patient_SelectionChanged, true);
        CollectionViewField.ItemsSource = _userData.Users;
    }

    private string GetIcon(bool isExpanded)
    {
        if (isExpanded)
        {
            return AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? ImageConstants.I_EXPAND_ICON_PNG : ImageConstants.I_COLLAPSE_ICON_PNG;
        }
        else
        {
            return AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? ImageConstants.I_COLLAPSE_ICON_PNG : ImageConstants.I_EXPAND_ICON_PNG;
        }
    }

    private ColumnDefinitionCollection CreateOverlayViewColumn()
    {
        return new ColumnDefinitionCollection
        {
            new ColumnDefinition{ Width = _screenWidth * 0.3 -1- _padding},
            new ColumnDefinition{ Width = GridLength.Auto }
        };
    }

    private async void Patient_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isShowButtonClicked && !_isCollectionRefreshed && ((_showInternetErrorMsg 
            && await ParentPage.CheckAndDisplayInternetErrorAsync(false, default).ConfigureAwait(true)) 
            || MobileConstants.CheckInternet))
        {
            _showInternetErrorMsg = true;
            var item = sender as CollectionView;
            if (item.SelectedItem != null)
            {
                App._essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_USER_ID_KEY, (item.SelectedItem as UserModel).UserID);
                await OnPatientClickAsync((item.SelectedItem as UserModel).UserID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            }
        }
        else
        {
            _showInternetErrorMsg = true;
            CollectionViewField.SelectedItem = _userData.Users.Find(x => x.UserID == _patientID);
        }
    }

    internal async Task OnPatientClickAsync(string patientID)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            if (ShellMasterPage.CurrentShell.CurrentPage is PatientsPage)
            {

                _patientID = Convert.ToInt64(patientID, CultureInfo.InvariantCulture);
                await UpdatePatientDetailsViewAsync().ConfigureAwait(true);
            }
            else
            {
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientsPage), GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), "0", patientID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
            }
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientsPage), GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), "0", patientID.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(true);
        }
    }

    private async Task SetDetailContentAsync(long patientID)
    {
        if (MobileConstants.IsTablet && !IsDashboardView(_userData.RecordCount))
        {
            if (patientID > 0)
            {
                _patientID = patientID;
                //// Added delay to wait until set header menu
                await Task.Delay(10).ConfigureAwait(true);
                await UpdatePatientDetailsViewAsync().ConfigureAwait(true);
            }
            else
            {
                await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, string.Empty, MenuAction.MenuActionProfileKey, 0, null, string.Empty, string.Empty, ShellMasterPage.CurrentShell?.CurrentPage?.IsAddEditPage ?? false)).ConfigureAwait(true);
                await Task.Delay(10).ConfigureAwait(true);
                _emptyDetailView.ControlResourceKey = ResourceConstants.R_EMPTY_PATIENT_VIEW_KEY;
                _emptyDetailView.Margin = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(_detailViewSpacing, 0, 0, 0) : new Thickness(0, 0, _detailViewSpacing, 0);
                if (_patinetView != null)
                {
                    _mainLayout.Children.Remove(_patinetView);
                    _patinetView = null;
                }
                _emptyDetailView.IsVisible = true;
                //todo:_mainLayout.RaiseChild(_overlayGrid);
            }
        }
    }

    private async Task UpdatePatientDetailsViewAsync()
    {
        if (_isExpanded)
        {
            ShowHidePatientList();
            int index = _userData.Users.IndexOf(_userData.Users.FirstOrDefault(x => x.UserID == _patientID));
            CollectionViewField.ScrollTo(index, animate: false);
        }
        //   await ParentPage.SetRightHeaderItemsAsync(nameof(PatientsPage)).ConfigureAwait(true);
        await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, string.Empty, MenuAction.MenuActionProfileKey, 0, null, string.Empty, string.Empty, ShellMasterPage.CurrentShell?.CurrentPage?.IsAddEditPage ?? false)).ConfigureAwait(true);

        await Task.Delay(10).ConfigureAwait(true);
        CollectionViewField.SelectedItem = _userData.Users.FirstOrDefault(x => x.UserID == _patientID);
        _emptyDetailView.IsVisible = false;
        bool isRefreshRequest = true;
        InitializePatientView();
        if (!_mainLayout.Children.Contains(_patinetView))
        {
            isRefreshRequest = false;
            _mainLayout.Add(_patinetView, 0, 0);
            //todo:_mainLayout.RaiseChild(_overlayGrid);
        }
        AppHelper.ShowBusyIndicator = true;
        _patinetView.Parameters = ParentPage.AddParameters(ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _patientID.ToString()));
        await _patinetView.LoadUIAsync(isRefreshRequest).ConfigureAwait(true);
        _isCollectionRefreshed = false;
    }

    private void InitializePatientView()
    {
        if (_patinetView == null)
        {
            _patinetView = new PatientHeaderView(ParentPage, null) { Margin = AppStyles.DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(_detailViewSpacing, 0, 0, 0) : new Thickness(0, 0, _detailViewSpacing, 0) };
            _patinetView.OnPatientListRefreshCallBack -= PatientListRefreshAsync;
            _patinetView.OnPatientListRefreshCallBack += PatientListRefreshAsync;
        }
    }

    private async void PatientListRefreshAsync(object sender, EventArgs e)
    {
        _showInternetErrorMsg = false;
        await LoadUIAsync(true).ConfigureAwait(true);
    }

    /// <summary>
    /// Loads Target view as content
    /// </summary>
    /// <param name="targetView">Name of Target View</param>
    public async Task LoadTragetViewAsync(string targetView)
    {
        if (_patinetView != null && _mainLayout.Children.Contains(_patinetView))
        {
            await _patinetView.LoadTragetViewAsync(targetView).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Loads Target view as content
    /// </summary>
    /// <param name="targetView">Name of Target View</param>
    public async Task AddSinglePatientClicked()
    {
        if (MobileConstants.IsTablet)
        {
            var parameter = ParentPage.AddParameters(
                ParentPage.CreateParameter(Constants.IS_PATIENTS_ADD_PAGE, "true")
            );
            var patientAddEditPage = new AddSinglePatientPopupPage(ParentPage, parameter);
            patientAddEditPage._profileView.OnListRefresh += ProfileView_OnListRefresh;
            //todo:await Navigation.PushPopupAsync(patientAddEditPage).ConfigureAwait(true);

        }
    }

    private async void ProfileView_OnListRefresh(object sender, EventArgs e)
    {
        await LoadUIAsync(true).ConfigureAwait(true);
    }

    /// <summary>
    /// Loads Target view as content
    /// </summary>
    /// <param name="targetView">Name of Target View</param>
    public async Task AddBulkPatientClicked()
    {
        if (MobileConstants.IsTablet)
        {
            var parameter = ParentPage.AddParameters(
               ParentPage.CreateParameter(Constants.IMAGE_WIDTH_CONSTANT, AppImageSize.ImageSizeL.ToString()),
               ParentPage.CreateParameter(Constants.IMAGE_HEIGHT_CONSTANT, AppImageSize.ImageSizeL.ToString()),
               ParentPage.CreateParameter(Constants.DEFAULT_VALUE_CONSTANT, ImageConstants.I_UPLOAD_ICON_PNG),
               ParentPage.CreateParameter(Constants.BASE64_STRING_CONSTANT, string.Empty),
               ParentPage.CreateParameter(Constants.IS_CIRCLE_CONSTANT, "false")
             );
            var patientAddEditPage = new PatientsBulkUploadPopupPage(ParentPage, parameter);
            patientAddEditPage._patientBulkUploadView.OnListRefresh += ProfileView_OnListRefresh;
            //todo:await Navigation.PushPopupAsync(patientAddEditPage).ConfigureAwait(true);
        }

    }
}