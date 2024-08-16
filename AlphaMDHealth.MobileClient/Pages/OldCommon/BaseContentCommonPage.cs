using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System.Globalization;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Base page to use common pages logic
/// </summary>
public partial class BasePage
{
    /// <summary>
    /// call when timer is running
    /// </summary>
    public event EventHandler<EventArgs> OnTimerChanging;

    /// <summary>
    /// timer stop event
    /// </summary>
    public event EventHandler<EventArgs> OnTimerStop;

    /// <summary>
    /// Callback action event 
    /// </summary>
    public EventHandler<int> OnActionClicked { get; set; }

    /// <summary>
    /// Add ScrollView as Parent only used for static pages
    /// </summary>
    public bool AddScrollView { get; set; } = true;

    #region Page Settings

    /// <summary>
    /// Gets settings for the given group
    /// </summary>
    /// <param name="groups">list of groups</param>
    public async Task GetSettingsAsync(params string[] groups)
    {
        await (PageService as BaseService).GetSettingsAsync(groups).ConfigureAwait(false);
        PageData.Settings = PageService.PageData?.Settings;
    }

    /// <summary>
    /// Finds a particular setting based on key from settingData.
    /// </summary>
    /// <param name="settingKey">key for which setting to be fetched.</param>
    /// <returns>setting for specified setting key.</returns>
    public SettingModel GetSettingByKey(string settingKey)
    {
        return LibSettings.GetSettingByKey(PageData?.Settings, settingKey);
    }

    /// <summary>
    /// Finds a particular setting based on key from settingData.
    /// </summary>
    /// <param name="settingKey">key for which setting to be fetched.</param>
    /// <returns>setting value for specified setting key.</returns>
    public string GetSettingsValueByKey(string settingKey)
    {
        return LibSettings.GetSettingValueByKey(PageData?.Settings, settingKey);
    }

