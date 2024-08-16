using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Serialization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom radio image list
    /// </summary>
    public class CustomRadioImageList : StackLayout
    {
        private readonly CustomLabel _lblTitle;

        /// <summary>
        /// custom radio image rendrer property
        /// </summary>
        [DataMember]
        public ObservableCollection<CustomRadioImage> Rads { get; set; }

        /// <summary>
        /// Custom radio image list initializer
        /// </summary>
        /// <param name="screenWidth">screen wisth data</param>
        public CustomRadioImageList(double screenWidth)
        {
            Rads = new ObservableCollection<CustomRadioImage>();
            // Add Label to StackLayout
            _lblTitle = new CustomLabel { WidthRequest = screenWidth };
            _lblTitle.SetBinding(CustomLabel.TextProperty, new Binding { Source = this, Path = Constants.CELL_TITLE });
            this.Add(_lblTitle);
            Spacing = GenericMethods.GetPlatformSpecificValue(3, 0, 0);
            double thickness = (double)Application.Current.Resources[StyleConstants.ST_APP_COMPONENT_PADDING];
            Padding = new Thickness(-thickness, GenericMethods.GetPlatformSpecificValue(-20, -40, 0), 0, 0);

        }

        /// <summary>
        /// event custom focused
        /// </summary>
        public event EventHandler<FocusEventArgs> CustomFocused;


        /// <summary>
        /// Focus mthod to invoke event
        /// </summary>
        public new bool Focus()
        {
            CustomFocused?.Invoke(this, (FocusEventArgs)EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Item source property with property changed event handler
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.
            Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CustomRadioImageList), propertyChanged: OnItemsSourceChanged);

        /// <summary>
        /// Selected Index property with OnSelectedIndexChanged event handler
        /// </summary>
        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.
            Create(nameof(SelectedIndex), typeof(int), typeof(CustomRadioImageList), -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        public static readonly BindableProperty ValidationsProperty = BindableProperty.
            Create(nameof(Validations), typeof(List<KeyValuePair<ValidationType, string>>), typeof(CustomRadioImageList), new List<KeyValuePair<ValidationType, string>>());

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        [DataMember]
        public List<KeyValuePair<ValidationType, string>> Validations
        {
            get => (List<KeyValuePair<ValidationType, string>>)GetValue(ValidationsProperty);
            set => SetValue(ValidationsProperty, value);
        }

        /// <summary>
        ///  Validation pattern
        /// </summary>
        public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(CustomRadioImageList), string.Empty);

        /// <summary>
        /// Validation pattern
        /// </summary>
        public string Pattern
        {
            get => (string)GetValue(PatternProperty);
            set => SetValue(PatternProperty, value);
        }

        /// <summary>
        ///  Validation range
        /// </summary>
        public static readonly BindableProperty RangeProperty = BindableProperty.Create(nameof(Range), typeof(string), typeof(CustomRadioImageList), string.Empty);

        /// <summary>
        /// Validation range
        /// </summary>
        public string Range
        {
            get => (string)GetValue(RangeProperty);
            set => SetValue(RangeProperty, value);
        }

        /// <summary>
        /// Gets or Sets the ItemSource
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or Sets the Selected Index
        /// </summary>
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets the Style of the RadioButtons. Style should be of type VHSRadioButton
        /// </summary>
        public Style RadioButtonStyle { get; set; }

        /// <summary>
        /// Gets or sets the Style of the VHSLabel. Style should be of type VHSLabel
        /// </summary>
        public Style TitleLabelStyle
        {
            get => _lblTitle.Style;
            set => _lblTitle.Style = value;
        }

        /// <summary>
        /// Checked Changed event
        /// </summary>
        public event EventHandler<CustomEventArgs> OnCheckedChangedCustom;

        /// <summary>
        /// Selected Index Changed Event
        /// </summary>
        public event EventHandler<EventArgs> OnSelectionChanged;

        private static void OnItemsSourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            CustomRadioImageList radButtons = bindable as CustomRadioImageList;
            radButtons.Rads.Clear();
            radButtons.Children.Clear();
            // Add the title Label back to the StackLayout
            radButtons.Add(radButtons._lblTitle);
            if (newvalue != null)
            {
                int radIndex = 0;
                foreach (object item in (IEnumerable)newvalue)
                {
                    CustomRadioImage rad = new()
                    {
                        Style = radButtons.RadioButtonStyle,
                        RadioImageId = radIndex,
                        ImageString = (byte[])item,
                        BackgroundColor = Colors.Transparent
                    };
                    rad.CheckedChanged += radButtons.OnCheckedChanged;
                    radButtons.Rads.Add(rad);
                    radButtons.Add(rad);
                    radIndex++;
                }
                radButtons.Add(new BoxView { Color = radButtons.BackgroundColor, HeightRequest = 0.25 });
            }
        }

        private void OnCheckedChanged(object sender, bool e)
        {
            if (!e)
            {
                return;
            }

            CustomRadioImage selectedRad = sender as CustomRadioImage;
            SetValue(SelectedIndexProperty, Rads.IndexOf(selectedRad));
            OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            foreach (CustomRadioImage rad in Rads)
            {
                if (!selectedRad.RadioImageId.Equals(rad.RadioImageId))
                {
                    rad.BackgroundColor = Colors.Transparent;
                    rad.Checked = false;
                }
                else
                {
                    rad.BackgroundColor = Colors.Transparent;
                    if (OnCheckedChangedCustom != null)
                    {
                        OnCheckedChangedCustom.Invoke(sender, new CustomEventArgs { Value = rad.RadioImageId });
                    }
                }
            }
        }

        private static void OnSelectedIndexChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (Convert.ToInt32(newvalue, CultureInfo.CurrentCulture) == -1)
            {
                return;
            }
            CustomRadioImageList bindableRadioGroup = bindable as CustomRadioImageList;
            foreach (CustomRadioImage rad in bindableRadioGroup.Rads)
            {
                if (rad.RadioImageId == bindableRadioGroup.SelectedIndex)
                {
                    rad.Checked = true;
                }
            }
        }
    }
}
