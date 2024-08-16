using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class AmhTabContentViewCell : DataTemplateSelector
{
    public DataTemplate DefaultTemplate { get; set; }
    public DataTemplate SpecialTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if(item is OptionModel optionModel)
        {
            if(optionModel.OptionText == "Tab1")
            {
                return SpecialTemplate;
            }
        }
        return DefaultTemplate;
    }

    public AmhTabContentViewCell()
    {
        DefaultTemplate = new DataTemplate(() =>
        {
           var  _initialRectangleImageControl = new AmhImageControl(FieldTypes.SquareImageControl) { Value = "Tab2" ,ImageHeight = AppImageSize.ImageSizeXXXL, ImageWidth = AppImageSize.ImageSizeXXXL };

            var grid = new Grid
            {
                _initialRectangleImageControl
            };
            return grid;
        });

        SpecialTemplate = new DataTemplate(() =>
        {
            var _initialCircleImage = new AmhImageControl(FieldTypes.CircleWithBorderImageControl) { ImageHeight = AppImageSize.ImageSizeL, ImageWidth = AppImageSize.ImageSizeL };
            _initialCircleImage.SetBinding(AmhImageControl.ValueProperty, nameof(OptionModel.OptionText));
            var grid = new Grid
            {
                _initialCircleImage
            };
            return grid;
        });
    }
}