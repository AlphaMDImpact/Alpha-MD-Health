using System.Globalization;
using System.Runtime.Serialization;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomWheelControl : View
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty SelectedItemProperty =
                BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(CustomWheelControl), default, propertyChanged: OnSelectedItemChanged);
        /// <summary>
        /// 
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty FontColorProperty =
                BindableProperty.Create(nameof(FontColor), typeof(Color), typeof(CustomWheelControl), Colors.Black);
        /// <summary>
        /// 
        /// </summary>
        public Color FontColor
        {
            get { return (Color)GetValue(FontColorProperty); }
            set { SetValue(FontColorProperty, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty FontSizeProperty =
                BindableProperty.Create(nameof(FontSize), typeof(double), typeof(CustomWheelControl), 16.0);
        /// <summary>
        /// 
        /// </summary>
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty ItemSourceProperty =
                BindableProperty.Create(nameof(ItemsSource), typeof(List<string>), typeof(CustomWheelControl), default(List<string>), propertyChanged: OnItemsSourceChanged);
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<string> ItemsSource
        {
            get { return (List<string>)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        private IList<string> _items;
        /// <summary>
        /// 
        /// </summary>
        public IList<string> Items
        {
            get
            {
                _items = _items ?? new List<string>();
                return _items;
            }
            private set
            {
                _items = value;
            }
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var picker = bindable as CustomWheelControl;
            picker.Items = picker.Items ?? new List<string>();

            picker.Items.Clear();
            if (newvalue != null)
            {
                //now it works like "subscribe once" but you can improve
                foreach (var item in (List<string>)newvalue)
                {
                    picker.Items.Add(item.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<PickerValueChangedEventArgs> PickerValueChanged;

        private static void OnSelectedItemChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var picker = bindable as CustomWheelControl;
            if (picker != null && picker.PickerValueChanged != null)
            {
                picker.PickerValueChanged(picker, new PickerValueChangedEventArgs(newvalue?.ToString() ?? string.Empty));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class PickerValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string SelectedValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_selectedValue"></param>
        public PickerValueChangedEventArgs(string _selectedValue)
        {
            SelectedValue = _selectedValue;
        }
    }
}

