using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class MultipleUserView : ViewManager
{
    private readonly UserDTO _userData = new UserDTO() { User = new UserModel() };
    private readonly AmhListViewControl<UserModel> _usersListView;


    /// <summary>
    /// Parameterized constructor of multiple users  containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public MultipleUserView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new UserService(App._essentials);
        if (Parameters?.Count > 0)
        {
            _userData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _usersListView = new AmhListViewControl<UserModel>(FieldTypes.OneRowListViewControl)
        {
            ResourceKey = AppPermissions.MultipleUsersView.ToString(),
            SourceFields = new AmhViewCellModel
            {
                ID = nameof(UserModel.UserID),
                LeftImage = nameof(UserModel.ImageName),
                LeftFieldType = FieldTypes.SquareImageControl,
                LeftHeader = nameof(UserModel.FirstName),
                RightIcon = nameof(UserModel.LastName),
                RightFieldType = FieldTypes.CircleImageControl
            },
            ShowSearchBar = false
        };
        ParentPage.PageLayout.Add(_usersListView, 0, 0);
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
        await (ParentPage.PageService as UserService).GetLinkedUsersAsync(_userData).ConfigureAwait(true);
        _usersListView.PageResources = ParentPage.PageData = _userData;
        ParentPage.ApplyPageResources();
        _usersListView.ErrorCode = _userData.ErrCode;
        _usersListView.DataSource = _userData.Users ?? new List<UserModel>();
        _usersListView.OnValueChanged += User_SelectionChanged;
        if (_userData.ErrCode == ErrorCode.OK && _userData.IsActive)
        {
            ParentPage.Heading.Value = LibPermissions.GetFeatureText(_userData.FeaturePermissions, AppPermissions.MultipleUsersView.ToString());
        }
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _usersListView.OnValueChanged -= User_SelectionChanged;
        await Task.CompletedTask;
    }

    private async void User_SelectionChanged(object sender, EventArgs e)
    {
        if (_usersListView.Value != null)
        {
            var selectedUserID = (_usersListView.Value as UserModel).UserID;
            if (App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0) != selectedUserID)
            {
                AppHelper.ShowBusyIndicator = true;
                App._essentials.SetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, selectedUserID);
                await ((App)Application.Current).SetupSignalRAsync(true).ConfigureAwait(true);
                await ParentPage.NavigateOnNextPageAsync(false, _userData.IsActive, LoginFlow.MultipleUsersPage).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
        }
    }

    private void MapParameters()
    {
        // Note : IsActive is used to store IsBeforeLogin flag data
        _userData.IsActive = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(BaseDTO.IsActive)));
        _userData.AddedBy = GenericMethods.MapValueType<string>(GetParameterValue("TargetPage"));
        _userData.LastModifiedBy = GenericMethods.MapValueType<string>(GetParameterValue("TargetPageParams"));
        _userData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
    }
}

//using AlphaMDHealth.ClientBusinessLayer;
//using AlphaMDHealth.Model;
//using AlphaMDHealth.Utility;
//using System.Globalization;

//namespace AlphaMDHealth.MobileClient;

//public class MultipleUserView : ViewManager
//{
//    private readonly CustomLabelControl _headerLabel;
//    private readonly UserDTO _userData = new UserDTO() { User = new UserModel() };
//    private readonly bool _isPatientPage;
//    private readonly Grid _mainLayout;
//    private readonly bool _isDashboard;
//    private CustomCellModel _customCellData;

//    /// <summary>
//    /// Multiple users view
//    /// </summary>
//    /// <param name="parentPage">Instance of parent page</param>
//    public MultipleUserView(BasePage page, object parameters) : base(page, parameters)
//    {
//        ParentPage.PageService = new UserService(App._essentials);
//        if (Parameters?.Count > 0)
//        {
//            _userData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
//        }
//        _isDashboard = IsDashboardView(_userData.RecordCount);
//        _isPatientPage = _userData.User.IsLinkedUser = IsPatientPage();
//        CreateCustomCells();
//        IsTabletListHeaderDisplay = _isPatientPage && _userData.RecordCount < 1;
//        _headerLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldCenter);
//        if (_isPatientPage)
//        {
//            _mainLayout = new Grid
//            {
//                Style = _isPatientPage && !_isDashboard ? (Style)App.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE] : (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
//                ColumnSpacing = !_isDashboard && DeviceInfo.Idiom == DeviceIdiom.Tablet
//                    ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)
//                    : Constants.ZERO_PADDING,
//                RowDefinitions =
//                {
//                    new RowDefinition { Height = GridLength.Auto }
//                },
//                ColumnDefinitions = CreateTabletViewColumn(true),
//            };
//        }
//        AssignCollectionView();
//        if (_isPatientPage)
//        {
//            SetPageContent(_mainLayout);
//        }
//    }
//    private void AssignCollectionView()
//    {
//        if (_isDashboard)
//        {
//            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
//            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
//            AddCollectionView(_mainLayout, _customCellData, 0, 0);
//        }
//        else if (_isPatientPage)
//        {
//            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
//            AddCollectionViewWithTabletHeader(_mainLayout, _customCellData);
//            SearchField.IsVisible = false;
//        }
//        else
//        {
//            ParentPage.PageLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
//            ParentPage.PageLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
//            AddCollectionView(ParentPage.PageLayout, _customCellData, 0, 1);
//        }
//    }

