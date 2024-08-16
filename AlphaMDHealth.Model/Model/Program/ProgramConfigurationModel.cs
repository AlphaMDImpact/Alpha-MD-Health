using SQLite;

namespace AlphaMDHealth.Model
{
    public class ProgramConfigurationModel
    {
        [PrimaryKey]
        public long ProgramReasonConfigurationID { get; set; }
        public long ProgramID { get; set; }
        public long FeatureID { get; set; }
        public string FeatureCode { get; set; }
        public string FeatureText { get; set; }
        public bool IsReasonRequired { get; set; }
        public bool IsSignatureRequired { get; set; }
        public bool IsActive { get; set; }
    }
}
