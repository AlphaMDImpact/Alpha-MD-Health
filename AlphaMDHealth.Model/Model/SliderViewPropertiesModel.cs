using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// SliderViewPropertiesModel
    /// </summary>
    public class SliderViewPropertiesModel
    {
        /// <summary>
        /// min label position
        /// </summary>
        public GridLabelPosition MinLabelPosition { get; set; }
        /// <summary>
        /// max label position
        /// </summary>
        public GridLabelPosition MaxLabelPosition { get; set; }
        /// <summary>
        /// min label text
        /// </summary>
        public string MinLabelText { get; set; }
        /// <summary>
        /// max label text
        /// </summary>
        public string MaxLabelText { get; set; }

        //todo:
        ///// <summary>
        ///// font size
        ///// </summary>
        //public NamedSize FontSize { get; set; }
        ///// <summary>
        ///// bg color
        ///// </summary>
        //public Color BackgroundColor { get; set; }
        ///// <summary>
        ///// text color
        ///// </summary>
        //public Color TextColor { get; set; }
        ///// <summary>
        ///// handle color
        ///// </summary>
        //public Color HandleColor { get; set; }

        /// <summary>
        /// HandleThickness
        /// </summary>
        public double HandleThickness { get; set; }
        /// <summary>
        /// UnitCode
        /// </summary>
        public string UnitCode { get; set; }
    }

    /// <summary>
    /// SliderViewModel
    /// </summary>
    public class SliderViewModel
    {
        /// <summary>
        /// SlidebarMinValue
        /// </summary>
        public string SlidebarMinValue { get; set; }
        /// <summary>
        /// SlidebarMaxValue
        /// </summary>
        public string SlidebarMaxValue { get; set; }
        /// <summary>
        /// SlidebarStepSize
        /// </summary>
        public double SlidebarStepSize { get; set; }
        /// <summary>
        /// SlidebarLeftLabelText
        /// </summary>
        public string SlidebarLeftLabelText { get; set; }
        /// <summary>
        /// SlidebarRightLabelText
        /// </summary>
        public string SlidebarRightLabelText { get; set; }
        /// <summary>
        /// SlidebarQuestionText
        /// </summary>
        public string SlidebarQuestionText { get; set; }
        /// <summary>
        ///  QuestionValue
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// unit code
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// IsVerticalSlider
        /// </summary>
        public bool IsVerticalSlider { get; set; }
    }
}