//    private void CreateCustomCells()
//    {
//        if (_isPatientPage)
//        {
//            _customCellData = new CustomCellModel
//            {
//                CellHeader = nameof(UserModel.FirstName),
//                RemoveColumnSpacing = false,
//                //todo:CellLeftSourceIcon = nameof(UserModel.ImageSource),
//                CellLeftDefaultIcon = nameof(UserModel.ImageName),
//                CellDescription = nameof(UserModel.LastName),
//            };
//        }
//        else
//        {
//            _customCellData = new CustomCellModel
//            {
//                CellHeader = nameof(UserModel.FirstName),
//                RemoveColumnSpacing = false,
//                //todo:CellLeftSourceIcon = nameof(UserModel.ImageSource),
//                CellLeftDefaultIcon = nameof(UserModel.ImageName),
//                CellRightIcon = nameof(UserModel.LastName),
//            };
//        }
//    }
//    public override async Task LoadUIAsync(bool isRefreshRequest)
//    {
//        if (!isRefreshRequest)
//        {
//            MapParameters();
//        }
//        await (ParentPage.PageService as UserService).GetLinkedUsersAsync(_userData).ConfigureAwait(true);
//        ParentPage.PageData = ParentPage.PageService.PageData;
//        _emptyListView.PageResources = ParentPage.PageData;
//        if (_userData.ErrCode == ErrorCode.OK)
//        {
//            if (_userData.IsActive)
//            {
//                var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
//                if (roleID != (int)RoleName.CareTaker)
//                {
//                    var minAddedON = _userData.Users?.Min(x => x.AddedON);
//                    if (minAddedON != null)
//                    {
//                        var userID = _userData.Users?.FirstOrDefault(p => p.AddedON == minAddedON)?.UserID;
//                        App._essentials.SetPreferenceValue(StorageConstants.PR_IS_MAIN_LOGGEDIN_USER_KEY, userID);
//                    }
//                }
//                ParentPage.PageLayout.RowSpacing = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT], CultureInfo.InvariantCulture);
//                ParentPage.PageLayout.Add(_headerLabel, 0, 0);
//                _headerLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_MULTIPLE_USER_KEY);
//            }
//            if (_userData.User.IsLinkedUser)
//            {
//                SetTableHeader(GenericMethods.IsListNotEmpty(_userData.Users) ? _userData.Users.Count : 0);
//                bool isPatientOverview = IsPatientOverview(_userData.RecordCount);
//                if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.LinkedUserAddEdit.ToString()) && !isPatientOverview)
//                {
//                    SearchField.IsVisible = true;
//                    SearchField.PageResources = ParentPage.PageData;
//                    SearchField.OnSearchTextChanged += OnUserSearch;
//                    if (IsTabletListHeaderDisplay)
//                    {
//                        TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
//                    }
//                    if (!IsPatientOverview(_userData.RecordCount) && !isRefreshRequest)
//                    {
//                        OnActionButtonClicked += OnAddButtonClicked;
//                    }
//                }
//            }
//            if (GenericMethods.IsListNotEmpty(_userData.Users))
//            {
//                CollectionViewField.ItemsSource = _userData.Users;
//                CollectionViewField.HeightRequest = _userData.Users.Count * CellRowHeight;
//                OnListItemSelection(User_SelectionChanged, true);
//            }
//            else
//            {
//                RenderErrorView(ParentPage.PageLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, _isDashboard
//                    , (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
//            }
//        }
//        else
//        {
//            RenderErrorView(ParentPage.PageLayout, _userData.ErrCode.ToString(), _isDashboard
//                , (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
//        }
//    }

//    private void MapParameters()
//    {
//        // Note : IsActive is used to store IsBeforeLogin flag data
//        _userData.IsActive = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(BaseDTO.IsActive)));
//        _userData.AddedBy = GenericMethods.MapValueType<string>(GetParameterValue("TargetPage"));
//        _userData.LastModifiedBy = GenericMethods.MapValueType<string>(GetParameterValue("TargetPageParams"));
//        _userData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
//    }

