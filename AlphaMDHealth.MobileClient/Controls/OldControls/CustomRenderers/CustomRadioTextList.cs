using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Serialization;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Device = Microsoft.Maui.Controls.Device;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom radio text list
    /// </summary>
    public class CustomRadioTextList : Grid
    {
        private readonly CustomLabel _lblTitle;

        /// <summary>
        /// Custom radio text list initializer
        /// </summary>
        public CustomRadioTextList()
        {
            Rads = new ObservableCollection<CustomRadioText>();
            // Add Label to StackLayout
            _lblTitle = new CustomLabel { BackgroundColor = Colors.Pink, };
            _lblTitle.SetBinding(CustomLabel.TextProperty, new Binding { Source = this, Path = Constants.CELL_TITLE });
            RowSpacing = GenericMethods.GetPlatformSpecificValue(0, 0, 0);
            Padding = GenericMethods.GetPlatformSpecificValue(new Thickness(0, 5), new Thickness(-5, 5), new Thickness(0));
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
        /// event custom focused
        /// </summary>
        public bool IsHorizontal { get; set; }

        /// <summary>
        /// Radio width
        /// </summary>
        public double RadioWidth { get; set; }

        /// <summary>
        /// To set radio text vertical alignment property
        /// </summary>
        public long VerticalAlignment { get; set; }

        /// <summary>
        /// Item source property with property changed event handler
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.
            Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CustomRadioTextList), propertyChanged: OnItemsSourceChanged);

        /// <summary>
        /// Selected Index property with OnSelectedIndexChanged event handler
        /// </summary>
        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.
            Create(nameof(SelectedIndex), typeof(int), typeof(CustomRadioTextList), -1, BindingMode.TwoWay, propertyChanged: OnSelectedIndexChanged);

        /// <summary>
        /// Title property to be shown on top of the radio list
        /// </summary>
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(string));

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        public static readonly BindableProperty ValidationsProperty = BindableProperty.
            Create(nameof(Validations), typeof(List<KeyValuePair<ValidationType, string>>), typeof(CustomRadioTextList), new List<KeyValuePair<ValidationType, string>>());

        /// <summary>
        /// custom radio image rendrer property
        /// </summary>
        [DataMember]
        public ObservableCollection<CustomRadioText> Rads { get; set; }

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
        public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(CustomRadioTextList), string.Empty);

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
        public static readonly BindableProperty RangeProperty = BindableProperty.Create(nameof(Range), typeof(string), typeof(CustomRadioTextList), string.Empty);

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
        /// Gets or sets the Title to be shown on top of the radio list
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
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
            CustomRadioTextList radButtons = bindable as CustomRadioTextList;
            radButtons.Rads.Clear();

            radButtons.ColumnDefinitions.Clear();
            radButtons.RowDefinitions.Clear();
            radButtons.Children.Clear();
            if (newvalue != null)
            {
                int left = 0, top = 0;
                int radIndex = 0;
                foreach (object item in (IEnumerable)newvalue)
                {
                    CustomRadioText rad = new()
                    {
                        Style = radButtons.RadioButtonStyle,
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(CustomRadioText)),
                        Value = item.ToString(),
                        RadioId = radIndex,
                        VerticalAlignment = radButtons.VerticalAlignment
                    };
                    
                    rad.CheckedChanged += radButtons.Rad_CheckedChanged;
                    rad.BackgroundColor = Colors.Transparent;
                    radButtons.Rads.Add(rad);
                    if (radButtons.IsHorizontal)
                    {
                        rad.Margin = new Thickness(0, -20, 0, 0);
                        rad.IsHorizontal = true;
                        radButtons.ColumnSpacing = 15;
                        radButtons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    }
                    else
                    {
                        radButtons.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    }
                    radButtons.Add(rad, left, top);
                    if(radButtons.IsHorizontal)
                    {
                        left++;
                        top =0;
                    }
                    else
                    {
                        left = 0;
                        top++;
                    }
                    radIndex++;
                }
            }
        }

        private void Rad_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            CustomRadioText selectedRad = sender as CustomRadioText;
            SetValue(SelectedIndexProperty, Rads.IndexOf(selectedRad));
            foreach (CustomRadioText rad in Rads)
            {
                if (!selectedRad.RadioId.Equals(rad.RadioId))
                {
                    //// rad.BackgroundColor = Color.Transparent;
                    rad.IsChecked = false;
                }
                else
                {
                    ///// rad.BackgroundColor = Color.Transparent;
                    OnCheckedChangedCustom?.Invoke(sender, new CustomEventArgs { Value = rad.RadioId });
                }
            }
        }

        private static void OnSelectedIndexChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (Convert.ToInt32(newvalue, CultureInfo.CurrentCulture) == -1 || oldvalue == newvalue)
            {
                return;
            }
            CustomRadioTextList bindableRadioGroup = bindable as CustomRadioTextList;

            foreach (CustomRadioText rad in bindableRadioGroup.Rads)
            {
                if (rad.RadioId == bindableRadioGroup.SelectedIndex)
                {
                    rad.IsChecked = true;
                    bindableRadioGroup.OnSelectionChanged?.Invoke(bindableRadioGroup, EventArgs.Empty);
                }
                rad.TextColor = rad.IsChecked 
                    ? (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR] 
                    : (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];
            }
        }
    }
}
