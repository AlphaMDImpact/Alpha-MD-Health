using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Connect Health App page
/// </summary>
public class HealthAccountConnectPage : BasePage
{
    private AmhLabelControl _headerLabel;
    private AmhButtonControl _connectButton;
    private AmhButtonControl _skipButton;
    private bool _isDuringLogin;
    private CustomWebView _staticContentWebview;

    /// <summary>
    /// Is during login
    /// </summary>
    public string IsDuringLogin
    {
        get
        {
            return _isDuringLogin.ToString(CultureInfo.InvariantCulture);
        }
        set => _isDuringLogin = Convert.ToBoolean(Uri.UnescapeDataString(value), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Connect Health App page
    /// </summary>
    public HealthAccountConnectPage() : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        LoadUI();
    }

    /// <summary>
    /// Connect Health App page
    /// </summary>
    /// <param name="isDuringLogin">true if is during login else false</param>
    public HealthAccountConnectPage(bool isDuringLogin) : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        _isDuringLogin = isDuringLogin;
        LoadUI();
    }

    private void LoadUI()
    {
        PageService = new ReadingService(App._essentials);
        _headerLabel = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterBoldLabelControl) { ResourceKey = ResourceConstants.R_HEALTH_ACCOUNT_TEXT_KEY };//(LabelType.DarkTheamBeforeLoginHeader);
        //if (_isDuringLogin)
        //{
            //todo:Content.Effects.Add(new CustomSafeAreaInsetEffect());
        //}
        _staticContentWebview = new CustomWebView
        {
            HeightRequest = 20,
            IsAutoIncreaseHeight = true,
        };
        _connectButton = new AmhButtonControl(FieldTypes.SecondaryButtonControl) { ResourceKey = ResourceConstants.R_CONNECT_BUTTON_TEXT_KEY };//(ButtonType.SecondryWithMargin);
        _skipButton = new AmhButtonControl(FieldTypes.TransparentButtonControl) { ResourceKey = ResourceConstants.R_SKIP_BUTTON_TEXT_KEY };//(ButtonType.TransparentWithMargin);
        HideFooter(true);
        AddRowColumnDefinition(GridLength.Auto, 1, true);
        AddRowColumnDefinition(new GridLength(1, GridUnitType.Star), 1, true);
        AddRowColumnDefinition(GridLength.Auto, 2, true);
        PageLayout.Margin = new Thickness(Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture), 0);
        PageLayout.Add(_headerLabel, 0, 0);
        PageLayout.Add(_staticContentWebview, 0, 1);
        PageLayout.Add(_connectButton, 0, 2);
        PageLayout.Add(_skipButton, 0, 3);
        SetPageContent(false);
    }

    /// <summary>
    /// Called when page appears in UI
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        await Task.WhenAll(
            GetResourcesAsync(GroupConstants.RS_USER_ACCOUNT_SETTINGS_GROUP),
            GetSettingsAsync(GroupConstants.RS_READING_RELATION_GROUP)
        );
        _headerLabel.PageResources = _connectButton.PageResources = _skipButton.PageResources = PageData;
        _staticContentWebview.Source = new HtmlWebViewSource
        {
            Html = Device.RuntimePlatform == Device.Android
            ? GetResourceByKey(ResourceConstants.R_ACCOUNT_ANDROID_HTML_FORMAT_KEY).InfoValue
            : GetResourceByKey(ResourceConstants.R_ACCOUNT_IOS_HTML_FORMAT_KEY).InfoValue
        };
        _connectButton.OnValueChanged += ConnectButtonClicked;
        _skipButton.OnValueChanged += SkipButtonClicked;
        AppHelper.ShowBusyIndicator = false;
    }

    private async void ConnectButtonClicked(object sender, EventArgs e)
    {
        _connectButton.IsControlEnabled = false;
        if (MobileConstants.CheckInternet)
        {
            await CheckHealthKitPermissionAsync().ConfigureAwait(true);
            await NavigateToNextPageAsync().ConfigureAwait(true);
        }
        else
        {
            DisplayOperationStatus(GetResourceValueByKey(ErrorCode.NoInternetConnection.ToString()));
        }
        _connectButton.IsControlEnabled = true;
    }

    private async void SkipButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        _skipButton.IsControlEnabled = false;
        BaseDTO result = new BaseDTO { IsActive = false };
        await new ReadingService(App._essentials).SaveReadingConnectAccountStatusAsync(result).ConfigureAwait(true);
        await NavigateToNextPageAsync().ConfigureAwait(true);
        _skipButton.IsControlEnabled = true;
    }

    private async Task NavigateToNextPageAsync()
    {
        if (_isDuringLogin)
        {
            await NavigateOnNextPageAsync(false, _isDuringLogin, LoginFlow.HealthAccountConnectPage).ConfigureAwait(true);
        }
        else
        {
            await ShellMasterPage.CurrentShell.PopMainPageAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Checks health kit permission
    /// </summary>
    /// <returns>true if bluetooth permission is provided else returns false</returns>
    public async Task<bool> CheckHealthKitPermissionAsync()
    {
        ReadingService readingService = new ReadingService(App._essentials);
        List<ReadingType> observations;
        PatientReadingDTO readings = new PatientReadingDTO();
        await readingService.GetHealthAppReadingsMetaDataAsync(readings).ConfigureAwait(true);
        observations = readings.ChartMetaData.Where(x => x.ReadingParentID > 0).GroupBy(x => x.ReadingParentID).Select(y => (ReadingType)y.First().ReadingParentID).ToList();
        observations.AddRange(readings.ChartMetaData.Where(x => x.ReadingParentID == 0).Select(x => (ReadingType)x.ReadingID).ToList());
        if (Device.RuntimePlatform == Device.iOS)
        {
            observations.AddRange(readings.ChartMetaData.Where(x => x.ReadingParentID == (short)ReadingType.Nutrition).Select(x => (ReadingType)x.ReadingID).ToList());
        }
        readingService.NormalizeReadingTypesForHealthApp(observations);
        BaseDTO result = new BaseDTO { IsActive = true };

        //todo:
        //ErrorCode permissionResult = await new HealthLibrary.HealthLibrary().CheckPermissionAsync(observations, PermissionType.Read).ConfigureAwait(true);
        //if (Device.RuntimePlatform == Device.iOS && observations.All(x =>App._essentials.GetPreferenceValue(x.ToString() + LibStorageConstants.PR_IS_PERMISSION_CALL_COMPLETE_KEY, false)))
        //{
        //    if (await DisplayMessagePopupAsync(string.Format(CultureInfo.InvariantCulture, GetResourceValueByKey(DeviceInfo.Version.Major >= 13
        //        ? LibResourceConstants.R_HEALTH_KIT_PERMISSION_TEXT_KEY
        //        : LibResourceConstants.R_HEALTH_KIT_PERMISSION_IOS_12_TEXT_KEY),
        //        GetResourceValueByKey(LibResourceConstants.R_APPLICATION_NAME_KEY)).Replace(LibConstants.SYMBOL_HASH.ToString(CultureInfo.InvariantCulture), Environment.NewLine), true, true, true).ConfigureAwait(true))
        //    {
        //        await new HealthLibrary.HealthLibrary().OpenPermissionApp().ConfigureAwait(true);
        //        permissionResult = ErrorCode.OK;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        //AppHelper.ShowBusyIndicator = true;
        //ErrorCode updateResult = await readingService.UpdatePermissionRequestStatusAsync(observations);
        //if (permissionResult == ErrorCode.OK && updateResult == ErrorCode.OK)
        //{
        //    await readingService.SaveReadingConnectAccountStatusAsync(result).ConfigureAwait(false);
        //    return result.ErrCode == ErrorCode.OK;
        //}
        AppHelper.ShowBusyIndicator = false;
        return false;
    }
}