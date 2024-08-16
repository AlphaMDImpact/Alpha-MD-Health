using AlphaMDHealth.Model;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class EducationCategoryDTO : BaseDTO
    {
        public EductaionCatergoryModel EductaionCatergory { get; set; }
        [DataMember]
        public List<EductaionCatergoryModel> EductaionCatergories { get; set; }
        [DataMember]
        public List<EducationCategoryDetailModel> CategoryDetails { get; set; }
        [DataMember]
        public List<LanguageModel> Languages { get; set; }
    }
}
