using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhCardsViewCell : Grid
{
    internal AmhCardsViewCell()
    {
        this.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        this.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        this.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        this.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        this.HorizontalOptions=LayoutOptions.Start;
        this.ColumnSpacing = 0;

        var icon = new AmhImageControl(FieldTypes.ImageControl) { ImageHeight = AppImageSize.ImageSizeD, ImageWidth = AppImageSize.ImageSizeD , HorizontalOptions = LayoutOptions.Start };
        icon.SetBinding(AmhImageControl.IconProperty, nameof(OptionModel.Icon));
        icon.SetBinding(AmhImageControl.IsVisibleProperty, nameof(OptionModel.Icon), converter: new StringToBooleanConvertorForVisibility());
        this.Add(icon, 0, 0);

        var label1 = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterBoldLabelControl) { HorizontalOptions = LayoutOptions.EndAndExpand, VerticalOptions = LayoutOptions.Center };
        label1.SetBinding(AmhLabelControl.ValueProperty, nameof(OptionModel.GroupName));
        this.Add(label1, 1, 0);

        var label2 = new AmhLabelControl(FieldTypes.PrimaryLargeHStartVCenterBoldLabelControl) { HorizontalOptions = LayoutOptions.StartAndExpand };
        label2.SetBinding(AmhLabelControl.ValueProperty, nameof(OptionModel.OptionText));
        label2.StyleName = StyleConstants.ST_CARD_LABEL_STYLE;
        this.Add(label2, 0, 1);

        var label3 = new AmhLabelControl(FieldTypes.TertiaryMicroHStartVCenterLabelControl) { HorizontalOptions = LayoutOptions.StartAndExpand, VerticalOptions = LayoutOptions.End };
        label3.SetBinding(AmhLabelControl.ValueProperty, nameof(OptionModel.ParentOptionText));
        this.Add(label3, 1, 1);
    }
}