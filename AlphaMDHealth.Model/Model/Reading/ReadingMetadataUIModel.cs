namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Reading meta data model
    /// </summary>
    public class ReadingMetadataUIModel : ReadingModel
    {
        /// <summary>
        /// Reading absolute range min value
        /// </summary>
        public double? AbsoluteMinValue { get; set; }

        /// <summary>
        /// Reading absolute range max value
        /// </summary>
        public double? AbsoluteMaxValue { get; set; }

        /// <summary>
        /// Reading absolute range band color
        /// </summary>
        public string AbsoluteBandColor { get; set; }

        /// <summary>
        /// Reading target range min value
        /// </summary>
        public double? TargetMaxValue { get; set; }

        /// <summary>
        /// Reading target range max value
        /// </summary>
        public double? TargetMinValue { get; set; }

        /// <summary>
        /// Reading target range band color
        /// </summary>
        public string TargetBandColor { get; set; }

        /// <summary>
        /// Reading normal range max value
        /// </summary>
        public double? NormalMaxValue { get; set; }

        /// <summary>
        /// Reading normal range max value
        /// </summary>
        public double? NormalMinValue { get; set; }

        /// <summary>
        /// Reading normal range band color
        /// </summary>
        public string NormalBandColor { get; set; }

        /// <summary>
        /// Default duration to fetch Reading readings from db in BL
        /// </summary>
        public string DefaultDuration { get; set; }

        /// <summary>
        /// Reading unit
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Reading unit
        /// </summary>
        public string UnitIdentifier { get; set; }

        /// <summary>
        /// Reading unit
        /// </summary>
        public string BaseUnitIdentifier { get; set; }

        /// <summary>
        /// Should unit be displayed
        /// </summary>
        public bool ShowUnit { get; set; }

        /// <summary>
        /// Should display daily data as sum
        /// </summary>
        public bool ShouldDailySum { get; set; }

        /// <summary>
        /// Should plot average values in chart
        /// </summary>
        public bool ShouldPlotAverage { get; set; }

        /// <summary>
        /// Merge value for entire day
        /// </summary>
        public string MergeValueForEntireDay { get; set; }

        /// <summary>
        /// Latest Reading Date Time
        /// </summary>
        public DateTimeOffset? ReadingDateTime { get; set; }

        /// <summary>
        /// Value of Latest Reading 
        /// </summary>
        public double? ReadingValue { get; set; }

        /// <summary>
        /// Value 2 of Latest Reading 
        /// </summary>
        public string ReadingValue2 { get; set; }

        /// <summary>
        /// Show spinner control
        /// </summary>
        public bool ShowSpinner { get; set; }

        /// <summary>
        /// Chart Width % factor of screen width
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Chart Height % factor of screen height
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Color of the Grapgh Plot
        /// </summary>
        public string PlotColor { get; set; }

        /// <summary>
        /// Is Line Plot have a Gradient
        /// </summary>
        public bool IsGradiant { get; set; }

        /// <summary>
        /// Show Axes
        /// </summary>
        public bool ShowAxes { get; set; }

        /// <summary>
        /// Show plot only
        /// </summary>
        public bool ShowSummary { get; set; }

        /// <summary>
        /// Axes Day Format
        /// </summary>
        public string DayFormat { get; set; }

        /// <summary>
        /// Axes Year Format
        /// </summary>
        public string YearFormat { get; set; }

        /// <summary>   
        /// Axes Month Format
        /// </summary>
        public string MonthFormat { get; set; }

        /// <summary>   
        /// Axes Hour Format
        /// </summary>
        public string HourFormat { get; set; }

        /// <summary>   
        /// Axes Time Format
        /// </summary>
        public string TimeFormat { get; set; }

        /// <summary>
        /// List Item Left Header 
        /// </summary>
        public string ListItemLeftHeader { get; set; }

        /// <summary>
        /// Duration filter position on top then true else false
        /// </summary>
        public bool ShowTopDurationFilter { get; set; }
    }
}