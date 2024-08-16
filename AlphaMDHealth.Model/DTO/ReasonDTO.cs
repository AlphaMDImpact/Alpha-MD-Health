using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Data transfer object for Reasons
    /// </summary>
    public class ReasonDTO : BaseDTO
    {
        /// <summary>
        /// Reason Information
        /// </summary>
        public ReasonModel Reason { get; set; }

        /// <summary>
        /// List Of Reasons
        /// </summary>
        [DataMember]
        public List<ReasonModel> Reasons { get; set; }
    }
}
