using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient Medications View
/// </summary>
public class PatientMedicationsView : BaseLibCollectionView, IDisposable
{
    private Grid _mainLayout;
    private CustomTabsControl _customTabs;
    private string _selectedTabKey;
    private readonly bool _isDashboard;
    private readonly CustomMessageControl _emptyMessageView;
    private PatientMedicationView _patientMedicationView;
    private readonly PatientMedicationDTO _medicationsData = new PatientMedicationDTO { Medication = new PatientMedicationModel() };
    private bool _isPatientData;

    /// <summary>
    /// Parameterized constructor containing page inance
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public PatientMedicationsView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new MedicationSevice(App._essentials);
        if (Parameters?.Count > 0)
        {
            _medicationsData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
        }
        _isDashboard = IsDashboardView(_medicationsData.RecordCount);
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        _isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        CustomCellModel customCellData = _isPatientData
            ? new CustomCellModel
            {
                CellHeader = nameof(PatientMedicationModel.FullName),
                CellDescription = nameof(PatientMedicationModel.MedicationDosesString),
                CellDescriptionStatusContent = nameof(PatientMedicationModel.IsCriticalStatusString),
                CellDescriptionStatusContentColor = nameof(PatientMedicationModel.IsCriticalStatusColorString),
                CellLeftDefaultIcon = nameof(PatientMedicationModel.LeftDefaultIcon),
                IconAsCellRightContentHeader = nameof(PatientMedicationModel.MedicationReminderImage),
                CellRightContentHeader = nameof(PatientMedicationModel.FormattedDate),
                CellRightContentDescription = nameof(PatientMedicationModel.FormattedDate),
                BandColor = !String.IsNullOrEmpty(nameof(PatientMedicationModel.ProgramColor)) ? nameof(PatientMedicationModel.ProgramColor) : null,
                IconSize = AppImageSize.ImageSizeM,
                CellRightContentDescriptionInPancakeView = false,
            }
            : new CustomCellModel
            {
                CellHeader = nameof(PatientMedicationModel.FullName),
                CellDescription = nameof(PatientMedicationModel.MedicationDosesString),
                CellDescriptionStatusContent = nameof(PatientMedicationModel.IsCriticalStatusString),
                CellDescriptionStatusContentColor = nameof(PatientMedicationModel.IsCriticalStatusColorString),
                CellLeftDefaultIcon = nameof(PatientMedicationModel.LeftDefaultIcon),
                CellRightContentHeader = nameof(PatientMedicationModel.FormattedDate),
                CellRightContentDescription = nameof(PatientMedicationModel.MedicationStatusString),
                CellDescriptionColor = nameof(PatientMedicationModel.MedicationStatusColorString),
                BandColor = !String.IsNullOrEmpty(nameof(PatientMedicationModel.ProgramColor)) ? nameof(PatientMedicationModel.ProgramColor) : null,
                IconSize = AppImageSize.ImageSizeM,
            };
        IsTabletListHeaderDisplay = IsPatientPage() && !_isDashboard;
        CreateGridAndAssignList(customCellData);
        SetPageContent(_mainLayout);
        _emptyMessageView = new CustomMessageControl(false);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Operation status</returns>
    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        Guid selectMedicationID = _medicationsData.Medication?.PatientMedicationID ?? Guid.Empty;
        if (!isRefreshRequest)
        {
            _medicationsData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            string medicationId = GenericMethods.MapValueType<string>(GetParameterValue(nameof(PatientMedicationModel.PatientMedicationID)));
            _medicationsData.Medication.PatientMedicationID = string.IsNullOrWhiteSpace(medicationId) ? Guid.Empty : new Guid(medicationId);
        }
        if ((CollectionViewField.SelectedItem as PatientMedicationModel)?.PatientMedicationID != selectMedicationID || selectMedicationID != Guid.Empty)
        {
            _selectedTabKey = isRefreshRequest ? _selectedTabKey : ResourceConstants.R_OPEN_TASK_KEY;
            OnListItemSelection(Medications_SelectionChanged, !IsPatientOverview(_medicationsData.RecordCount));
            await (ParentPage.PageService as MedicationSevice).GetMedicationsAsync(_medicationsData, _selectedTabKey).ConfigureAwait(true);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                ParentPage.PageData = ParentPage.PageService.PageData;
                _emptyListView.PageResources = ParentPage.PageData;
                _emptyMessageView.PageResources = ParentPage.PageData;
                GenerateTabs(isRefreshRequest);
                await RenderUIDataAsync(isRefreshRequest).ConfigureAwait(true);
            });
        }
    }

    /// <summary>
    /// Unregister events of View
    /// </summary>
    public async override Task UnloadUIAsync()
    {
        OnActionButtonClicked -= OnAddButtonClicked;
        OnListItemSelection(Medications_SelectionChanged, false);
        if (SearchField != null)
        {
            SearchField.OnSearchTextChanged -= CustomSearch_OnSearchTextChanged;
        }
        if (_patientMedicationView != null)
        {
            await _patientMedicationView.UnloadUIAsync().ConfigureAwait(true);
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Save medication data
    /// </summary>
    public async Task<bool> SaveMedicationAsync()
    {
        return _patientMedicationView != null && _mainLayout.Children.Contains(_patientMedicationView) && await _patientMedicationView.SaveMedicationAsync().ConfigureAwait(true);
    }

    internal void SetMainGridSize()
    {
        if (GenericMethods.IsListNotEmpty(_medicationsData.Medications))
        {
            _mainLayout.HeightRequest = ((double)AppImageSize.ImageSizeL + (2 * _margin)) * _medicationsData.Medications.Count + 60;
        }
        else
        {
            _mainLayout.HeightRequest = 500;
        }
    }

    internal async Task OnMedicationClickAsync(Guid medicationId, bool isAddButtonClicked)
    {
        if (isAddButtonClicked)
        {
            CollectionViewField.SelectedItem = null;
        }
        if (MobileConstants.IsTablet)
        {
            if (HasAddPermission())
            {
                if (ShellMasterPage.CurrentShell.CurrentPage is PatientMedicationsPage)
                {
                    //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientMedicationPage)).ConfigureAwait(true);
                    if (_patientMedicationView != null && _mainLayout.Children.Contains(_patientMedicationView))
                    {
                        _mainLayout.Children.Remove(_patientMedicationView);
                    }
                    if (_mainLayout.Children.Contains(_emptyMessageView))
                    {
                        _mainLayout.Children.Remove(_emptyMessageView);
                    }
                    _patientMedicationView = new PatientMedicationView(new BasePage(), ParentPage.AddParameters(
                        ParentPage.CreateParameter(nameof(PatientMedicationModel.PatientMedicationID), Convert.ToString(medicationId, CultureInfo.InvariantCulture)),
                        ParentPage.CreateParameter(nameof(BaseDTO.IsActive), Convert.ToString(false, CultureInfo.InvariantCulture))));
                    _mainLayout.Add(_patientMedicationView, 2, 0);
                    Grid.SetRowSpan(_patientMedicationView, 3);
                    _patientMedicationView.OnDisplayStatus -= OnRefreshCall;
                    _patientMedicationView.OnDisplayStatus += OnRefreshCall;
                    await _patientMedicationView.LoadUIAsync(false).ConfigureAwait(true);
                    //to handle kill state notification click
                    CollectionViewField.ItemsSource = new List<PatientMedicationModel>();
                    CollectionViewField.ItemsSource = _medicationsData.Medications;
                }
                else if (IsPatientPage())
                {
                    await OpenMedicationPopupAsync(medicationId).ConfigureAwait(true);
                }
                else
                {
                    await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientMedicationsPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.recordCount, Param.id), "0", medicationId.ToString()).ConfigureAwait(true);
                }
            }
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.PatientMedicationPage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.id), medicationId.ToString()).ConfigureAwait(true);
        }
    }

    private void GenerateTabs(bool isRefreshRequest)
    {
        if (!_isDashboard && !isRefreshRequest && !IsPatientOverview(_medicationsData.RecordCount) && !IsPatientPage())
        {
            _customTabs.LoadUIData((from medication in ParentPage.PageData.Resources
                                    where medication.ResourceKey == ResourceConstants.R_OPEN_TASK_KEY || medication.ResourceKey == ResourceConstants.R_HISTORY_TASK_KEY
                                    select new OptionModel { OptionID = medication.ResourceID, OptionText = medication.ResourceValue, GroupName = medication.ResourceKey }).ToList(), true);
            _customTabs.TabClicked += OnTaskTabClicked;
        }
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
        if (_medicationsData?.ErrCode == ErrorCode.OK)
        {
            await RenderMedicationDataAsync(isRefreshRequest).ConfigureAwait(true);
        }
        else
        {
            RenderErrorView(_mainLayout, _medicationsData?.ErrCode.ToString() ?? ErrorCode.ErrorWhileRetrievingRecords.ToString(), _isDashboard, _isDashboard ? (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] : 0, false, true);
        }
    }

    private async void CustomSearch_OnSearchTextChanged(object sender, EventArgs e)
    {
        var serchBar = sender as CustomSearchBar;
        CollectionViewField.Footer = null;
        if (string.IsNullOrWhiteSpace(serchBar.Text))
        {
            CollectionViewField.ItemsSource = new List<PatientMedicationModel>();
            await Task.Delay(2);
            CollectionViewField.ItemsSource = _medicationsData.Medications;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(_medicationsData.Medications.Count);
            }
            if (_medicationsData.Medications.Count == 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, false, true);
            }
        }
        else
        {
            var medications = _medicationsData.Medications.FindAll(y =>
            {
                return y.FullName.ToLowerInvariant().Contains(serchBar.Text.Trim().ToLowerInvariant());
            });
            CollectionViewField.ItemsSource = medications;
            if (IsPatientPage())
            {
                TabletHeader.Text = GetHeaderText(medications.Count);
            }
            if (medications.Count <= 0)
            {
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
            }
        }
    }

    private async Task RenderMedicationDataAsync(bool isRefreshRequest)
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
                TabletHeader.Text = GetHeaderText(_medicationsData.Medications.Count);
            }
        }
        CollectionViewField.ItemsSource = new List<PatientMedicationModel>();
        if (GenericMethods.IsListNotEmpty(_medicationsData.Medications))
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

    private bool HasAddPermission()
    {
        return ParentPage.CheckFeaturePermissionByCode(AppPermissions.PatientMedicationAddEdit.ToString());
    }

    private string GetHeaderText(int count)
    {
        return $"{ParentPage.GetFeatureValueByCode(AppPermissions.PatientMedicationsView.ToString())} ({count})";
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
        {
            OnActionButtonClicked -= OnAddButtonClicked;
            await OpenMedicationPopupAsync(Guid.Empty).ConfigureAwait(true);
            OnActionButtonClicked += OnAddButtonClicked;
        }
    }

    private async Task RenderViewsAsync()
    {
        CollectionViewField.ItemsSource = _medicationsData.Medications;
        if (!_isDashboard)
        {
            await RenderDetailViewAsync().ConfigureAwait(true);
        }
        else
        {
            if (MobileConstants.IsDevicePhone)
            {
                _mainLayout.HeightRequest = DeviceInfo.Platform == DevicePlatform.iOS ? (CellRowHeight * _medicationsData.Medications.Count) + 3 : (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] * _medicationsData.Medications.Count + new OnIdiom<int> { Phone = 5, Tablet = 0 };
            }
            else
            {
                _mainLayout.HeightRequest = _margin + (((IsPatientOverview(_medicationsData.RecordCount) ? GenericMethods.GetPlatformSpecificValue(5, 7, 0) : GenericMethods.GetPlatformSpecificValue(2, 6, 0)) * _medicationsData.Medications.Count) + GenericMethods.GetPlatformSpecificValue(2, 5, 0)) + (CellRowHeight * _medicationsData.Medications.Count);
            }
        }
    }

    private async void OnRefreshCall(object sender, EventArgs e)
    {
        ParentPage.DisplayOperationStatus((string)sender, e != EventArgs.Empty);
        if (e != EventArgs.Empty)
        {
            _medicationsData.Medication.PatientMedicationID = Guid.Empty;
            await LoadUIAsync(true).ConfigureAwait(true);
            if (ShellMasterPage.CurrentShell.CurrentPage is PatientMedicationsPage)
            {
                ParentPage.ShowHideLeftRightHeader(MenuLocation.Right, false);
                MenuView titleView = new MenuView(MenuLocation.Header, string.Empty, ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
                await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
            }
        }
    }

    private async Task OpenMedicationPopupAsync(Guid medicationId)
    {
        PatientMedicationPopupPage medicationAddEditPage = new PatientMedicationPopupPage(medicationId.ToString());
        medicationAddEditPage.OnDisplayStatus += OnRefreshCall;
        //todo:await Navigation.PushPopupAsync(medicationAddEditPage).ConfigureAwait(true);
    }

    private async Task RenderDetailViewAsync()
    {
        if (MobileConstants.IsTablet && !IsPatientPage())
        {
            if (_medicationsData.Medication?.PatientMedicationID != Guid.Empty)
            {
                await OnMedicationClickAsync(_medicationsData.Medication.PatientMedicationID, false).ConfigureAwait(true);
                CollectionViewField.SelectedItem = _medicationsData.Medications.FirstOrDefault(x => x.PatientMedicationID == _medicationsData.Medication.PatientMedicationID);
            }
            else
            {
                if (_patientMedicationView != null && _mainLayout.Children.Contains(_patientMedicationView))
                {
                    _mainLayout.Children.Remove(_patientMedicationView);
                }
                _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                _mainLayout.Add(_emptyMessageView, 2, 0);
                Grid.SetRowSpan(_emptyMessageView, 3);
            }
        }
    }

    private async void Medications_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isPatientData)
        {
            await MedicationSelectedAsync(sender).ConfigureAwait(true);
        }
        else
        {
            if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, ResourceConstants.R_OFFLINE_OPERATION_KEY))
            {
                await MedicationSelectedAsync(sender).ConfigureAwait(true);
            }
        }
    }

    private async Task MedicationSelectedAsync(object sender)
    {
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            await OnMedicationClickAsync((item.SelectedItem as PatientMedicationModel).PatientMedicationID, false).ConfigureAwait(true);
        }
    }

    private async void OnTaskTabClicked(object sender, EventArgs e)
    {
        _customTabs.TabClicked -= OnTaskTabClicked;
        if (_selectedTabKey != (string)sender)
        {
            AppHelper.ShowBusyIndicator = true;
            _selectedTabKey = (string)sender;
            SearchField.Value = string.Empty;
            _medicationsData.Medication = new PatientMedicationModel();
            await LoadUIAsync(true).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
        _customTabs.TabClicked += OnTaskTabClicked;
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
                if (_isPatientData)
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
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose method
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
    }
}