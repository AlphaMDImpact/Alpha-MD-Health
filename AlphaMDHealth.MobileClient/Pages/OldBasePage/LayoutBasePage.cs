using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public partial class BasePage : ContentPage, IDisposable
{
    #region Data members

    private Grid _buttonGrid;
    public event EventHandler<EventArgs> TabClicked;
    List<OptionModel> _moreOptions;
    private readonly AmhLabelControl _statusMessage;
    private PageLayoutType _pageLayoutType;
    private bool _isLandscape;
    private double _width;
    private double _appPadding;

    internal AmhLabelControl Heading { get; private set; }
    internal AmhLabelControl Description { get; private set; }

    /// <summary>
    /// Message popup used to display in app
    /// </summary>
    public CustomMessageControl MessagePopup { get; set; }

    /// <summary>
    /// View used to display sync status
    /// </summary>
    public SyncStatusView SyncStatusView { get; private set; }

    /// <summary>
    /// main layout
    /// </summary>
    public Grid MasterGrid { get; private set; }

    /// <summary>
    /// Header layout
    /// </summary>
    public Border HeaderLayout { get; private set; }

    /// <summary>
    /// Page Controls to validate
    /// </summary>
    private List<View> PageControls { get; set; } = new List<View>();

    /// <summary>
    /// Cancellation token used to call services so that we can stop a service call task at any time form anywhere
    /// </summary>
    public static CancellationTokenSource CancellationTokenSourceInstance { get; set; }

    /// <summary>
    /// Base content page used
    /// </summary>
    public BaseDTO PageData { get; set; }

    /// <summary>
    /// Base content page used
    /// </summary>
    public BaseService PageService { get; set; }

    /// <summary>
    /// Main page layout used to add content of child page on this layout
    /// </summary>
    public Grid PageLayout { get; set; }

    /// <summary>
    /// Page layout type to decide layout style of phone and tablet/i-pad
    /// </summary>
    protected PageLayoutType PageLayoutType
    {
        get { return _pageLayoutType; }
        set
        {
            _pageLayoutType = value;
            ApplyPageType(_pageLayoutType);
            ApplyMarginPadding(value);
        }
    }

    #endregion

    /// <summary>
    /// Default constructor of base page which will render content end to end with default padding
    /// </summary>
    public BasePage() : this(PageLayoutType.EndToEndPageLayout, true)
    {
    }

    /// <summary>
    /// Parameterized constructor of base page which will render content based on received parameter value
    /// </summary>
    /// <param name="pageLayoutType"> Page layout type to decide layout style of phone and tablet/ipad</param>
    /// <param name="withScrollBar">Flag which decides scrollbar will be available outside layout or not</param>
    public BasePage(PageLayoutType pageLayoutType, bool withScrollBar)
    {
        EnableSafeArea();
        PageData = new BaseDTO();
        _setHeaderCompletionSource = new TaskCompletionSource<bool>();
        _pageLayoutType = pageLayoutType;
        if (Application.Current.Resources == null || Application.Current.Resources?.Count < 1)
        {
            new AppStyles().CreateAppStyles();
        }
        _appPadding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture);
        BackgroundColor = (Color)App.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
        PageLayout = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowSpacing = 10,
            //// Padding = GetLayoutPadding(pageLayoutType),
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, }
        };
        ApplyMarginPadding(pageLayoutType);
        MasterGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, },
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };
        _statusMessage = new AmhLabelControl(FieldTypes.ErrorHVCenterBoldLabelControl)
        {
            IsVisible = false
        };
        MessagePopup = new CustomMessageControl(false)
        {
            IsVisible = false
        };
        MessagePopup.OnActionClicked += OnActionClicked;
        HideFooter(App._essentials.GetPreferenceValue(StorageConstants.PR_IS_ADD_EDIT_PAGE_KEY, false));
        ApplyPageType(pageLayoutType);
        SetPageContent(withScrollBar);
        PageService = new BaseService(App._essentials);
    }

    /// <summary>
    /// Called when page appears in UI
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!(ShellMasterPage.CurrentShell?.HasMainPage ?? true))
        {
            await SetHeaderMenusAsync().ConfigureAwait(false);
        }
        else
        {
            //todo:
            //if (Content != null && Content.Effects.FirstOrDefault(x => x is CustomSafeAreaInsetEffect) == null)
            //{
            //    //todo:Content.Effects.Add(new CustomSafeAreaInsetEffect());
            //}
        }
    }

    /// <summary>
    /// call when size is allocated to check landscape mode or portrait
    /// </summary>
    /// <param name="width">width of screen</param>
    /// <param name="height">height of screen</param>
    protected async override void OnSizeAllocated(double width, double height)
    {
        try
        {
            base.OnSizeAllocated(width, height);
            if (DeviceInfo.Platform == DevicePlatform.Android
                && App._essentials.GetPreferenceValue(StorageConstants.PR_IS_KEYBOARD_UP_KEY, false)
                && (_width < 1 || _width.Equals(width)))
            {
                return;
            }
            _width = width;
            await ResetLayoutAsync();
        }
        catch (Exception ex)
        {
        }
    }

    #region Page Layout Modification Methods

    /// <summary>
    /// Method which adds spacing in PancakeView to display separator properly
    /// </summary>
    /// <param name="content"></param>
    /// <param name="isDashboard"></param>
    public void AddSpacingForSeparatorLine(ContentView content, bool isDashboard) //todo:PancakeView content, bool isDashboard)
    {
        content.Margin = MobileConstants.IsTablet
            ? GenericMethods.GetPlatformSpecificValue(new Thickness(1, 1, 1, IsDashboard(isDashboard)), new Thickness(0), new Thickness(1, 1, 1, 0))
            : GenericMethods.GetPlatformSpecificValue(new Thickness(1), new Thickness(0), new Thickness(1, 1, 1, 0));
    }

    /// <summary>
    /// Set content
    /// </summary>
    public void SetContent()
    {
        ApplyPageType(_pageLayoutType);
    }

    /// <summary>
    /// Creates App Logo
    /// </summary>
    /// <returns>App logo SVG</returns>
    protected static View CreateAppLogo()
    {
        return new AmhImageControl(FieldTypes.ImageControl)
        {
            Icon = ImageConstants.I_LOGO_ATOM_VERTICAL_PNG,
            HorizontalOptions = LayoutOptions.Center
        };
    }

    /// <summary>
    /// Sets page content with or without scrollbar 
    /// </summary>
    /// <param name="withScrollBar">Flag which decides Base page layout grid will contain scrollbar or not</param>
    public void SetPageContent(bool withScrollBar)
    {
        MasterGrid.Children?.Clear();
        if (withScrollBar)
        {
            MasterGrid.Add(new ScrollView { Content = PageLayout }, 0, 2);
        }
        else
        {
            MasterGrid.Add(PageLayout, 0, 2);
        }
        if (_pageLayoutType == PageLayoutType.LoginFlowPageLayout)
        {
            CreateHeaderLayout();
            MasterGrid.Add(HeaderLayout, 0, 1);
        }
        MasterGrid.Add(_statusMessage, 0, 2);
    }

    private void CreateHeaderLayout()
    {
        if (HeaderLayout == null)
        {
            Heading = new AmhLabelControl(FieldTypes.PrimaryLargeHVCenterBoldLabelControl);
            Description = new AmhLabelControl(FieldTypes.PrimaryMediumHVCenterLabelControl);
            HeaderLayout = new Border
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                //MinimumHeightRequest = GetScreenHeight() * .2,
                WidthRequest = GetScreenWidth(),
                Background = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR], 
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 30, 30) },
            };
            var headerContent = new Grid
            {
                Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
                Padding = new Thickness(_appPadding),
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star } },
                RowDefinitions = new RowDefinitionCollection {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };
            headerContent.Add(CreateAppLogo(), 0, 0);
            headerContent.Add(Heading, 0, 1);
            headerContent.Add(Description, 0, 2);
            HeaderLayout.Content = headerContent;
        }
    }

    /// <summary>
    /// Gets thickness option to add as margin or padding for phone/tablet/i-pad based on page type
    /// </summary>
    /// <param name="pageLayoutType">Page type to decide layout of page and its arrangement</param>
    /// <returns>Thickness option to add as margin or padding</returns>
    public Thickness GetLayoutPadding(PageLayoutType pageLayoutType)
    {
        var padding = pageLayoutType switch
        {
            PageLayoutType.LoginFlowPageLayout => (double)new OnIdiom<double> { Phone = _appPadding, Tablet = GetScreenWidth() * 0.3 },
            PageLayoutType.MastersContentPageLayout => (double)new OnIdiom<double> { Phone = 0, Tablet = GetScreenWidth() * 0.3 },
            _ => (double)new OnIdiom<double> { Phone = 0, Tablet = 0 },
        };
        return new Thickness(padding, 0, padding, 0);
    }

    private async Task ResetLayoutAsync()
    {
        if (MobileConstants.IsTablet)
        {
            SetOrientation();
            PageLayout.Padding = new Thickness(0);
            await Task.Delay(10).ConfigureAwait(true);
            if (MessagePopup.IsVisible)
            {
                MessagePopup.ResetLayoutForMessagePopUp(_isLandscape);
            }
            ApplyMarginPadding(_pageLayoutType);
        }
    }

    private void ApplyMarginPadding(PageLayoutType value)
    {
        PageLayout.Padding = GetLayoutPadding(value);
    }

    private int IsDashboard(bool isDashboard)
    {
        return isDashboard ? 1 : 0;
    }

    private void ApplyPageType(PageLayoutType pageLayoutType)
    {
        MasterGrid.BackgroundColor = pageLayoutType == PageLayoutType.LoginFlowPageLayout
            ? (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR]
            : (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
        Content = MasterGrid;
    }

    /// <summary>
    /// Get PopUpPage Padding
    /// </summary>
    /// <param name="pageLayoutType">PopUpPageType</param>
    /// <returns>thickness based on Page type</returns>
    public Thickness GetPopUpPagePadding(PopUpPageType pageLayoutType)
    {
        switch (pageLayoutType)
        {
            case PopUpPageType.Long:
                return new Thickness(0.14 * GetScreenWidth(), 0.05 * GetScreenHeight());
            case PopUpPageType.Medium:
                return new Thickness(0.28 * GetScreenWidth(), 0.05 * GetScreenHeight());
            case PopUpPageType.Short:
                return new Thickness(0.5 * GetScreenWidth(), 0.05 * GetScreenHeight());
            default:
                return new Thickness(0);
        }
    }

    //private double GetTabletScreenWidth(double lmf, double pmf)
    //{
    //    return _isLandscape
    //        ? GetScreenWidth() * lmf
    //        : GetScreenWidth() * pmf;
    //}

    private double GetScreenWidth()
    {
        return App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0);
    }

    private static double GetScreenHeight()
    {
        return App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_HEIGHT_KEY, 0.0);
    }

    /// <summary>
    /// Sets layout option of base page layout
    /// </summary>
    /// <param name="layoutOptions">Layout option to set in for base page layout grid</param>
    /// <param name="isHorizontalOption">flag which decides it is for horizontal or vertical</param>
    public void SetPageLayoutOption(LayoutOptions layoutOptions, bool isHorizontalOption)
    {
        if (isHorizontalOption)
        {
            PageLayout.HorizontalOptions = layoutOptions;
        }
        else
        {
            PageLayout.VerticalOptions = layoutOptions;
        }
    }

    /// <summary>
    /// Add rows/ columns into base page layout
    /// </summary>
    /// <param name="gridLength">Grid length to add as row/column</param>
    /// <param name="count">number of rows/columns to add in base page layout</param>
    /// <param name="isRowDefinition">flag which decides rows needs to add or column</param>
    public void AddRowColumnDefinition(GridLength gridLength, int count, bool isRowDefinition)
    {
        for (int x = 0; x < count; x++)
        {
            if (isRowDefinition)
            {
                PageLayout.RowDefinitions.Add(new RowDefinition { Height = gridLength });
            }
            else
            {
                PageLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength });
            }
        }
    }

    /// <summary>
    /// Display operation status
    /// </summary>
    /// <param name="message">message to display</param>
    public void DisplayOperationStatus(string message)
    {
        DisplayOperationStatus(message, false);
    }

    /// <summary>
    /// Display operation status
    /// </summary>
    /// <param name="message">message to display</param>
    /// <param name="isSuccess">message to display</param>
    public void DisplayOperationStatus(string message, bool isSuccess)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _statusMessage.IsVisible = false;
        }
        else
        {
            _statusMessage.BackgroundColor = isSuccess
                 ? (Color)Application.Current.Resources[StyleConstants.ST_SUCCESS_COLOR]
                 : (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
            //todo:
            //MasterGrid.RaiseChild(_statusMessage);
            _statusMessage.Value = message;
            _statusMessage.Opacity = 1;
            _statusMessage.IsVisible = true;
            StartTimer();
        }
    }

    private IDispatcherTimer _timer;
    private int _milliSeconds = 500;

    private void StartTimer()
    {
        _milliSeconds = 500;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(10);
        _timer.Tick += OnTickOfDisplayStatus;
        _timer.Start();
    }

    private void OnTickOfDisplayStatus(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_milliSeconds >= 1)
            {
                _milliSeconds--;
                if (_milliSeconds < 50)
                {
                    _statusMessage.Opacity = _milliSeconds / (double)30;
                    _statusMessage.FadeTo(_statusMessage.Opacity);
                }
                if (!_statusMessage.IsVisible)
                {
                    _timer.Stop();
                }
            }
            else
            {
                _statusMessage.IsVisible = false;
                _timer.Stop();
            }
        });
    }

    #endregion
}