using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhTabViewCell : Grid
{
    internal AmhTabViewCell()
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_LIST_VIEWCELL_GRID_STYLE];
        ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star }
        };
        RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } };

        var icon = new AmhImageControl(FieldTypes.ImageControl) { ImageHeight = AppImageSize.ImageSizeXS, ImageWidth = AppImageSize.ImageSizeXS };
        icon.SetBinding(AmhImageControl.IconProperty, nameof(OptionModel.ParentOptionText));
        icon.SetBinding(AmhImageControl.IsVisibleProperty, nameof(OptionModel.ParentOptionText), converter: new StringToBooleanConvertorForVisibility());
        Add(icon); 
        Grid.SetColumn(icon, 0);

        var label = new AmhLabelControl(FieldTypes.PrimarySmallHVCenterLabelControl);
        label.SetBinding(AmhLabelControl.FieldTypeProperty, nameof(OptionModel.IsSelected), converter: new BoolToControlTypeConverter());
        label.SetBinding(AmhLabelControl.ValueProperty, nameof(OptionModel.OptionText));
        Add(label); 
        Grid.SetColumn(label, 1);
    }
}