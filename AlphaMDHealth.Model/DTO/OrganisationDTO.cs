using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class OrganisationDTO : BaseDTO
    {
        [DataMember]
        public int FeatureID { get; set; }
        
        [DataMember]
        public List<OptionModel> DropDownOptions { get; set; }
        
        [DataMember]
        public List<OptionModel> Languages { get; set; }
        
        [DataMember]
        public OrganisationModel OrganisationProfile { get; set; }
        
        [DataMember]
        public List<ContentDetailModel> PageDetails { get; set; }
        
        [DataMember]
        public List<ConfigureDashboardModel> FeatureTab { get; set; }
        
        [DataMember]
        public List<LanguageModel> DataLanguages { get; set; }

        [DataMember]
        public List<OrganisationModel> Organisations { get; set; }
        
        [DataMember]
        public List<OptionModel> PaymentPlans { get; set; }
        [DataMember]
        public List<OptionModel> ExternalServices { get; set; }

        [DataMember]
        public List<OrganisationExternalServiceModel> OrganisationExternalServices { get; set; }
    }
}
