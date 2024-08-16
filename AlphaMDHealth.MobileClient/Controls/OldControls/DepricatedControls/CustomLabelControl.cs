using AlphaMDHealth.Utility;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom label control
    /// </summary>
    public class CustomLabelControl : CustomLabel
    {
        /// <summary>
        /// custom label control initializer
        /// </summary>
        /// <param name="controlType">control type data data</param>
        /// <returns></returns>
        public CustomLabelControl(LabelType controlType)
        {
           
            switch (controlType)
            {
                case LabelType.PrimaryLargeRight:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_LARGE_STYLE];
                    break;
                case LabelType.PrimaryLargeCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_LARGE_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    break;
                case LabelType.PrimarySmallLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE];
                    break;
                case LabelType.LeftSubHeader:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE];
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(CustomLabel)) * 1.5;
                    break;
                case LabelType.PrimarySmallRight:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.End;
                    HorizontalTextAlignment = TextAlignment.End;
                    break;
                case LabelType.PrimarySmallCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    LineBreakMode = LineBreakMode.WordWrap;
                    break;
                case LabelType.PrimaryMediumLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_MEDIUM_STYLE];
                    break;
                case LabelType.PrimaryMediumCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_MEDIUM_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    break;
                case LabelType.MessageControlMediumCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_MESSAGE_TEXT_COLOR_MEDIUM_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    break;
                case LabelType.PrimaryMediumBoldLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_BOLD_MEDIUM_STYLE];
                    HorizontalOptions = LayoutOptions.StartAndExpand;
                    HorizontalTextAlignment = TextAlignment.Start;
                    break;
                case LabelType.PrimaryMediumBoldCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_BOLD_MEDIUM_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    break;
                case LabelType.SecondrySmallLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE];
                    break;
                case LabelType.SecondrySmallLeftTrailTruncate:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE];
                    LineBreakMode = LineBreakMode.TailTruncation;
                    break;
                case LabelType.SecondrySmallCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    break;
                case LabelType.SecondryExtraSmallRight:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_SECONTRY_EXTRA_SMALL_LABEL_STYLE];
                    HorizontalTextAlignment = TextAlignment.End;
                    HorizontalOptions = LayoutOptions.End;
                    break;
                case LabelType.TertiarySmallLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_TERTIARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.StartAndExpand;
                    HorizontalTextAlignment = TextAlignment.Start;
                    break;
                case LabelType.TertiarySmallRight:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_TERTIARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.End;
                    HorizontalTextAlignment = TextAlignment.End;
                    break;
                case LabelType.TertiarySmallCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_TERTIARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    VerticalTextAlignment = TextAlignment.Center;
                    break;
                case LabelType.ServerErrorLabel:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_SERVER_ERROR_LABEL_STYLE];
                    WidthRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, 0.0);
                    break;
                case LabelType.SecondryAppSmallLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_WHITE_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.StartAndExpand;
                    HorizontalTextAlignment = TextAlignment.Start;
                    break;
                case LabelType.SecondryAppExtarSmallLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_WHITE_COLOR_EXTRA_SMALL_STYLE];
                    break;
                case LabelType.SecondryAppExtarSmallCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_WHITE_COLOR_EXTRA_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    LineBreakMode = LineBreakMode.WordWrap;
                    break;
                case LabelType.SecondryAppMediumBoldLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_WHITE_COLOR_MEDIUM_STYLE];
                    FontAttributes = FontAttributes.Bold;
                    break;
                case LabelType.SecondryAppMediumBoldCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_WHITE_COLOR_MEDIUM_STYLE];
                    FontAttributes = FontAttributes.Bold;
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    break;
                case LabelType.SecondryAppSmallCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_WHITE_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    LineBreakMode = LineBreakMode.WordWrap;
                    break;
                case LabelType.PrimaryAppExtraSmallRight:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR_EXTRA_SMALL_STYLE];
                    FontAttributes = FontAttributes.Bold;
                    break;
                case LabelType.BeforeLoginHeaderWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_NO_MASTER_HEADER_STYLE_KEY];
                    Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]);
                    break;
                case LabelType.BeforeLoginHeaderWithNoTopMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_NO_MASTER_HEADER_STYLE_KEY];
                    Margin = new Thickness(Margin.Left, 0, Margin.Right, Margin.Bottom);
                    break;
                case LabelType.BeforeLoginHeader:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_NO_MASTER_HEADER_STYLE_KEY];
                    LineBreakMode = LineBreakMode.WordWrap;
                    break;
                case LabelType.DarkTheamBeforeLoginHeader:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_NO_MASTER_HEADER_STYLE_KEY];
                    TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_COLOR];
                    break;
                case LabelType.ClientErrorLabel:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_OLD_ERROR_LABEL_STYLE];
                    break;
                case LabelType.ListHeaderStyle:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LIST_HEADER_STYLE];
                    LineBreakMode = LineBreakMode.TailTruncation;
                    break;
                case LabelType.ListUnderLineHeaderStyle:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LIST_HEADER_STYLE];
                    TextDecorations = TextDecorations.Underline;
                    break;
                case LabelType.HeaderPrimarySmallLeftWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                    HeightRequest = 40;
                    Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0);
                    Margin = new Thickness(-(double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0, -(double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]);
                    break;
                case LabelType.HeaderPrimarySmallLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                    HeightRequest = 40;
                    Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0, 0, 0);
                    break;
                case LabelType.HeaderPrimarySmallLeftWithoutPadding:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_SMALL_STYLE];
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                    HeightRequest = 40;
                    break;
                case LabelType.HeaderPrimaryMediumBoldLeftWithoutPadding:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_BOLD_MEDIUM_STYLE];
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                    HeightRequest = 40;
                    HorizontalTextAlignment = TextAlignment.Start;
                    break;
                case LabelType.HeaderPrimaryMediumBoldForDashboard:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_PRIMARY_TEXT_COLOR_BOLD_MEDIUM_STYLE];
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_DEFAULT_BACKGROUND_COLOR];
                    HorizontalTextAlignment = TextAlignment.Start;
                    break;
                case LabelType.RemoveLabelLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_OLD_ERROR_LABEL_STYLE];
                    HeightRequest = 40;
                    break;
                case LabelType.RemoveLabelLeftWithPadding:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_OLD_ERROR_LABEL_STYLE];
                    HorizontalOptions = LayoutOptions.FillAndExpand;
                    VerticalTextAlignment = TextAlignment.Center;
                    Padding = new Thickness((double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0, 0, 0);
                    Margin = new Thickness(-(double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], 0, -(double)Application.Current.Resources[StyleConstants.ST_APP_PADDING], (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]);
                    HeightRequest = 40;
                    break;
                case LabelType.LinkLabelSmallLeft:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR_SMALL_STYLE];
                    LineBreakMode = LineBreakMode.TailTruncation;
                    break;
                case LabelType.SecondaryHeaderLargeBoldCenter:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_LABEL_WHITE_COLOR_BOLD_LARGE_STYLE];
                    ////FontAttributes = FontAttributes.Bold;
                    HorizontalOptions = LayoutOptions.CenterAndExpand;
                    HorizontalTextAlignment = TextAlignment.Center;
                    LineBreakMode = LineBreakMode.WordWrap;
                    break;
                case LabelType.DefaultLabel:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_FORMATTED_LABEL_SECONDARY_TEXT_COLOR_SMALL_STYLE];
                   
                    break;
                default:
                    //to be implimented
                    break;
            }
        }

        
    }
}
