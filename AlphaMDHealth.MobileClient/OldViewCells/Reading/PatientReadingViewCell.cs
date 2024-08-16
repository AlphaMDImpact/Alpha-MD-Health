using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// View cell for readings list
/// </summary>
public class PatientReadingViewCell : ContentView //todo:PancakeView
{
    /// <summary>
    /// View cell for readings list
    /// </summary>
    /// <param name="isHorizontal">if is for horizontal list</param>
    /// <param name="isCarauselView">Width of card to display</param>
    /// <param name="tileHeight">height of title label</param>
    /// <param name="reading">reading data to load on view cell</param>
    public PatientReadingViewCell(bool isHorizontal, bool isCarauselView, double tileHeight, PatientReadingDTO reading = null)
    {
        var padding = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_PADDING], CultureInfo.InvariantCulture);
        Grid vitalCell = new Grid
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_END_TO_END_GRID_STYLE],
            Padding = new Thickness(padding),
            RowSpacing = 5,
            ColumnSpacing = 2
        };
        CustomLabelControl titleLabel = new CustomLabelControl(LabelType.PrimarySmallLeft)
        {
            LineBreakMode = LineBreakMode.WordWrap,
            VerticalOptions = LayoutOptions.Start,
        };
        Label valueLabel = new Label
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_LEFT_LABEL_STYLE],
        };
        //todo:
        //CustomReadingDetailsControl readingControl = new CustomReadingDetailsControl
        //{
        //    HorizontalOptions = LayoutOptions.FillAndExpand
        //};
        if (isHorizontal)
        {
            if (isCarauselView)
            {
                vitalCell.HeightRequest = tileHeight;
            }
            else
            {
                vitalCell.WidthRequest = App._essentials.GetPreferenceValue(StorageConstants.PR_SCREEN_WIDTH_KEY,
                    (double)0) / new OnIdiom<double> { Phone = 2, Tablet = 4 } - (new OnIdiom<double> { Phone = 3.7, Tablet = 3.1 } * padding);
            }
            vitalCell.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star)},
                new RowDefinition { Height = GridLength.Auto }
            };
            vitalCell.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star }
            };
            //todo:vitalCell.Add(readingControl, 0, 2);
        }
        else
        {
            vitalCell.HeightRequest = tileHeight;
            vitalCell.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) }
            };
            vitalCell.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(ShellMasterPage.CurrentShell?.CurrentPage?.ToString()?
                    .EndsWith(Constants.PATIENTS_PAGE_CONSTANT, StringComparison.InvariantCultureIgnoreCase) ?? true
                    && Device.RuntimePlatform == Device.iOS ? 1.1 : 1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(ShellMasterPage.CurrentShell?.CurrentPage?.ToString()?
                    .EndsWith(Constants.PATIENTS_PAGE_CONSTANT, StringComparison.InvariantCultureIgnoreCase) ?? true
                    && Device.RuntimePlatform == Device.iOS ? 0.9 : 1.3, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Star }
            };
            //todo:
            //vitalCell.Add(readingControl, 1, 0);
            //Grid.SetRowSpan(readingControl, 2);
        }
        vitalCell.Add(titleLabel, 0, 0);
        vitalCell.Add(valueLabel, 0, 1);
        if (!isCarauselView)
        {
            CustomLabelControl dateLabel = new CustomLabelControl(LabelType.SecondrySmallLeft) { LineBreakMode = LineBreakMode.WordWrap, };
            if (isHorizontal)
            {
                vitalCell.Add(dateLabel, 0, 3);
            }
            else
            {
                dateLabel.HorizontalOptions = LayoutOptions.End;
                dateLabel.VerticalOptions = LayoutOptions.End;
                dateLabel.VerticalTextAlignment = TextAlignment.End;
                dateLabel.HorizontalTextAlignment = TextAlignment.End;
                vitalCell.Add(dateLabel, 2, 1);
            }
            if (reading == null)
            {
                dateLabel.SetBinding(CustomLabelControl.TextProperty, nameof(PatientReadingDTO.LatestValueDateText));
            }
            else
            {
                dateLabel.Text = reading.LatestValueDateText;
            }
        }
        if (reading == null)
        {
            valueLabel.SetBinding(Label.FormattedTextProperty, nameof(PatientReadingDTO.ReadingUnitValue));
            titleLabel.SetBinding(CustomLabelControl.TextProperty, nameof(PatientReadingDTO.Title));
            //todo: readingControl.SetBinding(CustomReadingDetailsControl.ValueProperty, ".");
        }
        else
        {
            valueLabel.FormattedText = reading.ReadingUnitValue;
            titleLabel.Text = reading.Title;
            //todo: readingControl.SetValue(CustomReadingDetailsControl.ValueProperty, reading);
        }
        Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE];
        Content = vitalCell;
    }
}