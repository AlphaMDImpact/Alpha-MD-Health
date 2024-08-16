using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class HealthScanDTO : BaseDTO
    {
        [DataMember]
        public HealthScanModel ExternalServiceTransaction { get; set; }

        [DataMember]
        public List<HealthScanModel> ExternalServiceTransactions { get; set; }
        public int OrganisationCredits { get; set; }
        public int CreditsAssigned {  get; set; }
        public int CreditsAvailable { get; set; }
        public int NumberOfPatient {  get; set; }
    }
}