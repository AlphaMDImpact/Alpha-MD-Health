using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// User contact language specific information model
    /// </summary>
    public class ContactDetailModel
    {
        /// <summary>
        /// Uniq id of the contact detail
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public long ContactDetailID { get; set; }

        /// <summary>
        /// ID of the user contact
        /// </summary>
        public Guid ContactID { get; set; }

        /// <summary>
        /// ID of the language
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Language specific contact value 
        /// </summary>
        public string ContactValue { get; set; }

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
        /// Name of the language
        /// </summary>
        public string LanguageName { get; set; }
    }
}