using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Style Configuration Model
    /// </summary>
    public class StyleConfigurationModel
    {
        /// <summary>
        /// Primary app color key
        /// </summary>
        public string PrimaryAppColor { get; set; } = string.Empty;

        /// <summary>
        /// Primary app color key
        /// </summary>
        public bool IsSeperatorBelowHeader { get; set; }

        /// <summary>
        /// Primary app color key
        /// </summary>
        public string GradientStartColor { get; set; } = string.Empty;

        /// <summary>
        /// Strip color used to display sync status color key
        /// </summary>
        public string SyncStatusStripColor { get; set; } = string.Empty;

        /// <summary>
        /// Primary app color key
        /// </summary>
        public string GradientEndColor { get; set; } = string.Empty;

        /// <summary>
        /// Secondary app color key
        /// </summary>
        public string SecondaryAppColor { get; set; } = string.Empty;

        /// <summary>
        /// Generic background color key
        /// </summary>
        public string ControlBackgroundColor { get; set; } = string.Empty;

        /// <summary>
        /// App background color key
        /// </summary>
        public string AppBackgroundColor { get; set; } = string.Empty;

        /// <summary>
        /// Primary text color key
        /// </summary>
        public string PrimaryTextColor { get; set; } = string.Empty;

        /// <summary>
        /// Secondary text color key
        /// </summary>
        public string SecondaryTextColor { get; set; } = string.Empty;

        /// <summary>
        /// Tertiary text color key
        /// </summary>
        public string TertiaryTextColor { get; set; } = string.Empty;

        /// <summary>
        /// Tertiary text color key
        /// </summary>
        public string AccentColor { get; set; } = string.Empty;

        /// <summary>
        /// Tertiary text color key
        /// </summary>
        public string ProgressBarBGColor { get; set; } = string.Empty;

        /// <summary>
        /// Tertiary text color key
        /// </summary>
        public string BeforeLoginHeaderColor { get; set; } = string.Empty;

        /// <summary>
        /// SwitchTrueColor  key
        /// </summary>
        public string SwitchTrueColor { get; set; } = string.Empty;

        /// <summary>
        /// SwitchTrueColor  key
        /// </summary>
        public string SwitchFalseColor { get; set; } = string.Empty;

        /// <summary>
        /// Error color key
        /// </summary>
        public string ErrorColor { get; set; } = string.Empty;

        /// <summary>
        /// MasterErrorColor
        /// </summary>
        public string MasterErrorColor { get; set; } = string.Empty;

        /// <summary>
        /// Success color key
        /// </summary>
        public string SuccessColor { get; set; } = string.Empty;

        /// <summary>
        /// Progress Color
        /// </summary>
        public string ProgressColor { get; set; } = string.Empty;

        /// <summary>
        /// before Login Color
        /// </summary>
        public string BeforLoginColor { get; set; } = string.Empty;

        /// <summary>
        /// before Login Color
        /// </summary>
        public string AfterLoginColor { get; set; } = string.Empty;

        /// <summary>
        /// Seperator color key
        /// </summary>
        public string SeperatorColor { get; set; } = string.Empty;

        /// <summary>
        /// Disabled color key
        /// </summary>
        public string DisabledColor { get; set; } = string.Empty;

        /// <summary>
        /// list selected item color key
        /// </summary>
        public string SelectedItemColor { get; set; } = string.Empty;

        /// <summary>
        /// SearchBackgroundcolor key
        /// </summary>
        public string SearchBackgroundColor { get; set; } = string.Empty;

        /// <summary>
        /// App logo image height 
        /// </summary>
        public int ImageSizeLogoHeight { get; set; }

        /// <summary>
        /// App logo image width 
        /// </summary>
        public int ImageSizeLogoWidth { get; set; }

        /// <summary>
        /// Default Card Height 
        /// </summary>
        public int DefaultCardHeight { get; set; } = 200;

        /// <summary>
        /// App Control Padding
        /// </summary>
        public double AppPadding { get; set; }

        /// <summary>
        /// App top Padding
        /// </summary>
        public double AppTopPadding { get; set; }

        /// <summary>
        /// App Control Padding
        /// </summary>
        public double AppComponentPadding { get; set; }

        /// <summary>
        /// App flow direction
        /// </summary>       
        public AppFlowDirection AppFlowDirection { get; set; }

        /// <summary>
        /// ControlCornerRadius
        /// </summary>
        public float ControlCornerRadius { get; set; }

        /// <summary>
        /// isCircle
        /// </summary>
        public bool IsProfileCircular { get; set; }

        /// <summary>
        /// SearchBackgroundcolor key
        /// </summary>
        public string EmptyMessageColor { get; set; } = string.Empty;

        /// <summary>
        /// BoxShadowColor key
        /// </summary>
        public string BoxShadowColor { get; set; } = string.Empty;

        /// <summary>
        /// isCircle
        /// </summary>
        public bool IsTabletScaleMode { get; set; }
    }
}
