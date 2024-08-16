using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// data transfer object for Dashboard logics
    /// </summary>
    public class DashboardLibDTO : BaseDTO
    {
        /// <summary>
        /// List of Dashboard Configurations
        /// </summary>
        [DataMember]
        public List<ConfigureDashboardModel> ConfigurationRecords { get; set; }

        /// <summary>
        /// List of Dashboard Configuuration parameters
        /// </summary>
        [DataMember]
        public List<SystemFeatureParameterModel> ConfigurationRecordParameters { get; set; }
    }
}