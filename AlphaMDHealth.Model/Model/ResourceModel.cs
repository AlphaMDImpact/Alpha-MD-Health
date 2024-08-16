using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Resource Model
    /// </summary>
    public class ResourceModel
    {
        /// <summary>
        /// Id used to identify uniquely
        /// </summary>
        [PrimaryKey]
        public int ResourceID { get; set; }

        /// <summary>
        /// Resource group ID
        /// </summary>
        public short GroupID { get; set; }

        /// <summary>
        /// Group to which the resource belongs to
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Stores Resource Key of a Resource
        /// </summary>
        public string GroupDesc { get; set; }

        /// <summary>
        /// Indicates if the resource is dynamic and should not be included in configuration count
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Stores Resource Key Id of a Resource
        /// </summary>
        public int ResourceKeyID { get; set; }

        /// <summary>
        /// Stores Resource Key of a Resource
        /// </summary>
        public string ResourceKey { get; set; }

        /// <summary>
        /// Stores Type of Field to render in UI
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// Stores Resource Value of a Resource
        /// </summary>
        public string ResourceValue { get; set; }

        /// <summary>
        /// Stores PaceHolder of a Resource
        /// </summary>
        public string PlaceHolderValue { get; set; }

        /// <summary>
        /// Stores Info Value of Resource
        /// </summary>
        public string InfoValue { get; set; }

        /// <summary>
        /// Stores KeyDescription of a Resource
        /// </summary>
        public string KeyDescription { get; set; }

        /// <summary>
        /// Determines if a field is required or not
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Stores Min Length of a Resource
        /// </summary>
        public double MinLength { get; set; }

        /// <summary>
        /// Stores MaxLength of a Resource
        /// </summary>
        public double MaxLength { get; set; }

        /// <summary>
        /// Last modified date time
        /// </summary>
        public DateTimeOffset? LastModifiedOn { get; set; }

        /// <summary>
        /// Id of the language to which the resource text belongs
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Code of the language to which the resource text belongs
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Indicates if the resource is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Determines if the resource is web link or normal text for consent
        /// </summary>
        public bool IsWebLink { get; set; }

        /// <summary>
        /// Determines resource is for which platform
        /// </summary>
        [Ignore]
        public ForPlatform ResourceKeyFor { get; set; }

    }
}