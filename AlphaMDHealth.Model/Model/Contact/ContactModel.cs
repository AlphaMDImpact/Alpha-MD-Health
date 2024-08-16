using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// User contact information model
    /// </summary>
    public class ContactModel
    {
        /// <summary>
        /// ID of the user contact
        /// </summary>
        [PrimaryKey]
        public Guid ContactID { get; set; }

        /// <summary>
        /// Type ID of the contact
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_CONTACT_KEY)]
        public int ContactTypeID { get; set; }

        /// <summary>
        /// Type detail ID of the contact
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_CONTACT_TYPE_KEY)]
        public int ContactTypeIsID { get; set; }

        /// <summary>
        /// ID of the language
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Language specific contact value 
        /// </summary>
        public string ContactValue { get; set; } = null;

        /// <summary>
        /// UserID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// error code
        /// </summary>
        public ErrorCode ErrCode { get; set; }

        /// <summary>
        /// Flag representing data is synced to server or not
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Flag representing record is deleted or active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Stores Created By User ID 
        /// </summary>
        public long AddedByID { get; set; }

        /// <summary>
        /// Stores Added On Date By User
        /// </summary>
        public DateTimeOffset? AddedON { get; set; }

        /// <summary>
        /// Stores Created By User ID 
        /// </summary>
        public long LastModifiedByID { get; set; }

        /// <summary>
        /// Stores Last Modified On Date By User
        /// </summary>
        public DateTimeOffset? LastModifiedON { get; set; }

        /// <summary>
        /// Type of the contact
        /// </summary>
        [Ignore]
        public string ContactType { get; set; }

        /// <summary>
        /// Type detail of the contact
        /// </summary>
        [Ignore]
        public string ContactTypeIs { get; set; }

        /// <summary>
        /// Name of the language
        /// </summary>
        [Ignore]
        public string LanguageName { get; set; }
    }
}