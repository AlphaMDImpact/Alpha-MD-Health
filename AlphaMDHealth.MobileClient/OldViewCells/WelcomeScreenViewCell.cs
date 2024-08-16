using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// App intro viewcell
/// </summary>
public class WelcomeScreenViewCell : Grid
{
    public WelcomeScreenViewCell()
    {
        AmhImageControl icon = new AmhImageControl(FieldTypes.ImageControl);
        icon._image.Aspect = Aspect.AspectFill;
        //{ Aspect = Aspect.AspectFill };
        //icon.SetBinding(AmhImageControl.SourceProperty, nameof(AppIntroModel.ImageBytes));
        //new Binding { Path = nameof(AppIntroModel.ImageBytes), Converter = new ByteArrayToImageSourceConverter() }

        AmhLabelControl cellHeader = new AmhLabelControl(FieldTypes.HtmlPrimaryCenterLabelControl);//(LabelType.PrimaryLargeCenter) { IsHtmlLabel = true };
        cellHeader.SetBinding(AmhLabelControl.ValueProperty, nameof(AppIntroModel.HeaderText));

        AmhLabelControl cellDescription = new AmhLabelControl(FieldTypes.HtmlPrimaryCenterLabelControl)//(LabelType.PrimarySmallCenter)
        {
            Padding = new Thickness(0, 0, 0, (double)Application.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT])
            //IsHtmlLabel = true,
        };
        cellDescription.SetBinding(AmhLabelControl.ValueProperty, nameof(AppIntroModel.SubHeaderText));

        Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE];
        RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Star },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto }
        };
        ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star }
        };
        VerticalOptions = LayoutOptions.FillAndExpand;

        this.Add(icon, 0, 0);
        Grid.SetRowSpan(icon, 3);
        this.Add(cellHeader, 0, 1);
        this.Add(cellDescription, 0, 2);
    }
}