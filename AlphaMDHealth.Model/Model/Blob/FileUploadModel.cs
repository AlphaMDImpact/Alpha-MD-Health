namespace AlphaMDHealth.Model
{
    /// <summary>
    /// File upload details
    /// </summary>
    public class FileDataModel
    {
        /// <summary>
        /// Record id to send data and update response data back in main object
        /// </summary>
        public string RecordID { get; set; }

        /// <summary>
        /// flag to decide it is single image or multiple
        /// </summary>
        public bool HasMultiple { get; set; }

        /// <summary>
        /// Base64 data
        /// </summary>
        public string Base64File { get; set; }
    }
}