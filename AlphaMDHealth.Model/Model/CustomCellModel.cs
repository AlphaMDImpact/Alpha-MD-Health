using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Custom Cell Model
    /// </summary>
    public class CustomCellModel
    {
        /// <summary>
        /// Cell Header
        /// </summary>
        public string CellID { get; set; } = string.Empty;

        /// <summary>
        /// Cell Header
        /// </summary>
        public string CellHeader { get; set; } = string.Empty;

        /// <summary>
        /// Cell Description
        /// </summary>
        public string CellDescription { get; set; } = string.Empty;

        /// <summary>
        /// Cell Right Content Header
        /// </summary>
        public string CellRightContentHeader { get; set; } = string.Empty;

        /// <summary>
        /// Cell Right Content Description
        /// </summary>
        public string CellRightContentDescription { get; set; } = string.Empty;

        /// <summary>
        /// image array
        /// </summary>
        public string ImageByte { get; set; }

        /// <summary>
        /// image array
        /// </summary>
        public string NameInitial { get; set; }

        /// <summary>
        /// image array
        /// </summary>
        public string ImageIcon { get; set; } 





        /// <summary>
        /// Cell Header in link style
        /// </summary>
        public bool IsLinkHeader { get; set; }

        /// <summary>
        /// Cell Header InBold
        /// </summary>
        public bool IsCellHeaderInBold { get; set; }
        /// <summary>
        /// Cell Header InBold
        /// </summary>
        public bool IsCellRightContentHeaderInBold { get; set; }

        /// <summary>
        /// Cell left Source Icon
        /// </summary>
        public string CellLeftSourceIcon { get; set; } = string.Empty;

        /// <summary>
        /// Cell left Default Icon
        /// </summary>
        public string CellLeftDefaultIcon { get; set; } = string.Empty;

        /// <summary>
        /// Cell left Url Icon
        /// </summary>
        public string CellLeftIconPath { get; set; } = string.Empty;

        /// <summary>
        /// Cell Right Icon
        /// </summary>
        public string CellRightIcon { get; set; } = string.Empty;

        /// <summary>
        /// Svg Image Icon Size 
        /// </summary>
        public AppImageSize IconSize { get; set; } = AppImageSize.ImageSizeL;

        /// <summary>
        /// Svg Image Icon Size 
        /// </summary>
        public AppImageSize RightIconSize { get; set; } = AppImageSize.ImageSizeXXS;

        /// <summary>
        /// Flag for Arrange Horizontal
        /// </summary>
        public bool ArrangeHorizontal { get; set; }

        /// <summary>
        /// Flag for RemoveLeftMargin
        /// </summary>
        public bool RemoveLeftMargin { get; set; }

        /// <summary>
        /// Flag for Arrange Horizontal
        /// </summary>
        public string ArrangeHorizontalFullWith { get; set; }

        /// <summary>
        /// both arranagement
        /// </summary>
        public bool ArrangeHorizontalVertical { get; set; }

        /// <summary>
        /// both arranagement
        /// </summary>
        public bool IsDescriptionHtml { get; set; }

        /// <summary>
        /// both arranagement
        /// </summary>
        public bool IsRightHeaderHtml { get; set; }

        /// <summary>
        /// both arranagement
        /// </summary>
        public bool IsCircle { get; set; }

        /// <summary>
        /// Arrange Horizontal Width 
        /// </summary>
        public string ArrangeHorizontalWidth { get; set; }

        /// <summary>
        /// Arrange Horizontal Height 
        /// </summary>
        public string ArrangeHorizontalHeight { get; set; }

        /// <summary>
        /// Flag for No Margin Separator
        /// </summary>
        public bool NoMarginNoSeprator { get; set; }

        /// <summary>
        /// Flag for No Margin Separator
        /// </summary>
        public bool IsList { get; set; }

        /// <summary>
        /// Cell Right Content Header
        /// </summary>
        public string IconAsCellRightContentHeader { get; set; } = string.Empty;

        /// <summary>
        /// Cell Right Content Header
        /// </summary>
        public string ShowRemoveButton { get; set; }

        /// <summary>
        /// Right Description Style
        /// </summary>
        public string RightDesciptionStyle { get; set; } = StyleConstants.ST_STATUS_PANCAKE_STYLE;
        
        /// <summary>
        /// Right Description Style
        /// </summary>
        public string RightHeaderStyle { get; set; } = StyleConstants.ST_STATUS_PANCAKE_STYLE;

        /// <summary>
        /// status without or with border
        /// </summary>
        public bool CellRightContentDescriptionInPancakeView { get; set; } = true;

        /// <summary>
        /// status without or with border
        /// </summary>
        public bool CellRightContentHeaderInPancakeView { get; set; }

        /// <summary>
        /// Cell Descitpiton Colour
        /// </summary>
        public string CellDescriptionColor { get; set; } = string.Empty;

        //below used in medication only
        /// <summary>
        /// Cell description status
        /// </summary>
        public string CellDescriptionStatusContent { get; set; } = string.Empty;

        /// <summary>
        /// Cell description status color
        /// </summary>
        public string CellDescriptionStatusContentColor { get; set; } = string.Empty;

        /// <summary>
        /// Cell header Colour
        /// </summary>
        public string CellRightHeaderColor { get; set; } = string.Empty;

        /// <summary>
        /// Cell Descitpiton Colour
        /// </summary>
        public string BandColor { get; set; } = string.Empty;

        /// <summary>
        /// Stores ErrorCode in String 
        /// </summary>
        public string ErrCode { get; set; }

        /// <summary>
        /// is un read header
        /// </summary>
        public string IsUnreadHeader { get; set; }

        /// <summary>
        /// is un read dot
        /// </summary>
        public string ShowUnreadBadge { get; set; }

        /// <summary>
        /// is un read dot
        /// </summary>
        public string ImageBackgroundColor { get; set; }

        //below used in appoinment only
        /// <summary>
        /// Cell First Middle Content Header Header
        /// </summary>
        public string CellFirstMiddleSatusContentHeader { get; set; } = string.Empty;

        /// <summary>
        /// Cell Right Content Header
        /// </summary>
        public string CellSecondMiddleSatusContentHeader { get; set; } = string.Empty;

        /// <summary>
        /// Right Description Style
        /// </summary>
        public string CellMiddleSatusHeaderStyle { get; set; } = StyleConstants.ST_STATUS_PANCAKE_STYLE;

        /// <summary>
        /// Cell header Colour
        /// </summary>
        public string CellFirstMiddleContentHeaderColor { get; set; } = string.Empty;

        /// <summary>
        /// Cell header Colour
        /// </summary>
        public string CellSecondMiddleSatusContentHeaderColor { get; set; } = string.Empty;

        /// <summary>
        /// tint color Property
        /// </summary>
        public string LeftTintColor { get; set; } = string.Empty;

        /// <summary>
        /// row height
        /// </summary>
        public double RowHeight { get; set; }

        /// <summary>
        /// ViewCell IsEnabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// RightContentDescriptionInPancakeViewWithoutBorder
        /// </summary>
        public bool RightContentDescriptionInPancakeViewWithoutBorder { get; set; }

        /// <summary>
        /// Show Icon Only
        /// </summary>
        public bool ShowIconOnly { get; set; }

        /// <summary>
        /// this is used to remove column spacing in case of no description available
        /// </summary>
        public bool RemoveColumnSpacing { get; set; } = true;

    }
}
