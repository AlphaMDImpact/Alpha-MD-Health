using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Color = Microsoft.Maui.Graphics.Color;

namespace AlphaMDHealth.MobileClient;

internal class CarouselViewCell : Grid
{
    public CarouselViewCell(FieldTypes controlType)
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE];
        RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Star },
            new RowDefinition { Height = GridLength.Star },
        };
        ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star }
        };

        var overLay = new Grid()
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        AmhImageControl carouselImage = new AmhImageControl(FieldTypes.ImageControl)
        {
            Opacity = 0.5,
            ZIndex = -1
        };
        carouselImage._image.Aspect = Aspect.Fill;
        carouselImage.SetBinding(AmhImageControl.IconProperty, nameof(OptionModel.Icon));
        carouselImage.SetBinding(AmhImageControl.ValueProperty, nameof(OptionModel.GroupName));

        AmhLabelControl cellHeader = new AmhLabelControl(FieldTypes.HtmlPrimaryCenterLabelControl)
        {
            ZIndex = 2,
        };
        cellHeader.SetBinding(AmhLabelControl.ValueProperty, nameof(OptionModel.ParentOptionText));

        AmhLabelControl cellDescription = new AmhLabelControl(FieldTypes.HtmlPrimaryCenterLabelControl)
        {
            ZIndex = 2,
            Padding = Constants.CONTENT_PADDING
        };
        cellDescription.SetBinding(AmhLabelControl.ValueProperty, nameof(OptionModel.OptionText));

        if (controlType == FieldTypes.HalfOverlayCarouselControl)
        {
            cellHeader.FieldType = cellDescription.FieldType = FieldTypes.HtmlLightCenterLabelControl;
            cellHeader.BackgroundColor = cellDescription.BackgroundColor = Color.FromArgb(StyleConstants.OVERLAY_COLOR);
            carouselImage.Opacity = 1;
        }

        if (controlType == FieldTypes.FullOverlayCarouselControl)
        {
            cellHeader.FieldType = cellDescription.FieldType = FieldTypes.HtmlLightCenterLabelControl;
            overLay.BackgroundColor = Color.FromArgb(StyleConstants.OVERLAY_COLOR);
            carouselImage.Opacity = 1;
        }
        overLay.Add(cellHeader, 0, 2);
        overLay.Add(cellDescription, 0, 3);

        this.Add(carouselImage, 0, 0);
        Grid.SetRowSpan(carouselImage, 3);
        this.Add(overLay, 0, 0);
        Grid.SetRowSpan(overLay, 3);
    }
}