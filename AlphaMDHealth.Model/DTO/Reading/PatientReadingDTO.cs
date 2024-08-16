using System.Runtime.Serialization;

namespace AlphaMDHealth.Model;

public class PatientReadingDTO : BaseDTO
{
    /// <summary>
    /// Flag representing data is for medical history or not
    /// </summary>
    public bool IsMedicalHistory { get; set; }

    public ChartUIDTO ChartData { get; set; }

    /// <summary>
    /// ReadingMasterModel : For ReadingMaster Data
    /// </summary>
    public List<ReadingMasterModel> ReadingMasters { get; set; }

    /// <summary>
    /// ReadingMasterModel : For ReadingMaster Data
    /// </summary>
    public ReadingMasterModel ReadingMaster { get; set; }

    /// <summary>
    /// PatientReadingModel : For patient reading (actual observation values)
    /// </summary>
    public PatientReadingModel PatientReading { get; set; }

    /// <summary>
    /// PatientReadingModel : For patient reading (actual observation values)
    /// </summary>
    [DataMember]
    public List<PatientReadingModel> PatientReadings { get; set; }

    /// <summary>
    /// ReadingDeviceModel : List of Organization Reading Devices
    /// </summary>
    [DataMember]
    public List<ReadingDeviceModel> ReadingDevices { get; set; }

    /// <summary>
    /// ReadingUnitI18NModel : For unit Text data of all type or readings
    /// </summary>
    [DataMember]
    public List<UnitI18NModel> ReadingUnitsI18N { get; set; }

    /// <summary>
    /// ReadingModel : For reading types master data(Ex : Blood Pressure Systolic, Gluecode Before sleep etc)
    /// </summary>
    [DataMember]
    public List<ReadingModel> Readings { get; set; }

    /// <summary>
    /// Metata data of reading type
    /// </summary>
    [DataMember]
    public List<ReadingMetadataUIModel> ChartMetaData { get; set; }

    /// <summary>
    /// Graph data
    /// </summary>
    [DataMember]
    public List<List<PatientReadingUIModel>> GraphData { get; set; }

    /// <summary>
    /// List of readings
    /// </summary>
    [DataMember]
    public List<PatientReadingUIModel> ListData { get; set; }

    /// <summary>
    /// Data for summary items
    /// </summary>
    [DataMember]
    public List<OptionModel> SummaryDataOptions { get; set; }

    /// <summary>
    /// Duration filters
    /// </summary>
    [DataMember]
    public List<OptionModel> FilterOptions { get; set; }

    /// <summary>
    /// Reading options
    /// </summary>
    [DataMember]
    public List<OptionModel> ReadingOptions { get; set; }

    /// <summary>
    /// Reading types options
    /// </summary>
    [DataMember]
    public List<OptionModel> ReadingParentOptions { get; set; }

    /// <summary>
    /// Collection of Reading Types data
    /// </summary>
    [DataMember]
    public List<PatientReadingDTO> ReadingDTOs { get; set; }

    /// <summary>
    /// ReadingUnitModel : For unit data of all type or readings with conversion factor
    /// </summary>
    [DataMember]
    public List<UnitModel> ReadingUnits { get; set; }

    /// <summary>
    /// UserAccountSettingsModel: List of User Account Settings 
    /// </summary>
    [DataMember]
    public List<UserAccountSettingsModel> UserAccountSettings { get; set; }

    /// <summary>
    /// DeviceModel : List of Patient Devices
    /// </summary>
    [DataMember]
    public List<PatientDeviceModel> PatientReadingDevices { get; set; }

    /// <summary>
    /// ReadingRangeModel For range master/user data(Absolute/Normal/Target ranges)
    /// </summary>
    public ReadingRangeModel ReadingRange { get; set; }

    /// <summary>
    /// ReadingRangeModel / ReadingRangeI18NModel : For all type of ranges master/user data(Absolute/Normal/Target ranges)
    /// </summary>
    [DataMember]
    public List<ReadingRangeModel> ReadingRanges { get; set; }

    /// <summary>
    /// List of patient reading targets
    /// </summary>
    [DataMember]
    public List<ReadingTargetModel> PatientReadingTargets { get; set; }

    /// <summary>
    /// Patient Reading Target
    /// </summary>
    [DataMember]
    public ReadingTargetModel PatientReadingTarget { get; set; }

    /// <summary>
    /// Nutritions Types
    /// </summary>
    [DataMember]
    public List<ResourceModel> NutritionTypes { get; set; }

    /// <summary>
    /// Object to store operation result of Bulk POST (Add/Edit/Delete)
    /// </summary>
    [DataMember]
    public List<SaveResultModel> SaveReadings { get; set; }

    /// <summary>
    /// Object to store operation result of Bulk POST (Add/Edit/Delete)
    /// </summary>
    [DataMember]
    public List<SaveResultModel> SaveReadingSources { get; set; }

    /// <summary>
    /// Reading Icon
    /// </summary>
    public string ReadingIcon { get; set; }

    /// <summary>
    /// Category type selected
    /// </summary>
    public short ReadingCategoryID { get; set; }

    /// <summary>
    /// Unique id of the reading
    /// </summary>
    public short ReadingID { get; set; }

    /// <summary>
    /// Unique id of the reading for Selected List Item
    /// </summary>
    public short SelectedListItemReadingID { get; set; }

    /// <summary>
    /// comma seperated reading ids to fetch data
    /// </summary>
    public string ReadingIDs { get; set; }

    /// <summary>
    /// Task ID of the reading
    /// </summary>
    public long PatientTaskID { get; set; }

    /// <summary>
    /// Patient Reading id
    /// </summary>
    public Guid PatientReadingID { get; set; }

    // Only for dashboard and result list

    /// <summary>
    /// Title for Reading Type
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Title with icon in dashboard case
    /// </summary>
    public string TitleWithIcon { get; set; }

    /// <summary>
    /// Latest value to be displayed in card
    /// </summary>
    public string LatestValue { get; set; }
    /// <summary>
    /// Latest value to be displayed in card
    /// </summary>
    public string LatestValueString { get; set; }

    /// <summary>
    /// Reading unit
    /// </summary>
    public string ReadingUnit { get; set; }

    /// <summary>
    /// Latest date time to be displayed in card
    /// </summary>
    public string LatestValueDateText { get; set; }

    /// <summary>
    /// selection color
    /// </summary>
    public string SelectedColor { get; set; }

    /// <summary>
    /// Formatted value string
    /// </summary>  
    public string ReadingUnitValue { get; set; }

    /// <summary>
    /// Has set target permission then true else false
    /// </summary>
    public bool ShowSetTargetButton { get; set; }


    /// <summary>
    /// Call comming From Questionnaire Page
    /// </summary>
    public bool IsCommingFromQuestionnaireTaskPage { get; set; }

    /// <summary>
    /// to map question data
    /// </summary>
    public bool IsRequiredQuestion { get; set; } = false;

    /// <summary>
    /// Action to be performed on NextAndPrevious button
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// List Of Genders
    /// </summary>
    [DataMember]
    public List<OptionModel> Genders { get; set; }
    public UserModel User { get; set; }
    public List<OptionModel> Posture { get; set; }
    public List<OptionModel> ScanType { get; set; }

    public string PostureID { get; set; }

    public string ScanTypeID { get; set; }

    public string ValueUnit { get; set; }
    public string MinMaxReadingRanges { get; set; }
}