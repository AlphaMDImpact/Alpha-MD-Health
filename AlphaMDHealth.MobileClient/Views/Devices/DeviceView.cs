using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Utils.Filtering;

namespace AlphaMDHealth.MobileClient;

public class DeviceView : ViewManager
{
    private readonly PatientDeviceDTO _deviceData = new PatientDeviceDTO { Device = new PatientDeviceModel() };
    private readonly CustomWebView _staticContentWebview;
    private readonly CustomButtonControl _removeDevice;
    private readonly CustomButtonControl _retreiveDataButton;

    public DeviceView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new DeviceService(App._essentials);
        _staticContentWebview = new CustomWebView
        {
            WidthRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0),
            HeightRequest = 100,
            IsAutoIncreaseHeight = true,
        };
        _retreiveDataButton = new CustomButtonControl(ButtonType.PrimaryWithMargin);
        _removeDevice = new CustomButtonControl(ButtonType.DeleteWithMargin);
        Grid mainGrid = new Grid
        {
            Style = (MobileConstants.IsTablet && ShellMasterPage.CurrentShell.CurrentPage is DevicesPage)
            ? (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE] : (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) },
            }
        };
        _retreiveDataButton = new CustomButtonControl(ButtonType.PrimaryWithMargin);
        _removeDevice = new CustomButtonControl(ButtonType.DeleteWithMargin);
        mainGrid.Add(_staticContentWebview, 0, 0);
        mainGrid.Add(_retreiveDataButton, 0, 1);
        mainGrid.Add(_removeDevice, 0, 2);
        ParentPage.PageLayout.Add(mainGrid, 0, 0);
        SetPageContent(ParentPage.PageLayout);
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        string readingSourceID = GenericMethods.MapValueType<string>(GetParameterValue(nameof(DevicePage.ReadingSourceID)));
        _deviceData.Device.ReadingSourceID = string.IsNullOrWhiteSpace(readingSourceID) ? Guid.Empty : new Guid(readingSourceID);
        _deviceData.Device.DeviceIdentifier = GenericMethods.MapValueType<string>(GetParameterValue(nameof(DevicePage.DeviceType)));
        if (_deviceData.Device.ReadingSourceID == Guid.Empty)
        {
            await ParentPage.DisplayMessagePopupAsync(ErrorCode.ErrorWhileSavingRecords.ToString(), false, false, false).ConfigureAwait(true);
            await ParentPage.PopPageAsyncForPhoneAsync().ConfigureAwait(false);
            return;
        }
        await Task.WhenAll(
            ParentPage.GetResourcesAsync(GroupConstants.RS_SUPPORTED_DEVICE_GROUP, GroupConstants.RS_DEVICE_GROUP),
             ParentPage.GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
            (ParentPage.PageService as DeviceService).GetPairedDeviceAsync(_deviceData)
        ).ConfigureAwait(true);
        if (_deviceData.ErrCode == ErrorCode.OK)
        {
            _retreiveDataButton.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_RETRIEVE_DATA_BUTTON_TEXT_KEY);
            _retreiveDataButton.IsVisible = _deviceData.Device.IsActive;
            _removeDevice.Text = ParentPage.GetResourceValueByKey(ResourceConstants.R_REMOVE_TEXT_KEY);
            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                MenuView titleView = new MenuView(MenuLocation.Header, ParentPage.GetResourceValueByKey(_deviceData.Device.DeviceIdentifier), false);
                await ParentPage.OverrideTitleViewAsync(titleView).ConfigureAwait(true);
            }
            RefreshDeviceDetails(false);
            _removeDevice.Clicked += OnRemoveDeviceClicked;
            _retreiveDataButton.Clicked += OnRetreiveBtnClicked;
            AppHelper.ShowBusyIndicator = false;
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(_deviceData.ErrCode.ToString(), false, true, false).ConfigureAwait(true);
            await ParentPage.PopPageAsyncForPhoneAsync().ConfigureAwait(false);
        }
    }

    internal void UnLoadUIData()
    {
        _removeDevice.Clicked -= OnRemoveDeviceClicked;
        _retreiveDataButton.Clicked -= OnRetreiveBtnClicked;
    }

    private async void OnRetreiveBtnClicked(object sender, EventArgs e)
    {
        _retreiveDataButton.Clicked += OnRetreiveBtnClicked;
        if (await ParentPage.RequestBluetoothPermissionAsync().ConfigureAwait(true))
        {
            DevicePairingPopupPage popupPage = new DevicePairingPopupPage(_deviceData, ParentPage, true);
            popupPage.OnRefreshButtonClicked += PopupPage_OnRefreshButtonClicked;
            //todo:await Navigation.PushPopupAsync(popupPage).ConfigureAwait(true);
        }
        _retreiveDataButton.Clicked -= OnRetreiveBtnClicked;
    }

    private void PopupPage_OnRefreshButtonClicked(object sender, EventArgs e)
    {
        if ((bool)sender)
        {
            RefreshDeviceDetails(true);
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private void RefreshDeviceDetails(bool isRefreshRequest)
    {
        LibSettings.TryGetDateFormatSettings(ParentPage.PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
        _staticContentWebview.Source = new HtmlWebViewSource
        {
            Html = ParentPage.GetResourceByKey($"{_deviceData.Device.DeviceIdentifier}{Constants.SYMBOL_UNDERSCORE_STRING}{Constants.INSTRUCTION_KEY}").ResourceValue
                       .Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_ZERO, string.Concat(Constants.PNG_IMAGE_CONST_PREFIX, (ParentPage.PageService as DeviceService).GetDeviceIcon(_deviceData.Device.DeviceIdentifier)))
                       .Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_ONE, _deviceData.Device.DeviceFirmwareVersion ?? string.Empty)
                       .Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_TWO, _deviceData.Device.DeviceSerialNo ?? string.Empty)
                       .Replace(Constants.FORMATTED_STRING_PARAMETER_CONSTANT_THREE, GenericMethods.GetDateTimeBasedOnCulture(isRefreshRequest ? _deviceData.PairdDevices[0].LastSyncDateTime : _deviceData.Device.LastSyncDateTime, DateTimeType.Date, dayFormat, monthFormat, yearFormat))
        };
    }

    private async void OnRemoveDeviceClicked(object sender, EventArgs e)
    {
        _removeDevice.Clicked -= OnRemoveDeviceClicked;
        if(await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_DELETE_ACTION_KEY, true, true, false).ConfigureAwait(true))
        {
            await RemoveDeviceAsync().ConfigureAwait(true);
        }
        ParentPage.MessagePopup.IsVisible = false;
        _removeDevice.Clicked += OnRemoveDeviceClicked;
    }

    private async Task RemoveDeviceAsync()
    {
        AppHelper.ShowBusyIndicator = true;
        await (ParentPage.PageService as DeviceService).RemoveDeviceAsync(_deviceData).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
        if (_deviceData.ErrCode == ErrorCode.OK)
        {
            await ParentPage.SyncDataWithServerAsync(Pages.DevicesPage, false, 0).ConfigureAwait(true);
            if (MobileConstants.IsDevicePhone)
            {
                await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
            }
            else
            {
                InvokeListRefresh(true, new EventArgs());
                //todo:
                //if (PopupNavigation.Instance.PopupStack?.Count > 0)
                //{
                //    //todo:await Navigation.PopPopupAsync().ConfigureAwait(true);
                //}
            }
        }
        else
        {
            ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ErrorCode.ErrorWhileDeletingRecords.ToString()));
        }
    }

    public override Task UnloadUIAsync()
    {
        UnLoadUIData();
        return Task.CompletedTask;
    }
}