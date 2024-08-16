using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model;

public class TableDataStructureModel
{
    public string DataField { get; set; }

    public string DataHeader { get; set; }
    public string DataHeaderValue { get; set; }
    public bool IsKey { get; set; }
    public bool IsHidden { get; set; }
    public bool IsSortable { get; set; } = true;
    public bool IsSearchable { get; set; } = true;
    public bool ShowRowColumnHeader { get; set; } = true;
    public bool IsDescending { get; set; }
    public bool ConcatField { get; set; }
    public string Formatter { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
    public string ResourceKey { get; set; }
    public string RegexPattern { get; set; }
    public bool HasImage { get; set; }

    public bool IsLink { get; set; }

    public string IsLinkField { get; set; } 

    public bool IsCheckBox { get; set; }
    public bool IsBadge { get; set; }
    public string BadgeFieldType { get; set; }
    public FieldTypes LinkFieldType { get; set; }
    public List<string> ImageFields { get; set; }
    public string LinkText { get; set; }
    public bool IsEndAligned { get; set; }
    public bool IsHtmlTag { get; set; }

    /// <summary>
    /// ImageSrc or ImageBytes 
    /// </summary>
    public string ImageSrc { get; set; }

    /// <summary>
    /// ImageSrc or ImageBytes 
    /// </summary>
    public string ImageBytes { get; set; }

    public string ImageIcon { get; set; }
    public string ImageBackgroundColor { get; set; }
    public FieldTypes ImageFieldType { get; set; }
    public AppImageSize ImageHeight { get; set; }
    public AppImageSize ImageWidth { get; set; }
    public string BorderColorDataField { get; set; } = string.Empty;
    public string MaxColumnWidthSize { get; set; }
}
