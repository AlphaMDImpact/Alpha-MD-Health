namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Error Log Model
    /// </summary>
    public class ErrorLogModel
    {
        /// <summary>
        ///  Error Function Name of an error
        /// </summary>
        public string ErrorFunction { get; set; }
        /// <summary>
        /// Stores Error Line Number
        /// </summary>
        public int ErrorLineNumber { get; set; }
        /// <summary>
        /// Error Log Level of an error
        /// </summary>
        public int ErrorLogLevel { get; set; }
        /// <summary>
        /// Error Message of an error
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Error Number of an error
        /// </summary>
        public int ErrorNumber { get; set; }
        /// <summary>
        /// Stores user's Account Id 
        /// </summary>
        public long AccountID { get; set; }
        /// <summary>
        /// Stores Created On Date of an error
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }
    }
}
