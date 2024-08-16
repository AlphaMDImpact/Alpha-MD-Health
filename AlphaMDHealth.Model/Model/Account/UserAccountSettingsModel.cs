using AlphaMDHealth.Utility;
using SQLite;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    public class UserAccountSettingsModel
    {
        /// <summary>
        /// setting type 
        /// </summary>
        public UserSettingType SettingType { get; set; }

        /// <summary>
        /// setting type id
        /// </summary>
        public int SettingTypeID { get; set; }

        /// <summary>
        /// Setting value
        /// </summary>
        public string SettingValue { get; set; }

        /// <summary>
        /// reading type id
        /// </summary>
        public int ReadingTypeID { get; set; }

        /// <summary>
        ///  Flag representing record is active or deleted
        /// </summary>
        public bool IsActive { get; set; }    
        
        /// <summary>
        /// reading type value
        /// </summary>
        public string ReadingType { get; set; }

        /// <summary>
        /// Flag representing record is synced with server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Flag representing record is toggled or not
        /// </summary>
        [Ignore]
        public bool IsToogled { get; set; }

        /// <summary>
        /// list of reading units of a particular reading 
        /// </summary>
        [Ignore]
        [DataMember]
        public List<string> ReadingUnitOptions { get; set; }

        /// <summary>
        /// list of reading units of a particular reading 
        /// </summary>
        [Ignore]
        public List<OptionModel> ReadingUnitOption { get; set; }

        /// <summary>
        /// Selected Reading Unit
        /// </summary>
        [Ignore]
        public string SelectedReadingUnit { get; set; }

        /// <summary>
        /// Reading unit data
        /// </summary>
        [Ignore]
        public string ReadingUnit { get; set; }

        /// <summary>
        /// reading type key
        /// </summary>
        public string ReadingTypeKey { get; set; }
    }
}
