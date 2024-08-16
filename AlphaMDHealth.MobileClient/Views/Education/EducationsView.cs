using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class EducationsView : BaseLibCollectionView
{
    private readonly ContentPageDTO _educationData = new ContentPageDTO { Page = new ContentPageModel(), EducationGroup = new List<EducationCategoryGroupModel>(), Pages = new List<ContentPageModel>() };
    private readonly Grid _mainLayout;
    private readonly CustomMessageControl _messageView;
    private StaticMessageView _educationDetailView;
    private readonly BasePage _parentPage;
    private readonly CustomCellModel _educationListCell;
    private readonly RowDefinition _listRowDefination;
    private bool _isPatientPage;
    private bool _isFirstTimeClick;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Feature parameters to render view</param>
    public EducationsView(BasePage page, object parameters) : base(page, parameters)
    {
        _parentPage = page;
        MapParameters();
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        _educationListCell = isPatientData
            ? new CustomCellModel
            {
                CellHeader = nameof(ContentPageModel.Title),
                CellDescription = nameof(ContentPageModel.CategoryName),
                //todo:CellLeftSourceIcon = nameof(ContentPageModel.LeftSourceIcon),
                CellLeftDefaultIcon = nameof(ContentPageModel.LeftDefaultIcon),
                CellRightContentHeader = nameof(ContentPageModel.ToDateString),
                BandColor = nameof(ContentPageModel.ProgramColor)
            }
            :
            new CustomCellModel
            {
                CellHeader = nameof(ContentPageModel.Title),
                CellDescription = nameof(ContentPageModel.CategoryName),
                //todo:CellLeftSourceIcon = nameof(ContentPageModel.LeftSourceIcon),
                CellLeftDefaultIcon = nameof(ContentPageModel.LeftDefaultIcon),
                CellRightContentHeader = nameof(ContentPageModel.FromDateString),
                BandColor = nameof(ContentPageModel.ProgramColor)
            };
        if (!isPatientData)
        {
            _educationListCell.CellRightContentDescription = nameof(ContentPageModel.Status);
            _educationListCell.CellDescriptionColor = nameof(ContentPageModel.StatusColor);
        }
        IsTabletListHeaderDisplay = IsPatientPage() && !IsDashboardView(_educationData.RecordCount) && _isPatientPage;
        _listRowDefination = new RowDefinition
        {
            Height = IsDashboardView(_educationData.RecordCount)
                ? new GridLength(200, GridUnitType.Absolute)
                : new GridLength(1, GridUnitType.Star)
        };
        _mainLayout = new Grid
        {
            Style = (Style)App.Current.Resources[IsTabletListHeaderDisplay ? StyleConstants.ST_END_TO_END_GRID_STYLE : StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = (!IsDashboardView(_educationData.RecordCount)) && DeviceInfo.Idiom == DeviceIdiom.Tablet ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture) : Constants.ZERO_PADDING,
            ColumnDefinitions = CreateTabletViewColumn(IsDashboardView(_educationData.RecordCount) || DeviceInfo.Idiom == DeviceIdiom.Phone),
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                _listRowDefination
            }
        };
        if (IsTabletListHeaderDisplay)
        {
            AddCollectionViewWithTabletHeader(_mainLayout, _educationListCell);
        }
        else
        {
            if (!IsDashboardView(_educationData.RecordCount))
            {
                AddSearchView(_mainLayout, false);
                SearchField.IsVisible = false;
            }
            _messageView = new CustomMessageControl(false);
        }
        _isFirstTimeClick = true;
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
        if (!IsTabletListHeaderDisplay)
        {
            LoadUI();
        }
        await LoadListAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        App._essentials.SetPreferenceValue(StorageConstants.PR_REMOVE_SCROLL_VIEW_KEY, false);
        OnListItemSelection(Educations_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        //_educationDetailView?.UnloadUIAsync();
        _mainLayout.Children.Remove(CollectionViewField);
        // if (_educationDetailView != null && _mainLayout.Children.Contains(_educationDetailView))
        // {
        //     _mainLayout.Children.Remove(_educationDetailView);
        //     await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, string.Empty, false));
        //  }
        OnActionButtonClicked -= OnAddButtonClicked;
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_educationData.Pages))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _educationData.Pages.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }

    private void MapParameters()
    {
        if (Parameters?.Count > 0)
        {
            _educationData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _educationData.Page.EducationCategoryID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ContentPageModel.EducationCategoryID)));
            _educationData.Page.PageID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(ContentPageModel.PageID)));
            _educationData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
            _isPatientPage = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(ContentPageModel.IsPatientPage)));
        }
    }

    private void LoadUI()
    {
        _educationListCell.ArrangeHorizontal = IsDashboardView(_educationData.RecordCount);
        if (CollectionViewField != null)
        {
            _mainLayout.Children.Remove(CollectionViewField);
        }
        AddCollectionView(_mainLayout, _educationListCell, 0, 1);
        if (!_educationListCell.ArrangeHorizontal)
        {
            CollectionViewField.Margin = 0;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                AddSeparatorView(_mainLayout, 1, 0);
                Grid.SetRowSpan(Separator, 2);
            }
        }
    }

    private async Task LoadListAsync()
    {
        ContentPageService contentPageService = new ContentPageService(App._essentials);
        await contentPageService.GetContentPagesAsync(_educationData).ConfigureAwait(true);
        await Device.InvokeOnMainThreadAsync(() =>
        {
            _parentPage.PageData = _emptyListView.PageResources = contentPageService.PageData;
            if (_messageView != null)
            {
                _messageView.PageResources = _parentPage.PageData;
            }
            if (_educationData.ErrCode == ErrorCode.OK)
            {
                ApplyPageData();
                if (GenericMethods.IsListNotEmpty(_educationData.EducationGroup) || GenericMethods.IsListNotEmpty(_educationData.Pages))
                {
                    DisplayDataInList();
                }
                else
                {
                    if (!IsDashboardView(_educationData.RecordCount))
                    {
                        SearchField.IsVisible = true;
                    }
                    _mainLayout.RowDefinitions.Clear();
                    _mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    isHorizontal = false;
                    CollectionViewField.ItemsLayout = ItemLayoutCreate(false);
                    RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, IsDashboardView(_educationData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
                    ShowNorecordView();
                }
            }
            else
            {
                RenderErrorView(_mainLayout, _educationData.ErrCode.ToString(), IsDashboardView(_educationData.RecordCount), 0, false, true);
                SearchField.IsVisible = !IsDashboardView(_educationData.RecordCount) && !IsPatientPage();
                ShowNorecordView();
            }
            if (TabletHeader != null && IsPatientPage())
            {
                ApplyTableHeaderText(GetListItemCount());
            }
        });
    }

    private void DisplayDataInList()
    {
        CollectionViewField.ItemsSource = null;
        CollectionViewField.ItemsSource = new List<EducationCategoryGroupModel>();
        CollectionViewField.ItemSizingStrategy = GenericMethods.GetPlatformSpecificValue(ItemSizingStrategy.MeasureFirstItem, ItemSizingStrategy.MeasureAllItems, default);
        if (IsDashboardView(_educationData.RecordCount) || IsPatientPage())
        {
            if (TabletHeader != null)
            {
                ApplyTableHeaderText(_educationData.Pages.Count);
            }
            CollectionViewField.ItemsSource = _educationData.Pages;
        }
        else
        {
            SearchField.IsVisible = true;
            CollectionViewField.ItemsSource = _educationData.EducationGroup;
            CollectionViewField.IsGrouped = true;
            CollectionViewField.GroupHeaderTemplate = _educationData.Page.EducationCategoryID > 0
                ? new DataTemplate(typeof(EducationCategoryDetailViewCell))
                : new DataTemplate(typeof(EducationCategoryViewCell));
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                if (_educationData.Page.PageID > 0)
                {
                    CollectionViewField.SelectedItem = _educationData.EducationGroup[0].FirstOrDefault(x => x.PageID == _educationData.Page.PageID);
                }
                else
                {
                    ShowNorecordView();
                }
            }
        }
    }

    private void ApplyPageData()
    {
        OnListItemSelection(Educations_SelectionChanged, !IsPatientOverview(_educationData.RecordCount));
        if (SearchField != null)
        {
            SearchField.Value = string.Empty;
            SearchField.PageResources = _parentPage.PageData;
            if (_messageView != null)
            {
                _messageView.ControlResourceKey = string.Empty;
            }
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
        }
        if (IsPatientPage())
        {
            AssignTabletValues();
        }
        else
        {
            if (_educationData.Page.EducationCategoryID > 0)
            {
                MenuView menuView = new MenuView(MenuLocation.Header, _educationData.Pages?.FirstOrDefault()?.CategoryName, false);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await ParentPage.OverrideTitleViewAsync(menuView);
                });
            }
        }
    }

    private void AssignTabletValues()
    {
        if (TabletActionButton != null && _parentPage.CheckFeaturePermissionByCode(AppPermissions.PatientEducationAddEdit.ToString()))
        {
            TabletActionButton.Text = _parentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
            OnActionButtonClicked += OnAddButtonClicked;
        }
    }

    private void ApplyTableHeaderText(int count)
    {
        TabletHeader.Text = $"{_parentPage.GetFeatureValueByCode(AppPermissions.PatientEducationsView.ToString())} ({count})";
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        if (CheckAndDisplayInternetError())
        {

            if (_isFirstTimeClick)
            {
                _isFirstTimeClick = false;
                var patientEducationPage = new AssignEducationPage(Constants.CONSTANT_ZERO);
                patientEducationPage.OnSaveButtonClicked += RefreshEducationList;
                patientEducationPage.OnLeftHeaderClickedEvent += OnAssignEducationPopUpClosed;
                patientEducationPage.OnRightHeaderClickedEvent += OnAssignEducationPopUpClosed;
                patientEducationPage.OnCloseButtonClickedEvent += OnAssignEducationPopUpClosed;
                //todo:await Navigation.PushPopupAsync(patientEducationPage).ConfigureAwait(false);
            }
        }
    }

    private void OnAssignEducationPopUpClosed(object sender, EventArgs e)
    {
        _isFirstTimeClick = true;
    }

    private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            if (IsPatientPage())
            {
                CollectionViewField.ItemsSource = _educationData.Pages;
            }
            else
            {
                CollectionViewField.ItemsSource = _educationData.EducationGroup;
                if (_educationData.EducationGroup.Count == 0)
                {
                    CollectionViewField.EmptyView = string.Empty;
                    RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, IsDashboardView(_educationData.RecordCount), (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
                }
            }
        }
        else
        {
            if (IsPatientPage())
            {
                CollectionViewField.ItemsSource = GetSearchEducations(serchBar.Text.Trim());
            }
            else
            {
                CollectionViewField.ItemsSource = GetSearchCategories(serchBar.Text.Trim());
            }
            if (GetListItemCount() == 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
        if (TabletHeader != null)
        {
            ApplyTableHeaderText(GetListItemCount());
        }
    }

    private int GetListItemCount()
    {
        return CollectionViewField.ItemsSource?.Cast<object>().Count() ?? 0;
    }

    private List<ContentPageModel> GetSearchEducations(string searchText)
    {
        return _educationData.Pages.FindAll(y =>
          (!string.IsNullOrWhiteSpace(y.CategoryName) && y.CategoryName.ToLowerInvariant().Contains(searchText.ToLowerInvariant().Trim())) ||
          (!string.IsNullOrWhiteSpace(y.Title) && y.Title.ToLowerInvariant().Contains(searchText.ToLowerInvariant().Trim())));
    }

    private List<EducationCategoryGroupModel> GetSearchCategories(string searchText)
    {
        EducationCategoryGroupModel filteredGroupData;
        return _educationData.EducationGroup.Select(x =>
       {
           filteredGroupData = new EducationCategoryGroupModel { Name = x.Name, SubHeader = x.SubHeader, CategoryDetails = x.CategoryDetails,
               //todo:CategoryImageSource = x.CategoryImageSource 
           };
           filteredGroupData.AddRange(x.FindAll(y =>
                (!string.IsNullOrWhiteSpace(y.CategoryName) && y.CategoryName.ToLowerInvariant().Contains(searchText.ToLowerInvariant().Trim()))
                || (!string.IsNullOrWhiteSpace(y.Title) && y.Title.ToLowerInvariant().Contains(searchText.ToLowerInvariant().Trim()))
            ));
           return filteredGroupData;
       }).Where(z => z.Count() > 0).ToList();
    }

    private async void Educations_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CollectionViewField.SelectionChanged -= Educations_SelectionChanged;
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            var selectedEducation = item.SelectedItem as ContentPageModel;
            var selectedID = selectedEducation.PageID.ToString(CultureInfo.InvariantCulture);
            if (IsPatientPage())
            {
                if (CheckAndDisplayInternetError())
                {
                    var patientEducationPage = new AssignEducationPage(selectedID);
                    patientEducationPage.OnSaveButtonClicked += RefreshEducationList;
                    //todo:await Navigation.PushPopupAsync(patientEducationPage).ConfigureAwait(false);
                }
            }
            else
            {
                var selectedName = selectedEducation.Title.ToString(CultureInfo.InvariantCulture);
                var selectedStatus = selectedEducation.Status.ToString(CultureInfo.InvariantCulture);
                var educationIDs = string.IsNullOrWhiteSpace(selectedEducation.EducationIDs)
                    ? selectedEducation.EducationID.ToString(CultureInfo.InvariantCulture)
                    : selectedEducation.EducationIDs;
                await SetRightPageHeaderAsync(null).ConfigureAwait(true);
                await EducationClickAsync(selectedID, selectedName, selectedStatus, selectedEducation.EducationID.ToString()).ConfigureAwait(true);
            }
        }
        CollectionViewField.SelectionChanged += Educations_SelectionChanged;
    }

    private async Task EducationClickAsync(string PageID, string selectedName, string status, string educationID)
    {
        ////App._essentials.SetPreferenceValue(LibStorageConstants.PR_IS_HEIGHT_RESET_KEY, false);
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            if (_messageView != null)
            {
                _mainLayout.Children.Remove(_messageView);
            }
            RemoveEducationDetailView();
            if (_parentPage.CheckFeaturePermissionByCode(AppPermissions.EducationPreview.ToString()))
            {
                if (IsDashboardView(_educationData.RecordCount))
                {
                    await ShellMasterPage.CurrentShell.BaseContentPageInstance.PushPageByTargetAsync(Pages.EducationCategoriesPage.ToString()
                        , GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id, Param.identifier)
                        , "0", _educationData.Pages.FirstOrDefault(x => x.PageID == Convert.ToInt64(PageID)).EducationCategoryID.ToString(CultureInfo.InvariantCulture), PageID
                    ).ConfigureAwait(true);
                }
                else
                {
                    if (Convert.ToInt64(PageID, CultureInfo.InvariantCulture) > 0)
                    {
                        _educationDetailView = new StaticMessageView(new BasePage(), null)
                        {
                            VerticalOptions = LayoutOptions.StartAndExpand
                        };
                        _educationDetailView.Parameters = ParentPage.AddParameters(
                            ParentPage.CreateParameter(nameof(StaticMessageView.Key), PageID),
                            ParentPage.CreateParameter(nameof(StaticMessageView.MessageType), PageType.ContentPage.ToString())
                        );
                        _educationDetailView.RedirectOnTargetPage += CustomAction_OnIhaveReadClicked;
                        _mainLayout.Add(_educationDetailView, 2, 0);
                        if (status == PatientEducationStatus.Open.ToString())
                        {
                            UpdateTaskStausAsync(PatientEducationStatus.InProgress, Convert.ToInt32(educationID));
                        }
                        Grid.SetRowSpan(_educationDetailView, 2);
                        await _educationDetailView.LoadUIAsync(false).ConfigureAwait(true);
                        _educationDetailView.OnListRefresh += OnRefreshCall;
                    }
                    else
                    {
                        RemoveEducationDetailView();
                        AddMessageView();
                    }
                }
            }
        }
        else
        {
            App._essentials.SetPreferenceValue(StorageConstants.PR_REMOVE_SCROLL_VIEW_KEY, true);
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Constants.STATIC_MESSAGE_PAGE_IDENTIFIER,
                GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.type/*, Param.name, Param.identifier*/)
                , PageID, PageType.ContentPage.ToString()/*, nameof(PatientEducationsPage), await ShellMasterPage.GenerateParamAsync(educationIDs, status, true.ToString())*/
            ).ConfigureAwait(true);
            if (status == PatientEducationStatus.Open.ToString())
            {
                UpdateTaskStausAsync(PatientEducationStatus.InProgress, Convert.ToInt32(educationID));
            }
        }
        await SetRightPageHeaderAsync(selectedName).ConfigureAwait(true);
    }

    private async void CustomAction_OnIhaveReadClicked(object sender, int e)
    {
        var item = sender as CustomMessageControl;
        UpdateTaskStausAsync(PatientEducationStatus.Completed, Convert.ToInt32(item.ControlResourceKey));
    }

    private async Task UpdateTaskStausAsync(PatientEducationStatus Status, int PageID)
    {
        ContentPageDTO educationData = new ContentPageDTO { PatientEducations = new List<PatientEducationModel> { new PatientEducationModel { PatientEducationID = PageID, Status = Status, IsSynced = false } } };
        ContentPageService contentPageService = new ContentPageService(App._essentials);
        contentPageService.SaveEducationStatusAsync(educationData).ConfigureAwait(true);
        if (educationData.ErrCode == ErrorCode.OK)
        {
            await new ContentPageService(App._essentials).SyncEducationStatusToServerAsync(educationData, CancellationToken.None).ConfigureAwait(true);
        }
    }

    private void AddMessageView()
    {
        _mainLayout.Add(_messageView, 2, 0);
        _messageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
        _mainLayout.Add(_messageView, 2, 0);
        Grid.SetRowSpan(_messageView, 2);
    }

    private void ShowNorecordView()
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !IsDashboardView(_educationData.RecordCount) && !IsPatientPage())
        {
            RemoveEducationDetailView();
            AddMessageView();
        }
    }

    private async void RefreshEducationList(object sender, EventArgs e)
    {
        CollectionViewField.SelectedItem = null;
        await LoadUIAsync(true).ConfigureAwait(true);
    }

    private bool CheckAndDisplayInternetError()
    {
        if (!MobileConstants.CheckInternet)
        {
            _parentPage.DisplayOperationStatus(_parentPage.GetResourceValueByKey(ResourceConstants.R_OFFLINE_OPERATION_KEY));
            return false;
        }
        return true;
    }

    private async Task SetRightPageHeaderAsync(string title)
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !IsDashboardView(_educationData.RecordCount))
        {
            //if (title == null)
            //{
                //await ParentPage.SetRightHeaderItemsAsync(nameof(EducationPreviewPage)).ConfigureAwait(true);
            //}
            //else
            //{
                await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, title, true)).ConfigureAwait(true);
            //}
        }
    }

    private void RemoveEducationDetailView()
    {
        if (_educationDetailView != null && _mainLayout.Children.Contains(_educationDetailView))
        {
            _educationDetailView.OnListRefresh -= OnRefreshCall;
            _mainLayout.Children.Remove(_educationDetailView);
        }
    }

    private async void OnRefreshCall(object sender, EventArgs e)
    {
        // when e == null, display operation status for detail page
        // when errorCode is success, display success status and refresh list 
        // when errorCode is not success, display error status and view default detail view of list page
        if (!(e is AlphaMDHealth.Model.CustomEventArgs))
        {
            var message = ParentPage.GetResourceValueByKey((string)sender);
            ParentPage.DisplayOperationStatus(
                string.IsNullOrWhiteSpace(message) ? (string)sender : message
                , (string)sender == ErrorCode.OK.ToString());
        }
        if (e != null && (string)sender == ErrorCode.OK.ToString())
        {
            AppHelper.ShowBusyIndicator = true;
            await LoadUIAsync(true).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
    }
}