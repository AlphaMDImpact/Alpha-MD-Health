using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class SystemIdentifierDTO : BaseDTO
    {
        [DataMember]
        public List<SystemIdentifierModel> SystemIdentifiers { get; set; }
    }
}