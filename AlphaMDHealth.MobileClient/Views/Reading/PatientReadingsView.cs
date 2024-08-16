using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Patient reading list view
/// </summary>
public class PatientReadingsView : BaseLibCollectionView
{
    private readonly PatientReadingDTO _readingsData;
    private readonly Grid _mainLayout;
    private readonly CustomMessageControl _emptyMessageView;
    private readonly CustomReadingCarauselView _carauselView;
    private Grid _readingsLayout;
    private ListStyleType _viewType = ListStyleType.BoxView;
    private ViewManager _detailView;
    private CustomFilterControl _customFilterControl;
    private CustomLabelControl _selectedCategoryLabel;
    private CustomButtonControl _bluetoothButton;
    private PatientReadingDTO selectedReading;
    private readonly double _componantPadding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture);
    private readonly bool _isDashboardView;
    private short _selectedReadingID;
    private bool _displayCategoryFilter = true;
    private bool _isAdd;
    private bool _healthKitStatus = true;
    private int _devicePairingStatus;
    private bool _isAddClicked;

    /// <summary>
    /// Patient readings list
    /// </summary>
    /// <param name="page">Instance of base page</param>
    /// <param name="parameters">View parameters</param>
    public PatientReadingsView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new ReadingService(App._essentials);
        _readingsData = new PatientReadingDTO
        {
            ReadingID = 0,
            LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0),
        };
        MapParameters();
        _isDashboardView = IsDashboardView(_readingsData.RecordCount);
        _mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[IsPatientPage() && !_isDashboardView
                ? StyleConstants.ST_END_TO_END_GRID_STYLE
                : StyleConstants.ST_DEFAULT_GRID_STYLE],
            ColumnSpacing = !_isDashboardView && DeviceInfo.Idiom == DeviceIdiom.Tablet && !IsPatientPage()
                ? _margin
                : Constants.ZERO_PADDING,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height =  _isDashboardView ? GridLength.Auto : GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions = CreateTabletViewColumn(_isDashboardView || DeviceInfo.Idiom == DeviceIdiom.Phone)
        };
        if (_viewType != ListStyleType.CarauselView)
        {
            AddCollectionView(_mainLayout, null, 0, 3);
        }
        else
        {
            _carauselView = new CustomReadingCarauselView(ParentPage, true);
            _mainLayout.Add(_carauselView, 0, 2);
        }
        if (!_isDashboardView)
        {
            MapProviderTabViews();
            AddSeparator();
        }
        SetPageContent(_mainLayout);
        _emptyMessageView = new CustomMessageControl(false);
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Creates required view</returns>
    public async override Task LoadUIAsync(bool isRefreshRequest)
    {
        if (!isRefreshRequest)
        {
            _readingsData.ReadingDTOs = new List<PatientReadingDTO> { };
            MapParameters();
        }
        await FetchAndUpdateDataAsync(isRefreshRequest).ConfigureAwait(true);
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public async override Task UnloadUIAsync()
    {
        UnLoadReadingsLayout();
        OnListItemSelection(OnCarauselSelectionChanged, false);
        if (_detailView != null && _mainLayout.Children.Contains(_detailView))
        {
            _detailView.OnListRefresh -= ReadingView_OnListRefresh;
            await _detailView.UnloadUIAsync();
            _mainLayout.Children.Remove(_detailView);
        }
        if (_customFilterControl != null)
        {
            _customFilterControl.SelectedItemChanged -= FilterSelectedValueChanged;
        }
        if (_emptyListView != null)
        {
            _emptyListView.OnActionClicked -= OnMessgeViewActionClicked;
        }
        if (_bluetoothButton != null)
        {
            _bluetoothButton.Clicked -= OnRefreshButtonClick;
        }
        await base.UnloadUIAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Refresh the list
    /// </summary>
    public async Task RefreshUIAsync()
    {
        await (ParentPage.PageService as ReadingService).GetPatientReadingsAsync(_readingsData).ConfigureAwait(true);
        if (_readingsData.ErrCode == ErrorCode.OK)
        {
            _customFilterControl.ItemSource = _readingsData.FilterOptions;
            ClearReadingLayout();
            RendereListItems();
            SetReadingRowAsSelectedItem();
            if (_selectedCategoryLabel != null)
            {
                _selectedCategoryLabel.Text = _readingsData.FilterOptions.First(x => x.IsSelected).OptionText;
            }
            if (!HasDataInList())
            {
                await SetDevicePairingStatusAsync().ConfigureAwait(true);
                _selectedReadingID = 0;
                await RenderReadingsDataAsync().ConfigureAwait(true);
            }
            else
            {
                RemoveBluetoothButton();
                SetHeightForProviderScreen();
            }
            if (_isDashboardView && HasDataInList() && CollectionViewField.SelectedItem != null)
            {
                try
                {
                    CollectionViewField.ScrollTo(CollectionViewField.SelectedItem);
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Action to perform on click of Top menus
    /// </summary>
    /// <param name="menuAction">Menus action needs to perform</param>
    /// <param name="isDetailView">Is Action perform from detail view</param>
    public async Task<bool> OnActionClickedAsync(MenuAction menuAction, bool isDetailView)
    {
        if (menuAction == MenuAction.MenuActionAddKey)
        {
            if (MobileConstants.IsTablet)
            {
                if (!_isAddClicked)
                {
                    _isAddClicked = true;
                    await LoadReadingAsync(Convert.ToString((isDetailView ? _selectedReadingID : 0), CultureInfo.InvariantCulture), Guid.Empty.ToString()).ConfigureAwait(false);
                    _isAddClicked = false;
                }
            }
            else
            {
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingPage),
                    GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.type, Param.id),
                    _readingsData.ReadingCategoryID.ToString(), string.Empty, string.Empty
                ).ConfigureAwait(false);
            }
        }
        else
        {
            if (menuAction == MenuAction.MenuActionSaveKey && MobileConstants.IsTablet)
            {
                await SaveReadingClickedAsync().ConfigureAwait(false);
            }
        }
        return true;
    }

    /// <summary>
    /// Open reading page based on the given parameters
    /// </summary>
    /// <param name="readingType">Reading type to be selected</param>
    /// <param name="patientReadingID">Reading id incase of edit</param>
    /// <returns>A task which loads the add/edit page of the given patient reading id</returns>
    public async Task LoadReadingAsync(string readingType, string patientReadingID)
    {
        await SetDetailViewAsync(Convert.ToInt16(readingType, CultureInfo.InvariantCulture), typeof(PatientReadingView),
            string.IsNullOrWhiteSpace(patientReadingID) ? Guid.Empty : new Guid(patientReadingID)).ConfigureAwait(true);
    }

    /// <summary>
    /// Invokes save click
    /// </summary>
    /// <returns>Task representing the save operation</returns>
    public async Task SaveReadingClickedAsync()
    {
        if (_detailView is PatientReadingView patientReadingView)
        {
            await patientReadingView.OnSaveButtonClickedAsync().ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Add collection view to given layout
    /// </summary>
    /// <param name="mainLayout">Layout to which ListView is to be added</param>
    /// <param name="customCellModel">Item model</param>
    /// <param name="left">Column position</param>
    /// <param name="top">Row position</param>
    public override void AddCollectionView(Grid mainLayout, CustomCellModel customCellModel, int left, int top)
    {
        var margin = IsPatientPage() ? _margin : 0;
        if (_isDashboardView)
        {
            CollectionViewField = new CollectionView
            {
                SelectionMode = SelectionMode.Single,
                ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
                ItemsLayout = ItemLayoutCreate(_isDashboardView),
                Margin = (FlowDirection)Application.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] == FlowDirection.RightToLeft
                    ? new Thickness(margin, 0, 0, 0)
                    : new Thickness(0, 0, margin, 0),
                ItemTemplate = new DataTemplate(() =>
                {
                    var content = new PatientReadingViewCell(_isDashboardView, false, Constants.PATIENT_READINGS_TILE_HEIGHT);
                    ParentPage.AddSpacingForSeparatorLine(content, _isDashboardView);
                    return new ContentView
                    {
                        Style = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE],
                        Content = content
                    };
                })
            };
            mainLayout?.Add(CollectionViewField, left, top);
        }
        else
        {
            _readingsLayout = new Grid
            {
                Margin = (FlowDirection)Application.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] == FlowDirection.RightToLeft
                    ? new Thickness(margin, 0, 0, 0)
                    : new Thickness(0, 0, margin, 0),
            };
            mainLayout?.Add(new ScrollView() { Content = _readingsLayout }, left, top);
        }
        CellRowHeight = _isDashboardView
            ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CARD_HEIGHT_STYLE], CultureInfo.InvariantCulture)
            : (double)AppImageSize.ImageSizeM + (2 * _margin) + new OnIdiom<double> { Phone = _margin + 8, Tablet = 8 };
    }

    private async Task FetchAndUpdateDataAsync(bool isRefreshRequest)
    {
        await (ParentPage.PageService as ReadingService).GetPatientReadingsAsync(_readingsData).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (!isRefreshRequest && !_isDashboardView
            && !App._essentials.GetPreferenceValue(StorageConstants.PR_IS_SYNCING_FROM_FITNESS_APP, false))
        {
            await HealthkitStatusAsync().ConfigureAwait(true);
        }
        if (!HasDataInList())
        {
            await SetDevicePairingStatusAsync().ConfigureAwait(true);
        }
        else
        {
            RemoveBluetoothButton();
        }
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadUIForPatientAsync(isRefreshRequest).ConfigureAwait(true);
        });
    }

    private async Task SetDevicePairingStatusAsync()
    {
        string seperator = string.Concat(Constants.SYMBOL_SINGE_INVERTED_COMMA_SEPERATOR_STRING, Constants.SYMBOL_COMMA_SEPERATOR_STRING, Constants.SYMBOL_SINGE_INVERTED_COMMA_SEPERATOR_STRING);
        //_devicePairingStatus = await new DeviceService(App._essentials).IsDevicePairedForReadingsAsync(string.Join(seperator, _readingsData.ReadingDTOs.Select(x => x.ReadingID))).ConfigureAwait(true);
    }

    private void RemoveBluetoothButton()
    {
        if (_bluetoothButton != null && _mainLayout.Children.Contains(_bluetoothButton))
        {
            _bluetoothButton.Clicked -= OnRefreshButtonClick;
            _mainLayout.Children.Remove(_bluetoothButton);
        }
    }

    private async Task HealthkitStatusAsync()
    {
        BaseDTO result = await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingsPage, true, 0).ConfigureAwait(true);
        _healthKitStatus = (Device.RuntimePlatform == Device.iOS && MobileConstants.IsTablet) || result.ErrCode == ErrorCode.OK;
    }

    private void AddSeparator()
    {
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            AddSeparatorView(_mainLayout, 1, 1);
            Grid.SetRowSpan(Separator, 5);
        }
    }

    private async Task LoadUIForPatientAsync(bool isRefreshRequest)
    {
        if (!_isDashboardView)
        {
            _emptyListView.ShowDiscription = true;
            _emptyListView.ShowIcon = false;
            _emptyListView.Padding = new Thickness(0, _componantPadding);
        }
        _emptyListView.PageResources = ParentPage.PageData;
        _emptyMessageView.PageResources = ParentPage.PageData;
        if (!isRefreshRequest)
        {
            ParentPage.ShowHideLeftRightHeader(MenuLocation.Left, _readingsData.IsActive);
        }
        if (_viewType == ListStyleType.CarauselView)
        {
            RenderCarousel();
        }
        else
        {
            await RenderCollectionViewAsync().ConfigureAwait(true);
        }
    }

    private void RenderCarousel()
    {
        ShowTitle = (_displayCategoryFilter && _readingsData.FilterOptions?.Count > 0);
        RenderCategories();
        if (!IsPatientOverview(_readingsData.RecordCount))
        {
            _carauselView.OnItemClicked += OnCarauselCardClicked;
        }
        // Use to remove margin added in dashboard when Title is not present
        _carauselView.Margin = new Thickness(0, ShowTitle ? 0 : -_componantPadding, 0, 0);
        _carauselView.ItemsSource = _readingsData;

        ////// Note : Below code is work arround of carausel width issue fix, as it is not taking width as expected in patient overview page. 
        ////var newcarauselView = new CustomCarauselView(ParentPage, Parameters);
        ////_mainLayout.Add(newcarauselView, 0, 3);
        ////newcarauselView.ItemsSource = _readingsData;
    }

    private async void OnCarauselCardClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        ReadingMetadataUIModel reading = ((sender as Button).BindingContext as PatientReadingDTO).ChartMetaData[0];

        await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(
            MobileConstants.IsTablet ? nameof(PatientReadingsPage) : nameof(PatientReadingDetailsPage),
            GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.id),
            reading.ReadingCategoryID.ToString(), reading.ReadingID.ToString()
        ).ConfigureAwait(true);
    }

    private async Task RenderCollectionViewAsync()
    {
        ShowHideAddAction();
        if (_isDashboardView)
        {
            CollectionViewField.Margin = new Thickness(0, _margin, 0, 0);
            //_readingsLayout.Margin = new Thickness(0, _margin, 0, 0);
        }
        if (_readingsData?.ErrCode == ErrorCode.OK)
        {
            if (_isDashboardView)
            {
                OnListItemSelection(OnCarauselSelectionChanged, !IsPatientOverview(_readingsData.RecordCount));
            }
            RenderCategories();
            await RenderReadingsDataAsync().ConfigureAwait(true);
        }
        else
        {
            RenderErrorView(_mainLayout, _readingsData?.ErrCode.ToString() ?? ErrorCode.ErrorWhileRetrievingRecords.ToString(), _isDashboardView, 0, false, true);
        }
    }

    private void ShowHideAddAction()
    {
        var isProviderPage = IsPatientPage();
        if (!_isDashboardView && isProviderPage)
        {
            if (TabletHeader != null && _readingsData.FeaturePermissions != null)
            {
                // First Feature Permission is of view 
                TabletHeader.Text = ParentPage.GetFeatureValueByCode(Utility.AppPermissions.PatientReadingsView.ToString());
            }
            if (_readingsData.IsActive && TabletActionButton != null && HasAddPermission())
            {
                // Second Permission is of Add Edit View. 
                TabletActionButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_ACTION_KEY);
                OnActionButtonClicked += OnAddButtonClicked;
            }
        }
        if (!_isDashboardView && !_readingsData.IsActive && !isProviderPage)
        {
            ShellMasterPage.CurrentShell.CurrentPage.ShowHideLeftRightHeader(ParentPage is PatientReadingsPage ? MenuLocation.Left : MenuLocation.Right, false);
        }
    }

    private bool HasAddPermission()
    {
        return _readingsData.FeaturePermissions?.Count > 0
            && ParentPage.CheckFeaturePermissionByCode(Utility.AppPermissions.PatientReadingAddEdit.ToString());
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        OnActionButtonClicked -= OnAddButtonClicked;
        ReadingPopupPage readingPopupPage = new ReadingPopupPage(_readingsData.ReadingCategoryID.ToString(), _readingsData.ReadingID, Guid.Empty.ToString(),
            _readingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture));
        readingPopupPage.OnSaveButtonClicked += ReadingView_OnListRefresh;
        //todo:await Navigation.PushPopupAsync(readingPopupPage).ConfigureAwait(true);
        OnActionButtonClicked += OnAddButtonClicked;
    }

    private void RenderCategories()
    {
        if (_displayCategoryFilter && _readingsData.FilterOptions?.Count > 0)
        {
            if (_customFilterControl == null)
            {
                bool isLeftDirection = IsPatientPage() && (FlowDirection)Application.Current.Resources[StyleConstants.ST_FLOW_DIRECTION] == FlowDirection.LeftToRight;
                _customFilterControl = new CustomFilterControl(IsPatientPage()) { Margin = new Thickness(isLeftDirection ? 0 : _margin, _margin, isLeftDirection ? _margin : 0, _margin) };
                _mainLayout.Add(_customFilterControl, 0, 1);
                if (!_isDashboardView)
                {
                    _selectedCategoryLabel = new CustomLabelControl(LabelType.PrimaryMediumBoldLeft) { Margin = new Thickness(0, 0, 0, _margin) };
                    _mainLayout.Add(_selectedCategoryLabel, 0, 2);
                }
            }
            else
            {
                _customFilterControl.SelectedItemChanged -= FilterSelectedValueChanged;
            }
            _customFilterControl.ItemSource = _readingsData.FilterOptions;
            _customFilterControl.SelectedItemChanged += FilterSelectedValueChanged;
            if (_selectedCategoryLabel != null)
            {
                _selectedCategoryLabel.Text = _readingsData.FilterOptions.First(x => x.IsSelected).OptionText;
            }
        }
    }

    private async void FilterSelectedValueChanged(object sender, SelectedItemChangedEventArgs e)
    {
        _customFilterControl.SelectedItemChanged -= FilterSelectedValueChanged;
        var selectedItem = e.SelectedItem as OptionModel;
        if (TabletActionButton != null)
        {
            OnActionButtonClicked -= OnAddButtonClicked;
        }
        if (_readingsData.ReadingCategoryID != selectedItem.OptionID)
        {
            AppHelper.ShowBusyIndicator = true;
            _readingsData.ReadingCategoryID = Convert.ToInt16(selectedItem.OptionID, CultureInfo.InvariantCulture);
            _selectedReadingID = 0;
            if (_selectedCategoryLabel != null)
            {
                _selectedCategoryLabel.Text = selectedItem.OptionText;
            }
            await FetchAndUpdateDataAsync(true).ConfigureAwait(true);
            AppHelper.ShowBusyIndicator = false;
        }
        _customFilterControl.SelectedItemChanged += FilterSelectedValueChanged;
    }

    private async Task RenderReadingsDataAsync()
    {
        if (_isDashboardView)
        {
            CollectionViewField.ItemsSource = new List<PatientReadingDTO>();
            await Task.Delay(1000).ConfigureAwait(true);
        }
        else
        {
            ClearReadingLayout();
        }
        if (HasDataInList())
        {
            RemoveBluetoothButton();
            isHorizontal = _isDashboardView;
            if (_isDashboardView)
            {
                CollectionViewField.HeightRequest = CellRowHeight;
                if (Device.RuntimePlatform == Device.iOS)
                {
                    CollectionViewField.ItemsLayout = ItemLayoutCreate(isHorizontal);
                }
            }
            await RenderReadingsAsync().ConfigureAwait(true);
        }
        else
        {
            await RenderEmptyReadingsViewAsync().ConfigureAwait(true);
        }
        SetHeightForProviderScreen();
    }

    private void ClearReadingLayout()
    {
        UnLoadReadingsLayout();
        _readingsLayout?.Children?.Clear();
    }

    private async Task RenderEmptyReadingsViewAsync()
    {
        isHorizontal = false;
        DisplayErrorMessageText();
        DisplayConnectButton();
        RenderErrorView(_mainLayout, _isDashboardView
                ? ResourceConstants.R_NO_DATA_FOUND_KEY
                : ResourceConstants.R_NOT_FOUND_MEASUREMENT_TEXT_KEY,
            _isDashboardView, (double)Application.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        if (_isDashboardView)
        {
            ////Delay to let system calculate height. Without delay it calculates incorrect height.
            if (Device.RuntimePlatform == Device.iOS)
            {
                CollectionViewField.ItemsLayout = ItemLayoutCreate(isHorizontal);
            }
            ////else
            ////{
            ////    await Task.Delay(1000).ConfigureAwait(true);
            ////}
        }
        await Task.Delay(1000).ConfigureAwait(true);
        await RenderDetailViewAsync().ConfigureAwait(true);
    }

    private void DisplayErrorMessageText()
    {
        ResourceModel noRecordsResource = ParentPage.GetResourceByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_TEXT_KEY);
        noRecordsResource.ResourceValue = string.Empty;
        noRecordsResource.InfoValue = string.Empty;
        if (!_isDashboardView && !_healthKitStatus && !IsPatientPage() && CanRetriveDataFromHealthKit())
        {
            CreatePlaceholderText(noRecordsResource);
            _emptyListView.Actions = new[] { new OptionModel { GroupName = ButtonType.PrimaryWithMargin.ToString(), OptionText = ResourceConstants.R_CONNECT_BUTTON_TEXT_KEY, SequenceNo = 1 } };
            _emptyListView.OnActionClicked = _emptyListView.OnActionClicked ?? OnMessgeViewActionClicked;
        }
        else
        {
            if (!GenericMethods.IsListNotEmpty(_readingsData.ChartMetaData) || !_readingsData.IsActive)
            {
                noRecordsResource.PlaceHolderValue = ParentPage.GetResourceValueByKey(ResourceConstants.R_NO_DATA_FOUND_KEY);
            }
            _emptyListView.Actions = null;
            _emptyListView.OnActionClicked = null;
        }
    }

    private bool CanRetriveDataFromHealthKit()
    {
        return GenericMethods.IsListNotEmpty(_readingsData.ChartMetaData) && _readingsData.ChartMetaData.Any(x => x.AllowHealthKitData);
    }

    private bool CanRetriveDataFromDevice()
    {
        return GenericMethods.IsListNotEmpty(_readingsData.ChartMetaData) && _readingsData.ChartMetaData.Any(x => x.AllowDeviceData);
    }

    private void DisplayConnectButton()
    {
        if (!_isDashboardView && _devicePairingStatus == 1 && !IsPatientPage() && CanRetriveDataFromDevice())
        {
            ResourceModel noRecordsResource = ParentPage.GetResourceByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_TEXT_KEY);
            noRecordsResource.PlaceHolderValue += string.Concat(Constants.STRING_SPACE,
                ParentPage.GetResourceByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_3_TEXT_KEY).PlaceHolderValue,
                ParentPage.GetResourceValueByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_3_TEXT_KEY)
            );
            _bluetoothButton = new CustomButtonControl(ButtonType.TransparentWithMargin)
            {
                ImageSource = ImageSource.FromResource(ImageConstants.I_BULETOOTH_BLUE_PNG),//todo:, (double)AppImageSize.ImageSizeXS, (double)AppImageSize.ImageSizeXS, default),
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, GenericMethods.GetPlatformSpecificValue(5, 0, 0)),
                HorizontalOptions = LayoutOptions.Center,
            };
            _bluetoothButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_RETRIEVE_DATA_BUTTON_TEXT_KEY);
            _mainLayout.Add(_bluetoothButton, 0, 5);
            _bluetoothButton.Clicked += OnRefreshButtonClick;
        }
        else
        {
            RemoveBluetoothButton();
        }
    }

    private void CreatePlaceholderText(ResourceModel noRecordsResource)
    {
        string healthAppName = GenericMethods.GetPlatformSpecificValue(
            ParentPage.GetResourceValueByKey(ResourceConstants.R_IHEALTH_TEXT_KEY),
            ParentPage.GetResourceValueByKey(ResourceConstants.R_GOOGLE_FIT_TEXT_KEY),
            string.Empty
        );
        noRecordsResource.PlaceHolderValue += string.Concat(
            Constants.STRING_SPACE, ParentPage.GetResourceByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_2_TEXT_KEY).PlaceHolderValue,
            Constants.STRING_SPACE, ParentPage.GetResourceValueByKey(ResourceConstants.R_NOT_FOUND_MEASUREMENT_2_TEXT_KEY),
            Constants.STRING_SPACE, string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_CONNECT_TO_HEALTHKIT_MESSAGE_KEY), healthAppName)
        );
    }

    private async Task RenderReadingsAsync()
    {
        RendereListItems();
        await Task.Delay(1000).ConfigureAwait(true);
        if (_isDashboardView)
        {
            CollectionViewField.Footer = null;
            await Task.Delay(1000).ConfigureAwait(true);
        }
        else
        {
            await RenderDetailViewAsync().ConfigureAwait(true);
        }
    }

    private void RendereListItems()
    {
        if (_isDashboardView)
        {
            CollectionViewField.ItemsSource = _readingsData.ReadingDTOs;
        }
        else
        {
            for (int index = 0; index < _readingsData.ReadingDTOs.Count; index++)
            {
                var content = new PatientReadingViewCell(_isDashboardView, false, Constants.PATIENT_READINGS_TILE_HEIGHT, _readingsData.ReadingDTOs[index]);
                content.MinimumHeightRequest = content.MinimumWidthRequest = Constants.PATIENT_READINGS_TILE_HEIGHT + CellRowHeight;
                ParentPage.AddSpacingForSeparatorLine(content, _isDashboardView);
                var cellLayout = new ContentView
                {
                    StyleId = index.ToString(),
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_SELECTED_CONTENT_STYLE],
                    Content = content
                };
                _readingsLayout.Add(cellLayout, 0, index);
                CustomButton cellButton = new CustomButton
                {
                    StyleId = index.ToString(),
                    //todo:BackgroundColor = Color.Transparent,
                    HeightRequest = cellLayout.HeightRequest,
                    WidthRequest = cellLayout.WidthRequest
                };
                _readingsLayout.Add(cellButton, 0, index);
                cellButton.Clicked += OnReadingTypeSelectionChanged;
            }
        }
    }

    private void SetHeightForProviderScreen()
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        if (!_isDashboardView && !isPatientData)
        {
            if (HasDataInList())
            {
                var listViewHeight = (_readingsData.ReadingDTOs.Count + 1) * (Constants.PATIENT_READINGS_TILE_HEIGHT + 2 * _margin) + 70;
                var detailViewHeight = _detailView is PatientReadingDetailView ? (_detailView as PatientReadingDetailView).GetViewHeight : 0;
                _mainLayout.HeightRequest = listViewHeight > detailViewHeight ? listViewHeight : detailViewHeight;
                if (_mainLayout.HeightRequest > Constants.PATIENT_DETAILS_MAX_HEIGHT)
                {
                    _mainLayout.HeightRequest = Constants.PATIENT_DETAILS_MAX_HEIGHT;
                }
            }
            else
            {
                _mainLayout.HeightRequest = Constants.PATIENT_DETAILS_MAX_HEIGHT;
            }
        }
        else
        {
            _mainLayout.HeightRequest = -1;
        }
    }

    private async void OnRefreshButtonClick(object sender, EventArgs e)
    {
        await SyncDataFromDevicesAsync(sender, string.Empty, _readingsData).ConfigureAwait(true);
    }

    /// <summary>
    /// Sync data from bluetooth devices
    /// </summary>
    /// <param name="sender">Sender button</param>
    /// <param name="readingType">currently selected reading type</param>
    /// <param name="result">Return result returns RefreshMasterPage if page needs to be refreshed</param>
    /// <returns>Task representing the sync from device</returns>
    public async Task SyncDataFromDevicesAsync(object sender, string readingType, PatientReadingDTO result)
    {
        Button synchronizeButton = sender as Button;
        synchronizeButton.IsEnabled = false;
        AppHelper.ShowBusyIndicator = true;
        if (await ParentPage.RequestBluetoothPermissionAsync().ConfigureAwait(true))
        {
            BaseDTO returnResult = new BaseDTO();
            //await new DeviceService(App._essentials).SyncReadingsFromDevicesAsync(returnResult, readingType).ConfigureAwait(true);
            if (returnResult.ErrCode == ErrorCode.OK)
            {
                if (returnResult.RecordCount > 0)
                {
                    await ParentPage.DisplayMessagePopupAsync(string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_RECORDS_UPDATED_MESSAGE_KEY), returnResult.RecordCount), false, true, true).ConfigureAwait(true);
                    if (MobileConstants.IsTablet)
                    {
                        result.ErrCode = ErrorCode.RefreshMasterPage;
                        InvokeListRefresh(result, new EventArgs());
                    }
                    else
                    {
                        await LoadUIAsync(true).ConfigureAwait(true);
                    }
                    await ParentPage.SyncDataWithServerAsync(Pages.PatientReadingPage, false, 0).ConfigureAwait(true);
                }
                else
                {
                    await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_NO_NEW_MEASUREMENT_TEXT_KEY, false, true, false).ConfigureAwait(true);
                }
            }
            else if (returnResult.ErrCode == ErrorCode.DeviceNotFound)
            {
                await ParentPage.DisplayMessagePopupAsync(ParentPage.GetResourceValueByKey(returnResult.ErrCode.ToString()), false, true, true).ConfigureAwait(true);
            }
            else
            {
                await ParentPage.DisplayMessagePopupAsync(ErrorCode.ErrorWhileRetrievingRecords.ToString(), false, true, false).ConfigureAwait(true);
            }
        }
        AppHelper.ShowBusyIndicator = false;
        synchronizeButton.IsEnabled = true;
    }

    private bool HasDataInList()
    {
        return GenericMethods.IsListNotEmpty(_readingsData.ReadingDTOs)
            && _readingsData.ReadingDTOs.Any(x => GenericMethods.IsListNotEmpty(x.ListData)
                || GenericMethods.IsListNotEmpty(x.GraphData)
                || x.LatestValue != Constants.SYMBOL_DOUBLE_HYPHEN
            );
    }

    private async void OnReadingTypeSelectionChanged(object sender, EventArgs e)
    {
        CustomButton item = sender as CustomButton;
        item.Clicked -= OnReadingTypeSelectionChanged;
        var selectedIndex = Convert.ToInt32((sender as CustomButton).StyleId);
        var selectedItem = _readingsData.ReadingDTOs[selectedIndex];
        await OnReadinClicked(selectedItem);
        SetReadingItemAsSelected(selectedItem);
        item.Clicked += OnReadingTypeSelectionChanged;
    }

    private async void OnCarauselSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CollectionViewField.SelectionChanged -= OnCarauselSelectionChanged;
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            if (item.SelectedItem is PatientReadingDTO selectedItem)
            {
                await OnReadinClicked(selectedItem);
            }
        }
        CollectionViewField.SelectionChanged += OnCarauselSelectionChanged;
    }

    private async Task OnReadinClicked(PatientReadingDTO selectedItem)
    {
        AppHelper.ShowBusyIndicator = true;
        _selectedReadingID = selectedItem.ReadingID;
        _readingsData.ReadingCategoryID = selectedItem.ReadingCategoryID;
        _isAdd = false;
        await ReadingTypeClickAsync(_selectedReadingID, false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    internal async Task ReadingTypeClickAsync(short readingID, bool isAddButtonClicked)
    {
        if (isAddButtonClicked || _isAdd)
        {
            SetSelectedItem(null);
        }
        if (MobileConstants.IsTablet)
        {
            await LoadDetailViewAsync(readingID).ConfigureAwait(true);
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingDetailsPage),
                GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.id),
                _readingsData.ReadingCategoryID.ToString(), readingID.ToString()
            ).ConfigureAwait(true);
        }
    }

    private async Task LoadDetailViewAsync(short readingID)
    {
        if (_isAdd)
        {
            await SetDetailViewAsync(_selectedReadingID, typeof(PatientReadingView), Guid.Empty);
        }
        if (ShellMasterPage.CurrentShell.CurrentPage is PatientReadingsPage || (IsPatientPage() && !_isDashboardView))
        {
            await SetDetailViewAsync(readingID, typeof(PatientReadingDetailView), Guid.Empty);
        }
        else if (ShellMasterPage.CurrentShell.CurrentPage.ToString().EndsWith(Constants.DASHBOARD_PAGE_CONSTANT, StringComparison.InvariantCultureIgnoreCase))
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingsPage),
                GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.type),
                _readingsData.ReadingCategoryID.ToString(), readingID.ToString()
            ).ConfigureAwait(true);
        }
        else
        {
            await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(nameof(PatientReadingDetailsPage),
                GenericMethods.GenerateParamsWithPlaceholder(Param.name, Param.id),
                _readingsData.ReadingCategoryID.ToString(), readingID.ToString()
            ).ConfigureAwait(true);
        }
    }

    private async Task SetDetailViewAsync(short readingID, Type viewType, Guid patientReadingID)
    {
        if (_detailView != null && _mainLayout.Children.Contains(_detailView))
        {
            _detailView.OnListRefresh -= ReadingView_OnListRefresh;
            await _detailView.UnloadUIAsync().ConfigureAwait(true);
            _mainLayout.Children.Remove(_detailView);
        }
        if (_mainLayout.Children.Contains(_emptyMessageView))
        {
            _mainLayout.Children.Remove(_emptyMessageView);
        }
        if (viewType == typeof(PatientReadingDetailView))
        {
            if (!IsPatientPage())
            {
                //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientReadingDetailsPage)).ConfigureAwait(true);
            }
            _detailView = new PatientReadingDetailView(new BasePage(), null)
            {
                Parameters = ParentPage.AddParameters(
                    ParentPage.CreateParameter(nameof(PatientReadingDTO.ReadingCategoryID), _readingsData.ReadingCategoryID.ToString()),
                    ParentPage.CreateParameter(nameof(PatientReadingDTO.ReadingID), readingID.ToString()),
                    ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _readingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture))
                )
            };
            (_detailView as PatientReadingDetailView).HandleNavigation = LoadReadingAsync;
        }
        else
        {
            if (viewType == typeof(PatientReadingView))
            {
                //await ParentPage.SetRightHeaderItemsAsync(nameof(PatientReadingPage)).ConfigureAwait(true);
                _detailView = new PatientReadingView(new BasePage(), null)
                {
                    Parameters = ParentPage.AddParameters(
                        ParentPage.CreateParameter(nameof(PatientReadingDTO.ReadingCategoryID), _readingsData.ReadingCategoryID.ToString()),
                        ParentPage.CreateParameter(nameof(PatientReadingUIModel.PatientTaskID), Constants.NUMBER_ZERO),
                        ParentPage.CreateParameter(nameof(PatientReadingUIModel.ReadingID), readingID.ToString()),
                        ParentPage.CreateParameter(nameof(PatientReadingUIModel.PatientReadingID), patientReadingID.ToString()),
                        ParentPage.CreateParameter(nameof(BaseDTO.SelectedUserID), _readingsData.SelectedUserID.ToString(CultureInfo.InvariantCulture))
                    )
                };
            }
        }
        _mainLayout.Add(_detailView, 2, 1);
        Grid.SetRowSpan(_detailView, 4);
        _detailView.OnListRefresh += ReadingView_OnListRefresh;
        await _detailView.LoadUIAsync(false).ConfigureAwait(true);
        SetHeightForProviderScreen();
    }

    private async Task RenderDetailViewAsync()
    {
        if (MobileConstants.IsTablet && !_isDashboardView)
        {
            if (_selectedReadingID < 1)
            {
                if (_isAdd)
                {
                    await SetDetailViewAsync(_selectedReadingID, typeof(PatientReadingView), Guid.Empty);
                }
                else
                {
                    SetSelectedItem(null);
                    if (_detailView != null && _mainLayout.Children.Contains(_detailView))
                    {
                        ///_mainLayout.Children.Remove(_detailView);//old code
                        ////Bug 8962: (All Tablet) Ui is distorted on below scenario
                        _detailView.OnListRefresh -= ReadingView_OnListRefresh;
                        await _detailView.UnloadUIAsync().ConfigureAwait(true);
                        _mainLayout.Children.Remove(_detailView);
                    }
                    _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                    _mainLayout.Add(_emptyMessageView, 2, 1);
                    Grid.SetRowSpan(_emptyMessageView, 4);
                }
            }
            else
            {
                await ReadingTypeClickAsync(_selectedReadingID, false).ConfigureAwait(true);
                SetReadingRowAsSelectedItem();
            }
        }
    }

    private void SetReadingRowAsSelectedItem()
    {
        SetSelectedItem(_readingsData.ReadingDTOs.FirstOrDefault(x => x.ReadingID == _selectedReadingID) ?? _readingsData.ReadingDTOs.FirstOrDefault());
    }

    private void SetSelectedItem(PatientReadingDTO selectedItem)
    {
        selectedReading = selectedItem;
        if (_isDashboardView)
        {
            CollectionViewField.SelectedItem = selectedReading;
        }
        SetReadingItemAsSelected(selectedItem);
    }

    private void SetReadingItemAsSelected(PatientReadingDTO selectedItem)
    {
        if (_readingsLayout?.Children?.Count > 0)
        {
            int selectedIndex = selectedItem != null ? _readingsData.ReadingDTOs.IndexOf(selectedItem) : -1;
            foreach (var child in _readingsLayout.Children)
            {
                if (child is ContentView)
                {
                    ContentView cellContent = child as ContentView;
                    //todo:cellContent.BackgroundColor = Color.Transparent;
                    if (selectedIndex == Convert.ToInt32(cellContent.StyleId, CultureInfo.InvariantCulture))
                    {
                        cellContent.BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_APP_COLOR];
                    }
                }
            }
        }
    }

    private void UnLoadReadingsLayout()
    {
        if (_readingsLayout != null && _readingsLayout.Children?.Count > 0)
        {
            foreach (var child in _readingsLayout.Children)
            {
                if (child is CustomButton)
                {
                    CustomButton cellButton = child as CustomButton;
                    cellButton.Clicked -= OnReadingTypeSelectionChanged;
                }
            }
        }
    }

    /// <summary>
    /// Button click event to perform some action on it
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">event</param>
    protected virtual async void OnMessgeViewActionClicked(object sender, int e)
    {
        _emptyMessageView.OnActionClicked -= OnMessgeViewActionClicked;
        _emptyListView.Actions = null;
        await ShellMasterPage.CurrentShell.PushMainPageAsync(new HealthAccountConnectPage()).ConfigureAwait(true);
    }

    private async void ReadingView_OnListRefresh(object sender, EventArgs e)
    {
        // when e == null, display operation status for detail page
        // when errorCode is success, display success status and refresh list 
        // when errorcode is not success, display error status and view default detail view of list page
        if (sender is PatientReadingDTO result)
        {
            if (e == null)
            {
                _isAdd = true;
                _selectedReadingID = 0;
                ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(result.ErrCode.ToString()));
            }
            else
            {
                if (result.ErrCode == ErrorCode.OK || result.ErrCode == ErrorCode.RefreshMasterPage)
                {
                    _isAdd = true;
                    _readingsData.ReadingCategoryID = result.ReadingCategoryID;
                    _selectedReadingID = result.ReadingID;
                    if (result.ErrCode == ErrorCode.OK)
                    {
                        ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ErrorCode.OK.ToString()), true);
                    }
                    await RefreshUIAsync().ConfigureAwait(true);
                }
                else
                {
                    ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(result.ErrCode.ToString()));
                }
            }
        }
        else
        {
            if (sender != null)
            {
                ParentPage.DisplayOperationStatus((string)sender);
            }
        }
    }

    private void MapProviderTabViews()
    {
        if (IsPatientPage())
        {
            AddCollectionViewWithTabletHeader(_mainLayout, null);
            TabletActionButton.HorizontalOptions = LayoutOptions.End;
            TabletHeader.Margin = new Thickness(0, 5, 0, _componantPadding);
            SearchField.IsVisible = false;
        }
    }

    private void MapParameters()
    {
        long id = 0;
        if (Parameters?.Count > 0)
        {
            _readingsData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.RecordCount)));
            _viewType = GenericMethods.MapValueType<string>(GetParameterValue(Constants.VIEW_TYPE_STRING)).ToEnum<ListStyleType>();
            _displayCategoryFilter = GenericMethods.MapValueType<bool>(GetParameterValue(Constants.DISPLAY_CATEGORY_FILTER_STRING));
            _readingsData.ReadingCategoryID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(ReadingModel.ReadingCategoryID)));

            _isAdd = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(PatientReadingsPage.IsAdd)));
            _selectedReadingID = GenericMethods.MapValueType<short>(GetParameterValue(nameof(ReadingModel.ReadingID)));
            id = GenericMethods.MapValueType<long>(GetParameterValue(nameof(BaseDTO.SelectedUserID)));
        }
        _readingsData.SelectedUserID = id == 0
            ? (ParentPage.PageService as ReadingService).GetUserID()
            : id;
        _readingsData.AccountID = App._essentials.GetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0);
    }
}