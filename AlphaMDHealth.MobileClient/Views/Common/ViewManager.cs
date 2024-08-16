using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;
using System.Runtime.Serialization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Common View used to display all pages as component by inheriting from it
/// </summary>
public abstract class ViewManager : ContentView
{
    /// <summary>
    /// Content body view
    /// </summary>
    private View ContentUIView { get; set; }

    /// <summary>
    /// on click event of Send Button
    /// </summary>
    public event EventHandler<EventArgs> OnListRefresh;

    /// <summary>
    /// On click event of More Option header
    /// </summary>
    public event EventHandler<EventArgs> OnMoreOptionClicked;

    /// <summary>
    /// on patient list refresh
    /// </summary>
    public event EventHandler<EventArgs> OnPatientListRefresh;

    /// <summary>
    /// Invoke callback event
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">event arguments</param>
    public void InvokeListRefresh(object sender, EventArgs e)
    {
        if (OnListRefresh != null)
        {
            OnListRefresh.Invoke(sender, e);
        }
    }

    /// <summary>
    /// Invoke callback event
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">event arguments</param>
    public void InvokePatientListRefresh(object sender, EventArgs e)
    {
        if (OnPatientListRefresh != null)
        {
            OnPatientListRefresh.Invoke(sender, e);
        }
    }

    /// <summary>
    /// View Parameters to display data based on parameter values
    /// </summary>
    [DataMember]
    public List<SystemFeatureParameterModel> Parameters { get; set; } = new List<SystemFeatureParameterModel>();

    /// <summary>
    /// Parent page instance to access its properties
    /// </summary>
    public BasePage ParentPage { get; set; }

    /// <summary>
    /// View Parameters to display data based on parameter values
    /// </summary>
    public bool _showViewOnNoData { get; set; } = true;

    /// <summary>
    /// Text to display for navigation
    /// </summary>
    public string NavigationText { get; set; }

    /// <summary>
    /// Flag which decides view will display with title or not
    /// </summary>
    public bool ShowTitle
    {
        get { return (bool)GetValue(ShowTitleProperty); }
        set { SetValue(ShowTitleProperty, value); }
    }

    /// <summary>
    /// Flag which decides view will display with title or not
    /// </summary>
    public static readonly BindableProperty ShowTitleProperty = BindableProperty.Create(nameof(ShowTitle), typeof(bool)
        , typeof(ViewManager), false, propertyChanged: OnChanged);

