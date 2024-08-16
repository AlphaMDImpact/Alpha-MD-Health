using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model;

/// <summary>
/// Amh View Cell Model
/// </summary>
public class AmhViewCellModel
{
    /// <summary>
    /// Cell Header
    /// </summary>
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// Group name Field
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// property name of Group ID Field
    /// </summary>
    public string GroupIDField { get; set; } = string.Empty;

    /// <summary>
    /// Group name Field
    /// </summary>
    public string ChildItems { get; set; } = string.Empty;

    //public IEnumerable<AmhViewCellModel> Items { get; set; } 

    /// <summary>
    /// Band color to display
    /// </summary>
    public string BandColor { get; set; } = string.Empty;

    /// <summary>
    /// Left Image Control Type
    /// </summary>
    public FieldTypes LeftFieldType { get; set; } = FieldTypes.SquareWithBorderAndBackgroundImageControl;

    /// <summary>
    /// Left Image Field
    /// </summary>
    public string LeftField { get; set; }

    /// <summary>
    /// Left Image base 64 string
    /// </summary>
    public string LeftImage { get; set; } = string.Empty;

    /// <summary>
    /// Left Image Icon 
    /// </summary>
    public string LeftIcon { get; set; } = string.Empty;

    /// <summary>
    /// Right Image Control Type
    /// </summary>
    public FieldTypes RightFieldType { get; set; } = FieldTypes.CircleImageControl;

    /// <summary>
    /// Right Image Field
    /// </summary>
    public string RightField { get; set; }

    /// <summary>
    /// Right Image base 64 string
    /// </summary>
    public string RightImage { get; set; } = string.Empty;

    /// <summary>
    /// Right Image Icon 
    /// </summary>
    public string RightIcon { get; set; } = string.Empty;

    /// <summary>
    /// Right Image base 64 string
    /// </summary>
    public string RightImageText { get; set; } = string.Empty;

    /// <summary>
    /// Right Html view field
    /// </summary>
    public string RightHTMLLabelField { get; set; } = string.Empty;

    ///// <summary>
    ///// Right view to render
    ///// </summary>
    //public string RightView { get; set; } = string.Empty;

    /// <summary>
    /// Cell Left Header Control Type
    /// </summary>
    public FieldTypes LeftHeaderFieldType { get; set; } = FieldTypes.PrimarySmallHStartVCenterBoldLabelControl;

    /// <summary>
    /// Cell Left Header Field
    /// </summary>
    public string LeftHeaderField { get; set; }

    /// <summary>
    /// Cell Left Header Field
    /// </summary>
    public string LeftHeader { get; set; } = string.Empty;

    /// <summary>
    /// IsLeftHeaderSearchable
    /// </summary>
    public bool IsLeftHeaderSearchable { get; set; } = true;

    /// <summary>
    /// Cell Left Description Control Type
    /// </summary>
    public FieldTypes LeftDescriptionFieldType { get; set; } = FieldTypes.TertiarySmallHEndVCenterLabelControl;

    /// <summary>
    /// Cell Right Description Field
    /// </summary>
    public string LeftDescriptionField { get; set; }

    /// <summary>
    /// Cell Left Description Field
    /// </summary>
    public string LeftDescription { get; set; } = string.Empty;

    /// <summary>
    /// Cell Right Header Control Type
    /// </summary>
    public FieldTypes RightHeaderFieldType { get; set; } = FieldTypes.SecondarySmallHEndVCenterLabelControl;

    /// <summary>
    /// Cell Right Header Field
    /// </summary>
    public string RightHeaderField { get; set; }

    /// <summary>
    /// Cell Right Header Field
    /// </summary>
    public string RightHeader { get; set; } = string.Empty;

    /// <summary>
    /// Cell Right Header Field
    /// </summary>
    public bool isClickableRightHeader { get; set; } = false;

    /// <summary>
    /// Cell Right Description Control Type
    /// </summary>
    public FieldTypes RightDescriptionFieldType { get; set; } = FieldTypes.TertiarySmallHEndVCenterLabelControl;

    /// <summary>
    /// Cell Right Description Field
    /// </summary>
    public string RightDescriptionField { get; set; }

    /// <summary>
    /// Cell Right Description Field
    /// </summary>
    public string RightDescription { get; set; } = string.Empty;
}
