using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Error Log DTO
    /// </summary>
    public class ErrorLogDTO : BaseDTO
    {
        /// <summary>
        /// List of ErrorLog
        /// </summary>
        [DataMember]
        public List<ErrorLogModel> ErrorLogs { get; set; }
    }
}
