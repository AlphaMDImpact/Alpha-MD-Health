using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Maui.Core.Internal;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class UserAccountSettingsView : ViewManager
{
    private readonly UserAccountSettingDTO _userSettings;
    private readonly Switch _notificationSwitch;
    private readonly Switch _healthKitSwitch;
    private readonly CustomLabelControl _notificationHeader;
    private readonly CustomLabelControl _notificationLabel;
    private readonly CustomLabelControl _healthHeader;
    private readonly CustomLabelControl _healthLabel;
    private readonly CustomLabelControl _measurementHeader;
    private readonly CollectionView _measurementCollection;
    private readonly bool _isPatientLogin;

    public UserAccountSettingsView(BasePage page, object param) : base(page, param)
    {
        _userSettings = new UserAccountSettingDTO();
        var padding = Convert.ToDouble(App.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        ParentPage.PageLayout.Margin = new Thickness(padding, 0);
        ParentPage.PageService = new UserAccountSettingService(App._essentials);
        _notificationSwitch = new Switch
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_TOGGLE_KEY]
        };
        _healthKitSwitch = new Switch
        {
            Style = (Style)App.Current.Resources[StyleConstants.ST_DEFAULT_TOGGLE_KEY]
        };
        _notificationHeader = GetHeaderLabel(new Thickness(0));
        _notificationLabel = GetContentLabel();
        _healthHeader = GetHeaderLabel(new Thickness(0, padding, 0, 0));
        _healthLabel = GetContentLabel();
        _measurementHeader = GetHeaderLabel(new Thickness(0, padding));
        _measurementCollection = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                SnapPointsType = SnapPointsType.None,
                SnapPointsAlignment = SnapPointsAlignment.Center,
                ItemSpacing = padding
            },
            ItemTemplate = new DataTemplate(() =>
            {
                return new UserAccountSettingViewCell(this);
            }),
            IsVisible = false
        };
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        _isPatientLogin = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 8, true);
        if (_isPatientLogin && MobileConstants.IsDevicePhone)
        {
            ParentPage.AddRowColumnDefinition(GridLength.Star, 1, false);
            ParentPage.PageLayout.Add(_notificationHeader, 0, 0);
            ParentPage.PageLayout.Add(_notificationLabel, 0, 1);
            ParentPage.PageLayout.Add(_notificationSwitch, 1, 1);
            ParentPage.PageLayout.Add(_healthHeader, 0, 3);
            ParentPage.PageLayout.Add(_healthLabel, 0, 4);
            ParentPage.PageLayout.Add(_healthKitSwitch, 1, 4);
            ParentPage.PageLayout.Add(_measurementHeader, 0, 6);
            ParentPage.PageLayout.Add(_measurementCollection, 0, 7);
            Grid.SetColumnSpan(_measurementCollection, 2);
        }
        else
        {
            ParentPage.AddRowColumnDefinition(GridLength.Star, 1, false);
            ParentPage.PageLayout.Add(_notificationHeader, 0, 0);
            ParentPage.PageLayout.Add(_notificationLabel, 0, 1);
            ParentPage.PageLayout.Add(_notificationSwitch, 1, 1);
            ParentPage.PageLayout.Add(_measurementHeader, 0, 3);
            ParentPage.PageLayout.Add(_measurementCollection, 0, 4);
        }
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        await (ParentPage.PageService as UserAccountSettingService).GetUserAccountSettingsAsync(_userSettings).ConfigureAwait(true);
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_userSettings.ErrCode == ErrorCode.OK)
        {
            LoadUI();
            if (!isRefreshRequest)
            {
                _notificationSwitch.Toggled += NotificationSwitch_Toggled;
                _healthKitSwitch.Toggled += HealthKitSwitch_Toggled;
            }
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(_userSettings.ErrCode.ToString(), OnClosePopoUp, false, true, false).ConfigureAwait(false);
        }
    }

    public override async Task UnloadUIAsync()
    {
        UnAssignEvents();
        await Task.CompletedTask;
    }

    private void LoadUI()
    {
        if (_userSettings.UserAccountSettings.Any(x => x.SettingType == UserSettingType.NotificationKey))
        {
            _notificationHeader.IsVisible = true;
            _notificationHeader.Text = ParentPage.GetResourceValueByKey(UserSettingType.NotificationKey.ToString());
            _notificationLabel.Text = ParentPage.GetResourceValueByKey(ResourceConstants.PUSH_NOTIFICATION_KEY);
            _notificationSwitch.IsToggled = _userSettings.UserAccountSettings.Find(x => x.SettingType == UserSettingType.NotificationKey).IsToogled;
        }
        if (_userSettings.UserAccountSettings.Any(x => x.SettingType == UserSettingType.HealthDataKey))
        {
            _healthHeader.IsVisible = true;
            _healthHeader.Text = ParentPage.GetResourceValueByKey(UserSettingType.HealthDataKey.ToString());
            _healthLabel.Text = ParentPage.GetResourceValueByKey(GenericMethods.GetPlatformSpecificValue(ResourceConstants.R_IHEALTH_TEXT_KEY, ResourceConstants.R_GOOGLE_FIT_TEXT_KEY, string.Empty));
            _healthKitSwitch.IsToggled = _userSettings.UserAccountSettings.Find(x => x.SettingType == UserSettingType.HealthDataKey).IsToogled;
        }
        if (_userSettings.UserAccountSettings.Any(x => x.SettingType == UserSettingType.MeasurementUnitsKey))
        {
            _measurementCollection.ItemsSource = _userSettings.UserAccountSettings.Where(x => x.SettingType == UserSettingType.MeasurementUnitsKey);
            _measurementCollection.HeightRequest = Constants.WIDTH_REQUEST_CONSTANT * _userSettings.UserAccountSettings.Count(x => x.SettingType == UserSettingType.MeasurementUnitsKey);
            _measurementHeader.IsVisible = true;
            _measurementCollection.IsVisible = true;
            _measurementHeader.Text = ParentPage.GetResourceValueByKey(UserSettingType.MeasurementUnitsKey.ToString());
        }
    }

    public void Picker_SelectedValuesChanged(long readingTypeID, string selectedItem)
    {
        var selectedID = _userSettings.ReadingUnitOptions.FirstOrDefault(x => x.OptionText == Convert.ToString(selectedItem, CultureInfo.InvariantCulture)).OptionID;
        _userSettings.ReadingUnitOptions.Where(x => x.ParentOptionID == readingTypeID).ForEach(x => x.IsSelected = x.OptionID == selectedID);
        _userSettings.UserAccountSettings.Where(x => x.ReadingTypeID == readingTypeID).ForEach(x =>
        {
            x.SettingValue = selectedID.ToString(CultureInfo.InvariantCulture);
            x.IsSynced = false;
        });
    }

    private CustomLabelControl GetContentLabel()
    {
        return new CustomLabelControl(LabelType.SecondrySmallLeft) { VerticalOptions = LayoutOptions.CenterAndExpand };
    }

    private CustomLabelControl GetHeaderLabel(Thickness margin)
    {
        return new CustomLabelControl(LabelType.HeaderPrimaryMediumBoldLeftWithoutPadding)
        {
            Margin = margin,
            BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR),
            IsVisible = false,

        };
    }

    private void UnAssignEvents()
    {
        _notificationSwitch.Toggled -= NotificationSwitch_Toggled;
        _healthKitSwitch.Toggled -= HealthKitSwitch_Toggled;
    }

    private async void HealthKitSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        _healthKitSwitch.Toggled -= HealthKitSwitch_Toggled;
        if (_healthKitSwitch.IsToggled)
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new HealthAccountConnectPage(false)).ConfigureAwait(false);
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_TOGGLE_CONFIRMATION_POPUP_TEXT_KEY),
                ParentPage.GetResourceValueByKey(GenericMethods.GetPlatformSpecificValue(ResourceConstants.R_IHEALTH_TEXT_KEY, ResourceConstants.R_GOOGLE_FIT_TEXT_KEY, string.Empty))),
                OnHealthToggleActionClicked, true, true, true).ConfigureAwait(true);
        }
    }

    private void OnHealthToggleActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                ParentPage.OnClosePupupAction(sender, e);
                _userSettings.UserAccountSettings.Where(x => x.SettingType == UserSettingType.HealthDataKey).ForEach(x =>
                {
                    x.SettingValue = _healthKitSwitch.IsToggled.ToString(CultureInfo.InvariantCulture);
                    x.IsSynced = false;
                });
                break;
            case 2:
                ParentPage.OnClosePupupAction(sender, e);
                _healthKitSwitch.IsToggled = true;
                break;
            default:// to do
                break;
        }
        _healthKitSwitch.Toggled += HealthKitSwitch_Toggled;

    }
    private async void NotificationSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (_notificationSwitch.IsToggled)
        {
            UpdateNotificationSwitchStatus();
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(string.Format(CultureInfo.InvariantCulture, ParentPage.GetResourceValueByKey(ResourceConstants.R_TOGGLE_CONFIRMATION_POPUP_TEXT_KEY), ParentPage.GetResourceValueByKey(ResourceConstants.R_NOTIFICATION_KEY)), OnNotificationToggleActionClicked, true, true, true).ConfigureAwait(true);
        }

    }

    private void OnNotificationToggleActionClicked(object sender, int e)
    {
        switch (e)
        {
            case 1:
                ParentPage.OnClosePupupAction(sender, e);
                UpdateNotificationSwitchStatus();
                break;
            case 2:
                ParentPage.OnClosePupupAction(sender, e);
                _notificationSwitch.IsToggled = true;
                break;
            default:// to do
                break;
        }
    }

    private void UpdateNotificationSwitchStatus()
    {
        _userSettings.UserAccountSettings.Where(x => x.SettingType == UserSettingType.NotificationKey).ForEach(x =>
        {
            x.SettingValue = _notificationSwitch.IsToggled.ToString(CultureInfo.InvariantCulture);
            x.IsSynced = false;
        });
    }

    private async void OnClosePopoUp(object sender, int e)
    {
        //todo:await Navigation.PopAllPopupAsync().ConfigureAwait(true);
        if (MobileConstants.IsTablet)
        {
            InvokeListRefresh(ErrorCode.NotFound.ToString(), new CustomEventArgs());
        }
        else
        {
            await ParentPage.PopPageAsync(true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Validate and Save Data
    /// </summary>
    /// <returns>Result of operations</returns>
    internal async Task OnSaveButtonClickedAsync()
    {
        if (ParentPage.IsFormValid() && await ParentPage.CheckAndDisplayInternetErrorAsync(false))
        {
            AppHelper.ShowBusyIndicator = true;
            await (ParentPage.PageService as UserAccountSettingService).SaveUserAccountSettingsAsync(_userSettings).ConfigureAwait(true);
            if (_userSettings.ErrCode == ErrorCode.OK)
            {
                var notificationSetting = _userSettings.UserAccountSettings.FirstOrDefault(X => X.SettingType == UserSettingType.NotificationKey);
                if (notificationSetting != null)
                {
                    bool isNotificationEnabled = Convert.ToBoolean(notificationSetting.SettingValue, CultureInfo.InvariantCulture);
                    App._essentials.SetPreferenceValue(StorageConstants.PR_IS_NOTIFICATIONS_ALLOWED_KEY, isNotificationEnabled);
                    //Unregister from notification when push notification is disabled
                    await (ParentPage.PageService as UserAccountSettingService).SetupRemoteNotificationAsync(isNotificationEnabled).ConfigureAwait(true);
                }
                var healthDataSetting = _userSettings.UserAccountSettings.FirstOrDefault(X => X.SettingType == UserSettingType.HealthDataKey);
                if (healthDataSetting != null)
                {
                    App._essentials.SetPreferenceValue(StorageConstants.PR_IS_HEALTH_ACCOUNT_CONNECTED_KEY, Convert.ToBoolean(healthDataSetting.SettingValue, CultureInfo.InvariantCulture));
                }
                _ = ParentPage.SyncDataWithServerAsync(Pages.UserAccountSettingsPage, ServiceSyncGroups.RSSyncToServerGroup, DataSyncFor.UserAccountSettings, DataSyncFor.UserAccountSettings.ToString(), App._essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)).ConfigureAwait(true);
                await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(true);
            }
            else
            {
                await ParentPage.DisplayMessagePopupAsync(_userSettings.ErrCode.ToString(), ParentPage.OnClosePupupAction, false, true, false).ConfigureAwait(false);
            }
            AppHelper.ShowBusyIndicator = false;
        }
    }
}