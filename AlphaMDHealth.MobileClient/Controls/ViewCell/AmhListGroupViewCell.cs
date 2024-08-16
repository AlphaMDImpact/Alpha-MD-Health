using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

internal class AmhListGroupViewCell : Grid
{
    internal AmhListGroupViewCell(string showMoreText, string showMoreIcon, EventHandler<EventArgs> onGroupShowMoreClicked)
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_HEADER_GRID_STYLE];
        ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star } };
        RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = GridLength.Auto } };

        var leftHeadinglabel = new AmhLabelControl(FieldTypes.PrimarySmallHStartVCenterLabelControl);
        leftHeadinglabel.SetBinding(AmhLabelControl.ValueProperty, "Value");
        this.Add(leftHeadinglabel, 0, 0);

        if (!string.IsNullOrWhiteSpace(showMoreText) || !string.IsNullOrWhiteSpace(showMoreIcon))
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            AmhBaseControl showMore;
            if (!string.IsNullOrWhiteSpace(showMoreIcon))
            {
                showMore = new AmhImageControl(FieldTypes.ImageControl)
                {
                    ImageHeight = AppImageSize.ImageSizeD,
                    ImageWidth = AppImageSize.ImageSizeD,
                    Icon = showMoreIcon
                };
            }
            else
            {
                showMore = new AmhLabelControl(FieldTypes.LinkHStartVCenterLabelControl)
                {
                    Value = showMoreText,
                };
            }
            showMore.OnValueChanged += (sender, e) =>
            {
                onGroupShowMoreClicked?.Invoke(leftHeadinglabel.Value, e);
            };
            this.Add(showMore, 1, 0);
        }
    }
}