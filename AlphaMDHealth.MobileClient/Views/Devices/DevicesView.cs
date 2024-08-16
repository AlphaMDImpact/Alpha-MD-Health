using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class DevicesView : BaseLibCollectionView
{
    private readonly PatientDeviceDTO _devicesData = new PatientDeviceDTO();
    private readonly CustomLabelControl _deviceListHeader;
    private readonly Grid _mainLayout;
    private DeviceView _deviceView;
    private bool _isAddPage;
    private readonly CustomInfoControl _instructionView;
    private DevicesView _addNewDevice;
    private readonly CustomMessageControl _emptyMessageView;

    public DevicesView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new DeviceService(App._essentials);
        _mainLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
        };
        AddSearchView(_mainLayout, false);
        Grid.SetColumnSpan(SearchField, new OnIdiom<int> { Phone = 3, Tablet = 1 });
        SearchField.IsVisible = false;
        _deviceListHeader = new CustomLabelControl(LabelType.HeaderPrimaryMediumBoldForDashboard) { IsVisible = false };
        _instructionView = new CustomInfoControl(true) { IsVisible = false };
        _mainLayout.Add(_deviceListHeader, 0, 0);
        Grid.SetColumnSpan(_deviceListHeader, 3);
        _mainLayout.Add(_instructionView, 0, 2);
        Grid.SetColumnSpan(_instructionView, 3);
        if (MobileConstants.IsTablet)
        {
            AddSeparatorView(_mainLayout, 1, 0);
            Grid.SetRowSpan(Separator, 3);
        }
        _emptyMessageView = new CustomMessageControl(false);
        SetPageContent(_mainLayout);
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        if (CollectionViewField == null || !_mainLayout.Children.Contains(CollectionViewField))
        {
            AddCollectionView(_mainLayout, GetCustomCell(), 0, 1);
            Grid.SetColumnSpan(CollectionViewField, new OnIdiom<int> { Phone = 3, Tablet = 1 });
        }
        if (!isRefreshRequest)
        {
            _isAddPage = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(Param.isAdd)));
            OnListItemSelection(OnDeviceListItemClicked, true);
            if (!_isAddPage)
            {
                _mainLayout.ColumnDefinitions = CreateTabletViewColumn(false);
                _mainLayout.ColumnSpacing = (DeviceInfo.Idiom == DeviceIdiom.Tablet)
                    ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture)
                    : Constants.ZERO_PADDING;
            }
        }
        if (_isAddPage)
        {
            Grid.SetColumnSpan(CollectionViewField, 3);
            await (ParentPage.PageService as DeviceService).GetDevicesToPairAsync(_devicesData).ConfigureAwait(true);
            if (MobileConstants.IsTablet)
            {
                Separator.IsVisible = false;
                _mainLayout.Padding = ParentPage.GetLayoutPadding(PageLayoutType.EndToEndPageLayout);
            }
            _instructionView.IsVisible = true;
            _deviceListHeader.IsVisible = true;
        }
        else
        {
            await (ParentPage.PageService as DeviceService).GetPairedDevicesAsync(_devicesData).ConfigureAwait(true);
            SearchField.IsVisible = true;
        }
        ParentPage.PageData = (ParentPage.PageService as DeviceService).PageData;
        _emptyListView.PageResources = ParentPage.PageData;
        _emptyMessageView.PageResources = ParentPage.PageData;
        SearchField.PageResources = ParentPage.PageData;
        await RenderUIAsync().ConfigureAwait(true);
    }

    private async void OnDeviceListItemClicked(object sender, SelectionChangedEventArgs e)
    {
        var item = sender as CollectionView;
        if (item.SelectedItem != null)
        {
            var device = item.SelectedItem as PatientDeviceModel;
            if (string.IsNullOrWhiteSpace(device.DeviceIdentifier))
            {
                return;
            }
            await NavigateToPageAsync(item.SelectedItem as PatientDeviceModel).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Handle navigation for tablet and mobile
    /// </summary>
    /// <param name="device">Device data</param>
    /// <returns>Task executing the navigation of page/view</returns>
    public async Task NavigateToPageAsync(PatientDeviceModel device)
    {
        AppHelper.ShowBusyIndicator = true;
        if (_isAddPage)
        {
            _devicesData.Device = device;
            DevicePairingPopupPage colorPickerPage = new DevicePairingPopupPage(_devicesData, ParentPage, false);
            colorPickerPage.OnRefreshButtonClicked += OnPopupClosed;
            //todo:await Navigation.PushPopupAsync(colorPickerPage).ConfigureAwait(true);
        }
        else
        {
            if (MobileConstants.IsTablet)
            {
                if (ShellMasterPage.CurrentShell.CurrentPage is DevicesPage)
                {
                    //await ParentPage.SetRightHeaderItemsAsync(nameof(DevicePage)).ConfigureAwait(true);
                    EmptyRightView();
                    _deviceView = new DeviceView(new BasePage(), ParentPage.AddParameters(ParentPage.CreateParameter(nameof(DevicePage.ReadingSourceID), device.ReadingSourceID.ToString()), ParentPage.CreateParameter(nameof(DevicePage.DeviceType), device.DeviceIdentifier)));
                    _mainLayout.Add(_deviceView, 2, 0);
                    Grid.SetRowSpan(_deviceView, 2);
                    _deviceView.OnListRefresh += OnPopupClosed;
                    await _deviceView.LoadUIAsync(false).ConfigureAwait(true);
                    //ParentPage.ClearRightHeaderItems();
                    await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, device.Name, true)).ConfigureAwait(true);
                }
                else
                {
                    await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.DevicePage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.type), device.ReadingSourceID.ToString(), device.DeviceIdentifier).ConfigureAwait(true);
                }
            }
            else
            {
                await ShellMasterPage.CurrentShell.CurrentPage.PushPageByTargetAsync(Pages.DevicePage.ToString(), GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.type), device.ReadingSourceID.ToString(), device.DeviceIdentifier).ConfigureAwait(false);
            }
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
        CollectionViewField.SelectedItem = null;
        if ((bool)sender && MobileConstants.IsTablet)
        {
            //ParentPage.ClearRightHeaderItems();
            InvokeListRefresh(new object(), new EventArgs());
        }
    }

    private async Task RenderUIAsync()
    {
        if (_isAddPage && MobileConstants.IsTablet)
        {
            MenuView titleView = new MenuView(MenuLocation.Header, ParentPage.GetFeatureValueByCode(Utility.AppPermissions.SupportedDevicesView.ToString()), ShellMasterPage.CurrentShell.CurrentPage.IsAddEditPage);
            //ParentPage.ClearRightHeaderItems();
            await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
        }
        if (_devicesData.ErrCode == ErrorCode.OK)
        {
            if (_isAddPage)
            {
                _deviceListHeader.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_ADD_DEVICES_TEXT_KEY);
                _instructionView.SetInfoValue(ParentPage.GetResourceValueByKey(ResourceConstants.R_DEVICE_INFO_KEY));
            }
            if (_devicesData.Devices?.Count > 0)
            {
                SearchField.OnSearchTextChanged += OnDeviceSearch;
                await LoadListDataAsync().ConfigureAwait(true);
            }
            else
            {
                ResetCollection();
                RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
            }
        }
        else
        {
            ResetCollection();
            RenderErrorView(_mainLayout, _devicesData.ErrCode.ToString(), false, (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT], false, true);
        }
        RenderNoDetailView();
    }

    private void ResetCollection()
    {
        CollectionViewField.ItemsSource = new List<PatientDeviceModel>();
        CollectionViewField.HeightRequest = CollectionViewField.Height;
    }

    private void RenderNoDetailView()
    {
        if (MobileConstants.IsTablet)
        {
            CollectionViewField.SelectedItem = null;
            EmptyRightView();
            if (!_isAddPage)
            {
                _emptyMessageView.ControlResourceKey = ResourceConstants.R_NO_DATA_FOUND_KEY;
                _mainLayout.Add(_emptyMessageView, 2, 0);
                Grid.SetRowSpan(_emptyMessageView, 2);
            }
        }
    }

    private async Task LoadListDataAsync()
    {
        CollectionViewField.ItemsSource = new List<PatientDeviceModel>();
        CollectionViewField.ItemsSource = _devicesData.Devices;
        if (MobileConstants.IsTablet)
        {
            if (_devicesData.Device != null && _devicesData.Device.ReadingSourceID != Guid.Empty)
            {
                await NavigateToPageAsync(_devicesData.Device).ConfigureAwait(true);
                CollectionViewField.SelectedItem = _devicesData.Devices.FirstOrDefault(x => x.ReadingSourceID == _devicesData.Device.ReadingSourceID);
            }
            else if (CollectionViewField.SelectedItem != null)
            {
                _devicesData.Device = _devicesData.Devices.FirstOrDefault(x => x.ReadingSourceID == (CollectionViewField.SelectedItem as PatientDeviceModel).ReadingSourceID);
                await NavigateToPageAsync(_devicesData.Device).ConfigureAwait(true);
            }
            else
            {
                CollectionViewField.SelectedItem = null;
            }
        }
        CollectionViewField.Footer = string.Empty;
    }

    public override Task UnloadUIAsync()
    {
        OnListItemSelection(OnDeviceListItemClicked, false);
        if (SearchField.Value != null)
        {
            SearchField.Value = string.Empty;
            SearchField.OnSearchTextChanged -= OnDeviceSearch;
        }
        _deviceView?.UnloadUIAsync();
        _mainLayout.Children.Remove(CollectionViewField);
        if (_deviceView != null && _mainLayout.Children.Contains(_deviceView))
        {
            _mainLayout.Children.Remove(_deviceView);
        }
        return base.UnloadUIAsync();
    }

    /// <summary>
    /// Invoked when Add button is clicked
    /// </summary>
    /// <returns>Loads add view and returns its instance</returns>
    public async Task<DevicesView> AddButtonClickedAsync()
    {
        CollectionViewField.SelectedItem = null;
        EmptyRightView();
        _addNewDevice = new DevicesView(ParentPage, ParentPage.AddParameters(ParentPage.CreateParameter(nameof(Param.isAdd), true.ToString(CultureInfo.InvariantCulture))));
        _mainLayout.Add(_addNewDevice, 2, 0);
        Grid.SetRowSpan(_addNewDevice, 2);
        await _addNewDevice.LoadUIAsync(false).ConfigureAwait(true);
        return _addNewDevice;
    }

    private void EmptyRightView()
    {
        if (_deviceView != null && _mainLayout.Children.Contains(_deviceView))
        {
            _mainLayout.Children.Remove(_deviceView);
        }
        if (_addNewDevice != null && _mainLayout.Children.Contains(_addNewDevice))
        {
            _mainLayout.Children.Remove(_addNewDevice);
        }
        if (_emptyMessageView != null && _mainLayout.Children.Contains(_emptyMessageView))
        {
            _mainLayout.Children.Remove(_emptyMessageView);
        }
    }

    private void OnDeviceSearch(object sender, EventArgs e)
    {
        if (_devicesData.Devices.Count > 0)
        {
            CollectionViewField.Footer = null;
            var searchBar = sender as CustomSearchBar;
            if (string.IsNullOrWhiteSpace(searchBar.Text))
            {
                CollectionViewField.ItemsSource = new List<PatientDeviceModel>();
                CollectionViewField.ItemsSource = _devicesData.Devices;
            }
            else
            {
                var searchedDevices = _devicesData.Devices.FindAll(y =>
                {
                    return ((!string.IsNullOrWhiteSpace(y.Name) && y.Name.ToLowerInvariant().Contains(searchBar.Text.Trim().ToLowerInvariant()))
                    || (!string.IsNullOrWhiteSpace(y.Description) && y.Description.ToLowerInvariant().Contains(searchBar.Text.ToLowerInvariant().Trim())));
                });
                if (searchedDevices.Count > 0)
                {
                    CollectionViewField.ItemsSource = searchedDevices;
                }
                else
                {
                    CollectionViewField.ItemsSource = new List<PatientDeviceModel>();
                    RenderErrorView(_mainLayout, ResourceConstants.R_NO_DATA_FOUND_KEY, false, 0, true, false);
                }
            }
        }
    }

    private CustomCellModel GetCustomCell()
    {
        return new CustomCellModel
        {
            CellID = nameof(PatientDeviceModel.DeviceUUID),
            CellHeader = nameof(PatientDeviceModel.Name),
            //todo:CellLeftSourceIcon = nameof(PatientDeviceModel.DeviceImage),
            CellDescription = nameof(PatientDeviceModel.Description),
            CellRightIcon = nameof(PatientDeviceModel.DeviceArrowIcon),
            ArrangeHorizontal = false,
        };
    }
}