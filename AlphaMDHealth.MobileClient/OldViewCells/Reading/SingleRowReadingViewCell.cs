using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Viewcell to display reading data in single row
/// </summary>
public class SingleRowReadingViewCell : ReadingViewCell
{
    /// <summary>
    /// Viewcell constructor to display reading data in single row
    /// </summary>
    public SingleRowReadingViewCell()
    {
        Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE];
        //todo: Padding = new Thickness(Convert.ToInt32(Application.Current.Resources[LibStyleConstants.ST_APP_PADDING], CultureInfo.CurrentCulture), Convert.ToInt32(Application.Current.Resources[LibStyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture) / 2);
        Grid contentGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            HeightRequest = (double)Application.Current.Resources[StyleConstants.ST_DEFAULT_CONTROL_HEIGHT],
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                // DateTime or Source Icon
                new ColumnDefinition { Width = GridLength.Star },
                // Meal Type Icon
                new ColumnDefinition { Width = GridLength.Auto },
                // Notes Icon
                new ColumnDefinition { Width = GridLength.Auto },
                // DateTime
                new ColumnDefinition { Width =  GridLength.Auto },
                // Value
                new ColumnDefinition { Width =  GridLength.Auto },

            },
            ColumnSpacing = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture) / 2
        };
        contentGrid.Add(_dateTimeLabel, 0, 1);
        contentGrid.Add(_sourceIcon, 1, 1);
        contentGrid.Add(_notesIcon, 2, 1);
        contentGrid.Add(_valueLabel, 3, 1);
        contentGrid.Add(_mealTypeIcon, 4, 1);
        Content = contentGrid;
    }
}