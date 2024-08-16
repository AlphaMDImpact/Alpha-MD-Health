using AlphaMDHealth.Utility;
using SQLite;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Parent data transfer object(DTO) for all DTOs
    /// </summary>
    public class BaseDTO
    {
        /// <summary>
        /// Feature For (1 =both, 2= Web, 3= Mobile)
        /// </summary>
        [Ignore]
        public byte FeatureFor { get; set; }

        /// <summary>
        /// Stores RecordCount 
        /// </summary>
        [Ignore]
        public long RecordCount { get; set; }

        /// <summary>
        /// Stores LanguageID
        /// </summary>
        [Ignore]
        public byte LanguageID { get; set; }

        /// <summary>
        /// Stores User's AccountID
        /// </summary>
        [Ignore]
        public long AccountID { get; set; }

        /// <summary>
        /// Stores User's SelectedUserID
        /// </summary>
        [Ignore]
        public long SelectedUserID { get; set; }

        /// <summary>
        /// Stores Permission At LevelID 
        /// </summary>
        [Ignore]
        public long PermissionAtLevelID { get; set; }

        /// <summary>
        /// Stores OrganisationID
        /// </summary>
        [Ignore]
        public long OrganisationID { get; set; }

        /// <summary>
        /// Operation status
        /// </summary>
        [Ignore]
        public ErrorCode ErrCode { get; set; } = ErrorCode.OK;

        /// <summary>
        /// Stores Error Description 
        /// </summary>
        [Ignore]
        public string? ErrorDescription { get; set; }

        /// <summary>
        /// From date time
        /// </summary>
        public string? FromDate { get; set; } = null;

        /// <summary>
        /// To date time
        /// </summary>
        public string? ToDate { get; set; } = null;

        ////////////////////////////////////////////
        
        /// <summary>
        /// Stores Created By User ID 
        /// </summary>
        [Ignore]
        public long CreatedByID { get; set; }

        /// <summary>
        /// Stores Added On Date By User
        /// </summary>
        [Ignore]
        public DateTimeOffset? AddedON { get; set; }

        /// <summary>
        /// Flag to Store a IsActive
        /// </summary>
        [Ignore]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Stores Last Modified On Date By User
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }

        /// <summary>
        /// Stores Added By User
        /// </summary>
        [Ignore]
        public string AddedBy { get; set; }

        /// <summary>
        /// Stores User's PhoneNumber
        /// </summary>
        [Ignore]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Stores User's Email ID
        /// </summary>
        [Ignore]
        public string EmailID { get; set; }

        /// <summary>
        /// Stores Last Modified On Date By User
        /// </summary>
        [Ignore]
        public string LastModifiedBy { get; set; }

        ///// <summary>
        ///// Response from Service in string Format
        ///// </summary>
        //[Ignore]
        //public Stream ResponseStream { get; set; }

        /// <summary>
        /// Response from Service in string Format
        /// </summary>
        [Ignore]
        public string Response { get; set; }

        /// <summary>
        /// List of Resources
        /// </summary>
        [Ignore]
        [DataMember]
        public List<ResourceModel> Resources { get; set; }

        /// <summary>
        /// List of Country Codes
        /// </summary>
        [Ignore]
        [DataMember]
        public List<CountryModel> CountryCodes { get; set; }

        /// <summary>
        /// List of Country Codes
        /// </summary>
        [Ignore]
        [DataMember]
        public List<SettingModel> Settings { get; set; }

        /// <summary>
        /// List of organisation permissions
        /// </summary>
        [Ignore]
        [DataMember]
        public List<OrganizationFeaturePermissionModel> FeaturePermissions { get; set; }

        /// <summary>
        /// Notification tags
        /// </summary>
        [Ignore]
        public string NotificationTags { get; set; }

        /// <summary>
        /// Fileds to Validate
        /// </summary>
        [Ignore]
        public List<FieldValidator> Fields { get; set; }

        [Ignore]
        public int LocaltoUtcTimeInSeconds { get; set; }
    }
}