using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class EducationCategoryDetailViewCell : Grid
{
    public EducationCategoryDetailViewCell()
    {
        CustomImageControl icon = new CustomImageControl(Constants.STRING_SPACE, Constants.STRING_SPACE);
        icon.SetBinding(CustomImageControl.SourceProperty, Constants.CATEGORY_IMAGE_SOURCE);

        var appPadding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);

        var cellHeader = new CustomLabelControl(LabelType.SecondryAppMediumBoldLeft)
        {
            Padding = new Thickness(appPadding, appPadding, appPadding, 5)
        };
        cellHeader.SetBinding(CustomLabel.TextProperty, Constants.CNT_NAME_TEXT);

        CustomLabelControl cellDescription = new CustomLabelControl(LabelType.SecondryAppExtarSmallLeft)
        {
            Padding = new Thickness(appPadding, Constants.ZERO_PADDING, appPadding, appPadding)
        };
        cellDescription.SetBinding(CustomLabel.TextProperty, Constants.CATEGORY_DESCRIPTION_TEXT);

        RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Star },
        };
        ColumnDefinitions = new ColumnDefinitionCollection
        {
        new ColumnDefinition { Width = GridLength.Star },
        };
        var overLayView = new Frame
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_CARD_OVERLAY_VIEW_FRAME],
            HeightRequest = 120
        };
        this.Add(icon, 0, 0);
        this.Add(overLayView, 0, 0);
        Grid.SetRowSpan(icon, 2);
        Grid.SetRowSpan(overLayView, 2);
        this.Add(cellHeader, 0, 0);
        this.Add(cellDescription, 0, 1);
        Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE];
        WidthRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY, (double)0);
        Padding = new Thickness(Constants.ZERO_PADDING, Constants.ZERO_PADDING, Constants.ZERO_PADDING, MobileConstants.IsTablet ? appPadding : Constants.ZERO_PADDING);
        HeightRequest = 120;
    }
}