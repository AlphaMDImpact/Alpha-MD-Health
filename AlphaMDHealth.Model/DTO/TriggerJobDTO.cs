using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Data transfer object for trigger job
    /// </summary>
    public class TriggerJobDTO : BaseDTO
    {
        /// <summary>
        /// List of jobs
        /// </summary>
        [DataMember]
        public List<TriggerJobModel> Jobs { get; set; }
    }
}
