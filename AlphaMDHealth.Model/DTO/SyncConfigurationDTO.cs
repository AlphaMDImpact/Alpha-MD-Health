using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Dto used to call services 
    /// </summary>
    public class SyncConfigurationDTO : BaseDTO
    {
        /// <summary>
        /// List of sync configurations 
        /// </summary>
        [DataMember]
        public List<SyncConfigurationModel> Configurations { get; set; }

        /// <summary>
        /// Group of sync configurations to call parallely
        /// </summary>
        [DataMember]
        public List<IGrouping<int, SyncConfigurationModel>> SyncGroups { get; set; }
    }
}