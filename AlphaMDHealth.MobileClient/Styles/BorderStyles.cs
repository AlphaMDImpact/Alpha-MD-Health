using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls.Shapes;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateBorderStyle()
    {
        Style defaultPancackeStyle = new(typeof(Border))
        {
            Setters =
            {
                new Setter {Property = Border.FlowDirectionProperty,Value=_appFlowDirection },
                new Setter {Property = Border.HorizontalOptionsProperty, Value = LayoutOptions.CenterAndExpand },
                new Setter {Property = Border.VerticalOptionsProperty, Value = LayoutOptions.CenterAndExpand },
                new Setter {Property = Border.MarginProperty , Value = Constants.ZERO_PADDING},
                new Setter {Property = Border.PaddingProperty,Value= Constants.ZERO_PADDING },
                new Setter {Property = Border.StrokeProperty,Value=Colors.Transparent},
                new Setter {Property = Border.StrokeThicknessProperty,Value=1},
                new Setter {Property = Border.StrokeShapeProperty,Value=new RoundRectangle
                {
                    CornerRadius = new CornerRadius(_controlCornerRadius)
                }},
                new Setter {Property = Border.BackgroundColorProperty, Value = new AppThemeBindingExtension{Light = Colors.Transparent, Dark = _genericBackgroundColor } }
            }
        };
        VisualState state = new()
        {
            Name = VisualStateManager.CommonStates.Normal,
            Setters =
            {
                new Setter {Property = Border.BackgroundColorProperty,  Value =_genericBackgroundColor },
            }
        };
        VisualState state1 = new()
        {
            Name = VisualStateManager.CommonStates.Selected,
            Setters =
            {
                new Setter {Property = Border.BackgroundColorProperty,  Value = _tertiaryAppColor },
            }
        };
        VisualStateGroup visualStateGroup = new() { Name = nameof(VisualStateManager.CommonStates) };
        visualStateGroup.States.Add(state);
        visualStateGroup.States.Add(state1);

        VisualStateGroupList list = new() { };
        list.Add(visualStateGroup);

        Style pancakeStyle = new(typeof(Border))
        {
            Setters =
            {
                new Setter {Property = Border.FlowDirectionProperty,Value=_appFlowDirection },
                new Setter {Property = Border.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter {Property = Border.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter {Property = Border.MarginProperty , Value = Constants.ZERO_PADDING},
                new Setter {Property = Border.PaddingProperty,Value= Constants.ZERO_PADDING },
                new Setter {Property = Border.StrokeProperty,Value=_separatorAndDisableColor},
                new Setter {Property = Border.StrokeThicknessProperty,Value=1},
                new Setter {Property = Border.StrokeShapeProperty,Value=new RoundRectangle
                {
                    CornerRadius = new CornerRadius(_controlCornerRadius)
                }},
            }
        };
        Style pancakeControlStyle = new(typeof(Border))
        {
            Setters =
            {
                new Setter {Property = Border.FlowDirectionProperty,Value=_appFlowDirection },
                new Setter {Property = VisualStateManager.VisualStateGroupsProperty, Value = list},
                new Setter {Property = Border.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter {Property = Border.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter {Property = Border.MarginProperty , Value = Constants.ZERO_PADDING},
                new Setter {Property = Border.PaddingProperty,Value= Constants.ZERO_PADDING },
                new Setter {Property = Border.StrokeProperty,Value=_separatorAndDisableColor},
                new Setter {Property = Border.StrokeThicknessProperty,Value=1},
                new Setter {Property = Border.StrokeShapeProperty,Value=new RoundRectangle
                {
                    CornerRadius = new CornerRadius(_controlCornerRadius),

                }},
            }
        };
        Style pancakeTabStyle = new(typeof(Border))
        {
            BasedOn = pancakeStyle,
            Setters =
            {
                new Setter {Property = Border.HorizontalOptionsProperty, Value = LayoutOptions.StartAndExpand },
                new Setter {Property = Border.VerticalOptionsProperty, Value = LayoutOptions.Start },
                new Setter {Property = Border.StrokeProperty,Value = _primaryAppColor},
                new Setter {Property = Border.MarginProperty , Value = new Thickness(0, 15, 0, 5)},
            }
        };
        Style defaultPancakeStyle = new(typeof(Border))
        {
            Setters =
            {
                new Setter {Property = Border.FlowDirectionProperty,Value= _appFlowDirection },
                new Setter {Property = Border.StrokeShapeProperty,Value=new RoundRectangle
                {
                    CornerRadius = new CornerRadius(_controlCornerRadius)
                }},
                new Setter {Property = Border.StrokeProperty, Value = _primaryAppColor},
                new Setter {Property = Border.StrokeThicknessProperty, Value = 3},
                new Setter {Property = Border.PaddingProperty, Value = new Thickness(15)},
            }
        };
        Style statusPancakeStyle = new(typeof(Border))
        {
            Setters =
            {
                new Setter {Property = Border.FlowDirectionProperty,Value= _appFlowDirection },
                new Setter {Property = Border.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter {Property = Border.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand },
                new Setter {Property = Border.PaddingProperty , Value = new Thickness(0,3)},
                new Setter {Property = Border.WidthRequestProperty,Value= 90 },
                new Setter {Property = Border.StrokeProperty,Value=_separatorAndDisableColor},
                new Setter {Property = Border.StrokeThicknessProperty,Value=1},
                new Setter {Property = Border.StrokeShapeProperty,Value=new RoundRectangle
                {
                    CornerRadius = new CornerRadius(_controlCornerRadius)
                }},
                new Setter {Property = Border.BackgroundColorProperty, Value = new AppThemeBindingExtension{Light=Colors.Transparent,Dark=_genericBackgroundColor } }
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_STATUS_PANCAKE_STYLE, statusPancakeStyle);
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_PANCACKEVIEW_STYLE, defaultPancackeStyle);
        Application.Current.Resources.Add(StyleConstants.ST_PANCAKE_TAB_STYLE, pancakeTabStyle);
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_PANCAKE_STYLE, defaultPancakeStyle);
        Application.Current.Resources.Add(StyleConstants.ST_PANCAKE_STYLE, pancakeStyle);
        Application.Current.Resources.Add(StyleConstants.ST_PANCAKE_CONTROL_STYLE, pancakeControlStyle);

        Style messagePopupStyle = new(typeof(Border))
        {
            BasedOn = pancakeStyle,
            Setters =
            {
                new Setter {Property = Border.VerticalOptionsProperty, Value = LayoutOptions.Center },
                new Setter {Property = Border.BackgroundColorProperty, Value = Color.FromArgb(StyleConstants.DEFAULT_BACKGROUND_COLOR) },
                new Setter {Property = Border.MarginProperty,Value= new Thickness(25,0) },
                new Setter {Property = Border.PaddingProperty, Value = new Thickness(5)},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_MESSAGE_POPUP_STYLE, messagePopupStyle);

       
    }
}