    /// <summary>
    /// Finds a particular setting based on key from settingData.
    /// </summary>
    /// <param name="settingKey">key for which setting to be fetched.</param>
    /// <returns>setting value for specified setting key.</returns>
    public async Task<string> GetSettingsValueByKeyAsync(string settingKey)
    {
        return await new SettingService(App._essentials).GetSettingsValueByKeyAsync(settingKey).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks self registration allowed or not
    /// </summary>
    /// <returns></returns>
    public bool IsSelfRegistrationAllowed()
    {
        string enableRegistration = GetSettingsValueByKey(SettingsConstants.S_ENABLE_SELF_REGISTRATION_KEY);
        return !string.IsNullOrWhiteSpace(enableRegistration)
            && Convert.ToBoolean(enableRegistration, CultureInfo.InvariantCulture);
    }

    #endregion

    #region Page Resources

    /// <summary>
    /// Gets resources for the given group
    /// </summary>
    /// <param name="groups">list of groups</param>
    public async Task GetResourcesAsync(params string[] groups)
    {
        await PageService.GetResourcesAsync(groups).ConfigureAwait(false);
        PageData.Resources = PageService.PageData?.Resources;
    }

    /// <summary>
    /// Gets Resource by key
    /// </summary>
    /// <param name="resourceKey">Resource key</param>
    /// <returns>Resource data for the given key</returns>
    public ResourceModel GetResourceByKey(string resourceKey)
    {
        return LibResources.GetResourceByKey(PageData?.Resources, resourceKey);
    }

    /// <summary>
    /// Finds a particular resource based on key from page resources
    /// </summary>
    /// <param name="resourceKey">Resource identifier</param>
    /// <returns>String value of a resource</returns>
    public string GetResourceValueByKey(string resourceKey)
    {
        return LibResources.GetResourceValueByKey(PageData?.Resources, resourceKey);
    }

    /// <summary>
    /// Finds a particular resource based on resource key ID from page resources
    /// </summary>
    /// <param name="resourceKeyID">Resource identifier</param>
    /// <returns>resource data</returns>
    public ResourceModel GetResourceByKeyID(long resourceKeyID)
    {
        return LibResources.GetResourceByKeyID(PageData?.Resources, resourceKeyID);
    }

    /// <summary>
    /// Gets resource group description by group id from available list of resources
    /// </summary>
    /// <param name="groupID">Resource Group IDKey</param>
    /// <returns>Resource group description</returns>
    public string GetResourceGroupDescriptionByGroupID(short groupID)
    {
        return LibResources.GetResourceGroupDescByGroupID(PageData?.Resources, groupID);
    }

    #endregion

    #region Alert Messages

    /// <summary>
    /// Method to display the alert message, created to reduce lines of code
    /// </summary>
    /// <param name="resourceKey">the costant name for a particular error message</param>
    /// <param name="action">action to be invoked on click</param>
    /// <returns>Displays alert </returns>
    public async Task DisplayMessagePopupAsync(string resourceKey, EventHandler<int> action)
    {
        await DisplayMessagePopupAsync(resourceKey, action, false, true, false).ConfigureAwait(false);
    }

    /// <summary>
    /// Method to display the alert message, created to reduce lines of code
    /// </summary>
    /// <param name="resourceKey">the costant name for a particular error message</param>
    /// <param name="action">action to be invoked on click</param> 
    /// <param name="showCancel">Show cancel button</param>
    /// <param name="isGroupCalled">flag to check group resource is called</param>
    /// <param name="isMessage">true if message is actual text, false if message is resource key</param>
    /// <returns>if showCancel is true returns true if user clicks ok else return false</returns>
    public async Task DisplayMessagePopupAsync(string resourceKey, EventHandler<int> action, bool showCancel, bool isGroupCalled, bool isMessage)
    {
        try
        {
            MessagePopup.OnActionClicked -= OnActionClicked;
            if (!isGroupCalled)
            {
                await GetResourcesAsync(GroupConstants.RS_COMMON_GROUP).ConfigureAwait(true);
            }
            ////Set isVisible and showinpopup false to solve hardware backbutton issue
            MessagePopup.ShowInPopup = false;
            MessagePopup.IsVisible = false;
            MessagePopup.MessageType = MessageType.ConfirmationPopup;
            MessagePopup.ControlResourceKey = resourceKey;
            MessagePopup.ShowIcon = false;
            MessagePopup.ShowHeadingOnTop = true;
            MessagePopup.ButtonsWithoutSpacing = true;
            MessagePopup.StyleId = resourceKey;
            MessagePopup.ButtonsWithoutSpacing = true;
            MessagePopup.ShowInfoValueInHtmlLabel = true;
            MessagePopup.PageResources = UpdatePopupData(resourceKey, PageData.Resources, isMessage);
            MessagePopup.IsVisible = true;
            MessagePopup.Actions = showCancel
                ? new[] {
                            new OptionModel { GroupName = ButtonType.TransparentWithBorder.ToString(), OptionText = ResourceConstants.R_OK_ACTION_KEY, SequenceNo= 1 },
                            new OptionModel { GroupName = ButtonType.TransparentWithBorder.ToString(), OptionText = ResourceConstants.R_CANCEL_ACTION_KEY, SequenceNo = 2 }
                    }
                : new[] {
                            new OptionModel { GroupName = ButtonType.TransparentWithBorder.ToString(), OptionText = ResourceConstants.R_OK_ACTION_KEY, SequenceNo= 1 },
                    };
            MessagePopup.ShowInPopup = true;
            OnActionClicked = action ?? OnPupupActionClicked;
            MessagePopup.OnActionClicked += OnActionClicked;
            MessagePopup.ResetLayoutForMessagePopUp(_isLandscape);
        }
        catch (Exception ex)
        {
            LogErrors(ex.Message, ex);
        }
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Method to display the alert message, created to reduce lines of code
    /// </summary>
    /// <param name="resourceKey">the costant name for a particular error message</param>
    /// <param name="showCancel">Show cancel button</param>
    /// <param name="isGroupCalled">flag to check group resource is called</param>
    /// <param name="isMessage">true if message is actual text, false if message is resource key</param>
    /// <returns>if showCancel is true returns true if user clicks ok else return false</returns>
    public async Task<bool> DisplayMessagePopupAsync(string resourceKey, bool showCancel, bool isGroupCalled, bool isMessage)
    {
        var isAccepted = false;
        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
        await DisplayMessagePopupAsync(resourceKey, (sender, e) =>
        {
            isAccepted = e == 1;
            OnPupupActionClicked(sender, e);
            taskCompletionSource.TrySetResult(true);
        }, showCancel, isGroupCalled, isMessage).ConfigureAwait(true);
        await taskCompletionSource.Task.ConfigureAwait(true);
        //Time required to pop popup from display
        await Task.Delay(500).ConfigureAwait(true);
        return isAccepted;
    }

    /// <summary>
    /// Method to display the alert message, created to reduce lines of code
    /// </summary>
    /// <param name="message">the costant name for a particular error message</param>
    /// <param name="isGroupCalled">flag to check group resource is called</param>
    /// <param name="showCancel">Show cancel button</param>
    /// <param name="isMessage">true if message is actual text, false if message is resource key</param>
    /// <returns>if showCancel is true returns true if user clicks ok else return false</returns>
    public async Task<bool> DisplaySystemPopupAsync(string message, bool isGroupCalled, bool showCancel, bool isMessage)
    {
        try
        {
            if (!isGroupCalled)
            {
                await GetResourcesAsync(GroupConstants.RS_COMMON_GROUP).ConfigureAwait(true);
            }
            AppHelper.ShowBusyIndicator = false;
            string titleText = GetResourceValueByKey(ResourceConstants.R_APPLICATION_NAME_KEY);
            string okText = GetResourceValueByKey(ResourceConstants.R_OK_ACTION_KEY);
            message = isMessage ? message : GetResourceValueByKey(message);
            if (showCancel)
            {
                string cancelText = GetResourceValueByKey(ResourceConstants.R_CANCEL_ACTION_KEY);
                if (!string.IsNullOrWhiteSpace(cancelText))
                {
                    return await ShellMasterPage.CurrentShell.DisplayAlert(titleText, message, okText, cancelText).ConfigureAwait(false);
                }
            }
            else
            {
                await ShellMasterPage.CurrentShell.DisplayAlert(titleText, message, okText).ConfigureAwait(false);
                return true;
            }
        }
        catch
        {
            AppHelper.ShowBusyIndicator = false;
        }
        return false;
    }

    /// <summary>
    /// Action to close popup
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnPupupActionClicked(object sender, int e)
    {
        MessagePopup.ShowInPopup = false;
        MessagePopup.IsVisible = false;
    }

    /// <summary>
    /// Action to close popup
    /// </summary>
    public async void OnClosePupupAction(object sender, int e)
    {
        MessagePopup.PopCustomMessageControlAsync();
        MessagePopup.IsVisible = false;
    }

    private BaseDTO UpdatePopupData(string resourceKey, in List<ResourceModel> res, bool isMessage)
    {
        var pageData = new BaseDTO { Resources = res };
        var existingresource = GetResourceByKey(resourceKey);
        if (existingresource != null)
        {
            pageData.Resources.Remove(existingresource);
        }
        var resource = new ResourceModel
        {
            ResourceKey = resourceKey,
            LanguageID = (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 1)
        };
        MapResourceDataInInfo(resourceKey, isMessage, existingresource, resource);
        resource.ResourceValue = (string.IsNullOrWhiteSpace(existingresource?.ResourceValue) || existingresource?.ResourceValue == resource.InfoValue)
            ? GetResourceValueByKey(ResourceConstants.R_APPLICATION_NAME_KEY)
            : existingresource?.ResourceValue;
        pageData.Resources.Add(resource);
        return pageData;
    }

    private void MapResourceDataInInfo(string resourceKey, bool isMessage, ResourceModel existingresource, ResourceModel resource)
    {
        if (!string.IsNullOrWhiteSpace(existingresource?.InfoValue))
        {
            resource.InfoValue = existingresource.InfoValue;
        }
        else if (!string.IsNullOrWhiteSpace(existingresource?.PlaceHolderValue))
        {
            resource.InfoValue = existingresource.PlaceHolderValue;
        }
        else if (isMessage)
        {
            resource.InfoValue = resourceKey;
        }
        else
        {
            resource.InfoValue = existingresource?.ResourceValue;
        }
    }

    #endregion

    #region Common Methods

    /// <summary>
    /// Call service layer to Log errors in local db.
    /// </summary>
    /// <param name="errorLocation">Class and method name combination</param>
    /// <param name="ex">Exception object</param>
    public void LogErrors(string errorLocation, Exception ex)
    {
        PageService.LogError(errorLocation, ex);
    }

    /// <summary>
    /// Adjust footer padding when keyboard is open
    /// </summary>
    /// <param name="keyboardHeight">height of keyboard</param>
    public void AdjustPageForKeyboard(double keyboardHeight)
    {
        Padding = new Thickness(Padding.Left, Padding.Top, Padding.Right, Padding.Bottom + keyboardHeight);
    }

    /// <summary>
    /// Sets top space in case of iOS notch phones, in case of android it is automatically handled 
    /// </summary>
    protected void EnableSafeArea()
    {
        On<iOS>().SetUseSafeArea(true);
    }

    /// <summary>
    /// Map data into system feature parameters
    /// </summary>
    /// <param name="parameters">parameters</param>
    /// <returns>list of feature parameters</returns>
    public List<SystemFeatureParameterModel> AddParameters(params SystemFeatureParameterModel[] parameters)
    {
        var list = new List<SystemFeatureParameterModel>();
        list.AddRange(parameters.ToList());
        return list;
    }

    /// <summary>
    /// Maps parameter name and value into model
    /// </summary>
    /// <param name="name">name of parameter</param>
    /// <param name="value">value of parameter</param>
    /// <returns>Feature parameter model</returns>
    public SystemFeatureParameterModel CreateParameter(string name, string value)
    {
        return new SystemFeatureParameterModel { ParameterName = name, ParameterValue = value };
    }

    /// <summary>
    /// Checks and shows internet error popup
    /// </summary>
    /// <param name="showPopup">true if should show popup else false</param>
    /// <returns>true when connected with internet</returns>
    public Task<bool> CheckAndDisplayInternetErrorAsync(bool showPopup)
    {
        return CheckAndDisplayInternetErrorAsync(showPopup, default);
    }

    /// <summary>
    /// Checks and shows internet error popup
    /// </summary>
    /// <param name="showPopup">true if should show popup else false</param>
    /// <param name="errorKey">error key for message to be displayed</param>
    /// <returns>true when connected with internet</returns>
    public async Task<bool> CheckAndDisplayInternetErrorAsync(bool showPopup, string errorKey)
    {
        if (!MobileConstants.CheckInternet)
        {
            errorKey = errorKey == default ? ErrorCode.NoInternetConnection.ToString() : errorKey;
            if (showPopup)
            {
                await DisplayMessagePopupAsync(GetResourceValueByKey(errorKey), false, true, true).ConfigureAwait(true);
            }
            else
            {
                var message = GetResourceValueByKey(errorKey);
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = errorKey;
                }
                DisplayOperationStatus(message);
            }
            return false;
        }
        return true;
    }

    #endregion

    /// <summary>
    /// common timer for login sms and reset page
    /// </summary>
    /// <param name="seconds"></param>
    public void StartTimer(int seconds)
    {
        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            App._essentials.SetPreferenceValue(StorageConstants.PR_IS_TIMER_STARTED_KEY, true);
            if (seconds >= 1)
            {
                if (MobileConstants.IsIosPlatform && App._essentials.GetPreferenceValue(StorageConstants.PR_IS_TIMER_STARTED_KEY, false) && App._essentials.GetPreferenceValue(StorageConstants.PR_IS_APP_MINIMISE_FOR_TIMER_KEY, false))
                {
                    seconds -= GenericMethods.GetUtcDateTime.Subtract(DateTimeOffset.FromUnixTimeSeconds(App._essentials.GetPreferenceValue(StorageConstants.PR_TIMER_SLEEP_DATE_TIME_KEY, (long)0))).Seconds;
                    App._essentials.SetPreferenceValue(StorageConstants.PR_IS_APP_MINIMISE_FOR_TIMER_KEY, false);
                }
                seconds--;
                OnTimerChanging?.Invoke(seconds, new EventArgs());

            }
            else
            {
                App._essentials.SetPreferenceValue(StorageConstants.PR_TIMER_SLEEP_DATE_TIME_KEY, (long)0);
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_APP_MINIMISE_FOR_TIMER_KEY, false);
                App._essentials.SetPreferenceValue(StorageConstants.PR_IS_TIMER_STARTED_KEY, false);
                OnTimerStop?.Invoke(seconds, new EventArgs());
                return false;
            }
            return true;
        });
    }

    /// <summary>
    /// Check a particular feature permission based on key from page feature
    /// </summary>
    /// <param name="featureCode">feature identifier</param>
    /// <returns>Bool value</returns>
    public bool CheckFeaturePermissionByCode(string featureCode)
    {
        if (GenericMethods.IsListNotEmpty(PageData?.FeaturePermissions))
        {
            return PageData.FeaturePermissions.Any(x => x.FeatureCode.Trim() == featureCode);
        }
        return false;
    }

    /// <summary>
    /// Checks ble permission
    /// </summary>
    /// <returns>true if bluetooth permission is provided else returns false</returns>
    public async Task<bool> RequestBluetoothPermissionAsync()
    {
        if (MobileConstants.IsIosPlatform)
        {
            //todo:
            //var deviceManager = new DeviceLibrary.DeviceManager(AppDevices.Default);
            //ErrorCode bluetoothPermission = await deviceManager.TurnOnMobileBluetoothAsync().ConfigureAwait(true);
            //if (bluetoothPermission == ErrorCode.OK)
            //{
            //    return true;
            //}
            //else
            //{
            //    if (bluetoothPermission == ErrorCode.BluetoothUnauthorized)
            //    {
            //        if (await DisplayMessagePopupAsync(GetResourceValueByKey(LibResourceConstants.R_REQUEST_BLUETOOTH_PERMISSION_TEXT_KEY), true, true, true).ConfigureAwait(true))
            //        {
            //            AppInfo.ShowSettingsUI();
            //        }
            //        else
            //        {
            //            await DisplayMessagePopupAsync(LibResourceConstants.R_DENIED_BLUETOOTH_PERMISSION_TEXT_KEY, false, true, false).ConfigureAwait(true);
            //        }
            //    }
            //    return false;
            //}
        }
        PermissionStatus permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(true);
        if (permissionStatus == PermissionStatus.Granted)
        {
            //todo:
            //ILocationService applicationInfo = DependencyService.Get<ILocationService>();
            //if (applicationInfo.IsLocationOn())
            //{
            //    //var result = await new DeviceLibrary.DeviceManager(AppDevices.Default).TurnOnMobileBluetoothAsync().ConfigureAwait(true);
            //    return result == ErrorCode.OK;
            //}
            //if (await DisplayMessagePopupAsync(GetResourceValueByKey(LibResourceConstants.R_LOCATION_SETTING_ENABLE_TEXT_KEY), true, true, true).ConfigureAwait(true))
            //{
            //    return applicationInfo.TurnOnLocation();
            //}
        }
        else
        {
            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                await DisplayMessagePopupAsync(ResourceConstants.R_LOCATION_PERMISSION_TEXT_KEY, false, true, false).ConfigureAwait(true);
            }
            else
            {
                if (await DisplayMessagePopupAsync(GetResourceValueByKey(ResourceConstants.R_LOCATION_TURN_ON_INSTRUCTION_TEXT_KEY), true, true, true).ConfigureAwait(true))
                {
                    AppInfo.ShowSettingsUI();
                }
            }
        }
        return false;
    }
}