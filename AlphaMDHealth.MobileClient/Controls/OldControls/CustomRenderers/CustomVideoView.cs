using AlphaMDHealth.Utility;

namespace AlphaMDHealth.MobileClient
{
    /// <summary>
    /// Represents custom video view
    /// </summary>
    public class CustomVideoView : View
    {
        /// <summary>
        /// handle property
        /// </summary>
        //this hold view id
        public IntPtr Handle { get; set; }

        /// <summary>
        /// Density
        /// </summary>
        //For maintaining the phone resolution and image factor so that the video is not blurred.
        public float Density { get; set; }
        //For maintaining the height and width of video.

        /// <summary>
        /// Video width
        /// </summary>
        public uint VideoWidth => (uint)(WidthRequest * Density);

        /// <summary>Video height
        /// Represents custom time picker
        /// </summary>
        public uint VideoHeight => (uint)(HeightRequest * Density);

        /// <summary>
        /// Represents custom video view initializer
        /// </summary>
        public CustomVideoView()
        {
            Density = GenericMethods.GetPlatformSpecificValue(1.0F, 4.0F, 0.0F);
        }
    }
}
