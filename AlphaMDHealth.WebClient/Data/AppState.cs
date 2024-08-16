using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Runtime.Serialization;

namespace AlphaMDHealth.WebClient;

public class AppState
{
    private readonly NavigationManager _navigationManager;
    private readonly StorageState _localStorage;
    private readonly ILocalStorageService _localStorageService;

    public byte SelectedLanguageID { get; set; } = Constants.NUMBER_ONE_VALUE;
    public string AppDirection { get; private set; } = Constants.LTR_DIRECTION_CONSTANT;
    public string BasePath { get; set; }
    public int LocalOffset { get; set; }

    public AmhLoader Loader;

    public int TaskCount;
    public string CountryCode { get; set; }

    public bool IsLeftVisible;

    public WebEssentials webEssentials { get; set; }

    /// <summary>
    /// Even invoked when a notification is received
    /// </summary>
    public event EventHandler<SignalRNotificationEventArgs> OnReceiveNotification;

    /// <summary>
    /// Set to true if page is to be shown as before login page
    /// </summary>
    public bool IsBeforeLoginLayout => ShouldShowBeforeLoginView(RouterData?.SelectedRoute?.Page);

    /// <summary>
    /// Master Page Data, contains master resources, user Data, features and menus
    /// </summary>
    public MasterDTO MasterData { get; set; }

    public OrganisationModel OrganizationDetails { get; set; } = new OrganisationModel { };
    public QuestionnaireDTO QuestionnaireDetails { get; set; } = new QuestionnaireDTO();
    public UserDTO UserDetails { get; set; } = new UserDTO();

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// App state
    /// </summary>
    /// <param name="navigationManager">Instance of navigation manager service</param>
    /// <param name="storageState">Instance of storage state service</param>
    /// <param name="localStorageService">Instance of local storage service</param>
    public AppState(NavigationManager navigationManager, StorageState storageState, ILocalStorageService localStorageService, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _navigationManager = navigationManager;
        _localStorage = storageState;
        _localStorageService = localStorageService;
        _httpContextAccessor = httpContextAccessor;
    }

    public void UpdateOrgDetails(string name, string domain, DateTimeOffset addedOn)
    {
        OrganizationDetails ??= new OrganisationModel();
        OrganizationDetails.OrganisationName = name;
        OrganizationDetails.OrganisationDomain = domain;
        OrganizationDetails.AddedON = addedOn;
    }

    public EventCallback<bool> ShowLoader { get; set; }

    /// <summary>
    /// Route Data for handling Routes
    /// </summary>
    public bool IsChanged { get; set; }

    /// <summary>
    /// Route Data for handling Routes
    /// </summary>
    public RouterDataRoot RouterData { get; private set; }

    [DataMember]
    public IList<OrganizationFeaturePermissionModel> Tabs { get; set; }

    public string SelectedTab { get; set; }

    public string SelectedTabTitle { get; set; }

    public bool ShowDetailPage { get; set; }

    #region Style Methods

    /// <summary>
    /// Fill the current App State
    /// </summary>
    /// <param name="masterData">master ui data in which data is fetched</param>
    /// <param name="basePath">base Browser Url</param>
    /// <returns></returns>
    public void RenderAppState(MasterDTO masterData, string basePath)
    {
        if (masterData != null)
        {
            MasterData = masterData;
            BasePath = basePath;
            SelectedLanguageID = MasterData.LanguageID;
            AppDirection = GetDirection(MasterData.Languages?.FirstOrDefault(x => x.LanguageID == MasterData.LanguageID)?.IsRightToLeft == true);
            RouterData = GenerateFeatures();
        }
    }

    /// <summary>
    /// ClassName Handling for RTL and LTR Handling
    /// </summary>
    /// <param name="classes"></param>
    /// <returns></returns>
    public string ReverseClassName(string classes)
    {
        return ClassName(classes, GetDirection(AppDirection == Constants.LTR_DIRECTION_CONSTANT));
    }

    /// <summary>
    /// ClassName Handling for RTL and LTR Handling
    /// </summary>
    /// <param name="classes"></param>
    /// <returns></returns>
    public string ClassName(string classes)
    {
        return string.Concat(classes, " ", AppDirection);
    }

