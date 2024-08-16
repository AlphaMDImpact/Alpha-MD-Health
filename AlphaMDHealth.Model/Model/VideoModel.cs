using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Video Model
    /// </summary>
    public class VideoModel
    {
        /// <summary>
        /// Session/room id
        /// </summary>
        public string VideoRoomID { get; set; }

        /// <summary>
        /// Token Id
        /// </summary>
        public string VideoToken { get; set; }

        /// <summary>
        /// Video link
        /// </summary>
        public string VideoLink { get; set; }

        /// <summary>
        /// Application ID
        /// </summary>

        public string ApplicationID { get; set; }

        /// <summary>
        /// Secret key
        /// </summary>
        public string SecretKey { get; set; }


        /// <summary>
        /// Service type
        /// </summary>
        public ServiceType ServiceType { get; set; }

    }
}
