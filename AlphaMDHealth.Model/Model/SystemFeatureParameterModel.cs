namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used to decide service calls
    /// </summary>
    public class SystemFeatureParameterModel
    {
        /// <summary>
        /// ID of Parameter
        /// </summary>
        public int ParameterID { get; set; }

        /// <summary>
        /// ID of Dashboard configuration
        /// </summary>
        public long DashboardSettingID { get; set; }

        /// <summary>
        /// ID of Featue 
        /// </summary>
        public int FeatureID { get; set; }

        /// <summary>
        /// Name of Feature Parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Description of Feature parameter
        /// </summary>
        public string ParameterDescription { get; set; }

        /// <summary>
        /// Value stored for the feature parametre
        /// </summary>
        public string ParameterValue { get; set; }

        /// <summary>
        /// Sequence number of Parameter in that feature
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Flag which indicates parameter is active or not
        /// </summary>
        public bool IsActive { get; set; }

        ////[Ignore]
        ////public long ContentPageID { get; set; }
    }
}