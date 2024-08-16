using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class FeatureRelationDTO : BaseDTO
    {
        [DataMember]
        public List<FeatureRelationModel> FeatureRelations { get; set; }
    }
}
