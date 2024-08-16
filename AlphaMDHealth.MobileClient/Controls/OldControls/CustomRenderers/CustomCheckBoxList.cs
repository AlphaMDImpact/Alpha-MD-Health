using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using AlphaMDHealth.Utility;
using Microsoft.Maui.Controls;
namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// CustomCheckBox List
    /// </summary>
    public class CustomCheckBoxList : Grid
    {
        private readonly ObservableCollection<CustomCheckBox> _checkBoxes;

        /// <summary>
        /// Gets or sets the Style of the RadioButtons. Style should be of type VHSRadioButton
        /// </summary>
        public Style CheckBoxStyle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs> OnCheckedChangedCustom;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<FocusEventArgs> CustomFocused;

        /// <summary>
        /// Item source property with property changed event handler
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CustomCheckBoxList), propertyChanged: OnItemsSourceChanged);

        /// <summary>
        /// Selected Index property
        /// </summary>
        private readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndexValues), typeof(List<string>), typeof(List<string>));

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        private readonly BindableProperty ValidationsProperty = BindableProperty.Create(nameof(Validations), typeof(List<KeyValuePair<ValidationType, string>>), typeof(CustomCheckBoxList), new List<KeyValuePair<ValidationType, string>>());

        /// <summary>
        ///  Validation range
        /// </summary>
        private readonly BindableProperty RangeProperty = BindableProperty.Create(nameof(Range), typeof(string), typeof(CustomCheckBoxList), string.Empty);

        /// <summary>
        ///  Validation pattern
        /// </summary>
        public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(CustomCheckBoxList), string.Empty);

        /// <summary>
        ///  HasBorder
        /// </summary>
        public static readonly BindableProperty CheckBoxTypeProperty = BindableProperty.Create(nameof(CheckBoxType), typeof(ListStyleType), typeof(CustomCheckBoxList), default);

        /// <summary>
        /// Validation pattern
        /// </summary>
        public string Pattern
        {
            get => (string)GetValue(PatternProperty);
            set => SetValue(PatternProperty, value);
        }

        /// <summary>
        /// Gets or Sets the Selected Index list
        /// </summary>  
        public ListStyleType CheckBoxType
        {
            get => (ListStyleType)GetValue(CheckBoxTypeProperty);
            set => SetValue(CheckBoxTypeProperty, value);
        }

        /// <summary>
        /// ItemSource property
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Viewes to display with checkbox list property w
        /// </summary>
        public static readonly BindableProperty RightViewesProperty = BindableProperty.Create(nameof(RightViewes), typeof(Dictionary<long, View>), typeof(CustomCheckBoxList));


        /// <summary>
        /// Viewes to display with checkbox list
        /// </summary>
        public Dictionary<long, View> RightViewes
        {
            get => (Dictionary<long, View>)GetValue(RightViewesProperty);
            set => SetValue(RightViewesProperty, value);
        }

        /// <summary>
        /// pipe seperated Validation types
        /// </summary>
        [DataMember]
        public IList<KeyValuePair<ValidationType, string>> Validations
        {
            get => (IList<KeyValuePair<ValidationType, string>>)GetValue(ValidationsProperty);
            set => SetValue(ValidationsProperty, value);
        }

        /// <summary>
        /// Flag to decide Apply Margin To CheckBox
        /// </summary>
        public bool AppliyMarginToCheckBox
        {
            get; set;

        }
        /// <summary>
        /// Validation range
        /// </summary>
        public string Range
        {
            get => (string)GetValue(RangeProperty);
            set => SetValue(RangeProperty, value);
        }

        /// <summary>
        /// Gets or Sets the Selected Index list
        /// </summary>       
        [DataMember]
        public List<string> SelectedIndexValues
        {
            get => (List<string>)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="screenWidth"></param>
        public CustomCheckBoxList(double screenWidth)
        {
            _checkBoxes = new ObservableCollection<CustomCheckBox>();
            // Add Label to StackLayout
            CustomLabel lblTitle = new CustomLabel { BackgroundColor = Colors.Red, WidthRequest = screenWidth };
            lblTitle.SetBinding(CustomLabel.TextProperty, new Binding { Source = this, Path = Constants.CELL_TITLE });
            RowSpacing = GenericMethods.GetPlatformSpecificValue(0, 40, 0);
            var padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
            Padding = new Thickness(DeviceInfo.Platform == DevicePlatform.iOS && AppStyles.DefaultFlowDirection == Microsoft.Maui.FlowDirection.RightToLeft ? 1.5 * padding : padding, GenericMethods.GetPlatformSpecificValue(0, 0, 0));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new bool Focus()
        {
            CustomFocused?.Invoke(this, (FocusEventArgs)EventArgs.Empty);
            return true;
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            CustomCheckBoxList checkBoxList = bindable as CustomCheckBoxList;
            checkBoxList._checkBoxes.Clear();
            checkBoxList.Children.Clear();
            checkBoxList.RowDefinitions.Clear();
            checkBoxList.ColumnDefinitions.Clear();
            if (newvalue != null)
            {
                int rowIndex = 0, colIndex = 0, horzontalRowIndex = 0;
                var padding = (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING];
                switch (checkBoxList.CheckBoxType)
                {
                    case ListStyleType.BoxView:
                        checkBoxList.RowSpacing = padding;
                        break;
                    case ListStyleType.SeperatorView:
                        checkBoxList.Padding = new Thickness(10, 0, 0, 0);
                        checkBoxList.RowSpacing = 0;
                        break;
                    case ListStyleType.HorizontalView:

                        checkBoxList.Padding = new Thickness(2, 0, 0, 0);

                        checkBoxList.RowSpacing = new OnIdiom<double> { Phone = GenericMethods.GetPlatformSpecificValue(0, 30, 0), Tablet = GenericMethods.GetPlatformSpecificValue(0, padding, 0) };

                        break;
                }
                AddColumnDefinations(checkBoxList, checkBoxList);
                foreach (object item in (IEnumerable)newvalue)
                {
                    ((CustomCheckBox)item).BackgroundColor = Colors.Transparent;
                    ((CustomCheckBox)item).HorizontalOptions = LayoutOptions.FillAndExpand;
                    ((CustomCheckBox)item).CheckedChanged += checkBoxList.VHSCheckBoxList_CheckedChanged;
                    checkBoxList._checkBoxes.Add((CustomCheckBox)item);
                    if (checkBoxList.AppliyMarginToCheckBox)
                    {
                        ((CustomCheckBox)item).Margin = AppStyles.DefaultFlowDirection == Microsoft.Maui.FlowDirection.LeftToRight ? new Thickness(1.5 * padding, 0, 0, 0) : new Thickness(0, 0, 1.5 * padding, 0);
                    }
                    if (checkBoxList.CheckBoxType == ListStyleType.HorizontalView)
                    {
                        if (colIndex == 0)
                        {
                            AddRowDefinations(checkBoxList);
                        }
                    }
                    else
                    {
                        AddRowDefinations(checkBoxList);
                    }
                    switch (checkBoxList.CheckBoxType)
                    {
                        case ListStyleType.BoxView:
                            ((CustomCheckBox)item).AddResizeText = true;
                            var layout = new Grid
                            {
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                BackgroundColor = Colors.Transparent,
                                Padding = new Thickness(2 * padding, GenericMethods.GetPlatformSpecificValue(0, padding, 0), padding, GenericMethods.GetPlatformSpecificValue(0, padding, 0)),
                            };
                            AddRowDefinations(layout);
                            AddColumnDefinations(checkBoxList, layout);
                            AddViewes(checkBoxList, layout, 0, 0, (CustomCheckBox)item, ((CustomCheckBox)item).CheckBoxId);
                            checkBoxList.Add(new Border
                            {
                                Style = (Style)Application.Current.Resources[StyleConstants.ST_PANCAKE_STYLE],
                                BackgroundColor = Colors.Transparent,
                                Content = layout
                            }, 0, rowIndex);
                            break;
                        case ListStyleType.SeperatorView:
                            ((CustomCheckBox)item).AddResizeText = true;
                            AddViewes(checkBoxList, checkBoxList, rowIndex, colIndex, (CustomCheckBox)item, ((CustomCheckBox)item).CheckBoxId);
                            rowIndex++;
                            AddSeperator(checkBoxList, rowIndex, padding);
                            break;
                        case ListStyleType.HorizontalView:

                            AddViewes(checkBoxList, checkBoxList, horzontalRowIndex, colIndex, (CustomCheckBox)item, ((CustomCheckBox)item).CheckBoxId);
                            if (checkBoxList.Children.Count % 2 == 0)
                            {
                                colIndex = 0;
                                horzontalRowIndex++;
                            }
                            else
                            {
                                colIndex++;
                            }
                            break;
                        default:
                            ((CustomCheckBox)item).AddResizeText = true;
                            AddViewes(checkBoxList, checkBoxList, rowIndex, colIndex, (CustomCheckBox)item, ((CustomCheckBox)item).CheckBoxId);
                            break;
                    }
                    rowIndex++;
                }
            }
        }

        private static void AddRowDefinations(Grid layout)
        {
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        private static void AddColumnDefinations(CustomCheckBoxList control, Grid layout)
        {
            if (control.CheckBoxType == ListStyleType.HorizontalView)
            {
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            else
            {
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                if (control.RightViewes != null && (control != layout || control.CheckBoxType != ListStyleType.BoxView))
                {
                    layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                }
            }
        }

        private static void AddViewes(CustomCheckBoxList checkBoxList, Grid layout, int rowIndex, int colIndex, View view, string id)
        {
            layout.Add(view, colIndex, rowIndex);
            if (checkBoxList.RightViewes != null && checkBoxList.RightViewes.Count > 0 && (checkBoxList != layout || checkBoxList.CheckBoxType != ListStyleType.BoxView))
            {
                layout.Add(checkBoxList.RightViewes[Convert.ToInt64(id)], 1, rowIndex);
            }
        }

        private static void AddSeperator(CustomCheckBoxList checkBoxList, int checkIndex, double padding)
        {
            checkBoxList.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            var separator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = (Color)Application.Current.Resources[StyleConstants.ST_SEPARATOR_AND_DISABLE_COLOR_STYLE],
                Margin = new Thickness(-(padding * 2), GenericMethods.GetPlatformSpecificValue(0, padding, 0))
            };
            checkBoxList.Add(separator, 0, checkIndex);
            Grid.SetColumnSpan(separator, 2);
        }

        private void VHSCheckBoxList_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            string boxId = ((CustomCheckBox)sender).CheckBoxId;
            CustomCheckBox selectedBox = _checkBoxes.FirstOrDefault(c => c.CheckBoxId == boxId);
            SelectedIndexValues = SelectedIndexValues ?? new List<string>();
            List<string> selectedValues = SelectedIndexValues;
            if (SelectedIndexValues.Contains(boxId))
            {
                selectedBox.BackgroundColor = Colors.Transparent;
                selectedBox.IsChecked = false;
                selectedBox.Color = selectedBox.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_TERTIARY_TEXT_COLOR];
                selectedValues.Remove(boxId);
            }
            else
            {
                selectedBox.BackgroundColor = Colors.Transparent;
                selectedBox.IsChecked = true;
                selectedBox.Color = selectedBox.TextColor = (Color)Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR];
                selectedValues.Add(boxId);
            }
            SetValue(SelectedIndexProperty, selectedValues);
            OnCheckedChangedCustom?.Invoke(this, EventArgs.Empty);
        }
    }
}