    /// <summary>
    /// View title text to display in view header
    /// </summary>
    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    /// <summary>
    /// View title text to display in view header
    /// </summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string)
        , typeof(ViewManager), string.Empty, propertyChanged: OnChanged);

    /// <summary>
    /// FeatureCode which decides redirection on click of view title
    /// </summary>
    public string FeatureCode
    {
        get { return (string)GetValue(FeatureCodeProperty); }
        set { SetValue(FeatureCodeProperty, value); }
    }

    /// <summary>
    /// NodeID which decides redirection on click of view title
    /// </summary>
    public static readonly BindableProperty FeatureCodeProperty = BindableProperty.Create(nameof(FeatureCode)
        , typeof(string), typeof(ViewManager), default(string), propertyChanged: OnChanged);

    /// <summary>
    /// NodeID which decides redirection on click of view title
    /// </summary>
    public long NodeID
    {
        get { return (long)GetValue(NodeIDProperty); }
        set { SetValue(NodeIDProperty, value); }
    }

    /// <summary>
    /// NodeID which decides redirection on click of view title
    /// </summary>
    public static readonly BindableProperty NodeIDProperty = BindableProperty.Create(nameof(NodeID), typeof(long)
        , typeof(ViewManager), default(long), propertyChanged: OnChanged);

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((ViewManager)bindable).SetContent();
    }

    /// <summary>
    /// Parameterized constructor containing page instance and Parameters
    /// </summary>
    /// <param name="page">page instance on which view is rendering</param>
    /// <param name="parameters">Featue parameters to render view</param>
    protected ViewManager(BasePage page, object parameters)
    {
        if (parameters != null)
        {
            Parameters = (List<SystemFeatureParameterModel>)parameters;
        }
        ParentPage = page ?? new BasePage();
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="isRefreshRequest">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public abstract Task LoadUIAsync(bool isRefreshRequest);

    /// <summary>
    /// Unregister events of Views
    /// </summary>
    public abstract Task UnloadUIAsync();

    /// <summary>
    /// Default message view type 
    /// </summary>
    protected MessageViewType _messageViewType;

    /// <summary>
    /// Default content view
    /// </summary>
    protected View _contentView;

    /// <summary>
    /// Creates Default Message Viewes
    /// </summary>
    /// <param name="viewType"></param>
    public void CreateDefaultContentView(MessageViewType viewType)
    {
        if (_contentView == null || _messageViewType != viewType)
        {
            _messageViewType = viewType;
            switch (_messageViewType)
            {
                case MessageViewType.PinacleView:
                    var padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
                    _contentView = new ContentView
                    {
                        Padding = new Thickness(padding),
                        //todo:
                        //Content = new PancakeView
                        //{
                        //    Style = (Style)Application.Current.Resources[LibStyleConstants.ST_PANCAKE_STYLE],
                        //    BorderColor = (Color)Application.Current.Resources[LibStyleConstants.ST_SEPERATOR_COLOR_STYLE],
                        //    BackgroundColor = (Color)Application.Current.Resources[LibStyleConstants.ST_CONTROL_BACKGROUND_COLOR],
                        Content = new AmhLabelControl(FieldTypes.PrimaryLargeHStartVCenterBoldLabelControl)
                        {
                            HeightRequest = (Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT], CultureInfo.InvariantCulture) + (2 * padding) + (padding / 2))
                        }
                        //}
                    };
                    break;
                case MessageViewType.StaticMessageView:
                    _contentView = new AmhMessageControl()
                    {
                        VerticalOptions = LayoutOptions.CenterAndExpand
                    };
                    break;
                ////case MessageViewType.LabelView:
                default:
                    _contentView = new AmhLabelControl(FieldTypes.PrimaryLargeHStartVCenterBoldLabelControl)
                    {
                        HeightRequest = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT])
                    };
                    break;
            }
        }
    }

    /// <summary>
    /// Load UI data of view
    /// </summary>
    /// <param name="key">Flag which decides needs to create or refresh</param>
    /// <returns>Returns true if required view is found, else return false</returns>
    public void RenderMessageView(string key)
    {
        if (_contentView != null)
        {
            switch (_messageViewType)
            {
                case MessageViewType.PinacleView:
                    //todo:
                    //(((_contentView as ContentView).Content as PancakeView).Content as CustomLabelControl).Text = LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, string.IsNullOrWhiteSpace(key) ? ErrorCode.NotImplemented.ToString() : key);
                    ((_contentView as ContentView).Content as AmhLabelControl).ResourceKey = string.IsNullOrWhiteSpace(key) ? ErrorCode.NotImplemented.ToString() : key;
                    ((_contentView as ContentView).Content as AmhLabelControl).PageResources = ParentPage.PageData;
                    break;
                case MessageViewType.StaticMessageView:
                    (_contentView as AmhMessageControl).ResourceKey = string.IsNullOrWhiteSpace(key) ? ResourceConstants.R_NO_DATA_FOUND_KEY : key;
                    (_contentView as AmhMessageControl).PageResources = ParentPage.PageData;
                    break;
                ////case MessageViewType.LabelView:
                default:
                    (_contentView as AmhLabelControl).Value = LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, string.IsNullOrWhiteSpace(key) ? ErrorCode.NotImplemented.ToString() : key);
                    break;
            }
        }
    }

    /// <summary>
    /// Hides current view from page
    /// </summary>
    /// <param name="isVisible">Value which needs to set in view visible property</param>
    protected void ShowHidePageContent(bool isVisible)
    {
        this.IsVisible = isVisible;
    }

    /// <summary>
    /// Set page content based on properties
    /// </summary>
    /// <param name="view">View which needs to set as content body</param>
    protected void SetPageContent(View view)
    {
        ContentUIView = view;
        SetContent();
    }

    /// <summary>
    /// Fetch parameter detais by name
    /// </summary>
    /// <param name="name">name of parameter</param>
    /// <returns>feature parameter</returns>
    protected SystemFeatureParameterModel GetParameter(string name)
    {
        return Parameters?.FirstOrDefault(x => x.ParameterName == name);
    }

    /// <summary>
    /// Fetch parameter value by name
    /// </summary>
    /// <param name="name">name of parameter</param>
    /// <returns>parameter value</returns>
    public string GetParameterValue(string name)
    {
        return GetParameter(name)?.ParameterValue;
    }

    /// <summary>
    /// Check it is patient overview page or not
    /// </summary>
    public bool IsPatientPage()
    {
        return ShellMasterPage.CurrentShell.CurrentPage != null
            && ShellMasterPage.CurrentShell.CurrentPage.ToString()
                .EndsWith(Constants.PATIENTS_PAGE_CONSTANT, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Check if it is DashboardView or not
    /// </summary>
    public bool IsDashboardView(long recordCount)
    {
        return recordCount > 0;
    }

    /// <summary>
    /// Check if it is current page is patient overview or not
    /// </summary>
    public bool IsPatientOverview(long recordCount)
    {
        var roleID = App._essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0);
        return recordCount > 0 && IsPatientPage() && roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker;
    }

    private void SetContent()
    {
        if (ContentUIView != null)
        {
            if (ShowTitle)
            {
                var headerView = string.IsNullOrWhiteSpace(FeatureCode)
                    ? new HeaderView(SetTitle(), NavigationText, NodeID)
                    : new HeaderView(SetTitle(), NavigationText, FeatureCode);
                if (OnMoreOptionClicked != null)
                {
                    headerView.OnMoreOptionClicked -= OnHeaderClicked;
                    headerView.OnMoreOptionClicked += OnHeaderClicked;
                }
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Spacing = 0,
                    Children = {
                        headerView,
                        ContentUIView
                    }
                };
            }
            else
            {
                Content = ContentUIView;
                ContentUIView.Margin = new Thickness(0);
            }
            Margin = new Thickness(0, ShowTitle ? Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_TOP_PADDING], CultureInfo.InvariantCulture) : 0);
        }
    }

    private string SetTitle()
    {
        return !string.IsNullOrWhiteSpace(Title) ? Title : string.Empty;
    }

    private void OnHeaderClicked(object sender, EventArgs e)
    {
        OnMoreOptionClicked.Invoke(sender, e);
    }
}