    /// <summary>
    /// Is Patient Login Check
    /// </summary>
    /// <returns></returns>
    public bool IsPatient //(string classes) 
    {
        get
        {
            return MasterData.Users.FirstOrDefault().RoleID == 4;
        }
    }

    /// <summary>
    /// Is Provider Check
    /// </summary>
    /// <returns></returns>
    public bool IsProvider//(string classes)
    {
        get
        {
            return MasterData.Users.FirstOrDefault().RoleID == 5
                || MasterData.Users.FirstOrDefault().RoleID == 6;
        }
    }

    /// <summary>
    /// select classname based on direction
    /// </summary>
    /// <param name="ltrClasses">ltrclass</param>
    /// <param name="rtlClasses">rtlclass</param>
    /// <returns></returns>
    public string ClassName(string ltrClasses, string rtlClasses)
    {
        return AppDirection == Constants.LTR_DIRECTION_CONSTANT ? ltrClasses : rtlClasses;
    }

    private string GetDirection(bool isLtr)
    {
        return isLtr
            ? Constants.RTL_DIRECTION_CONSTANT
            : Constants.LTR_DIRECTION_CONSTANT;
    }

    #endregion

    #region Common Methods

    /// <summary>
    /// Generate Navigation String
    /// </summary>
    /// <param name="route">Page Route for Navigation</param>
    /// <param name="queryParams">Page Input Query Parameters</param>
    /// <returns></returns>
    public string? NavigationString(string route, params string[] queryParams)
    {
        string navigationRoute = RouterData.Routes?.FirstOrDefault(x => x.Page == route)?.Path ?? "/";
        if (navigationRoute == "/" + AppPermissions.DashboardView.ToString() && MasterData.DefaultRoute != null)
        {
            navigationRoute = MasterData.DefaultRoute;
        }
        if (string.IsNullOrWhiteSpace(navigationRoute))
        {
            return null;
        }
        if (queryParams == null || !queryParams.Any())
        {
            return navigationRoute;
        }
        return string.Concat(navigationRoute, Constants.SYMBOL_SLASH, string.Join(Constants.SYMBOL_SLASH, queryParams));
    }

    /// <summary>
    /// Gets the default route
    /// </summary>
    /// <returns>Default route based on organisation</returns>
    public string GetDefaultRoute()
    {
        return (string.IsNullOrWhiteSpace(MasterData?.OrganisationDomain) || MasterData.OrganisationDomain == BasePath)
            ? "/"
            : "/organisation/" + MasterData.OrganisationDomain + "/";
    }

