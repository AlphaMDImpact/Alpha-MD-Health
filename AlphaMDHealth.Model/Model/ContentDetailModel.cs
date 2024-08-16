using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Content Detail Model
    /// </summary>
    public class ContentDetailModel
    {
        /// <summary>
        /// PageID of Education/Static Page
        /// </summary>
        public long PageID { get; set; }

        /// <summary>
        /// LanguageName
        /// </summary>
        public string LanguageName { get; set; }

        /// <summary>
        /// Langugage ID
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Content ID
        /// </summary>
        public string ContentKey { get; set; }

        /// <summary>
        /// Page Heading of Education/Static Page
        /// </summary>
        [MyCustomAttributes($"{ResourceConstants.R_TITLE_KEY},{ResourceConstants.R_ORGANISATION_NAME_KEY},{ResourceConstants.R_RECOMMENDATION_TEXT_KEY},{ResourceConstants.R_APPOINTMENT_SUBJECT_TEXT_KEY}")]
        public string PageHeading { get; set; }

        /// <summary>
        /// Page Heading of Education/Static Page
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// PageData of Education/Static Page
        /// </summary>
        [MyCustomAttributes($"{ResourceConstants.R_EDITOR_KEY},{ResourceConstants.R_DESCRIPTION_TEXT_KEY},{ResourceConstants.R_INFORMATION_TEXT_KEY}")]
        public string PageData { get; set; }

        /// <summary>
        /// Flag to store IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Stores Status ID of Education/Static Page
        /// </summary>
        public short StatusID { get; set; }

        /// <summary>
        /// page name
        /// </summary>
      
        public PageType PageName { get; set; }

        /// <summary>
        /// Synced Image
        /// </summary>
        public bool IsDataDownloaded { get; set; }

        /// <summary>
        /// Content Status
        /// </summary>
        public PatientEducationStatus Status { get; set; }

        /// <summary>
        /// ErrCode Enum
        /// </summary>
        [Ignore]
        public ErrorCode ErrCode { get; set; }
    }
}