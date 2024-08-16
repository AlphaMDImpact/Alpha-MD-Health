using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class DevicePairingPopupPage : BasePopupPage
{
    private readonly PatientDeviceDTO _devicesData;
    private bool _isSuccessPage;
    private readonly BasePage _parent;
    private readonly bool _isRetrieve;
    private readonly CustomMessageControl _devicePairingView;

    /// <summary>
    /// On refresh event
    /// </summary>
    public event EventHandler<EventArgs> OnRefreshButtonClicked;

    /// <summary>
    /// Device Pairing Page
    /// </summary>
    /// <param name="deviceData">Device data</param>
    /// <param name="parent">Instance of parent page</param>
    /// <param name="isRetrieve">true if for retrieving data else false</param>
    public DevicePairingPopupPage(PatientDeviceDTO deviceData, BasePage parent, bool isRetrieve) : base(new BasePage(PageLayoutType.EndToEndPageLayout, false))
    {
        _parent = parent;
        _parentPage.PageData = _parent.PageData;
        _parent.PageService = new DeviceService(App._essentials);
        _devicesData = deviceData;
        _isRetrieve = isRetrieve;
        var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        if (MobileConstants.IsDevicePhone)
        {
            Padding = new Thickness(0, padding * 3, 0, 0);
        }
        _devicePairingView = new CustomMessageControl(false)
        {
            HeightRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0) * 0.6,
            Margin = new Thickness(padding, padding, padding, padding),
            MessageType = MessageType.PageDetails,
            ShowCloseButton = false,
            ShowHeadingOnTop = false,
            ButtonsWithoutSpacing = false,
            ShowIcon = false,
            ShowHeader = false
        };
        _parentPage.PageLayout.Add(_devicePairingView, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await DisplayPairingViewAsync().ConfigureAwait(true);
        if (MobileConstants.IsTablet)
        {
            OnRightHeaderClickedEvent += RightHeaderButtonClicked;
            OnLeftHeaderClickedEvent += OnPopupClose;
        }
        else
        {
            OnCloseButtonClickedEvent += OnPopupClose;
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private async void RightHeaderButtonClicked(object sender, EventArgs e)
    {
        OnRightHeaderClickedEvent -= RightHeaderButtonClicked;
        await UpdateDeviceUIAsync().ConfigureAwait(true);
        OnRightHeaderClickedEvent += RightHeaderButtonClicked;
    }

    private async void OnPopupClose(object sender, EventArgs e)
    {
        await HandlePageNavigationAsync().ConfigureAwait(true);
    }

    private async Task HandlePageNavigationAsync()
    {
        OnRefreshButtonClicked.Invoke(_isSuccessPage, new EventArgs());
        //todo:
        //await Navigation.PopAllPopupAsync().ConfigureAwait(true);
        if (_isSuccessPage && MobileConstants.IsDevicePhone)
        {
            await _parent.PopPageAsync(true).ConfigureAwait(true);
        }
    }

    protected override void OnDisappearing()
    {
        if (MobileConstants.IsTablet)
        {
            OnRightHeaderClickedEvent -= RightHeaderButtonClicked;
            OnLeftHeaderClickedEvent -= OnPopupClose;
        }
        else
        {
            OnCloseButtonClickedEvent -= OnPopupClose;
        }
        _devicePairingView.OnActionClicked -= OnActionButtonClick;
        base.OnDisappearing();
    }

    private async void OnActionButtonClick(object sender, int e)
    {
        _devicePairingView.OnActionClicked -= OnActionButtonClick;
        await UpdateDeviceUIAsync().ConfigureAwait(true);
        _devicePairingView.OnActionClicked += OnActionButtonClick;
    }

    private async Task UpdateDeviceUIAsync()
    {
        if (_isSuccessPage)
        {
            // Update Current UTC date time as last modified since
            await SaveDeviceInfoAsync().ConfigureAwait(true);
        }
        else
        {
            if (await _parent.RequestBluetoothPermissionAsync().ConfigureAwait(true))
            {
                if (_isRetrieve)
                {
                    await RetrieveDataFromDeviceAsync();
                }
                else
                {
                    await ConnectToDeviceAsync().ConfigureAwait(true);
                }
            }
        }
    }

    private async Task RetrieveDataFromDeviceAsync()
    {
        AppHelper.ShowBusyIndicator = true;
        _devicesData.PairdDevices = new List<PatientDeviceModel> { _devicesData.Device };
        await (_parent.PageService as DeviceService).SyncReadingsFromDevicesAsync(_devicesData).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        if (_devicesData.ErrCode == ErrorCode.OK)
        {
            if (_devicesData.RecordCount > 0)
            {
                await _parent.SyncDataWithServerAsync(Pages.DevicesPage, false, 0).ConfigureAwait(true);
                await _parent.DisplayMessagePopupAsync(string.Format(CultureInfo.InvariantCulture, _parent.GetResourceValueByKey(ResourceConstants.R_RECORDS_UPDATED_MESSAGE_KEY), _devicesData.RecordCount), false, true, true).ConfigureAwait(true);
            }
            else
            {
                OnRefreshButtonClicked.Invoke(true, new EventArgs());
                await _parent.DisplayMessagePopupAsync(_parent.GetResourceValueByKey(ResourceConstants.R_NO_NEW_MEASUREMENT_TEXT_KEY), false, true, true).ConfigureAwait(true);
            }
            await HandleActionClickAsync(true).ConfigureAwait(false);
            return;
        }
        else if (_devicesData.ErrCode == ErrorCode.DeviceNotFound)
        {
            await _parent.DisplayMessagePopupAsync(_parent.GetResourceValueByKey(_devicesData.ErrCode.ToString()), false, true, true).ConfigureAwait(true);
        }
        else
        {
            await _parent.DisplayMessagePopupAsync(_parent.GetResourceValueByKey(ErrorCode.ErrorWhileRetrievingRecords.ToString()), false, true, true).ConfigureAwait(true);
        }
        await HandleActionClickAsync(false).ConfigureAwait(false);
    }

    private async Task HandleActionClickAsync(bool isSuccess)
    {
        _parent.MessagePopup.ShowInPopup = false;
        _parent.MessagePopup.IsVisible = false;
        OnRefreshButtonClicked.Invoke(isSuccess, new EventArgs());
        await HandlePageNavigationAsync().ConfigureAwait(true);
    }

    private async Task SaveDeviceInfoAsync()
    {
        AppHelper.ShowBusyIndicator = true;
        await (_parent.PageService as DeviceService).SaveDeviceAndReadingsAsync(_devicesData).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        if (_devicesData.ErrCode == ErrorCode.OK)
        {
            if (_devicesData.RecordCount > 0)
            {
                await _parent.DisplayMessagePopupAsync(string.Format(CultureInfo.InvariantCulture, _parent.GetResourceByKey(ResourceConstants.R_RECORDS_UPDATED_MESSAGE_KEY).ResourceValue, _devicesData.RecordCount), false, true, true).ConfigureAwait(true);
                await _parent.SyncDataWithServerAsync(Pages.DevicesPage, false, 0).ConfigureAwait(true);
            }
            await HandlePageNavigationAsync().ConfigureAwait(true);
        }
        else
        {
            await _parent.DisplayMessagePopupAsync(_devicesData.ErrCode.ToString(), false, true, false).ConfigureAwait(true);
        }
    }

    private async Task ConnectToDeviceAsync()
    {
        AppHelper.IndicatorText = _parent.GetResourceValueByKey(ResourceConstants.R_PAIRING_DEVICE_TEXT_KEY);
        AppHelper.ShowBusyIndicator = true;
        await (_parent.PageService as DeviceService).PairDeviceAndFetchDataAsync(_devicesData).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        if (_devicesData.ErrCode == ErrorCode.OK)
        {
            await DisplaySuccessViewAsync(_parent.GetResourceByKey(ResourceConstants.R_SUCCESS_TEXT_KEY)).ConfigureAwait(true);
            _isSuccessPage = true;
            AppHelper.IndicatorText = string.Empty;
        }
        else
        {
            AppHelper.IndicatorText = string.Empty;
            await _parent.DisplayMessagePopupAsync(ResourceConstants.R_PAIRING_FAILED_ERROR_KEY, _parent.OnPupupActionClicked).ConfigureAwait(true);
        }
    }

    private async Task DisplayPairingViewAsync()
    {
        ////  When user clicks the retrieve button
        if (_isRetrieve)
        {
            _devicePairingView.ControlResourceKey = string.Concat(_devicesData.Device.DeviceIdentifier);
            SetTitle(_parent.GetResourceValueByKey(ResourceConstants.R_RETRIEVE_DATA_BUTTON_TEXT_KEY));
            string result = await GenericMethods.SvgToBase64StringAsync(ImageConstants.I_BLUETOOTH_PNG, App.Current.GetType().Assembly).ConfigureAwait(true);
            _parent.PageData.Resources.FirstOrDefault(x => x.ResourceKey == _devicePairingView.ControlResourceKey).InfoValue = _parent.GetResourceByKey(_devicesData.Device.DeviceIdentifier).InfoValue.Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_ZERO, result);
            _parent.PageData.Resources.ForEach(x =>
            {
                if (x.ResourceKey == _devicePairingView.ControlResourceKey)
                {
                    x.ResourceValue = string.Empty;
                    x.PlaceHolderValue = string.Empty;
                }
            });
            _devicePairingView.PageResources = _parent.PageData;
            if (MobileConstants.IsTablet)
            {
                DisplayRightHeader(ResourceConstants.R_RETRIEVE_DATA_BUTTON_TEXT_KEY);
                DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            }
            else
            {
                ShowCloseButton(true);
                _devicePairingView.Actions = new[] {
                    new OptionModel { GroupName = ButtonType.PrimaryWithMargin.ToString(), OptionText = ResourceConstants.R_RETRIEVE_DATA_BUTTON_TEXT_KEY, SequenceNo= 1 }
                };
            }
        }
        else
        {
            _devicePairingView.ControlResourceKey = string.Concat(_devicesData.Device.DeviceIdentifier, Constants.SYMBOL_UNDERSCORE_STRING, Constants.INSTRUCTION_KEY);
            SetTitle(_parent.GetResourceValueByKey(_devicesData.Device.DeviceIdentifier));
            _parent.PageData.Resources.FirstOrDefault(x => x.ResourceKey == _devicePairingView.ControlResourceKey).InfoValue =
                _parent.GetResourceByKey(string.Concat(_devicesData.Device.DeviceIdentifier, Constants.SYMBOL_UNDERSCORE_STRING, Constants.INSTRUCTION_KEY)).InfoValue.Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_ZERO, string.Concat(Constants.PNG_IMAGE_CONST_PREFIX, (_parent.PageService as DeviceService).GetDeviceIcon(_devicesData.Device.DeviceIdentifier)));
            _parent.PageData.Resources.ForEach(x =>
            {
                if (x.ResourceKey == _devicePairingView.ControlResourceKey)
                {
                    x.ResourceValue = string.Empty;
                    x.PlaceHolderValue = string.Empty;
                }
            });
            _devicePairingView.PageResources = _parent.PageData;
            if (MobileConstants.IsTablet)
            {
                DisplayRightHeader(ResourceConstants.R_START_PAIRING_TEXT_KEY);
                DisplayLeftHeader(ResourceConstants.R_CANCEL_ACTION_KEY);
            }
            else
            {
                ShowCloseButton(true);
                _devicePairingView.Actions = new[] {
                    new OptionModel { GroupName = ButtonType.PrimaryWithMargin.ToString(), OptionText = ResourceConstants.R_START_PAIRING_TEXT_KEY, SequenceNo= 1 }};
            }
        }
        _devicePairingView.OnActionClicked += OnActionButtonClick;
    }

    private async Task DisplaySuccessViewAsync(ResourceModel res)
    {
        _devicePairingView.ControlResourceKey = res.ResourceKey;
        string result = await GenericMethods.SvgToBase64StringAsync(ImageConstants.I_DEVICE_SUCCESS_PNG, App.Current.GetType().Assembly).ConfigureAwait(true);
        SetTitle(res.ResourceValue);
        _parent.PageData.Resources.FirstOrDefault(x => x.ResourceKey == res.ResourceKey).InfoValue = res.InfoValue
                    .Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_ZERO, result)
                    .Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_ONE, _parentPage.GetResourceValueByKey(_devicesData.Device.DeviceIdentifier));
        _parent.PageData.Resources.FirstOrDefault(x => x.ResourceKey == res.ResourceKey).ResourceValue = string.Empty;
        _parent.PageData.Resources.FirstOrDefault(x => x.ResourceKey == res.ResourceKey).PlaceHolderValue = string.Empty;
        _devicePairingView.PageResources = _parent.PageData;
        if (MobileConstants.IsTablet)
        {
            //_leftHeaderButton.IsVisible = true;
            //_leftHeaderButton.Text = string.Empty;
            //OnLeftHeaderClickedEvent -= OnPopupClose;
            //_rightHeaderButton.Text = _parent.GetResourceValueByKey(ResourceConstants.R_OK_ACTION_KEY);
        }
        else
        {
            ShowCloseButton(false);
            _devicePairingView.Actions = new[] {
                    new OptionModel { GroupName = ButtonType.PrimaryWithMargin.ToString(), OptionText = ResourceConstants.R_OK_ACTION_KEY, SequenceNo= 1 }
            };
        }
    }
}
