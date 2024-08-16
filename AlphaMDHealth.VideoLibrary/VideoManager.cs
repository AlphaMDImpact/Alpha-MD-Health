using AlphaMDHealth.Utility;

namespace AlphaMDHealth.VideoLibrary
{
    /// <summary>
    /// Video manager
    /// </summary>
    public static class VideoManager
    {
        /// <summary>
        /// Get the video view of the given type
        /// </summary>
        /// <param name="videoServiceType">Type of video service</param>
        /// <returns>View based on the given service type</returns>
        public static VideoView GetVideoView(ServiceType videoServiceType)
        {
            switch (videoServiceType)
            {
                case ServiceType.Twillio:
                    return new TwilioVideoView();
                //case ServiceType.LiveSwitch:
                //    return new FMVideoView();
                //case ServiceType.Vidyo_Io:
                //    return new VidyoVideoView();
                //case ServiceType.OpenTok:
                //    return new OpenTokVideoView();
                default:
                    return new TwilioVideoView();
            }
        }
    }
}
