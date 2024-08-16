using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ConsentDTO : BaseDTO
    {
        public ConsentModel Consent { get; set; }

        [DataMember]
        public List<UserConsentModel> UserConsents { get; set; }

        [DataMember]
        public List<ConsentModel> Consents { get; set; }
        [DataMember]
        public List<OptionModel> Pages { get; set; }
        [DataMember]
        public List<OptionModel> Roles { get; set; }
        public List<OptionModel> ConsentPlatforms { get; set; }
    }
}