    public string? GetImageInitials(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var nameSbstring = name?.Split(" ");
            nameSbstring = nameSbstring.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var imgString = nameSbstring?[0].Substring(0, 1);
            if (nameSbstring?.Count() > 1)
            {
                imgString += nameSbstring?[1].Substring(0, 1);
            }
            return imgString;
        }
        return null;
    }

    /// <summary>
    /// Invokes SignalR notification
    /// </summary>
    /// <param name="eventArgs">Event args for which the notification is to be invoked</param>
    public void InvokeReceiveNotification(SignalRNotificationEventArgs eventArgs)
    {
        OnReceiveNotification?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Navigate to given uri
    /// </summary>
    /// <param name="path">
    /// The destination URI. This can be absolute, or relative to the base URI 
    /// (as returned by Microsoft.AspNetCore.Components.NavigationManager.BaseUri).
    /// </param>
    /// <param name="forceLoad">
    /// If true, bypasses client-side routing and forces the browser to load the new
    /// page from the server, whether or not the URI would normally be handled by the
    /// client-side router.
    /// </param>
    /// <returns>Navigates to the specified URI</returns>
    public async Task NavigateToAsync(string path, bool forceLoad)
    {
        ShowDetailPage = false;
        if (forceLoad)
        {
            // Save the current local storage values
            await _localStorageService.SetItemAsync(StorageConstants.PR_LOCAL_STORAGE_KEY, _localStorage).ConfigureAwait(true);
        }
        _navigationManager.NavigateTo(path, forceLoad);
    }

    #endregion


    public bool GetTempToken()
    {
        var TempToken = webEssentials?.GetSecureStorageValueAsync(Convert.ToString(nameof(TempSessionModel.TempToken)));
        if (!string.IsNullOrEmpty(TempToken?.Result))
        {
            var userAgent = Convert.ToString(_httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"]);
            return IsMobileBrowser(userAgent);
        }
        return false;
    }

    public bool IsMobileBrowser(string userAgent)
    {
        if (!string.IsNullOrEmpty(userAgent))
        {
            return userAgent.Contains("Mobi") || userAgent.Contains("Android");
        }
        else
        {
            return false;
        }
    }

    private RouterDataRoot GenerateFeatures()
    {
        var routes = new List<RouterData>();
        string defaultRoute = GetDefaultRoute();
        routes.Add(new RouterData { Page = "NavigationComponent", Path = defaultRoute });
        if (defaultRoute != "/")
        {
            routes.Add(new RouterData { Page = "NavigationComponent", Path = "/" });
        }
        routes.Add(new RouterData { FeatureId = -1, Page = AppPermissions.DynamicLinkPage.ToString(), Path = "/" + AppPermissions.DynamicLinkPage.ToString() });
        routes.Add(new RouterData { FeatureId = -2, Page = "ContentPreviewPage", Path = defaultRoute + "ContentPreviewPage" });
        routes.Add(new RouterData { FeatureId = -100, Page = "VideoCall", Path = defaultRoute + "videocall" });
        routes.Add(new RouterData { FeatureId = -3, Page = AppPermissions.StaticMessageView.ToString(), Path = defaultRoute + AppPermissions.StaticMessageView.ToString() });
        routes.Add(new RouterData { FeatureId = -4, Page = AppPermissions.VideoCallingView.ToString(), Path = defaultRoute + AppPermissions.VideoCallingView.ToString() });
        if (MasterData.ErrCode == ErrorCode.OK && MasterData?.OrganisationFeatures != null)
        {
            foreach (var feature in MasterData.OrganisationFeatures)
            {
                if (!string.IsNullOrWhiteSpace(feature?.TargetPage))
                {
                    var aa = routes.FirstOrDefault(x => x.Page == feature.FeatureCode && x.Path == (defaultRoute + feature.FeatureCode));
                    if (aa != null)
                    {
                        aa.FeatureId = feature.FeatureID;
                        aa.FeatureText = feature.FeatureText;
                    }
                    else
                    {
                        routes.Add(new RouterData { Page = feature.FeatureCode, Path = (defaultRoute + feature.FeatureCode), FeatureId = feature.FeatureID, FeatureText = feature.FeatureText });
                    }
                }
            }
        }
        RouterDataRoot routerData = new RouterDataRoot();
        routerData.SetRoutes(routes);
        return routerData;
    }

    /// <summary>
    /// Checks if route is to be shown with before login layout
    /// </summary>
    /// <param name="selectedRoute">The current route</param>
    /// <returns>true if before login layout is to be shown</returns>
    public bool ShouldShowBeforeLoginView(string selectedRoute)
    {
        List<string> beforeLoginViews = new List<string>
        {
            AppPermissions.PinCodeView.ToString(),
            AppPermissions.PincodeLoginView.ToString(),
            AppPermissions.OrganisationSetup.ToString(),
            AppPermissions.DynamicLinkPage.ToString(),
        }; 
        if (MasterData?.HasWelcomeScreens == true)
        {
            beforeLoginViews.Add(AppPermissions.UserWelcomeScreensView.ToString());
        }
        if (MasterData?.IsConsentAccepted == false)
        {
            beforeLoginViews.Add(AppPermissions.UserConsentsView.ToString());
        }
        if (MasterData?.IsSubscriptionRequired == true)
        {
            beforeLoginViews.Add(AppPermissions.SubscriptionPlansView.ToString());
        }
        if (MasterData?.IsProfileCompleted == false)
        {
            beforeLoginViews.Add(AppPermissions.ProfileView.ToString());
        }
        return beforeLoginViews.Any(x => x == selectedRoute);
    }
}