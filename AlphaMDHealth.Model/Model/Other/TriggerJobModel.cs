using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model to store trigger job information
    /// </summary>
    public class TriggerJobModel
    {
        /// <summary>
        /// Job Identifier
        /// </summary>
        public JobAction Job { get; set; }

        /// <summary>
        /// Operation status
        /// </summary>
        public ErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Description of operation status
        /// </summary>
        public string ErrorDescription { get; set; }
    }
}
