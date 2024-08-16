using AlphaMDHealth.Utility;
using SQLite;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model;

/// <summary>
/// Application Menus
/// </summary>
public class MenuModel
{
    /// <summary>
    /// Id of menu
    /// </summary>
    [PrimaryKey]
    public long MenuID { get; set; }

    /// <summary>
    /// Location of th menu
    /// </summary>
    [MyCustomAttributes(ResourceConstants.R_MENU_LOCATION_KEY)]
    public MenuLocation MenuLocation { get; set; }

    /// <summary>
    /// Type of page
    /// </summary>
    public MenuType PageTypeID { get; set; }

    /// <summary>
    /// Show both icon and text or only text or icon
    /// </summary>
    public MenuRenderType RenderType { get; set; }

    /// <summary>
    /// Id of Target menu
    /// </summary>
    [MyCustomAttributes(ResourceConstants.R_PAGES_KEY)]
    public long TargetID { get; set; }

    /// <summary>
    /// Sequence no
    /// </summary>
    [MyCustomAttributes(ResourceConstants.R_SEQUENCE_NO_KEY)]
    public byte SequenceNo { get; set; }

    /// <summary>
    /// true if menu is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Menu Type
    /// </summary>
    public string MenuType { get; set; }

    /// <summary>
    /// Name of target page
    /// </summary>
    public string TargetPage { get; set; }

    /// <summary>
    /// Display name for node
    /// </summary>
    public string NodeName { get; set; }

    /// <summary>
    /// Position to scroll page to
    /// </summary>
    public bool ScrollToPage { get; set; }

    /// <summary>
    /// Heading for menu group
    /// </summary>
    public string GroupHeading { get; set; }

    /// <summary>
    /// Page title to be displayed
    /// </summary>
    public string PageHeading { get; set; }

    /// <summary>
    /// Group icon
    /// </summary>
    public string GroupIcon { get; set; }

    /// <summary>
    /// Image
    /// </summary>
    public string Image { get; set; }

    /// <summary>
    /// Group icon
    /// </summary>
    public string SelectedGroupIcon { get; set; }

    /// <summary>
    /// Group ID of menu
    /// </summary>
    public long MenuGroupID { get; set; }

    /// <summary>
    /// Type of group: Content, Link or Both
    /// </summary>
    public ContentType GroupType { get; set; }

    /// <summary>
    /// Content of menu
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Indicates if page is to be shown as master
    /// </summary>
    public bool IsAddEditPage { get; set; }

    /// <summary>
    /// Indicates if page is to be shown as master
    /// </summary>
    public bool IsMaster { get; set; }

    /// <summary>
    /// User name to display initials
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Nav icon
    /// </summary>
    [Ignore]
    public string NavIcon { get; set; }

    //todo:
    ///// <summary>
    ///// Menu image source
    ///// </summary>
    //[Ignore]
    //public ImageSource ImageSource { get; set; }
    //public byte[] ImageBytes { get; set; }

    ///// <summary>
    ///// Base64 of image
    ///// </summary>
    //public string ImageBase64 { get; set; }

    ///// <summary>
    ///// Bytes of image
    ///// </summary>
    //public byte[] ImageBytes { get; set; }

    /// <summary>
    /// True if menu is available at organisaton level
    /// </summary>
    [Ignore]
    public bool AvailableAtOrganisationLevel { get; set; }

    /// <summary>
    /// True if menu is available at branch level
    /// </summary>
    [Ignore]
    public bool AvailableAtBranchLevel { get; set; }

    /// <summary>
    /// True if menu is available at department level
    /// </summary>
    [Ignore]
    public bool AvailableAtDepartmentLevel { get; set; }

    /// <summary>
    /// Yes If Scroll to is enable
    /// </summary>
    [Ignore]
    public string IsScrollable { get; set; }

    /// <summary>
    /// Menu items for managing hierarchical menus
    /// </summary>
    [Ignore]
    [DataMember]
    public List<MenuModel> MenuItems { get; set; }

    /// <summary>
    /// Badge count to be displayed
    /// </summary>
    public string BadgeCount { get; set; }

    /// <summary>
    /// Is Patient menu 
    /// </summary>
    public bool IsPatientMenu { get; set; }

    /// <summary>
    /// Is Patient menu 
    /// </summary>
    [Ignore]
    public string PaientMenuRoute { get; set; }
}
