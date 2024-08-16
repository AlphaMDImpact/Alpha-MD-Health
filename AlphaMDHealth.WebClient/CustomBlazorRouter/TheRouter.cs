using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Blazor.Analytics;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;

namespace AlphaMDHealth.WebClient;

/// <summary>
/// Custom route component that displays whichever other component corresponds to the
/// current navigation location.
/// </summary>
public sealed class TheRouter : IComponent, IHandleAfterRender, IDisposable
{
    private static readonly char[] _queryOrHashStartChar = new[] { '?', '#' };
    private static readonly ReadOnlyDictionary<string, object> _emptyParametersDictionary = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
    private RenderHandle _renderHandle;
    private Uri _baseUri;
    private string _locationAbsolute;
    private bool _navigationInterceptionEnabled;
    private ILogger<Router> _logger;

    [Inject]
    private AppState AppState { get; set; }

    [Inject]
    private StorageState LocalStorage { get; set; }

    [Inject]
    private ILocalStorageService LocalStorageService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject] private INavigationInterception NavigationInterception { get; set; }

    [Inject] private ILoggerFactory LoggerFactory { get; set; }

    [Inject] private NavRefreshService NavRefreshService { get; set; }

    [Inject]
    private IAnalytics Analytics { get; set; }

    [Parameter] public RouterDataRoot RouteValues { get; set; }

    [Parameter] public Assembly AppAssembly { get; set; }

    [Parameter] public RenderFragment NotFound { get; set; }

    /// <summary>
    /// Gets or sets the content to display when a match is found for the requested route.
    /// </summary>
    [Parameter] public RenderFragment<RouteData> Found { get; set; }

    /// <summary>
    /// We need it in order to set the current route language parameter
    /// </summary>
    private RouteTable Routes { get; set; }

    public void Attach(RenderHandle renderHandle)
    {
        _logger = LoggerFactory.CreateLogger<Router>();
        _renderHandle = renderHandle;
        _baseUri = new Uri(NavigationManager.BaseUri);
        _locationAbsolute = NavigationManager.Uri;
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        Routes = RouteTableFactory.CreateFromRouterDataRoot(RouteValues);
        Routes = RouteTableFactory.Create(AppAssembly);
        return RefreshAsync(isNavigationIntercepted: false);
    }

    /// <summary>
    /// Dispose 
    /// </summary>
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    private string StringUntilAny(string str, char[] chars)
    {
        int firstIndex = str.IndexOfAny(chars);
        return firstIndex < 0
            ? str
            : str.Substring(0, firstIndex);
    }

    private string AddRouterParams(string[] parsms)
    {
        string route = string.Empty;
        if (parsms == null)
        {
            route = string.Empty;
        }
        else
        {
            foreach (var item in parsms)
            {
                route = string.Concat(route, Constants.SYMBOL_SLASH, item);
            }
        }
        return route;
    }

    private async Task RefreshAsync(bool isNavigationIntercepted)
    {
        try
        {
            string locationPath = NavigationManager.ToBaseRelativePath(_locationAbsolute);
            locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);
            var slashArray = locationPath.Split(Constants.SYMBOL_SLASH);
            var calledFeatureString = string.IsNullOrWhiteSpace(AppState.MasterData?.OrganisationDomain) || AppState.MasterData?.OrganisationDomain == AppState.BasePath
                ? GetPartFromDomain(slashArray, 0)
                : GetPartFromDomain(slashArray, 2);
            var calledFeature = string.IsNullOrWhiteSpace(calledFeatureString) ? "NavigationComponent" : calledFeatureString;
            var featureParams = string.IsNullOrWhiteSpace(AppState.MasterData?.OrganisationDomain) || AppState.MasterData?.OrganisationDomain == AppState.BasePath
                ? slashArray.Skip(1)
                : slashArray.Skip(3);
            var routerData = AppState.RouterData?.Routes?.FirstOrDefault(x => x.Page.ToLower(CultureInfo.InvariantCulture) == calledFeature.ToLower(CultureInfo.InvariantCulture));
            if (routerData == null)
            {
                locationPath = null;
            }
            else
            {
                locationPath = await GetPathAsync(locationPath, calledFeatureString, featureParams, routerData);
                if (Convert.ToBoolean(LibSettings.GetSettingValueByKey(AppState.MasterData?.Settings, SettingsConstants.S_ANALYTICS_ON_OFF_KEY)))
                {
                    _ = Analytics.TrackEvent("page-visit", "web-pages", calledFeatureString).ConfigureAwait(false);
                }
            }
            await RouteNavigationAsync(isNavigationIntercepted, locationPath, routerData, GetRouterContext(locationPath)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
        }
    }

    private async Task<string> GetPathAsync(string locationPath, string calledFeatureString, IEnumerable<string> featureParams, RouterData? routerData)
    {
        if (routerData.Page == AppPermissions.LoginView.ToString())
        {
            await new AuthService(AppState.webEssentials).ClearAccountTokensAndIdAsync().ConfigureAwait(true);
        }
        await LocalStorageService.SetItemAsync(StorageConstants.PR_LOCAL_STORAGE_KEY, LocalStorage).ConfigureAwait(true);
        locationPath = Constants.SYMBOL_SLASH + calledFeatureString + AddRouterParams(featureParams?.ToArray());
        SetupTabs(routerData);
        AppState.RouterData.SelectedRoute = routerData;
        SetMenu(routerData);
        return locationPath;
    }

    private void SetMenu(RouterData? routerData)
    {
        if (NavRefreshService != null && AppState != null && AppState.MasterData != null && AppState.MasterData.Menus != null && AppState.MasterData.Menus.FirstOrDefault(x => x.TargetID == routerData.FeatureId) != null)
        {
            AppState.MasterData.Menus.ForEach(x => x.IsActive = false);
            AppState.MasterData.Menus.FirstOrDefault(x => x.TargetID == routerData.FeatureId).IsActive = true;
            NavRefreshService.CallRequestRefresh();
        }
    }

    private static string GetPartFromDomain(string[] slashArray, int partPosition)
    {
        return slashArray != null && slashArray.Count() > partPosition ? slashArray[partPosition] : "";
    }

    private RouteContext GetRouterContext(string locationPath)
    {
        RouteContext context = null;
        if (locationPath != null)
        {
            context = new RouteContext(locationPath);
            Routes.Route(context);
        }
        return context;
    }


    private void SetupTabs(RouterData routerData)
    {
        var tempToken = AppState.GetTempToken();
        OrganizationFeaturePermissionModel tab = null;
        if (tempToken && routerData.Page == AppPermissions.DashboardView.ToString())
        {
            tab = AppState.MasterData?.FeaturePermissions ?.FirstOrDefault(x => x.FeatureGroupID == 42);
        }
        else
        {
            tab = AppState.MasterData?.FeaturePermissions?.FirstOrDefault(x => x.FeatureID == routerData.FeatureId);
        }
        if (tab != null)
        {
            AppState.Tabs = AppState.MasterData?.FeaturePermissions?.Where(x => x.FeatureGroupID == tab.FeatureGroupID)?.OrderBy(x => x.SequenceNo).ToList();
            if (AppState.Tabs?.Count > 0)
            {
                AppState.SelectedTab = tab.FeatureCode;
                AppState.IsChanged = !AppState.IsChanged;
            }
        }
        else
        {
            AppState.SelectedTab = string.Empty;
            AppState.Tabs = new List<OrganizationFeaturePermissionModel>();
        }
    }

    private async Task RouteNavigationAsync(bool isNavigationIntercepted, string locationPath, RouterData routerData, RouteContext context)
    {
        if (context?.Handler != null)
        {
            if (!typeof(IComponent).IsAssignableFrom(context.Handler))
            {
                throw new InvalidOperationException($"The type {context.Handler.FullName} " +
                    $"does not implement {typeof(IComponent).FullName}.");
            }
            Log.NavigatingToComponent(_logger, context.Handler, locationPath, _baseUri);
            RouteData routeData = new RouteData(
               context.Handler,
               context.Parameters ?? _emptyParametersDictionary, routerData);
            _renderHandle.Render(Found(routeData));
        }
        else
        {
            if (!isNavigationIntercepted)
            {
                Log.DisplayingNotFound(_logger, locationPath, _baseUri);
                // We did not find a Component that matches the route.
                // Only show the NotFound content if the application developer programatically got us here i.e we did not
                // intercept the navigation. In all other cases, force a browser navigation since this could be non-Blazor content.
                _renderHandle.Render(NotFound);
            }
            else
            {
                Log.NavigatingToExternalUri(_logger, new Uri(_locationAbsolute), locationPath, _baseUri);
                await AppState.NavigateToAsync(_locationAbsolute, true).ConfigureAwait(false);
            }
        }
    }

    private async void OnLocationChanged(object sender, LocationChangedEventArgs args)
    {
        _locationAbsolute = args.Location;
        if (_renderHandle.IsInitialized && Routes != null)
        {
            await RefreshAsync(args.IsNavigationIntercepted);
        }
    }

    Task IHandleAfterRender.OnAfterRenderAsync()
    {
        if (!_navigationInterceptionEnabled)
        {
            _navigationInterceptionEnabled = true;
            return NavigationInterception.EnableNavigationInterceptionAsync();
        }

        return Task.CompletedTask;
    }

    private static class Log
    {
        private static readonly Action<ILogger, string, string, Exception> _displayingNotFound =
            LoggerMessage.Define<string, string>
            (LogLevel.Debug, new EventId(1, "DisplayingNotFound"), $"Displaying {nameof(NotFound)} because path '{{Path}}' with base URI '{{BaseUri}}' does not match any component route");

        private static readonly Action<ILogger, Type, string, string, Exception> _navigatingToComponent =
            LoggerMessage.Define<Type, string, string>
            (LogLevel.Debug, new EventId(2, "NavigatingToComponent"), "Navigating to component {ComponentType} in response to path '{Path}' with base URI '{BaseUri}'");

        private static readonly Action<ILogger, string, string, string, Exception> _navigatingToExternalUri =
            LoggerMessage.Define<string, string, string>
            (LogLevel.Debug, new EventId(3, "NavigatingToExternalUri"), "Navigating to non-component URI '{ExternalUri}' in response to path '{Path}' with base URI '{BaseUri}'");

        internal static void DisplayingNotFound(ILogger logger, string path, System.Uri baseUri)
        {
            _displayingNotFound(logger, path, baseUri.ToString(), null);
        }

        internal static void NavigatingToComponent(ILogger logger, Type componentType, string path, System.Uri baseUri)
        {
            _navigatingToComponent(logger, componentType, path, baseUri.ToString(), null);
        }

        internal static void NavigatingToExternalUri(ILogger logger, System.Uri externalUri, string path, System.Uri baseUri)
        {
            _navigatingToExternalUri(logger, externalUri.ToString(), path, baseUri.ToString(), null);
        }
    }
}
