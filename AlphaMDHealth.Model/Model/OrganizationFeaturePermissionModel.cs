using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Feature permission data
    /// </summary>
    public class OrganizationFeaturePermissionModel
    {
        /// <summary>
        /// Unique id for Feature
        /// </summary>
        [PrimaryKey]
        public short FeatureID { get; set; }

        /// <summary>
        /// Feature code used for checking permission
        /// </summary>
        public string FeatureCode { get; set; }

        /// <summary>
        /// Indicates if permission is available for this feature
        /// </summary>
        public bool HasPermission { get; set; }

        /// <summary>
        /// Language ID for feature
        /// </summary>
        public byte LanguageID { get; set; }

        /// <summary>
        /// Language specific text for Feature
        /// </summary>
        public string FeatureText { get; set; }

        /// <summary>
        /// Indicates if feature is for a blob page
        /// </summary>
        public bool IsBlogpage { get; set; }

        /// <summary>
        /// Indicates if feature is should be shown in menu
        /// </summary>
        public bool ShowInMenu { get; set; }

        /// <summary>
        /// Target page for navigation when feature is clicked or selected
        /// </summary>
        public string TargetPage { get; set; }

        /// <summary>
        /// Sequence no for ordering features
        /// </summary>
        public byte SequenceNo { get; set; }

        /// <summary>
        /// Group Id to which the feature belongs
        /// </summary>
        public long FeatureGroupID { get; set; }

        /// <summary>
        /// Feature group icon to which the feature belongs
        /// </summary>
        public string GroupIcon { get; set; }

        /// <summary>
        /// Parent Id to which the feature belong. Used for tab relation.
        /// </summary>
        public long FeatureParentID { get; set; }

        /// <summary>
        /// Indicates if the feature is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates if feature is available at organisation level
        /// </summary>
        public bool AvailableAtOrganisationLevel { get; set; }

        /// <summary>
        /// Indicates if feature is available at branch level
        /// </summary>
        public bool AvailableAtBranchLevel { get; set; }

        /// <summary>
        /// Indicates if feature is available at department level
        /// </summary>
        public bool AvailableAtDepartmentLevel { get; set; }
    }
}