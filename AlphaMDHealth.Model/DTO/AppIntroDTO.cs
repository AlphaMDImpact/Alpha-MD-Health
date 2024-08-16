using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class AppIntroDTO : BaseDTO
    {
        public AppIntroModel AppIntro { get; set; }
        [DataMember]
        public List<AppIntroModel> AppIntros { get; set; }
    }
}
