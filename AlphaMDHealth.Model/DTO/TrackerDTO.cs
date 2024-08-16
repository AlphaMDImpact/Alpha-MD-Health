using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class TrackerDTO : UsersDTO
    {
        public List<ProgramTrackerModel> ProgramTrackers { get; set; }

        public ProgramTrackerModel ProgramTracker { get; set; }
        public List<PatientTrackersModel> PatientTrackers { get; set; }

        public PatientTrackersModel PatientTracker { get; set; }
        /// <summary>
        /// Tracker Types Options
        /// </summary>
        [DataMember]
        public List<OptionModel> TrackerTypes { get; set; }

        /// <summary>
        /// List Of Trackers
        /// </summary>
        [DataMember]
        public List<TrackersModel> Trackers { get; set; }

        /// <summary>
        /// Tracker Information
        /// </summary>
        public TrackersModel Tracker { get; set; }
        [DataMember]
        public TrackersI18NModel TrackerI18N { get; set; }
        public List<TrackersI18NModel> TrackersI18N { get; set; }


        /// <summary>
        /// List Of Tracker Ranges
        /// </summary>
        [DataMember]
        public List<TrackerRangeModel> TrackerRanges { get; set; }

        /// <summary>
        /// Tracker Range Information
        /// </summary>
        public TrackerRangeModel TrackerRange { get; set; }

        /// <summary>
        /// List Of Tracker Range Details
        /// </summary>
        [DataMember]
        public List<TrackerRangesI18N> TrackerRangesI18N { get; set; }

        /// <summary>
        /// Tracker Range Information
        /// </summary>
        public TrackerRangesI18N TrackerRangeI18N { get; set; }

        [DataMember]
        public List<PatientTrackersValuesModel> PatientTrackerValues { get; set; }

        /// <summary>
        /// Program Information
        /// </summary>
        public PatientTrackersValuesModel PatientTrackerValue { get; set; }

        public List<LanguageModel> Languages { get; set; }

        /// <summary>
        /// Bulk PatientTrackers information
        /// </summary>
        [DataMember]
        public List<SaveResultModel> SavePatientTrackers { get; set; }

    }
}
