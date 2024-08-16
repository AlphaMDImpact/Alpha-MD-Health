using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Custom Slider Control
    /// </summary>
    public class CustomSliderView : StackLayout
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="props">properties </param>
        /// <param name="slider">slider view</param>
        /// <param name="labelHandleHeight">Label height</param>
        public CustomSliderView(SliderViewPropertiesModel props, View slider, int labelHandleHeight)
        {
            if (props.MinLabelPosition == props.MaxLabelPosition)
            {
                return;
            }
            if (props.MinLabelPosition == GridLabelPosition.TopRight || props.MinLabelPosition == GridLabelPosition.TopLeft)
            { //todo
                //this.Add(new LabelGrid(props.MinLabelPosition, props.MinLabelText, labelHandleHeight, props.TextColor, props.FontSize, new Thickness(10, 0), props.BackgroundColor, props.HandleColor, props.HandleThickness));
                this.Add(slider);
                //this.Add(new LabelGrid(props.MaxLabelPosition, props.MaxLabelText, labelHandleHeight, props.TextColor, props.FontSize, new Thickness(10, 0), props.BackgroundColor, props.HandleColor, props.HandleThickness));
            }
            else
            { //todo
                //this.Add(new LabelGrid(props.MaxLabelPosition, props.MaxLabelText, labelHandleHeight, props.TextColor, props.FontSize, new Thickness(10, 0), props.BackgroundColor, props.HandleColor, props.HandleThickness));
                this.Add(slider);
                //this.Add(new LabelGrid(props.MinLabelPosition, props.MinLabelText, labelHandleHeight, props.TextColor, props.FontSize, new Thickness(10, 0), props.BackgroundColor, props.HandleColor, props.HandleThickness));
            }
        }
    }

    /// <summary>
    /// GridPoint
    /// </summary>
    internal struct GridPoint : IEquatable<GridPoint>
    {
        public int Row { get; set; }

        public int Column { get; set; }

        public GridPoint(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }

        public bool Equals(GridPoint other)
        {
            return Row == other.Row && Column == other.Column;
        }
    }

    internal class LabelGrid : Grid
    {
        private double _firstRowHeight = 0.1;
        private double _secondRowHeight = 0.1;
        private double _thirdRowHeight = 0.8;
        private double _firstColWidth = 0.1;
        private double _secondColWidth = 0.9;
        private Thickness _paddingForLabelText = new Thickness(10, 0, 0, 0);
        private GridPoint _horizontalLinePoint;
        private GridPoint _verticalLinePoint;
        private GridPoint _labelTextPoint;
        private TextAlignment _alignmentForlabel = TextAlignment.Start;
        private TextAlignment _verticalAlignmentForlabel = TextAlignment.Start;
        private LayoutOptions HorizontalLineLayout { get; set; } = LayoutOptions.End;
        private LayoutOptions VerticalLineLayout { get; set; } = LayoutOptions.Start;
        private LayoutOptions LabelTextLayout { get; set; } = LayoutOptions.Start;
        private GridPoint EmptyBoxPoint { get; set; }

        public LabelGrid(GridLabelPosition labelPosition, string labelString, int labelHeight, Color labelColor, NamedSize labelFontSize, Thickness paddingForLabels, Color bgColor, Color handleColor, double handleThickness)
        {
            switch (labelPosition)
            {
                case GridLabelPosition.TopLeft:
                    TopLeft();
                    break;
                case GridLabelPosition.BottomLeft:
                    BottomLeft();
                    break;
                case GridLabelPosition.TopRight:
                    TopRight();
                    break;
                case GridLabelPosition.BottomRight:
                    BottomRight();
                    break;
                default://to do
                    break;
            }
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_firstRowHeight, GridUnitType.Star) });
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_secondRowHeight, GridUnitType.Star) });
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_thirdRowHeight, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_firstColWidth, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_secondColWidth, GridUnitType.Star) });
            this.ColumnSpacing = 0;
            this.RowSpacing = 0;
            this.Padding = paddingForLabels;
            this.BackgroundColor = bgColor;
            this.VerticalOptions = LayoutOptions.FillAndExpand;
            this.HorizontalOptions = LayoutOptions.FillAndExpand;
            if (labelHeight > 0)
            {
                this.HeightRequest = labelHeight;
            }
            this.AddChild(this, new ContentView
            {
                //  BackgroundColor = Color.DeepPink,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Content = new BoxView
                {
                    VerticalOptions = HorizontalLineLayout,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = handleThickness,
                    BackgroundColor = handleColor
                },
                Padding = new Thickness(GenericMethods.GetPlatformSpecificValue(0, 5, 0), 0, GenericMethods.GetPlatformSpecificValue(0, 5, 0), 0)
            }, _horizontalLinePoint);
            this.AddChild(this, new ContentView
            {
                // BackgroundColor=Color.Peru,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Content = new BoxView
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = VerticalLineLayout,
                    WidthRequest = handleThickness,
                    BackgroundColor = handleColor
                },
                Padding = new Thickness(GenericMethods.GetPlatformSpecificValue(0, 5, 0), 0, GenericMethods.GetPlatformSpecificValue(0, 5, 0), 0)
            }, _verticalLinePoint, 2);
            this.AddChild(this, new ContentView
            {

                // BackgroundColor = Color.CadetBlue,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = _paddingForLabelText,
                Content = new Label
                {
                    Text = labelString,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HorizontalTextAlignment = _alignmentForlabel,
                    VerticalTextAlignment = _verticalAlignmentForlabel,
                    TextColor = labelColor
                }
            }, _labelTextPoint, 3);
            // bad hack to get fontsize from device as above code is async and runs on different than UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                new Label
                {
                    Text = labelString,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HorizontalTextAlignment = _alignmentForlabel,
                    VerticalTextAlignment = _verticalAlignmentForlabel,
                    TextColor = labelColor
                }.FontSize = Device.GetNamedSize(labelFontSize, typeof(Label));
            });
        }

        private void BottomRight()
        {
            _thirdRowHeight = _secondColWidth = _secondRowHeight = 0.1;
            _firstRowHeight = 0.8;
            _firstColWidth = 0.9;
            VerticalLineLayout = LayoutOptions.End;
            HorizontalLineLayout = LayoutOptions.Start;
            LabelTextLayout = LayoutOptions.End;
            _paddingForLabelText = new Thickness(0, 0, 10, 0);
            EmptyBoxPoint = new GridPoint(0, 0);
            _verticalLinePoint = new GridPoint(0, 1);
            _horizontalLinePoint = new GridPoint(2, 1);
            _labelTextPoint = new GridPoint(0, 0);
            _alignmentForlabel = TextAlignment.End;
            _verticalAlignmentForlabel = TextAlignment.End;
        }

        private void TopRight()
        {
            _firstRowHeight = _secondColWidth = _secondRowHeight = 0.1;
            _thirdRowHeight = 0.8;
            _firstColWidth = 0.9;
            VerticalLineLayout = LayoutOptions.End;
            HorizontalLineLayout = LayoutOptions.End;
            LabelTextLayout = LayoutOptions.Start;
            _paddingForLabelText = new Thickness(0, 0, 10, 0);
            EmptyBoxPoint = new GridPoint(2, 0);
            _verticalLinePoint = new GridPoint(1, 1);
            _horizontalLinePoint = new GridPoint(0, 1);
            _labelTextPoint = new GridPoint(0, 0);
            _alignmentForlabel = TextAlignment.End;
        }

        private void BottomLeft()
        {
            _thirdRowHeight = _firstColWidth = _secondRowHeight = 0.1;
            _firstRowHeight = 0.8;
            _secondColWidth = 0.9;
            VerticalLineLayout = LayoutOptions.Start;
            HorizontalLineLayout = LayoutOptions.Start;
            LabelTextLayout = LayoutOptions.End;
            _paddingForLabelText = new Thickness(10, 0, 0, 0);
            EmptyBoxPoint = new GridPoint(0, 1);
            _verticalLinePoint = new GridPoint(0, 0);
            _horizontalLinePoint = new GridPoint(2, 0);
            _labelTextPoint = new GridPoint(0, 1);
            _verticalAlignmentForlabel = TextAlignment.End;
        }

        private void TopLeft()
        {
            _firstRowHeight = _firstColWidth = _secondRowHeight = 0.1;
            _thirdRowHeight = 0.8;
            _secondColWidth = 0.9;
            VerticalLineLayout = LayoutOptions.Start;
            HorizontalLineLayout = LayoutOptions.End;
            LabelTextLayout = LayoutOptions.Start;
            _paddingForLabelText = new Thickness(10, 0, 0, 0);
            EmptyBoxPoint = new GridPoint(2, 1);
            _verticalLinePoint = new GridPoint(1, 0);
            _horizontalLinePoint = new GridPoint(0, 0);
            _labelTextPoint = new GridPoint(0, 1);
        }

        public void AddChild(Grid grid, View view, GridPoint point, int rowspan = 1, int columnspan = 1)
        {
            if (point.Row < 0 || point.Column < 0 || rowspan <= 0 || columnspan <= 0 || view == null)
            {
                return;
            }
            Grid.SetRow(view, point.Row);
            Grid.SetRowSpan(view, rowspan);
            Grid.SetColumn(view, point.Column);
            Grid.SetColumnSpan(view, columnspan);
            grid.Add(view);
        }
    }
}