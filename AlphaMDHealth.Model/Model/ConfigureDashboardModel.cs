using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model used for dashboard data
    /// </summary>
    public class ConfigureDashboardModel
    {
        /// <summary>
        /// Id of Fashboard setting
        /// </summary>
        [PrimaryKey]
        public long DashboardSettingID { get; set; }

        /// <summary>
        /// Users role id
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_ROLES_KEY)]
        public byte RoleID { get; set; }

        /// <summary>
        /// ID of feature
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_FEATURE_NAME_KEY)]
        public int FeatureID { get; set; }

        /// <summary>
        /// Sequence number of feature
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_SEQUENCE_NO_KEY)]
        public byte SequenceNo { get; set; }

        /// <summary>
        /// Flat whic indicates feature is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Text of feature
        /// </summary>
        ////[Ignore]
        public string FeatureText { get; set; }

        /// <summary>
        /// Unique Feature code
        /// </summary>
        ////[Ignore]
        public string FeatureCode { get; set; }

        /// <summary>
        /// Menu node id
        /// </summary>
        ////[Ignore]
        public long NodeID { get; set; }

        /// <summary>
        /// Wizet size used to decide feature is allowed to configure or not
        /// </summary>
        [Ignore]
        public string WidgetSizeInWebPage { get; set; }

        /// <summary>
        /// Type used to decide it is feature or not
        /// </summary>
        [Ignore]
        public string MenuType { get; set; }

        /// <summary>
        /// Name of target page
        /// </summary>
        public string TargetPage { get; set; }

    }
}