using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class PatientBillingsView : BaseLibCollectionView
{
    private readonly BillingItemDTO _billingsData = new BillingItemDTO { PatientBills = new List<PatientBillModel>() };
    private readonly Grid _mainLayout;
    private readonly bool _isDashboard;
    private Guid _selectPatientBillingID = Guid.Empty;
    private PatientBillDetailsView _detailView;
    private ScrollView _content;
    private string _fileName;
    private readonly CustomMessageControl _emptyMessageView;

    public PatientBillingsView(BasePage page, object parameters) : base(page, parameters)
    {
        _emptyMessageView = new CustomMessageControl(false);
        ParentPage.PageService = new PatientBillService(App._essentials);
        if (Parameters?.Count > 0)
        {
            _billingsData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _isDashboard = IsDashboardView(_billingsData.RecordCount);
        bool isPatientView = IsPatientPage();
        CustomCellModel customCellModel = new CustomCellModel
        {
            CellHeader = nameof(PatientBillModel.ProgramName),
            //todo: CellLeftSourceIcon = nameof(PatientBillModel.LeftSourceIcon),
            CellLeftDefaultIcon = nameof(PatientBillModel.LeftDefaultIcon),
            CellDescription = nameof(PatientBillModel.ProgramDiscription),
            CellRightContentHeader = nameof(PatientBillModel.BillDateTimeString),
            BandColor = nameof(PatientBillModel.ProgramColor),
            ArrangeHorizontal = false
        };
        IsTabletListHeaderDisplay = isPatientView && !_isDashboard;
        _mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
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
            AddCollectionViewWithTabletHeader(_mainLayout, customCellModel);
            SearchField.IsVisible = false;
        }
        else
        {
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

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            MapParameters();
        }
        await (ParentPage.PageService as PatientBillService).GetPatientBillingsDataAsync(_billingsData).ConfigureAwait(true);
        _emptyListView.PageResources = ParentPage.PageData = ParentPage.PageService.PageData;
        if (IsTabletListHeaderDisplay)
        {
            SearchField.PageResources = ParentPage.PageData;
            SearchField.Value = string.Empty;
        }
        if (_billingsData.ErrCode == ErrorCode.OK)
        {
            if (ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientBillAddEdit.ToString()))
            {
                if (IsTabletListHeaderDisplay)
                {
                    TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                }
                if (!isRefreshRequest && !IsPatientOverview(_billingsData.RecordCount))
                {
                    OnActionButtonClicked += OnAddButtonClicked;
                }
            }
            OnListItemSelection(PatientBillingSelectionChanged, !IsPatientOverview(_billingsData.RecordCount));
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
            TabletHeader.Text = GetHeaderText(_billingsData.PatientBills.Count);
        }
        if (!_isDashboard && !IsPatientOverview(_billingsData.RecordCount))
        {
            SearchField.IsVisible = true;
            SearchField.PageResources = ParentPage.PageData;
            SearchField.OnSearchTextChanged += CustomSearch_OnSearchTextChanged;
        }
        if (GenericMethods.IsListNotEmpty(_billingsData.PatientBills))
        {
            CollectionViewField.ItemsSource = _billingsData.PatientBills;
            if (IsTabletListHeaderDisplay || _billingsData.RecordCount > 0)
            {
                CollectionViewField.HeightRequest = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _billingsData.PatientBills.Count + new OnIdiom<int> { Phone = 10, Tablet = 0 };
            }
            if (!IsPatientPage() && DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                PatientTabletUIDataAsync();
            }
        }
        else
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                if (!IsPatientPage())
                {
                    _emptyMessageView.PageResources = ParentPage.PageData;
                    _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                    _mainLayout.Add(_emptyMessageView, 2, 0);
                    Grid.SetRowSpan(_emptyMessageView, 2);
                }
            }
            CollectionViewField.ItemsSource = new List<PatientBillModel>();
            RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, _isDashboard, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
    }

    private string GetHeaderText(int count)
    {
        return $"{ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientBillsView.ToString())} ({count})";
    }

    private List<PatientBillModel> GetSearchBill(string searchText)
    {
        return _billingsData.PatientBills.FindAll(y =>
            (!string.IsNullOrWhiteSpace(y.ProgramDiscription) && y.ProgramDiscription.ToLowerInvariant().Contains(searchText.ToLowerInvariant().Trim()))
            || (!string.IsNullOrWhiteSpace(y.ProgramName) && y.ProgramName.ToLowerInvariant().Contains(searchText.ToLowerInvariant().Trim()))
        );
    }

    private void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = _billingsData.PatientBills;
            if (IsTabletListHeaderDisplay)
            {
                TabletHeader.Text = GetHeaderText(_billingsData.PatientBills.Count);
            }
        }
        else
        {
            var searchedUsers = GetSearchBill(serchBar.Text);
            CollectionViewField.ItemsSource = searchedUsers;
            if (IsTabletListHeaderDisplay)
            {
                TabletHeader.Text = GetHeaderText(searchedUsers.Count);
            }
            if (searchedUsers.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
    }

    private async void PatientBillingSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CollectionViewField.SelectionChanged -= PatientBillingSelectionChanged;
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            _fileName = (item.SelectedItem as PatientBillModel).PatientBillID.ToString();
            if (IsTabletListHeaderDisplay)
            {
                var parameter = ParentPage.AddParameters(
                     ParentPage.CreateParameter(nameof(PatientBillModel.PatientBillID), _fileName),
                     ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _billingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture))
                );
                var patientAddEditPage = new PatientBillingAddEditPagePopupPage(ParentPage, parameter);
                patientAddEditPage.OnSaveButtonClicked += RefreshCaregiversList;
                //todo:await Navigation.PushPopupAsync(patientAddEditPage).ConfigureAwait(false);
            }
            else if (DeviceInfo.Idiom == DeviceIdiom.Tablet && !(ShellMasterPage.CurrentShell.CurrentPage is DashboardPage))
            {
                if (_detailView != null)
                {
                    _mainLayout.Children.Remove(_detailView);
                }
                if (_content != null)
                {
                    _mainLayout.Children.Remove(_content);
                }
                _detailView = new PatientBillDetailsView(new BasePage(), ParentPage.AddParameters(
                    ParentPage.CreateParameter(nameof(PatientBillModel.PatientBillID), _fileName),
                    ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _billingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture))
                ));

                _content = new ScrollView { Content = _detailView, Orientation = ScrollOrientation.Vertical };

                _mainLayout.Add(_content, 2, 0);
                Grid.SetRowSpan(_content, 2);
                await _detailView.LoadUIAsync(false).ConfigureAwait(true);
                //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientBillDetailsPage)).ConfigureAwait(true);
                AppHelper.ShowBusyIndicator = false;
            }
            else
            {
                if (IsPatientPage())
                {
                    if (IsPatientPage())
                    {
                        _emptyMessageView.PageResources = ParentPage.PageData;
                        _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                        _mainLayout.Add(_emptyMessageView, 2, 0);
                        Grid.SetRowSpan(_emptyMessageView, 2);
                    };
                }
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientBillDetailsPage.ToString()
                   , GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.identifier)
                   , _fileName, _billingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture)
                ).ConfigureAwait(true);
            }
        }
        CollectionViewField.SelectionChanged += PatientBillingSelectionChanged;
    }

    internal async Task ShareButtonClicked()
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            var parameter = ParentPage.AddParameters(
                ParentPage.CreateParameter(nameof(PatientBillModel.PatientBillID), _fileName),
                ParentPage.CreateParameter(nameof(PatientBillModel.ProgramColor), StyleConstants.TRANSPARENT_COLOR_STRING),
                ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _billingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture))
            );
            var patientAddEditPage = new PatientBillSharePopupPage(ParentPage, parameter);
            //todo:  //todo:await Navigation.PushPopupAsync(patientAddEditPage).ConfigureAwait(false);
        }
        else
        {
            var screenshot = await Screenshot.CaptureAsync();
            var stream = await screenshot.OpenReadAsync();
            byte[] fileData = null;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                fileData = ms.ToArray();
            }

            var file = Path.Combine(FileSystem.CacheDirectory, _billingsData.PatientBillItem.PatientBillID.ToString() + ".jpeg");
            if (File.Exists(file))
            {
                // If file found, delete it    
                File.Delete(file);
            }

            File.WriteAllBytes(file, fileData);
            stream.Close();

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = Title,
                File = new ShareFile(file),
                PresentationSourceBounds = new Rect(0, 0, 1, 1)
            });
        }
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_billingsData.PatientBills))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _billingsData.PatientBills.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }

    private void PatientTabletUIDataAsync()
    {
        if (_selectPatientBillingID != Guid.Empty)
        {
            var selectedItem = _billingsData.PatientBills.FirstOrDefault(x => x.PatientBillID == _selectPatientBillingID);
            CollectionViewField.SelectedItem = selectedItem;
        }
    }

    private void MapParameters()
    {
        if (Parameters?.Count > 0)
        {
            _billingsData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _selectPatientBillingID = GenericMethods.MapValueType<Guid>(GetParameterValue(nameof(PatientBillModel.PatientBillID)));
            _billingsData.SelectedUserID = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        }
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        OnActionButtonClicked -= OnAddButtonClicked;
        var parameter = ParentPage.AddParameters(
            ParentPage.CreateParameter(nameof(PatientBillModel.PatientBillID), Guid.Empty.ToString()),
            ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _billingsData.SelectedUserID.ToString())
        );
        var patientBillingAddEdit = new PatientBillingAddEditPagePopupPage(ParentPage, parameter);
        patientBillingAddEdit.OnSaveButtonClicked += RefreshCaregiversList;
        //todo:await Navigation.PushPopupAsync(patientBillingAddEdit).ConfigureAwait(false);
        OnActionButtonClicked += OnAddButtonClicked;
    }

    private async void RefreshCaregiversList(object sender, EventArgs e)
    {
        CollectionViewField.SelectedItem = null;
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

    public override async Task UnloadUIAsync()
    {
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        OnActionButtonClicked -= OnAddButtonClicked;
        OnListItemSelection(PatientBillingSelectionChanged, false);
        await base.UnloadUIAsync().ConfigureAwait(true);
    }
}