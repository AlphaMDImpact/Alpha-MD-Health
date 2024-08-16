using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// Default simple view cell
/// </summary>
public class SimpleViewCell : ViewCell
{
    /// <summary>
    /// Defalt constructor
    /// </summary>
    public SimpleViewCell()
    {
        CustomLabelControl languageLabel = new CustomLabelControl(LabelType.PrimaryMediumLeft)
        {
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
        };
        AutomationProperties.SetIsInAccessibleTree(languageLabel, true);
        languageLabel.SetBinding(CustomLabelControl.AutomationIdProperty, nameof(LanguageModel.LanguageCode));
        //languageLabel.SetBinding(CustomLabelControl.TextColorProperty, nameof(LanguageModel.TextColor));
        languageLabel.SetBinding(Label.TextProperty, nameof(LanguageModel.LanguageName));
        Grid mainGrid = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_GRID_STYLE],
            RowSpacing = 10,
            ////Padding = new Thickness(10, 0),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            }
        };
        mainGrid.Add(languageLabel, 0, 0);
        //mainGrid.SetBinding(Grid.BackgroundColorProperty, nameof(LanguageModel.BGColor));
        View = mainGrid;
    }
}