using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ContactDTO : BaseDTO
    {
        public ContactModel Contact { get; set; }
        [DataMember]
        public List<ContactModel> Contacts { get; set; }
        [DataMember]
        public List<ContactDetailModel> PatientContactDetails { get; set; } 
        [DataMember]
        public List<ContactModel> ContactDetails { get; set; }
        [DataMember]
        public List<OptionModel> ContactTypeOptions { get; set; }
        [DataMember]
        public List<OptionModel> ContactTypeIsOptions { get; set; } 
        public ContactType ContactType { get; set; }
        [DataMember]
        public List<SaveResultModel> SaveResults { get; set; }
    }
}
