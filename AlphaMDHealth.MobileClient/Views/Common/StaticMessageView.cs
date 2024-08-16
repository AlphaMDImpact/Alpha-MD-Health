using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class StaticMessageView : ViewManager
{
    private string _appURL;

    internal AmhMessageControl _staticMessgeView;

    /// <summary>
    /// Group from where data needs to fetched
    /// </summary>
    public string GroupName { get; set; } = GroupConstants.RS_COMMON_GROUP;

    /// <summary>
    /// Target page to perform action 
    /// </summary>
    public string TargetPage { get; set; }

    /// <summary>
    /// Target page to perform action 
    /// </summary>
    public string TargetPageParams { get; set; }

    /// <summary>
    /// No of chat conversation to be displayed
    /// </summary>
    public string Key { get; set; }

    ///// <summary>
    ///// To pass Education ID as parameter 
    ///// </summary>
    //public string EducationID { get; set; }

    /// <summary>
    /// Type of message data source to fetch data from it
    /// </summary>
    public PageType MessageType { get; set; } = PageType.Default;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public StaticMessageView(BasePage page, object parameters) : base(page, parameters) 
    {
        ParentPage.PageService = new ContentPageService(App._essentials);
        BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR);
        _staticMessgeView = new AmhMessageControl();
        SetPageContent(_staticMessgeView);
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
        if (string.IsNullOrWhiteSpace(Key))
        {
            Key = ErrorCode.NotFound.ToString();
        }
        await GetPageDataAsync();
        if (!ShellMasterPage.CurrentShell.HasMainPage)
        {
            var resource = ParentPage.PageData.Resources.FirstOrDefault(x => x.ResourceKey == ParentPage.PageData.AddedBy);
            if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                //await ParentPage.OverrideTitleViewAsync(new MenuView(MenuLocation.Header, resource.ResourceValue, true)).ConfigureAwait(true);
            }
            resource.ResourceValue = string.Empty;
        }
        if (MessageType == PageType.ContentPage && ParentPage.PageData.Resources[0].IsWebLink && !MobileConstants.CheckInternet)
        {
            await PopNavigationAsync();
            return;
        }

        _staticMessgeView.PageResources = ParentPage.PageData;
        _staticMessgeView.ResourceKey = Key;
        // _staticMessgeView.BindableControlResourceKey = EducationID;
        switch (Key.ToEnum<ErrorCode>())
        {
            case ErrorCode.NoInternetConnection:
                _staticMessgeView.Actions = new List<ButtonActionModel> {
                    new ButtonActionModel {
                        ButtonResourceKey = ResourceConstants.R_NEXT_ACTION_KEY,
                        ButtonID = Constants.NUMBER_ONE
                    } };
                _staticMessgeView.OnValueChanged += OnMessgeViewActionClicked;
                break;
            case ErrorCode.UpdateApp:
                await ParentPage.PageService.GetSettingsAsync(GroupConstants.RS_STORE_LINKS_GROUP).ConfigureAwait(true);
                _appURL = LibSettings.GetSettingValueByKey(ParentPage.PageService.PageData?.Settings, GenericMethods.GetPlatformSpecificValue(
                    SettingsConstants.S_IOS_APPSTORE_LINK_KEY,
                    SettingsConstants.S_ANDROID_PLAYSTORE_LINK_KEY,
                    SettingsConstants.S_WINDOWS_MARKET_LINK_KEY
                ));
                if (!string.IsNullOrWhiteSpace(_appURL))
                {
                    _staticMessgeView.Actions = new List<ButtonActionModel> {
                    new ButtonActionModel {
                        ButtonResourceKey = ResourceConstants.R_NEXT_ACTION_KEY,
                        ButtonID = Constants.NUMBER_ONE
                    } };
                    _staticMessgeView.OnValueChanged += OnMessgeViewActionClicked;
                }
                break;
            case ErrorCode.UnknownCertificate:
                App._essentials.SetPreferenceValue(StorageConstants.PR_APPLY_CERTIFICATE_KEY, false);
                break;
            default:
                //_staticMessgeView.IsHeightReset = App._essentials.GetPreferenceValue(LibStorageConstants.PR_IS_HEIGHT_RESET_KEY, false);
                break;
        }
        DisplayPageContentButton();
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _staticMessgeView.OnValueChanged -= OnMessgeViewActionClicked;
        await Task.CompletedTask;
        ////    App._essentials.SetPreferenceValue(LibStorageConstants.PR_IS_HEIGHT_RESET_KEY, true);
    }

    protected async void RedirectOnTarget(object sender, EventArgs e)
    {
        if (MessageType == PageType.ConsentPage)
        {
             //await ConsentNavigationAsync(e).ConfigureAwait(false);
        }
        else if (MessageType == PageType.ContentPage && !string.IsNullOrWhiteSpace(TargetPageParams))
        {
            await UpdateContentStatusAsync(true);
        }
        else if (!ShellMasterPage.CurrentShell.HasMainPage && ShellMasterPage.CurrentShell.Navigation.ModalStack?.Count > 0)
        {
            await ShellMasterPage.CurrentShell.Navigation.PopModalAsync().ConfigureAwait(true);
        }
        else if (!string.IsNullOrWhiteSpace(TargetPage) && TargetPage == Pages.ResetPasswordPage.ToString())
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new ResetPasswordPage(Pages.ResetPasswordPage)).ConfigureAwait(false);
        }
        else
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new InitializationPage()).ConfigureAwait(false);
        }
    }

    private async Task GetPageDataAsync()
    {
        if (MessageType == PageType.ContentPage || MessageType == PageType.ConsentPage)
        {
            ParentPage.PageData.AddedBy = Key;
            ParentPage.PageData.LastModifiedBy = PageType.ContentPage.ToString();
            ParentPage.PageData.ErrorDescription = GroupName;
            ParentPage.PageData.IsActive = ShellMasterPage.CurrentShell.HasMainPage;
            await new ContentPageService(App._essentials).GetContentDetailsAsync(ParentPage.PageData).ConfigureAwait(true);
            Key = ParentPage.PageData.AddedBy;
        }
        else
        {
            await ParentPage.PageService.GetResourcesAsync(GroupName).ConfigureAwait(false);
            ParentPage.PageData = ParentPage.PageService.PageData;
        }
        if (ParentPage.PageData.Resources == null || ParentPage.PageData.Resources.Count < 1
            || !ParentPage.PageData.Resources.Any(x => x.ResourceKey == Key))
        {
            new BaseService(App._essentials).GetDefaultText(ParentPage.PageData, Key
                , (byte)App._essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 1));
        }
    }

    private async Task PopNavigationAsync()
    {
        if (MobileConstants.IsTablet)
        {
            AppHelper.ShowBusyIndicator = false;
            InvokeListRefresh(ResourceConstants.R_OFFLINE_OPERATION_KEY, null);
        }
        else
        {
            await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_OFFLINE_OPERATION_KEY, OnPopupActionClicked, false, true, false).ConfigureAwait(true);
        }
    }

    private async void OnPopupActionClicked(object sender, int e)
    {
        ParentPage.OnClosePupupAction(sender, e);
        await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
    }

    private void DisplayPageContentButton()
    {
        switch (MessageType)
        {
            case PageType.ConsentPage:
                DisplayPageContent(Convert.ToInt32(TargetPageParams, CultureInfo.InvariantCulture) == 2
                    ? ResourceConstants.R_DECLINE_KEY
                    : ResourceConstants.R_ACCEPT_KEY);
                break;
            case PageType.ContentPage:
                DisplayPageContent(string.IsNullOrWhiteSpace(TargetPageParams)
                    ? default
                    : ResourceConstants.R_READ_BUTTON_TEXT_KEY);
                _ = UpdateContentStatusAsync(false);
                break;
            default:
                // For future implementation
                break;
        }
    }

    private void DisplayPageContent(string buttonResourceKey)
    {
        if (!string.IsNullOrWhiteSpace(buttonResourceKey))
        {
            _staticMessgeView.Actions = new List<ButtonActionModel> {
                new ButtonActionModel
                {
                    ButtonResourceKey = buttonResourceKey,
                    ButtonID = Constants.NUMBER_ONE
                }
           };
            _staticMessgeView.OnValueChanged += OnMessgeViewActionClicked;
        }
    }

    private async Task UpdateContentStatusAsync(bool isCompleted)
    {
        if (!string.IsNullOrWhiteSpace(TargetPageParams))
        {
            var parameters = TargetPageParams.Split(Constants.SYMBOL_PARAM_SEPERATOR);
            if (parameters?.Length > 4 && (parameters[4] == ResourceConstants.R_NEW_STATUS_KEY
                || (isCompleted && parameters[4] == ResourceConstants.R_INPROGRESS_STATUS_KEY)))
            {
                await UpdateEducationTaskStausAsync(isCompleted);
            }
            else if (parameters?.Length > 5)
            {
                await UpdateEducationStatusAsync(parameters[2], parameters[4]
                    , isCompleted ? PatientEducationStatus.Completed : PatientEducationStatus.InProgress);
            }
        }
    }

    private async Task UpdateEducationStatusAsync(string IDs, string existingStatus, PatientEducationStatus status)
    {
        AppHelper.ShowBusyIndicator = true;
        ContentPageDTO userEducation = new ContentPageDTO
        {
            ErrCode = ErrorCode.OK
        };
        if (existingStatus == PatientEducationStatus.Completed.ToString())
        {
            HideCompleteButton();
        }
        else if (existingStatus != status.ToString())
        {
            userEducation.PatientEducations = (from educationID in IDs.Split(Constants.COMMA_SEPARATOR)
                                               select new PatientEducationModel
                                               {
                                                   PatientEducationID = Convert.ToInt64(educationID),
                                                   Status = status,
                                                   IsSynced = false
                                               }).ToList();
            await new ContentPageService(App._essentials).SaveEducationStatusAsync(userEducation).ConfigureAwait(true);
        }
        if (userEducation.ErrCode == ErrorCode.OK)
        {
            await ParentPage.SyncDataWithServerAsync(Pages.PatientEducationsPage, false, default).ConfigureAwait(true);
            if (status == PatientEducationStatus.Completed)
            {
                if (MobileConstants.IsTablet)
                {
                    HideCompleteButton();
                    InvokeListRefresh(userEducation.ErrCode.ToString(), new EventArgs());
                }
                else
                {
                    await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
                }
            }
        }
    }

    private void HideCompleteButton()
    {
        _staticMessgeView.OnValueChanged -= OnMessgeViewActionClicked;
        _staticMessgeView.Actions = null;
    }

    private async Task UpdateEducationTaskStausAsync(bool isCompleted)
    {
        AppHelper.ShowBusyIndicator = true;
        ErrorCode result = await new PatientTaskService(App._essentials).UpdateTaskStatusAsync(
        Convert.ToInt64(TargetPageParams?.Split(Constants.SYMBOL_PARAM_SEPERATOR)[2], CultureInfo.InvariantCulture),
            isCompleted
                ? ResourceConstants.R_COMPLETED_STATUS_KEY
                : ResourceConstants.R_INPROGRESS_STATUS_KEY
        ).ConfigureAwait(true);
        if (result == ErrorCode.OK)
        {
            await ParentPage.SyncDataWithServerAsync(Pages.EducationPreviewPage, false, default).ConfigureAwait(true);
            if (isCompleted)
            {
                if (MobileConstants.IsTablet)
                {
                    InvokeListRefresh(result.ToString(), new EventArgs());
                }
                else
                {
                    await ShellMasterPage.CurrentShell.Navigation.PopAsync(false).ConfigureAwait(true);
                }
            }
        }
        else
        {
            if (isCompleted)
            {
                if (MobileConstants.IsTablet)
                {
                    InvokeListRefresh(result, null);
                }
                else
                {
                    ParentPage.DisplayOperationStatus(LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, result.ToString()));
                }
            }
        }
        AppHelper.ShowBusyIndicator = false;
    }

    private async Task ConsentNavigationAsync(int e)
    {
        AppHelper.ShowBusyIndicator = true;
        if (e != 0)
        {
            var consentData = new ConsentDTO
            {
                Consents = new List<ConsentModel>
                    {
                        new ConsentModel
                        {
                            PageID = Convert.ToInt64(Key, CultureInfo.InvariantCulture),
                            IsAccepted = Convert.ToInt32(TargetPageParams, CultureInfo.InvariantCulture) != 2,
                            AcceptedOn = GenericMethods.GetUtcDateTime,
                            IsSynced = false
                        }
                    }
            };
            await new ConsentService(App._essentials).SaveConsentAsync(consentData).ConfigureAwait(true);
            if (consentData.ErrCode != ErrorCode.OK)
            {
                ParentPage.DisplayOperationStatus(LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, ErrorCode.ErrorWhileSavingRecords.ToString()));
                AppHelper.ShowBusyIndicator = false;
                return;
            }
        }
        if (ShellMasterPage.CurrentShell.HasMainPage)
        {
            // await ShellMasterPage.CurrentShell.PushMainPageAsync(new UserConsentsPage(true)).ConfigureAwait(false);
        }
        else
        {
            await ParentPage.PopPageAsync(false).ConfigureAwait(true);
        }
    }

    private void MapParameters()
    {
        Key = GetParameterValue(nameof(Key));
       // EducationID = GetParameterValue(nameof(EducationID));
        TargetPage = GetParameterValue(nameof(TargetPage));
        TargetPageParams = GetParameterValue(nameof(TargetPageParams));
       // AddScrollView = Convert.ToBoolean(GetParameterValue(nameof(AddScrollView)));
        GroupName = GetParameterValue(nameof(GroupName));
        MessageType = GetParameterValue(nameof(MessageType)).ToEnum<PageType>();
    }

    protected virtual async void OnMessgeViewActionClicked(object sender, EventArgs e)
    {
        switch (Key.ToEnum<ErrorCode>())
        {
            case ErrorCode.NoInternetConnection:
                AppHelper.ShowBusyIndicator = true;
                if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, default).ConfigureAwait(false))
                {
                    RedirectOnTarget(sender,e);
                }
                else
                {
                    AppHelper.ShowBusyIndicator = false;
                }
                break;
            case ErrorCode.UpdateApp:
                MainThread.BeginInvokeOnMainThread(() => { Launcher.OpenAsync(new Uri(_appURL)); });
                break;
            default:
                ////add default case
                if ((MessageType == PageType.ConsentPage || MessageType == PageType.ContentPage) && !string.IsNullOrWhiteSpace(TargetPageParams))
                {
                    RedirectOnTarget(sender, e);
                }
                break;
        }
        //_staticMessgeView.OnValueChanged += OnMessgeViewActionClicked;
    }
}