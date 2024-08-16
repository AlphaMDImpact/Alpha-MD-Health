namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Parent model for all models
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// Flag representing data is synced to server or not
        /// </summary>
        public bool IsSynced { get; set; } = true;

        /// <summary>
        /// Flag to store a information about record is deleted(false) or not(true)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Stores Added On DateTime
        /// </summary>
        public DateTimeOffset? AddedON { get; set; }

        /// <summary>
        /// Stores Added By User info
        /// </summary>
        public string? AddedBy { get; set; }

        /// <summary>
        /// Stores Last Modified On DateTime
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }

        /// <summary>
        /// Stores Last Modified By User info
        /// </summary>
        public string? LastModifiedBy { get; set; }
    }
}