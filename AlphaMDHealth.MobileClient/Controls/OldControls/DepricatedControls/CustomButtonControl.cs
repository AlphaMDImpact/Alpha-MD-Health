using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents Custom Button Control
    /// </summary>
    public class CustomButtonControl : CustomButton
    {
        /// <summary>
        /// is diable button
        /// </summary>
        public void SetIsDisableButton(bool isDisabled)
        {
            if (isDisabled)
            {
                this.Opacity = 0.3;
                this.IsEnabled = false;
            }
            else
            {
                this.IsEnabled = true;
                this.Opacity = 1;
            }
        }

        /// <summary>
        /// Custom Button Control class constructor
        /// </summary>
        /// <param name="controlType"></param>
        public CustomButtonControl(ButtonType controlType)
        {
            switch (controlType)
            {
                case ButtonType.PrimaryEndWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    VerticalOptions = LayoutOptions.End;
                    Margin = new Thickness(Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.InvariantCulture));
                    break;
                case ButtonType.PrimaryWithoutMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_PRIMARY_BUTTON_STYLE];
                    break;
                case ButtonType.SecondryWithoutMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_SECONDARY_BUTTON_STYLE];
                    break;
                case ButtonType.TransparentWithoutMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    break;
                case ButtonType.TransparentWithoutMarginMediumSize:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    FontSize = 28;
                    break;
                case ButtonType.TransparentWithBorder:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_WITH_BORDER_KEY];
                    break;
                case ButtonType.PrimaryWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_PRIMARY_BUTTON_STYLE];
                    Margin = new Thickness(0, 0, 0, 10);
                    break;
                case ButtonType.SecondryWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_SECONDARY_BUTTON_STYLE];
                    Margin = new Thickness(0, 0, 0, 10);
                    break;
                case ButtonType.TransparentWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    Margin = new Thickness(0, 0, 0, 10);
                    break;
                case ButtonType.WhiteTextTransparent:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    TextColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                    break;
                case ButtonType.WhiteTextTransparentWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    TextColor = (Color)Application.Current.Resources[StyleConstants.ST_GENERIC_BACKGROUND_COLOR];
                    Margin = new Thickness(0, 0, 0, 10);
                    break;
                case ButtonType.TabButton:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    HeightRequest = 30;
                    CornerRadius = 0;
                    Padding = new Thickness(30, 0);
                    break;
                case ButtonType.DeleteWithMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
                    VerticalOptions = LayoutOptions.End;
                    Margin = new Thickness(0, 0, 0, 10);
                    break;
                case ButtonType.DeleteWithoutMargin:
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_TRANSPARENT_BUTTON_STYLE];
                    TextColor = (Color)Application.Current.Resources[StyleConstants.ST_ERROR_COLOR];
                    VerticalOptions = LayoutOptions.End;
                    break;
                default:
                    //to be implimented
                    break;
            }
        }

       
    }
}