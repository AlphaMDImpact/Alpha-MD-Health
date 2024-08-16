namespace AlphaMDHealth.Model
{
    /// <summary>
    /// User health permission data
    /// </summary>
    public class UserHealthPermissionRequestModel
    {
        /// <summary>
        /// ID of the user
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Reading id isued for permission
        /// </summary>
        public short ReadingID { get; set; }

        /// <summary>
        /// true if permission is requested else false
        /// </summary>
        public bool IsRequested { get; set; }
    }
}
