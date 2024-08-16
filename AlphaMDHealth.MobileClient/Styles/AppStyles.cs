using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private Style _defaultExtraSmallLabelStyle;

    //button Heights
    private const double _defaultControlHeight = 50;
    private const double _controlPaddingMargin = 20;
    private const double _singleRowHeight = 60; //toto: to remove
    private const double _doubleRowHeight = 90;//toto: to remove
    private const double _tripleRowHeight = 110;//toto: to remove
    private const double _fiveRowHeight = 126;//toto: to remove
    private const double _sixRowHeight = 150;//toto: to remove

    private int _controlCornerRadius = 4;
    private int _imageSizeLogoHeight = 42;
    private int _imageSizeLogoWidth = 480;
    //private int _tabViewHeaderPanelItemSpacing = 30;
    private double _appPadding = 15;
    private double _appTopPadding = 15;
    private double _appComponentPadding = 15;
    private int _defaultCardHeight = 200;
    private bool _isProfileCircular = false;
    private bool _isSeperatorBelowHeader = true;

    //FontSize values
    //todo: remove all hardcoded sizes as recommended here: https://github.com/dotnet/maui/issues/8798#issuecomment-1213001071
    private readonly double microLabelSize = Device.GetNamedSize(NamedSize.Micro, typeof(AmhLabelControl));
    private readonly double smallLabelSize = Device.GetNamedSize(NamedSize.Small, typeof(AmhLabelControl));
    private readonly double mediumLabelSize = Device.GetNamedSize(NamedSize.Medium, typeof(AmhLabelControl));
    private readonly double largeLabelSize = Device.GetNamedSize(NamedSize.Large, typeof(AmhLabelControl));

    private const double ImageSizeS = 20;

    private readonly Color _defaultBackgroundColor = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR);
    private Color _primaryAppColor = Color.FromArgb(StyleConstants.PRIMARY_APP_COLOR);
    private readonly Color _secondaryAppColor = Color.FromArgb(StyleConstants.SECONDARY_APP_COLOR);
    private readonly Color _tertiaryAppColor = Color.FromArgb(StyleConstants.TERTIARY_APP_COLOR);

    private readonly Color _primaryTextColor = Color.FromArgb(StyleConstants.PRIMARY_TEXT_COLOR);
    private readonly Color _secondaryTextColor = Color.FromArgb(StyleConstants.SECONDARY_TEXT_COLOR);
    private readonly Color _tertiaryTextColor = Color.FromArgb(StyleConstants.TERTIARY_TEXT_COLOR);

    private readonly Color _genericBackgroundColor = Color.FromArgb(StyleConstants.GENERIC_BACKGROUND_COLOR);
    private readonly Color _accentColor = Color.FromArgb(StyleConstants.ACCENT_COLOR);
    private readonly Color _separatorAndDisableColor = Color.FromArgb(StyleConstants.SEPARATOR_N_DISABLED_COLOR);
    private readonly Color _errorColor = Color.FromArgb(StyleConstants.ERROR_COLOR);
    private readonly Color _successColor = Color.FromArgb(StyleConstants.SUCCESS_COLOR);
    private readonly Color _boxShadowColor = Color.FromArgb(StyleConstants.BOX_SHADOW_COLOR);

    //private readonly double basePageBackgroudheightPercent = GenericMethods.GetPlatformSpecificValue(3.2, 3.6, 0);

    /// <summary>
    /// DefaultFlowDirection
    /// </summary>
    internal static FlowDirection DefaultFlowDirection { get; set; }
    private AppFlowDirection _appFlowDirection = (AppFlowDirection)DefaultFlowDirection;

    ///// <summary>
    ///// IsTabletScaledView
    ///// </summary>
    //internal static bool IsTabletScaledView { get; set; }

    /// <summary>
    /// get namespace of calling project
    /// </summary>
    /// <returns>namespace</returns>
    internal static string NameSpaceImage => Application.Current.GetType().Namespace + ".Images.";

    /// <summary>
    /// get namespace of calling project
    /// </summary>
    /// <returns>namespace</returns>
    internal static string NameSpace => Application.Current.GetType().Namespace;

    /// <summary>
    /// Creates default styles when style settings are not yet loaded
    /// </summary>
    internal void CreateAppStyles()
    {
        Application.Current.Resources = new ResourceDictionary();

        DefaultFlowDirection = App._essentials.GetPreferenceValue(StorageConstants.PR_IS_RIGHT_ALIGNED_KEY, false)
           ? FlowDirection.RightToLeft
           : FlowDirection.LeftToRight;

        CreateColorStyle();
        CreateImageSizeStyle();
        CreateLabelStyle();
        CreateButtonStyle();
        CreateEntryStyle();

        CreateWebViewStyle();
        CreateCalenderStyle();

        CreatePancakeStyle();
        CreateFrameStyles();
        CreateBorderStyle();
        CreateDateTimeStyle();
        CreateProgressBarStyle();
        //CreateLayoutStyle();
        CreateListStyle();
        CreateViewsStyle();
        CreateChartStyle();
        CreateCheckBoxStyle();
        CreateRadioButtonStyle();
        CreateSliderStyle();
        CreateDropdownStyle();
        CreateUploadStyle();
        CreateTabStyle();
        CreateCardsStyle();
        CreateOtherStyle();
        CreateImageStyles();
        CreateBadgeCountStyle();
        CreateSwitchStyle();
        CreateCarouselStyles();
    }

    /// <summary>
    ///  Generic implementation to apply a particular style for Label, ExtendedButton
    /// </summary>
    /// <returns>ResourceDictionary</returns>
    internal async Task<BaseDTO> LoadAppStylesAsync()
    {
        BaseDTO resultData = new BaseDTO();
        try
        {
            await Task.Delay(1).ConfigureAwait(true);
            var primaryAppColor = await new SettingService(App._essentials).GetSettingsValueByKeyAsync(SettingsConstants.S_PRIMARY_APP_COLOR_KEY).ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(primaryAppColor))
            {
                _primaryAppColor = Color.FromArgb(primaryAppColor);
            }
            //IsTabletScaledView = DeviceInfo.Idiom == DeviceIdiom.Tablet ? true : false;
            CreateAppStyles();
            resultData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            resultData.ErrCode = ErrorCode.RestartApp;
        }
        return resultData;
    }
}