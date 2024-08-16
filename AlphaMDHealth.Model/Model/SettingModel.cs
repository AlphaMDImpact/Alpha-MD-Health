using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Setting Model
    /// </summary>
    public class SettingModel
    {
        /// <summary>
        /// Stores Setting Id of Setting
        /// </summary>
        [PrimaryKey]
        public int SettingID { get; set; }

        /// <summary>
        /// Stores Group Name of a Setting
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Stores Organisation Id of Setting
        /// </summary>
        public long OrganisationID { get; set; }

        /// <summary>
        /// Stores Setting Key of a Setting
        /// </summary>
        public string SettingKey { get; set; }

        /// <summary>
        /// Stores Setting Value of a Setting
        /// </summary>
        public string SettingValue { get; set; }

        /// <summary>
        /// Stores Setting Type of a Setting
        /// </summary>
        public string SettingType { get; set; }

        /// <summary>
        /// Flag to Store IsDynamic
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Flag to Store IsActive
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Stores Last Modified On Date of a Setting
        /// </summary>
        public DateTimeOffset? LastModifiedOn { get; set; }

        /// <summary>
        /// Stores Settings Descitption of a Setting
        /// </summary>
        public string SettingDescription { get; set; }
    }
}
