using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateFrameStyles()
    {
        Style deviceManagerFrameStyle = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter {Property = Frame.FlowDirectionProperty , Value = DefaultFlowDirection},
                new Setter {Property = Frame.HasShadowProperty, Value = false},
                new Setter {Property = Frame.HorizontalOptionsProperty, Value = LayoutOptions.Start },
                new Setter {Property = Frame.BackgroundColorProperty , Value = _genericBackgroundColor},
                new Setter {Property = Frame.MarginProperty , Value =new Thickness(0, 10, 0, 0)},
                new Setter {Property = Frame.PaddingProperty,Value= Constants.ZERO_PADDING },
                new Setter {Property = Frame.BorderColorProperty,Value=_separatorAndDisableColor},
                new Setter {Property = Frame.IsClippedToBoundsProperty,Value=true},
                new Setter {Property = Frame.HeightRequestProperty,Value=40 },
            },
        };

        //Style instructionFrameStyle = new Style(typeof(Frame))
        //{
        //    Setters =
        //    {
        //        new Setter {Property = Frame.FlowDirectionProperty , Value = DefaultFlowDirection},
        //        new Setter {Property = Frame.BackgroundColorProperty, Value = Color.FromRgba(255, 255, 255, 0.2)},
        //        new Setter {Property = Frame.PaddingProperty, Value = new Thickness(10, 10, 10, 0)},
        //        new Setter {Property = Frame.MarginProperty, Value = new Thickness(0, 0, 0, 10)}
        //    }
        //};
        Style chatMessageStyle = new Style(typeof(Frame))
        {
            Setters =
            {
                new Setter { Property = Frame.VerticalOptionsProperty, Value =  LayoutOptions.FillAndExpand },
                new Setter { Property = Frame.CornerRadiusProperty, Value =  5 },
                new Setter { Property = Frame.PaddingProperty, Value = _controlPaddingMargin },
                new Setter { Property = Frame.HasShadowProperty, Value = false },
                new Setter { Property = Frame.FlowDirectionProperty , Value = DefaultFlowDirection }
            }
        };

        Style sentMessageStyle = new Style(typeof(Frame))
        {
            BasedOn = chatMessageStyle,
            Setters =
            {
                new Setter { Property = Frame.HorizontalOptionsProperty, Value =  LayoutOptions.End },
                new Setter { Property = Frame.MarginProperty, Value = DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(50, 5, new OnIdiom<double>{ Phone= 15, Tablet = 0 }, 5) : new Thickness(new OnIdiom<double>{ Phone= 15, Tablet = 0 }, 5, 50, 5) },
                new Setter { Property = Frame.BackgroundColorProperty, Value = _primaryAppColor }
            }
        };

        Style receivedMessageStyle = new Style(typeof(Frame))
        {
            BasedOn = chatMessageStyle,
            Setters =
            {
                new Setter { Property = Frame.HorizontalOptionsProperty, Value =  LayoutOptions.Start },
                new Setter { Property = Frame.MarginProperty, Value =  DefaultFlowDirection == FlowDirection.LeftToRight ? new Thickness(new OnIdiom<double>{ Phone= 15, Tablet = 0 }, 5, 50, 5) : new Thickness(50, 5, new OnIdiom<double>{ Phone= 15, Tablet = 0 }, 5) },
                new Setter { Property = Frame.BackgroundColorProperty, Value = _primaryAppColor }
            }
        };
        //Application.Current.Resources.Add(StyleConstants.ST_INSTRUCTION_FRAME_STYLE, instructionFrameStyle);
        Application.Current.Resources.Add(StyleConstants.ST_DEVICE_MANAGER_FRAME_STYLE, deviceManagerFrameStyle);
        Application.Current.Resources.Add(StyleConstants.ST_SENT_MESSAGE_STYLE, sentMessageStyle);
        Application.Current.Resources.Add(StyleConstants.ST_RECEIVED_MESSAGE_STYLE, receivedMessageStyle);

        Style dotBoxViewFrame = new(typeof(Frame))
        {
            Setters =
            {
                  new Setter {Property = Frame.HasShadowProperty, Value = false},
                  new Setter {Property = Frame.HorizontalOptionsProperty, Value = LayoutOptions.CenterAndExpand },
                  new Setter {Property = Frame.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand },
                  new Setter {Property = Frame.MarginProperty , Value = Constants.ZERO_PADDING},
                  new Setter {Property = Frame.PaddingProperty,Value = Constants.ZERO_PADDING },
                  new Setter {Property = Frame.BorderColorProperty,Value = Colors.Transparent},
                  new Setter {Property = Frame.IsClippedToBoundsProperty,Value = true},
                  new Setter {Property = Frame.FlowDirectionProperty,Value=_appFlowDirection },
                  new Setter {Property = Frame.HeightRequestProperty ,Value = 8 },
                  new Setter {Property = Frame.WidthRequestProperty ,Value = 8 },
                  new Setter {Property = Frame.BackgroundColorProperty , Value =_accentColor },
                  new Setter {Property = Frame.CornerRadiusProperty  , Value= _controlCornerRadius },
            }
        };

        Style cardOverLayViewFrame = new(typeof(Frame))
        {
            Setters =
            {
                  new Setter {Property = Frame.FlowDirectionProperty,Value=_appFlowDirection },
                  new Setter {Property = Frame.HeightRequestProperty ,Value = 160 },
                  new Setter {Property = Frame.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                  new Setter {Property = Frame.IsClippedToBoundsProperty,Value=true},
                  new Setter {Property = Frame.BackgroundColorProperty , Value = Color.FromArgb(StyleConstants.CARD_BACKGROUND_COLOR) },
                  new Setter {Property = Frame.CornerRadiusProperty  , Value= _controlCornerRadius},
                  new Setter {Property = Frame.OpacityProperty, Value=  0.6 }
            }
        };
        Style defaultFrameStyle = new(typeof(Frame))
        {
            Setters =
            {
                new Setter {Property = Frame.FlowDirectionProperty,Value=_appFlowDirection },
                new Setter {Property = Frame.HasShadowProperty, Value = false},
                new Setter {Property = Frame.MarginProperty , Value = Constants.ZERO_PADDING},
                new Setter {Property = Frame.PaddingProperty,Value= Constants.ZERO_PADDING },
                new Setter {Property = Frame.BorderColorProperty,Value=Colors.Transparent},
                new Setter {Property = Frame.IsClippedToBoundsProperty,Value=true},
                new Setter {Property = Frame.BackgroundColorProperty, Value = _genericBackgroundColor  }
            }
        };

        Application.Current.Resources.Add(StyleConstants.ST_DOT_FRAME, dotBoxViewFrame);
        Application.Current.Resources.Add(StyleConstants.ST_CARD_OVERLAY_VIEW_FRAME, cardOverLayViewFrame);
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_FRAME_STYLE, defaultFrameStyle);
    }
}