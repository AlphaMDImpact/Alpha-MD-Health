using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class CaregiversView : BaseLibCollectionView
{
    private readonly CaregiverDTO _caregiverData = new CaregiverDTO { Caregivers = new List<CaregiverModel>(), Caregiver = new CaregiverModel() };
    private readonly bool _isDashboard;
    private readonly Grid _mainLayout;
    private RowDefinition _listRowDefination;
    private CaregiverView _detailView;
    private long _selectPatientCaregiverID;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public CaregiversView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new UserService(App._essentials);
        MapParameters();
        _isDashboard = IsDashboardView(_caregiverData.RecordCount);
        bool isPatientView = IsPatientPage();
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellID = nameof(CaregiverModel.PatientCareGiverID),
            CellHeader = nameof(CaregiverModel.FirstName),
            CellDescription = isPatientData ? nameof(CaregiverModel.FromDateValue): nameof(CaregiverModel.Department),
            CellRightContentDescription = isPatientData ? string.Empty : nameof(CaregiverModel.FromDateValue),
            BandColor = nameof(CaregiverModel.ProgramColor),
            CellDescriptionColor = nameof(CaregiverModel.DateStyle),
            CellRightContentDescriptionInPancakeView = false,
            CellLeftSourceIcon = nameof(CaregiverModel.ImageName),
            CellLeftDefaultIcon = nameof(CaregiverModel.Initials),
            ArrangeHorizontal = false
        };
        IsTabletListHeaderDisplay = isPatientView && !_isDashboard;
        _listRowDefination = new RowDefinition
        {
            Height = _isDashboard ? new GridLength(200, GridUnitType.Absolute) : new GridLength(1, GridUnitType.Star)
        };
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[!_isDashboard && isPatientView 
                ? StyleConstants.ST_END_TO_END_GRID_STYLE 
                : StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = !_isDashboard && DeviceInfo.Idiom == DeviceIdiom.Tablet 
                ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture) 
                : Constants.ZERO_PADDING,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = CreateTabletViewColumn(_isDashboard || DeviceInfo.Idiom == DeviceIdiom.Phone),
        };
        if (_isDashboard)
        {
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddCollectionView(_mainLayout, customCellModel, 0, 0);
        }
        else if (isPatientView)
        {
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddCollectionViewWithTabletHeader(_mainLayout, customCellModel);
            SearchField.IsVisible = false;
        }
        else
        {
            _mainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddSearchView(_mainLayout, false);
            AddCollectionView(_mainLayout, customCellModel, 0, 1);
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                AddSeparatorView(_mainLayout, 1, 0);
                Grid.SetRowSpan(Separator, 2);
            }
        }
        SetPageContent(_mainLayout);
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
        await (ParentPage.PageService as UserService).GetPatientCareGiversAsync(_caregiverData).ConfigureAwait(true);
        _emptyListView.PageResources = ParentPage.PageData = ParentPage.PageService.PageData;
        if (IsTabletListHeaderDisplay)
        {
            SearchField.PageResources = ParentPage.PageData;
            SearchField.Value = string.Empty;
        }
        if (_caregiverData.ErrCode == ErrorCode.OK)
        {
            if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.CaregiverAddEdit.ToString()))
            {
                if (IsTabletListHeaderDisplay)
                {
                    TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                }
                OnListItemSelection(CaregiverSelectionChanged, !IsPatientOverview(_caregiverData.RecordCount));
                if (!isRefreshRequest && !IsPatientOverview(_caregiverData.RecordCount))
                {
                    OnActionButtonClicked += OnAddButtonClicked;
                }
            }
            else
            {
                if (!_isDashboard)
                {
                    HideAddButton(_mainLayout, true);
                }
            }
            LoadUIData();
        }
        else
        {
            RenderErrorView(_mainLayout, ErrorCode.NoInternetConnection.ToString(), _isDashboard, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }

    private void LoadUIData()
    {
        if (IsTabletListHeaderDisplay)
        {
            TabletHeader.Text = GetHeaderText(_caregiverData.Caregivers.Count);
        }
        if (!_isDashboard && !IsPatientOverview(_caregiverData.RecordCount))
        {
            SearchField.IsVisible = true;
            SearchField.PageResources = ParentPage.PageData;
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
        }
        if (GenericMethods.IsListNotEmpty(_caregiverData.Caregivers))
        {
            var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            if (isPatientData)
            {
                _caregiverData.Caregivers.ForEach(x => x.Department = string.Empty);
            }
            CollectionViewField.ItemsSource = _caregiverData.Caregivers;
            if (IsTabletListHeaderDisplay || _caregiverData.RecordCount > 0)
            {
               _mainLayout.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _caregiverData.Caregivers.Count + new OnIdiom<int> { Phone = 10, Tablet = 0 };
            }
            if (!IsPatientPage() && DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
               PatientTabletUIDataAsync();
            }
        }
        else
        {
            CollectionViewField.ItemsSource = new List<CaregiverModel>();
            RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, _isDashboard, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }

    private void PatientTabletUIDataAsync()
    {
        if (_selectPatientCaregiverID > 0)
        {
            var selectedItem = _caregiverData.Caregivers.FirstOrDefault(x => x.PatientCareGiverID == _selectPatientCaregiverID);
            CollectionViewField.SelectedItem = selectedItem;
        }
    }

    private void MapParameters()
    {
        if (Parameters?.Count > 0)
        {
            _caregiverData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _selectPatientCaregiverID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(CaregiverModel.PatientCareGiverID)));
        }
    }

    public override async Task UnloadUIAsync()
    {
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        OnActionButtonClicked -= OnAddButtonClicked;
        OnListItemSelection(CaregiverSelectionChanged, false);
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    private string GetHeaderText(int count)
    {
        return $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.CaregiversView.ToString())} ({count})";
    }

    private async void CaregiverSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            CollectionViewField.SelectionChanged -= CaregiverSelectionChanged;
            var item = sender as CollectionView;
            if (item.SelectedItem != null)
            {
                if (IsTabletListHeaderDisplay)
                {
                    var caregiverPage = new CaregiverPopUpPage((item.SelectedItem as CaregiverModel).PatientCareGiverID.ToString(CultureInfo.InvariantCulture));
                    caregiverPage.OnSaveButtonClicked += RefreshCaregiversList;
                    //todo:await Navigation.PushPopupAsync(caregiverPage).ConfigureAwait(false);
                }
                else
                {
                    if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                    {
                        if (ShellMasterPage.CurrentShell.CurrentPage is DashboardPage)
                        {
                            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(CaregiversPage)
                                , GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), Constants.CONSTANT_ZERO
                                , (item.SelectedItem as CaregiverModel).PatientCareGiverID.ToString(CultureInfo.InvariantCulture)
                            ).ConfigureAwait(true);
                        }
                        else
                        {
                            if (_detailView != null)
                            {
                                _mainLayout.Children.Remove(_detailView);
                            }
                            _detailView = new CaregiverView(new BasePage(), ParentPage.AddParameters(
                                ParentPage.CreateParameter(nameof(CaregiverModel.PatientCareGiverID), 
                                (item.SelectedItem as CaregiverModel).PatientCareGiverID.ToString(CultureInfo.InvariantCulture))));
                            _mainLayout.Add(_detailView, 2, 0);
                            Grid.SetRowSpan(_detailView, 2);
                            await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                            //await ParentPage.SetRightHeaderItemsAsync(nameof(CaregiverPage)).ConfigureAwait(true);
                        } 
                        AppHelper.ShowBusyIndicator = false;
                    }
                    else {
                        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.CaregiverPage.ToString()
                            , GenericMethods.GenerateParamsWithPlaceholder(Param.id)
                            , (item.SelectedItem as CaregiverModel).PatientCareGiverID.ToString(CultureInfo.InvariantCulture)
                        ).ConfigureAwait(true);
                    }
                }
            }
            CollectionViewField.SelectionChanged += CaregiverSelectionChanged;
        }
    }

    private async void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = new List<CaregiverModel>();
            await Task.Delay(10);
            CollectionViewField.ItemsSource = _caregiverData.Caregivers;
            if (IsTabletListHeaderDisplay)
            {
                TabletHeader.Text = GetHeaderText(_caregiverData.Caregivers.Count);
            }
        }
        else
        {
            var searchedUsers = _caregiverData.Caregivers.FindAll(y =>
            {
                return !string.IsNullOrWhiteSpace(y.FirstName) && y.FirstName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant())
                    || (!string.IsNullOrWhiteSpace(y.Department)
                    && y.Department.ToLowerInvariant().Contains(serchBar.Text.ToLowerInvariant().Trim())
                    || y.FromDateValue.ToLowerInvariant().Contains(serchBar.Text.ToLowerInvariant().Trim()));
            });
            if (GenericMethods.IsListNotEmpty(searchedUsers))
            {
                if (IsTabletListHeaderDisplay)
                {
                    TabletHeader.Text = GetHeaderText(searchedUsers.Count);
                }
                CollectionViewField.ItemsSource = searchedUsers;
            }
            else
            {
                CollectionViewField.ItemsSource = new List<CaregiverModel>();
                if (IsTabletListHeaderDisplay)
                {
                    TabletHeader.Text = GetHeaderText(searchedUsers.Count);
                }
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }  
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY).ConfigureAwait(true))
        {
            var caregiverPage = new CaregiverPopUpPage(Constants.CONSTANT_ZERO);
            caregiverPage.OnSaveButtonClicked += RefreshCaregiversList;
            //todo:await Navigation.PushPopupAsync(caregiverPage).ConfigureAwait(false);
        }
    }

    private async void RefreshCaregiversList(object sender, EventArgs e)
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
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_caregiverData.Caregivers))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _caregiverData.Caregivers.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }
}