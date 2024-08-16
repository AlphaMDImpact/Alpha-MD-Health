using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Mobile menu nodes
    /// </summary>
    public class MobileMenuNodeModel
    {
        /// <summary>
        /// Mobile menu node id
        /// </summary>
        public long MobileMenuNodeID { get; set; }

        /// <summary>
        /// Name of node
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_NODE_NAME_KEY)]
        public string NodeName { get; set; }

        /// <summary>
        /// Type of node
        /// </summary>
        public MenuType NodeType { get; set; }

        /// <summary>
        /// ID of node to be navigated to
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_NODE_TARGET_KEY)]
        public long TargetID { get; set; }

        /// <summary>
        /// Left header item action id
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_MENU_ACTION_KEY)]
        public MenuAction LeftMenuActionID { get; set; }

        /// <summary>
        /// Left header menu node id
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_MENU_NODES_KEY)]
        public long? LeftMenuNodeID { get; set; }

        /// <summary>
        /// Indicates if left menu should be displayed as icon
        /// </summary>
        public bool? ShowIconInLeftMenu { get; set; }

        /// <summary>
        /// Right header item action id
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_MENU_ACTION_KEY)]
        public MenuAction RightMenuActionID { get; set; }

        /// <summary>
        /// Right header menu node id
        /// </summary>
        [MyCustomAttributes(ResourceConstants.R_MENU_NODES_KEY)]
        public long? RightMenuNodeID { get; set; }

        /// <summary>
        /// Indicates if right menu should be displayed as icon
        /// </summary>
        public bool? ShowIconInRightMenu { get; set; }

        /// <summary>
        /// Indicated is the node is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Type of menu
        /// </summary>
        public string MenuType { get; set; }

        /// <summary>
        /// Target page
        /// </summary>
        public string TargetPage { get; set; }

        /// <summary>
        /// Indicates if page is to be shown as master
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// Indicates if page is to be shown for add edit page
        /// </summary>
        public bool IsAddEditPage { get; set; }

        /// <summary>
        /// Action text for left menu
        /// </summary>
        public string LeftMenuAction { get; set; }

        /// <summary>
        /// Action text for left menu
        /// </summary>
        public string LeftMenuActionText { get; set; }

        /// <summary>
        /// Action text for right menu
        /// </summary>
        public string RightMenuAction { get; set; }

        /// <summary>
        /// Action text for right menu
        /// </summary>
        public string RightMenuActionText { get; set; }

        /// <summary>
        /// Heading of the page
        /// </summary>
        public string PageHeading { get; set; }

        /// <summary>
        /// Id of the group to which the node belongs to
        /// </summary>
        public long MobileMenuGroupID { get; set; }

        /// <summary>
        /// Sequence in which node is to sorted
        /// </summary>
        public byte SequenceNo { get; set; }

        /// <summary>
        /// Icon to be shown for group
        /// </summary>
        public string GroupIcon { get; set; }

        /// <summary>
        /// Initials of image
        /// </summary>
        [Ignore]
        public string ImageName { get; set; }
    }
}
