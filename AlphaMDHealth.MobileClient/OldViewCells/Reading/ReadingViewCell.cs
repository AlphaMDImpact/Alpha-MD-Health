using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

/// <summary>
/// View cell for reading list
/// </summary>
public class ReadingViewCell : ContentView //todo: PancakeView
{
    /// <summary>
    /// _dateTimeLabel
    /// </summary>
    protected CustomLabelControl _dateTimeLabel;

    /// <summary>
    /// _sourceIcon
    /// </summary>
    protected CustomImageControl _sourceIcon;

    /// <summary>
    /// _mealTypeIcon
    /// </summary>
    protected CustomImageControl _mealTypeIcon;

    /// <summary>
    /// _notesIcon
    /// </summary>
    protected CustomImageControl _notesIcon;

    /// <summary>
    /// _valueLabel Value and Unit
    /// </summary>
    protected Label _valueTypeLabel;

    /// <summary>
    /// _valueLabel Value and Unit
    /// </summary>
    protected Label _valueLabel;

    protected readonly double _margin = Convert.ToDouble(Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING], CultureInfo.CurrentCulture) / 2;
    
    /// <summary>
    /// Reading view cell for list
    /// </summary>
    public ReadingViewCell()
    {
        _dateTimeLabel = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Start,
            LineBreakMode = LineBreakMode.TailTruncation,
            Margin = new Thickness(0, 0, 0, _margin)
        };
        _dateTimeLabel.SetBinding(CustomLabelControl.TextProperty, nameof(PatientReadingUIModel.ReadingDateTimeText));
        _sourceIcon = new CustomImageControl(AppImageSize.ImageSizeXXS, AppImageSize.ImageSizeXXS, Constants.STRING_SPACE, Constants.STRING_SPACE, false) { HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, _margin, 0) };
        _sourceIcon.SetBinding(CustomImageControl.DefaultValueProperty, nameof(PatientReadingUIModel.ReadingSourceIcon));
        _sourceIcon.SetBinding(CustomImageControl.IsVisibleProperty, nameof(PatientReadingUIModel.ReadingSourceIcon), converter: new StringToBooleanConvertorForVisibility());
        _mealTypeIcon = new CustomImageControl(AppImageSize.ImageSizeXXS, AppImageSize.ImageSizeXXS, Constants.STRING_SPACE, Constants.STRING_SPACE, false) { HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, _margin, 0) };
        _mealTypeIcon.SetBinding(CustomImageControl.DefaultValueProperty, nameof(PatientReadingUIModel.ReadingMomentIcon));
        _mealTypeIcon.SetBinding(CustomImageControl.IsVisibleProperty, nameof(PatientReadingUIModel.ReadingMomentIcon), converter: new StringToBooleanConvertorForVisibility());
        _notesIcon = new CustomImageControl(AppImageSize.ImageSizeXXS, AppImageSize.ImageSizeXXS, Constants.STRING_SPACE, Constants.STRING_SPACE, false) { HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, _margin, 0) };
        _notesIcon.SetBinding(CustomImageControl.DefaultValueProperty, nameof(PatientReadingUIModel.ReadingNotesIcon));
        _notesIcon.SetBinding(CustomImageControl.IsVisibleProperty, nameof(PatientReadingUIModel.ReadingNotesIcon), converter: new StringToBooleanConvertorForVisibility());
        _valueTypeLabel = new CustomLabelControl(LabelType.SecondrySmallLeft)
        {
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.End,
            HorizontalTextAlignment = TextAlignment.End,
        };
        _valueTypeLabel.SetBinding(CustomLabelControl.TextProperty, nameof(PatientReadingUIModel.Reading));
        _valueTypeLabel.SetBinding(CustomLabelControl.IsVisibleProperty, nameof(PatientReadingUIModel.Reading), converter: new StringToBooleanConvertorForVisibility());

        _valueLabel = new Label
        {
            Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_LEFT_LABEL_STYLE],
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            HorizontalTextAlignment = TextAlignment.End,
        };
        _valueLabel.SetBinding(Label.FormattedTextProperty, nameof(PatientReadingUIModel.UnitText));
    }
}