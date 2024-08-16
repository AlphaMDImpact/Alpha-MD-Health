namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represnts Custom radio image Renderer
    /// </summary>
    public class CustomRadioImage : View
    {
        /// <summary>
        /// Checkde property with default value as Unchecked/false
        /// </summary>
        public static readonly BindableProperty CheckedProperty = BindableProperty.Create(nameof(Checked), typeof(bool), typeof(CustomRadioImage), false);

        /// <summary>
        /// Text property with default Text as string.Empty
        /// </summary>
        public static readonly BindableProperty ImageStringProperty = BindableProperty.Create(nameof(ImageString), typeof(byte[]), typeof(CustomRadioImage));

        /// <summary>
        /// Identifies the SelectedBackgroundColorProperty bindable property.
        /// </summary>
        public static readonly BindableProperty SelectedBackgroundColorProperty =
            BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(Color), typeof(CustomRadioImage), Colors.Transparent);

        /// <summary>
        /// The checked changed event.
        /// </summary>
        public EventHandler<bool> CheckedChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the control is checked.
        /// </summary>
        /// <value>The checked state.</value>
        public bool Checked
        {
            get => (bool)GetValue(CheckedProperty);
            set
            {
                SetValue(CheckedProperty, value);
                EventHandler<bool> eventHandler = CheckedChanged;
                if (eventHandler != null)
                {
                    eventHandler.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Gets or Sets Text
        /// </summary>
        public byte[] ImageString
        {
            get => (byte[])GetValue(ImageStringProperty);
            set => SetValue(ImageStringProperty, value);
        }

        /// <summary>
        /// Gets or Sets color of selected radio
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get => (Color)GetValue(SelectedBackgroundColorProperty);
            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        /// <summary>
        /// property radio image id
        /// </summary>
        public int RadioImageId { get; set; }
    }
}
