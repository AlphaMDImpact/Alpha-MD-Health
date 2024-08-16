using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// view cell for readings list
/// </summary>
public class TwoRowReadingViewCell : ReadingViewCell
{
    /// <summary>
    /// view cell for readings list
    /// </summary>
    public TwoRowReadingViewCell()
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE];
        Padding = new Thickness(Convert.ToInt32(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture), Convert.ToInt32(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture) / 2);
        Grid contentGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            //HeightRequest = ((double)Application.Current.Resources[LibStyleConstants.ST_DEFAULT_BUTTON_HEIGHT] + 10),
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        var iconsLayout = new Grid
        {
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            HorizontalOptions = LayoutOptions.Start,
            RowDefinitions = new RowDefinitionCollection {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            }
        };
        iconsLayout.Add(_sourceIcon, 0, 0);
        iconsLayout.Add(_mealTypeIcon, 1, 0);
        iconsLayout.Add(_notesIcon, 2, 0);

        contentGrid.Add(_dateTimeLabel, 0, 0);
        contentGrid.Add(iconsLayout, 0, 1);
        contentGrid.Add(_valueTypeLabel, 1, 0);
        contentGrid.Add(_valueLabel, 1, 1);

        Content = contentGrid;
    }
}