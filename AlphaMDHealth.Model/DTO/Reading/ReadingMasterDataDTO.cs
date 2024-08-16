using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class ReadingMasterDataDTO : BaseDTO
    {
        [DataMember]
        public ReadingModel ReadingMetadata { get; set; }
        [DataMember]
        public List<ReadingModel> ReadingMetadatas { get; set; }
        [DataMember]
        public List<ReadingRangeModel> ReadingRanges { get; set; }
        [DataMember]
        public ReadingRangeModel ReadingRange { get; set; }
        [DataMember]
        public List<OptionModel> ValueAddedByType { get; set; }
        [DataMember]
        public List<OptionModel> ChartType { get; set; }
        [DataMember]
        public List<OptionModel> FilterType { get; set; }
        [DataMember]
        public List<OptionModel> FrequencyType { get; set; }
        [DataMember]
        public List<OptionModel> ManualReadingType { get; set; }
        [DataMember]
        public List<OptionModel> HealthKitDataType { get; set; }
        [DataMember]
        public List<OptionModel> ShowInGraphType { get; set; }
        [DataMember]
        public List<OptionModel> ShowInDataType { get; set; }
        [DataMember]
        public List<OptionModel> DeviceDataType { get; set; }
        [DataMember]
        public List<OptionModel> CanBeDeletedType { get; set; }
        [DataMember]
        public List<OptionModel> ShowInDifferentLinesType { get; set; }
        [DataMember]
        public List<OptionModel> GenderOptions { get; set; }
        [DataMember]
        public List<OptionModel> AgeOptions { get; set; }
        [DataMember]
        public List<OptionModel> OrganisationReadingDevices { get; set; }
    }
}
