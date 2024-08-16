using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Object which holds Data sync details
    /// </summary>
    public class BaseDataSyncDTO : BaseDTO
    {
        /// <summary>
        /// Name of batch
        /// </summary>
        [DataMember]
        public string SyncBatch { get; set; }

        /// <summary>
        /// Collection of tables which will be in current batch
        /// </summary>
        [DataMember]
        public string[] DataSyncFor { get; set; }

        /// <summary>
        /// PatientID for which data needs to fetch
        /// </summary>
        public long PatientID { get; set; }

        /// <summary>
        /// List of Data sync models
        /// </summary>
        [DataMember]
        public List<DataSyncModel> DataSyncForRecords { get; set; }
    }
}