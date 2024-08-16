using System.Globalization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// CustomSliderControl
    /// </summary>
    public class CustomSliderControl : ContentView
    {
        private CustomSlider _sliAnswer;
        private readonly CustomLabelControl _sliderCurrentValue;
        private readonly Frame _labelContainer;
        private readonly List<double> _sliderSteps = new List<double>();

        /// <summary>
        /// to get control text value
        /// </summary>
        public string GetSliderValue()
        {
            return _sliderCurrentValue?.Text ?? string.Empty;
        }

        /// <summary>
        /// CustomSliderControl
        /// </summary>
        public CustomSliderControl(SliderViewModel slider)
        {
            _sliderCurrentValue = new CustomLabelControl(LabelType.SecondryAppSmallCenter);
            SliderViewPropertiesModel props = new SliderViewPropertiesModel
            {
                MinLabelPosition = GridLabelPosition.TopLeft,
                MaxLabelPosition = GridLabelPosition.BottomRight,
                //todo
                //TextColor = (Color)Application.Current.Resources[LibStyleConstants.ST_PRIMARY_APP_COLOR],
                //HandleColor = (Color)Application.Current.Resources[LibStyleConstants.ST_PRIMARY_APP_COLOR],
                HandleThickness = 0.5,
                //FontSize = NamedSize.Medium,
                MinLabelText = string.IsNullOrWhiteSpace(slider.SlidebarLeftLabelText) ? slider.SlidebarMinValue : GetMinValue(slider),
                MaxLabelText = string.IsNullOrWhiteSpace(slider.SlidebarRightLabelText) ? slider.SlidebarMaxValue : GetMaxValue(slider),
                UnitCode = slider.UnitCode
            };
            _labelContainer = new Frame
            {
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                HasShadow = false,
                CornerRadius = 3,
                Padding = new Thickness(5),
            };
            //todo
            //CustomSliderView sliderView = new CustomSliderView(props, RenderSliderQuestion(_labelContainer, slider), 120)
            //{
            //    VerticalOptions = LayoutOptions.FillAndExpand,
            //    HorizontalOptions = LayoutOptions.FillAndExpand,
            //    Spacing = 0,
            //    Rotation = slider.IsVerticalSlider ? -90 : 0,
            //};
            //AutomationProperties.SetIsInAccessibleTree(sliderView, true);

            //var mainLayout = new RelativeLayout();

            //if (slider.IsVerticalSlider)
            //{

            //    mainLayout.Add(sliderView,
            //    xConstraint: Constraint.RelativeToParent((parent) =>
            //    {
            //        return parent.X - ((parent.Height - parent.Width) / 2);
            //    }),
            //    yConstraint: Constraint.RelativeToParent((parent) =>
            //    {
            //        return MobileConstants.IsTablet ? 0 : parent.Y + 150;
            //    }),
            //    widthConstraint: Constraint.RelativeToParent((parent) =>
            //    {
            //        return parent.Height;
            //    }),
            //    heightConstraint: Constraint.RelativeToParent((parent) =>
            //    {
            //        return parent.Width * (MobileConstants.IsIosPlatform ? 0.8 : 0.7);
            //    }));
            //    mainLayout.Add(_labelContainer,
            //               yConstraint: Constraint.RelativeToParent((parent) => (parent.Y + 150)),
            //               widthConstraint: Constraint.RelativeToParent((parent) => parent.Width * 0.92));
            //}
            //else
            //{
            //    mainLayout.HorizontalOptions = LayoutOptions.FillAndExpand;
            //    mainLayout.Padding = new Thickness(0);

            //    mainLayout.Add(sliderView, Constraint.RelativeToParent(x => 0), Constraint.RelativeToParent(y => 0),
            //    Constraint.RelativeToParent((parent) => parent.Width - 3));
            //    mainLayout.Add(new ContentView { Content = _labelContainer },
            //            yConstraint: Constraint.RelativeToParent((parent) => (parent.Y + 120) - (50 + 30)),
            //            widthConstraint: Constraint.RelativeToParent((parent) => parent.Width));
            //}
            //mainLayout.Margin = new Thickness(0, 15);
            //Content = mainLayout;

        }

        private static string GetMaxValue(SliderViewModel slider)
        {
            return (string.IsNullOrWhiteSpace(slider.SlidebarMaxValue)
                ? slider.SlidebarRightLabelText
                : string.Format(CultureInfo.InvariantCulture, "{0} ({1})", slider.SlidebarRightLabelText, slider.SlidebarMaxValue));
        }

        private static string GetMinValue(SliderViewModel slider)
        {
            return (string.IsNullOrWhiteSpace(slider.SlidebarMinValue)
                ? slider.SlidebarLeftLabelText
                : string.Format(CultureInfo.InvariantCulture, "{0} ({1})", slider.SlidebarLeftLabelText, slider.SlidebarMinValue));
        }

        private ContentView RenderSliderQuestion(Frame labelContainer, SliderViewModel control)
        {
            double min = string.IsNullOrWhiteSpace(control.SlidebarMinValue) ? 0 : Convert.ToDouble(control.SlidebarMinValue, CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE));
            _sliAnswer = new CustomSlider
            {
                HeightRequest = 30,
                SliderColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR],
                ImageResource = ImageConstants.I_PROGRESSBAR_WHITE_ICON_PNG,
                Maximum = string.IsNullOrWhiteSpace(control.SlidebarMaxValue) ? 0.1 : Convert.ToDouble(control.SlidebarMaxValue, CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)),
                Minimum = min
            };
            double step = control.SlidebarStepSize.Equals(0) ? 0.1 : control.SlidebarStepSize;
            _sliAnswer.Value = string.IsNullOrWhiteSpace(control.Value)
                ? min
                : (Convert.ToDouble(control.Value.Trim(), CultureInfo.GetCultureInfo(Constants.ENGLISH_US_LOCALE)));
            _sliderCurrentValue.Text = string.IsNullOrWhiteSpace(control.Value)
                ? GetCurrentValue(control)
                : Math.Round(Convert.ToDouble(control.Value.Trim(), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
            _sliAnswer.DragStarted += _sliAnswer_DragStarted;
            _sliAnswer.ValueChanged += (sender, e) =>
            {
                _sliderCurrentValue.IsVisible = !string.IsNullOrWhiteSpace(control.Value);
                _labelContainer.IsVisible = _sliderCurrentValue.IsVisible;
                control.Value = CalculateSliderValue(e.NewValue);
                _sliderCurrentValue.Text = Math.Round(Convert.ToDouble(control.Value.Trim(), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
            };
            // Create Steps for Slider
            var minValue = _sliAnswer.Minimum;
            while (minValue <= _sliAnswer.Maximum)
            {
                _sliderSteps.Add(minValue);
                minValue += step;
            }

            _sliderCurrentValue.IsVisible = !string.IsNullOrWhiteSpace(control.Value);
            _labelContainer.IsVisible = _sliderCurrentValue.IsVisible;
            //StackLayout sliderStack = new StackLayout() {VerticalOptions=LayoutOptions.FillAndExpand};
            //sliderStack.Add(_sliAnswer);

            if (string.IsNullOrWhiteSpace(control.UnitCode))
            {
                labelContainer.Content = _sliderCurrentValue;
            }
            else
            {
                labelContainer.Content = new StackLayout
                {
                    Style = (Style)Application.Current.Resources[StyleConstants.ST_DEFAULT_END_TO_END_LAYOUT_KEY],
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 5,
                    Children =
                    {
                        _sliderCurrentValue,
                        new CustomLabelControl(LabelType.PrimaryAppExtraSmallRight)
                        {
                           Text = control.UnitCode
                        }
                    }
                };
            }
            return new ContentView
            {
                Content = _sliAnswer,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(GenericMethods.GetPlatformSpecificValue(10, 0, 0), 0)
            };
        }

        private void _sliAnswer_DragStarted(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("=======drag started=======");
        }

        private static string GetCurrentValue(SliderViewModel control)
        {
            return string.IsNullOrWhiteSpace(control.SlidebarMinValue)
                ? Constants.NUMBER_ZERO
                : Math.Round(Convert.ToDouble(control.SlidebarMinValue, CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
        }

        private string CalculateSliderValue(double newValue)
        {
            //Set Value as Step which is closest to newValue
            _sliAnswer.Value = _sliderSteps.OrderBy(x => Math.Abs(x - newValue)).First();
            return _sliAnswer.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}