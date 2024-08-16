using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// color picker Control
    /// </summary>
    public class CustomColorPickerControl : Picker //todo: ColorWheel
    {
        //todo: private readonly ColorCircle _colorPicker;

        /// <summary>
        /// selected Color Value changed event
        /// </summary>
        public event EventHandler<EventArgs> SelectedColorValueChanged;

        /// <summary>
        /// to get and set control color value
        /// </summary>
        public Color ColorValue
        {
            get;set;
            //todo: 
            //get
            //{
            //    //if (_colorPicker == null)
            //    //{
            //    //    return Colors.Transparent;
            //    //}
            //    //return _colorPicker.SelectedColor;
            //}
            //set
            //{
            //    _colorPicker.SelectedColor = value;
            //}
        }



        /// <summary>
        /// color picker constructor
        /// </summary>
        public CustomColorPickerControl()
        {
            //todo: _colorPicker = new ColorCircle() { WidthRequest = 300, HeightRequest = 300, ShowLuminosityWheel = true };
        }

        //todo:
        ///// <summary>
        ///// selected color changed event
        ///// </summary>
        ///// <param name="color"></param>
        //protected override void SelectedColorChanged(Color color)
        //{
        //    _colorPicker.SelectedColor = color;
        //    _colorPicker.WheelBackgroundColor = color;
        //    SelectedColorValueChanged?.Invoke(this, new EventArgs());
        //}

        /// <summary>
        /// get hex color without alpha 
        /// </summary>
        /// <returns> hex value</returns>
        public string GetHexValue()
        {
            return ColorValue.ToString();
            //todo: return string.Concat(LibConstants.SYMBOL_HASH_STRING, _colorPicker.SelectedColor.ToHex().Substring(3));
        }
    }
}