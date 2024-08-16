using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ContentPageDTO : BaseDTO
    {
        /// <summary>
        /// Flag representing data is for medical history or not
        /// </summary>
        public bool IsMedicalHistory { get; set; }

        [DataMember]
        public ContentPageModel Page { get; set; }
        [DataMember]
        public List<ContentPageModel> Pages { get; set; }
        [DataMember]
        public List<ContentDetailModel> PageDetails { get; set; }
        [DataMember]
        public List<OptionModel> PageTypes { get; set; }
        [DataMember]
        public List<LanguageModel> Languages { get; set; }
        [DataMember]
        public PatientEducationModel PatientEducation { get; set; }

        [DataMember]
        public List<PatientEducationModel> PatientEducations { get; set; }
        [DataMember]
        public List<OptionModel> EducationTypes { get; set; }
        [DataMember]
        public List<OptionModel> Educations { get; set; }

        [DataMember]
        public List<OptionModel> EducationCategory { get; set; }
        [DataMember]
        public List<EducationCategoryGroupModel> EducationGroup { get; set; }

        public ProgramModel Program { get; set; }
        [DataMember]
        public List<ProgramModel> Programs { get; set; }

    }
}
