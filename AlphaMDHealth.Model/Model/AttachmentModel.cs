using SQLite;

namespace AlphaMDHealth.Model;

/// <summary>
/// Attachment Model
/// </summary>
public class AttachmentModel
{
    /// <summary>
    /// Id of the file
    /// </summary>
    public Guid FileID { get; set; }

    /// <summary>
    /// Stores FileName 
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Stores FileDescription 
    /// </summary>
    public string FileDescription { get; set; }

    /// <summary>
    /// Extension of file
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    /// Stores Image
    /// </summary>
    public string FileIcon { get; set; }

    /// <summary>
    /// base64 string or url or  name initial
    /// </summary>
    public string FileValue { get; set; }

    /// <summary>
    /// Stores icon to display in list
    /// </summary>
    public string ThumbnailValue { get; set; }

    /// <summary>
    /// Flag representing record is active or not
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// AddedBy
    /// </summary>
    public string AddedBy { get; set; }
    /// <summary>
    /// AddedOnDate
    /// </summary>      
    public string AddedOnDate { get; set; }

    /// <summary>
    /// IsSent
    /// </summary>
    public bool IsSent { get; set; }

    /// <summary>
    /// IsRelationNotExpired
    /// </summary>
    public bool IsRelationNotExpired { get; set; }

    /// <summary>
    /// IsDeleteAllowed
    /// </summary>
    public bool IsDeleteAllowed { get; set; } = true;

    /// <summary>
    /// First Popup
    /// </summary>
    public bool IsFirstPopup { get; set; } = false;

    /// <summary>
    /// editor value
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// text Color
    /// </summary>
    public string TextColor { get; set; }

    /// <summary>
    /// AddedOnDate
    /// </summary>      
    public string DateColor { get; set; }
}