//    private void OnUserSearch(object sender, EventArgs e)
//    {
//        if (GenericMethods.IsListNotEmpty(_userData.Users))
//        {
//            CollectionViewField.Footer = null;
//            var serchBar = sender as CustomSearchBar;
//            if (string.IsNullOrWhiteSpace(serchBar.Text))
//            {
//                CollectionViewField.ItemsSource = new List<UserModel>();
//                CollectionViewField.ItemsSource = _userData.Users;
//                SetTableHeader(_userData.Users.Count);
//                SetMainGridSize();
//            }
//            else
//            {
//                var searchedUsers = _userData.Users.FindAll(y =>
//                {
//                    return (!string.IsNullOrWhiteSpace(y.FirstName) && y.FirstName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())) ||
//                                                (!string.IsNullOrWhiteSpace(y.LastName) && y.LastName.ToLowerInvariant().Contains(serchBar.Text.ToLowerInvariant().Trim()));
//                });
//                if (searchedUsers.Count > 0)
//                {
//                    CollectionViewField.ItemsSource = searchedUsers;
//                    _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * searchedUsers.Count + 60;
//                }
//                else
//                {
//                    CollectionViewField.ItemsSource = new List<UserModel>();
//                    RenderErrorView(ParentPage.PageLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
//                    _mainLayout.HeightRequest = 500;
//                }
//                SetTableHeader(searchedUsers.Count);
//            }
//        }
//    }

//    internal void UnLoadUIData()
//    {
//        if (SearchField != null && SearchField.Value != null)
//        {
//            SearchField.Value = string.Empty;
//            SearchField.OnSearchTextChanged -= OnUserSearch;
//        }
//        OnActionButtonClicked -= OnAddButtonClicked;
//        OnListItemSelection(User_SelectionChanged, false);
//    }

//    private async void User_SelectionChanged(object sender, SelectionChangedEventArgs e)
//    {
//        long selectedUserID = (e.CurrentSelection[0] as UserModel).UserID;
//        if (_isPatientPage)
//        {
//            selectedUserID = selectedUserID == 0 ? (e.CurrentSelection[0] as UserModel).UserTempID : selectedUserID;
//            await NavigateToLinkedUserPageAsync(selectedUserID).ConfigureAwait(true);
//        }
//        else
//        {
//            if (App._essentials.GetPreferenceValue<long>(StorageConstants.PR_LOGIN_USER_ID_KEY, 0) != selectedUserID)
//            {
//                AppHelper.ShowBusyIndicator = true;
//                App._essentials.SetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, selectedUserID);
//                await ((App)Application.Current).SetupSignalRAsync(true).ConfigureAwait(true);
//                await ParentPage.NavigateOnNextPageAsync(false, _userData.IsActive, LoginFlow.MultipleUsersPage).ConfigureAwait(true);
//                AppHelper.ShowBusyIndicator = false;
//            }
//        }
//    }

//    private async void OnAddButtonClicked(object sender, EventArgs e)
//    {
//        OnActionButtonClicked -= OnAddButtonClicked;
//        await NavigateToLinkedUserPageAsync(0).ConfigureAwait(true);
//        OnActionButtonClicked += OnAddButtonClicked;
//    }

//    private async Task NavigateToLinkedUserPageAsync(long userID)
//    {
//        var parameter = ParentPage.AddParameters(
//            ParentPage.CreateParameter(nameof(UserModel.IsLinkedUser), "true"),
//            ParentPage.CreateParameter(nameof(UserModel.UserID), userID.ToString())
//        );
//        var linkedUserPage = new LinkedUserPopUpPage(new BasePage(), parameter)
//        {
//            ShowDeleteButton = !(userID == 0)
//        };
//        linkedUserPage.OnSaveButtonClicked += RefreshLinkedUserList;
//        //todo:await Navigation.PushPopupAsync(linkedUserPage).ConfigureAwait(true);
//    }

//    private async void RefreshLinkedUserList(object sender, EventArgs e)
//    {
//        var data = sender as UserDTO;
//        if (data.ErrCode == ErrorCode.OK)
//        {
//            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(data.ErrCode.ToString()), true);
//            AppHelper.ShowBusyIndicator = true;
//            await LoadUIAsync(true).ConfigureAwait(true);
//            if (data.User.UserTempID < 0 || !_userData.User.IsActive)
//            {
//                InvokePatientListRefresh(sender, e);
//            }
//            AppHelper.ShowBusyIndicator = false;
//        }
//    }

//    private void SetTableHeader(int count)
//    {
//        if (TabletHeader != null)
//        {
//            TabletHeader.Text = string.Concat(ParentPage.GetFeatureValueByCode(Utility.AppPermissions.LinkedUsersView.ToString()), " ", $"({count})");
//        }
//    }

//    internal void SetMainGridSize()
//    {
//        if (GenericMethods.IsListNotEmpty(_userData.Users))
//        {
//            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _userData.Users.Count + 60;
//        }
//        else
//        {
//            _mainLayout.HeightRequest = 500;
//        }
//    }
//} 