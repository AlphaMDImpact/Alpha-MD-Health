using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public partial class AppStyles
{
    private void CreateMauiLayoutStyles()
    {
        CreateGridLayoutStyle();
        CreateStackLayoutStyle();
    }

    private void CreateStackLayoutStyle()
    {
        var defaultEndToEndLayoutStyle = new Style(typeof(StackLayout))
        {
            Setters =
            {
                new Setter {Property = StackLayout.FlowDirectionProperty , Value = _appFlowDirection},
                new Setter {Property = StackLayout.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter {Property = StackLayout.VerticalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter {Property = StackLayout.OrientationProperty, Value = StackOrientation.Vertical},
                new Setter {Property = StackLayout.SpacingProperty, Value = 0},
                new Setter {Property = StackLayout.PaddingProperty, Value =0},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_END_TO_END_LAYOUT_KEY, defaultEndToEndLayoutStyle);

        var defaultStackLayoutLayoutStyle = new Style(typeof(StackLayout))
        {
            Setters =
            {
                new Setter {Property = StackLayout.FlowDirectionProperty , Value = _appFlowDirection},
                new Setter {Property = StackLayout.PaddingProperty, Value = _appPadding },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_STACK_LAYOUT_KEY, defaultStackLayoutLayoutStyle);

        Style defaultAttachmentLayoutStyle = new(typeof(StackLayout))
        {
            BasedOn = defaultEndToEndLayoutStyle,
            Setters =
            {
                new Setter {Property = StackLayout.OrientationProperty, Value = StackOrientation.Horizontal},
                new Setter {Property = StackLayout.HorizontalOptionsProperty , Value = LayoutOptions.Start},
                new Setter {Property = StackLayout.SpacingProperty, Value = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING] },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_ATTACHMENT_LAYOUT_KEY, defaultAttachmentLayoutStyle);
    }

    private void CreateGridLayoutStyle()
    {
        Style endToEndGridStyle = new(typeof(Grid))
        {
            Setters =
            {
                new Setter { Property = Grid.FlowDirectionProperty , Value = _appFlowDirection },
                new Setter { Property = Grid.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Grid.VerticalOptionsProperty,Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Grid.ColumnSpacingProperty, Value = Constants.ZERO_PADDING},
                new Setter { Property = Grid.RowSpacingProperty, Value = Constants.ZERO_PADDING},
                new Setter { Property = Grid.MarginProperty, Value = Constants.ZERO_PADDING},
                new Setter { Property = Grid.PaddingProperty , Value = Constants.ZERO_PADDING },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_END_TO_END_GRID_STYLE, endToEndGridStyle);

        Style defaultGridStyle = new(typeof(Grid))
        {
            Setters =
            {
                new Setter { Property = Grid.FlowDirectionProperty , Value = _appFlowDirection},
                new Setter { Property = Grid.HorizontalOptionsProperty, Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Grid.VerticalOptionsProperty,Value = LayoutOptions.FillAndExpand},
                new Setter { Property = Grid.ColumnSpacingProperty, Value = Constants.ZERO_PADDING},
                new Setter { Property = Grid.RowSpacingProperty, Value = Constants.ZERO_PADDING},
                new Setter { Property = Grid.MarginProperty, Value = Constants.ZERO_PADDING},
                new Setter { Property = Grid.PaddingProperty , Value = new Thickness(_appPadding)},
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_DEFAULT_GRID_STYLE, defaultGridStyle);

        Style listViewCellGridStyle = new(typeof(Grid))
        {
            BasedOn = endToEndGridStyle,
            Setters =
            {
                new Setter { Property = Grid.PaddingProperty , Value = new Thickness(_appPadding/2, _appPadding)},
                new Setter { Property = Grid.VerticalOptionsProperty,Value = LayoutOptions.CenterAndExpand },
                new Setter { Property = Grid.ColumnSpacingProperty, Value = _appPadding/2 },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_LIST_VIEWCELL_GRID_STYLE, listViewCellGridStyle);


        Style headerGridStyle = new(typeof(Grid))
        {
            BasedOn = listViewCellGridStyle,
            Setters =
            {
                new Setter { Property = Grid.BackgroundColorProperty, Value = _separatorAndDisableColor },
            }
        };
        Application.Current.Resources.Add(StyleConstants.ST_HEADER_GRID_STYLE, headerGridStyle);
    }
}