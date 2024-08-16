using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient;

public class PrescriptionViewCell : ViewCell
{
    private readonly CustomCheckBoxListControl _medicationAdditionalNotesMultipleCheckBox;
    private readonly CustomLabelControl _medicationNameLabel;
    private readonly CustomLabelControl _formattedDateLabel;
    private readonly CustomLabelControl _howOften;
    //todo: private readonly CustomStepProgressBarControl _progressBar;
    private readonly Grid _mainGrid;
    private readonly BoxView _separator;

    public PrescriptionViewCell()
    {
        _mainGrid = new Grid
        {
            Padding = new Thickness(0, 0, 0, 0),
            HeightRequest = 300,
            InputTransparent = true,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition {Width = GridLength.Auto},
            }
        };
        _medicationNameLabel = new CustomLabelControl(LabelType.PrimaryMediumLeft) { LineBreakMode = LineBreakMode.WordWrap }; ;
        _medicationNameLabel.SetBinding(Label.TextProperty, nameof(PatientMedicationModel.ShortName));
        _formattedDateLabel = new CustomLabelControl(LabelType.SecondrySmallLeft) { LineBreakMode = LineBreakMode.WordWrap }; ;
        _formattedDateLabel.SetBinding(Label.TextProperty, nameof(PatientMedicationModel.FormattedDate));
        _howOften = new CustomLabelControl(LabelType.PrimaryMediumLeft);
        _howOften.SetBinding(Label.TextProperty, nameof(PatientMedicationModel.HowOftenString));
        _medicationAdditionalNotesMultipleCheckBox = new CustomCheckBoxListControl()
        {
            CheckBoxType = new OnIdiom<ListStyleType>
            {
                Phone = ListStyleType.HorizontalView,
                Tablet = ListStyleType.HorizontalView
            },
            HeightRequest = -1
        };
        //todo:
        //_progressBar = new CustomStepProgressBarControl
        //{
        //    VerticalOptions = LayoutOptions.StartAndExpand,
        //    HorizontalOptions = LayoutOptions.StartAndExpand,
        //};
        _medicationAdditionalNotesMultipleCheckBox.SetBinding(CustomCheckBoxListControl.ItemsSourceProperty, nameof(PatientMedicationModel.AdditionalNotesOptionSelect));
        _separator = new BoxView
        {
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE],
            IsVisible = true
        };
        _mainGrid.Add(_medicationNameLabel, 0, 0);
        _mainGrid.Add(_formattedDateLabel, 0, 1);
        _mainGrid.Add(_howOften, 0, 2);

        //todo:
        //_progressBar.SetBinding(CustomStepProgressBarControl.StepsProperty, nameof(PatientMedicationModel.steps));
        //_progressBar.SetBinding(CustomStepProgressBarControl.TagsSourceProperty, nameof(PatientMedicationModel.FrequencyOptionsString));
        //_mainGrid.Add(_progressBar, 0, 3);

        _mainGrid.Add(_medicationAdditionalNotesMultipleCheckBox, 0, 4);
        _mainGrid.Add(_separator, 0, 5);
        View = _mainGrid;
    }
}