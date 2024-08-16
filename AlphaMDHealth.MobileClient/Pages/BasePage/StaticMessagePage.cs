using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

[RouteRegistration(nameof(StaticMessagePage))]
[QueryProperty(nameof(ID), "id")]
[QueryProperty(nameof(Type), "type")]
[QueryProperty(nameof(Key), "recordCount")]
[QueryProperty(nameof(MessageType), "view")]
[QueryProperty(nameof(TargetPage), "name")]
[QueryProperty(nameof(TargetPageParams), "identifier")]
[QueryProperty(nameof(GroupName), "isAdd")]
public class StaticMessagePage : BasePage
{
    private string _key;
    private string _messageType;
    private string _targetPageParams;
    private StaticMessageView _staticMessgeView;
    private readonly bool _isBeforeLogin;

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
    public string TargetPageParams
    {
        get { return _targetPageParams?.ToString(CultureInfo.InvariantCulture); }
        set => _targetPageParams = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// No of chat conversation to be displayed
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// Type of message data source to fetch data from it
    /// </summary>
    public string Type { get; set; } = PageType.Default.ToString();

    /// <summary>
    /// No of chat conversation to be displayed
    /// </summary>
    public string Key
    {
        get { return _key.ToString(CultureInfo.InvariantCulture); }
        set => _key = Uri.UnescapeDataString(value);
    }

    /// <summary>
    /// Type of message data source to fetch data from it
    /// </summary>
    public string MessageType
    {
        get { return _messageType.ToString(CultureInfo.InvariantCulture); }
        set => _messageType = Uri.UnescapeDataString(value);
    }
    /// <summary>
    /// remove safe area insert
    /// </summary>
    public bool RemoveSafeAreaInsert
    {
        get
        {
            return false;
        }
        set
        {
            if (value && Content != null)
            {
                //todo:Content.Effects.Add(new CustomSafeAreaInsetEffect());
            }
        }
    }

    /// <summary>
    /// Default constructer works with key parameter
    /// </summary>
    public StaticMessagePage() : base(PageLayoutType.EndToEndPageLayout, true)
    {
        LoadUI();
    }

    /// <summary>
    /// Default parameter of static message page
    /// </summary>
    /// <param name="key">Resource key to display message</param>
    public StaticMessagePage(string key) : base(PageLayoutType.LoginFlowPageLayout, true)
    {
        _key = key;
        LoadUI();
    }

    /// <summary>
    /// parameterized  constructor of static message page
    /// </summary>
    /// <param name="key">Resource key to display message</param>
    /// <param name="targetPage">target page on which needs to navigate on click of action</param>
    public StaticMessagePage(string key, string targetPage) : this(key, GroupConstants.RS_COMMON_GROUP, targetPage, default) { }

    /// <summary>
    /// Default parameter of static message page
    /// </summary>
    /// <param name="key">Resource key to display message</param>
    /// <param name="groupName">Groupname from where resource need to fetch</param>
    /// <param name="targetPage">target page on which needs to navigate on click of action</param>
    public StaticMessagePage(string key, string groupName, string targetPage) : this(key, groupName, targetPage, default) { }

    /// <summary>
    /// Default parameter of static message page
    /// </summary>
    /// <param name="key">Resource key to display message</param>
    /// <param name="groupName">Groupname from where resource need to fetch</param>
    /// <param name="targetPage">target page on which needs to navigate on click of action</param>
    /// <param name="targetPageParams">Parameter of target page which needs to pass during navigate on click of action</param>
    public StaticMessagePage(string key, string groupName, string targetPage, string targetPageParams) : this(key, groupName, targetPage, targetPageParams, PageType.Default.ToString()) { }

    /// <summary>
    /// Default parameter of static message page
    /// </summary>
    /// <param name="key">Resource key to display message</param>
    /// <param name="groupName">Groupname from where resource need to fetch</param>
    /// <param name="targetPage">target page on which needs to navigate on click of action</param>
    /// <param name="targetPageParams">Parameter of target page which needs to pass during navigate on click of action</param>
    /// <param name="type">page type to display data</param>
    public StaticMessagePage(string key, string groupName, string targetPage, string targetPageParams, string type) : base(PageLayoutType.LoginFlowPageLayout, false)
    {
        _isBeforeLogin = true;
        _key = key;
        TargetPage = targetPage;
        TargetPageParams = targetPageParams;
        if (!string.IsNullOrWhiteSpace(groupName))
        {
            GroupName = groupName;
        }
        if (!string.IsNullOrWhiteSpace(type))
        {
            _messageType = type;
        }
        LoadUI();
    }

    /// <summary>
    /// Called when page appears in UI
    /// </summary>
    protected override async void OnAppearing()
    {
        await Task.Delay(Constants.PARAMETER_READ_DELAY).ConfigureAwait(true);
        base.OnAppearing();
        _key = string.IsNullOrWhiteSpace(_key) ? ID : _key;
        _messageType = string.IsNullOrWhiteSpace(_messageType) ? Type : _messageType;
        _staticMessgeView.Parameters = AddParameters(
            CreateParameter(nameof(Key), _key),
            CreateParameter(nameof(MessageType), _messageType),
            CreateParameter(nameof(TargetPage), TargetPage),
            CreateParameter(nameof(TargetPageParams), TargetPageParams),
            CreateParameter(nameof(GroupName), GroupName)
        );
        switch (_messageType.ToEnum<PageType>())
        {
            case PageType.ConsentPage:
                PageLayoutType = PageLayoutType.LoginFlowPageLayout;
                SetPageLayoutOption(LayoutOptions.FillAndExpand, false);
                HideFooter();
                if (!_isBeforeLogin)
                {
                    //todo:Content.Effects.Add(new CustomSafeAreaInsetEffect());
                }

                break;
            case PageType.ContentPage:
                PageLayoutType = PageLayoutType.LoginFlowPageLayout;
                SetPageLayoutOption(string.IsNullOrWhiteSpace(TargetPageParams) ? LayoutOptions.StartAndExpand : LayoutOptions.FillAndExpand, false);
                HideFooter();
                if (!_isBeforeLogin)
                {
                    //todo:Content.Effects.Add(new CustomSafeAreaInsetEffect());
                }
                break;
            default:
                //// for future implementation
                break;
        }
        await _staticMessgeView.LoadUIAsync(false).ConfigureAwait(true);
        AppHelper.ShowBusyIndicator = false;
    }

    /// <summary>
    /// Un register events
    /// </summary>
    protected override async void OnDisappearing()
    {
        await _staticMessgeView.UnloadUIAsync().ConfigureAwait(true);
        base.OnDisappearing();
    }

    private void HideFooter()
    {
        if (ShellMasterPage.CurrentShell.HasMainPage)
        {
            PageLayout.Padding = new Thickness(PageLayout.Padding.Left, 0, PageLayout.Padding.Right, PageLayout.Padding.Bottom);
        }
        else
        {
            Shell.SetTabBarIsVisible(this, false);
        }
    }

    private void LoadUI()
    {
        BackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR);
        PageLayout.Children?.Clear();
        PageLayout.RowDefinitions?.Clear();
        SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.CenterAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        AddRowColumnDefinition(GridLength.Star, 1, true);
        _staticMessgeView = new StaticMessageView(this, null);
        PageLayout.Add(_staticMessgeView, 0, 0);
        SetPageContent(false);
        HideFooter();
    }
}