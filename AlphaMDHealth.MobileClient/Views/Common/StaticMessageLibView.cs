using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Static message view
/// </summary>
public class StaticMessageLibView : ViewManager
{
    private string _appURL;

    /// <summary>
    /// Message view
    /// </summary>
    internal AmhMessageControl _staticMessgeView;

    /// <summary>
    /// Callback action event 
    /// </summary>
    public EventHandler<EventArgs> RedirectOnTargetPage { get; set; }

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

    /// <summary>
    /// Add ScrollView as Parent only used for static pages
    /// </summary>
    public bool AddScrollView { get; set; } = true;

    /// <summary>
    /// To pass Education ID as parameter 
    /// </summary>
    public string EducationID { get; set; }

    /// <summary>
    /// Type of message data source to fetch data from it
    /// </summary>
    public PageType MessageType { get; set; } = PageType.Default;

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    public StaticMessageLibView(BasePage page, object parameters) : base(page, parameters)
    {
        if (parameters != null)
        {
           // MapParameters();
        }
        if (page.AddScrollView && App._essentials.GetPreferenceValue(StorageConstants.PR_REMOVE_SCROLL_VIEW_KEY, false))
        {
            AddScrollView = false;
        }
        else
        {
            AddScrollView = page.AddScrollView;
        }
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
                await ParentPage.GetSettingsAsync(GroupConstants.RS_STORE_LINKS_GROUP).ConfigureAwait(true);
                _appURL = ParentPage.GetSettingsValueByKey(GenericMethods.GetPlatformSpecificValue(
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
    }

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public override async Task UnloadUIAsync()
    {
        _staticMessgeView.OnValueChanged -= OnMessgeViewActionClicked;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Button click event to perform some action on it
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">event</param>
    protected virtual async void OnMessgeViewActionClicked(object sender, EventArgs e)
    {     
        switch (Key.ToEnum<ErrorCode>())
        {
            case ErrorCode.NoInternetConnection:
                AppHelper.ShowBusyIndicator = true;
                if (await ParentPage.CheckAndDisplayInternetErrorAsync(false, default).ConfigureAwait(false))
                {
                    RedirectOnTargetPage(sender, e);
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
                    RedirectOnTargetPage(sender, e);
                }
                break;
        }
        //_staticMessgeView.OnValueChanged += OnMessgeViewActionClicked;
    }

    /// <summary>
    /// Map paramters into property
    /// </summary>
    //protected void MapParameters()
    //{
    //    Key = GetParameterValue(nameof(Key));
    //    EducationID = GetParameterValue(nameof(EducationID));
    //    TargetPage = GetParameterValue(nameof(TargetPage));
    //    TargetPageParams = GetParameterValue(nameof(TargetPageParams));
    //    AddScrollView = Convert.ToBoolean(GetParameterValue(nameof(AddScrollView)));
    //    GroupName = GetParameterValue(nameof(GroupName));
    //    MessageType = GetParameterValue(nameof(MessageType)).ToEnum<PageType>();
    //}
}