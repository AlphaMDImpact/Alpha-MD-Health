using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class TabDataStructureModel
    {
        public string DataField { get; set; }
        public FieldTypes FieldType { get; set; }
        public string ResourceKey { get; set; }
        public string SettingKey { get; set; }
        public bool IsDisable { get; set; }
        public bool IsRequired { get; set; } = true;
        public string RegexPattern { get; set; }
        public string Height { get; set; } = "250px";
        public string FieldId { get; set; }
        public bool AllowImage { get; set; } = true;
        public string MaxFileDimensionSize { get; set; }
        public string SupportedImageTypes { get; set; }
        public string MaxFileUploadSize { get; set; }

        [DataMember]
        public List<CountryModel> CountryCodes { get; set; }

    }
}
