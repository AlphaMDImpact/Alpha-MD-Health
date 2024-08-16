namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Video DTO
    /// </summary>
    public class VideoDTO : BaseDTO
    {
        /// <summary>
        /// Video
        /// </summary>
        public VideoModel Video { get; set; }

        /// <summary>
        /// For Application
        /// </summary>
        public string ForApplication { get; set; }
    }
}
