using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class CardModel
    {
        public string CardId { get; set; }
        public string ImageBase64 { get; set; }
        public string ImageInitial { get; set; }
        public string BadgeText { get; set; }
        public string BadgeCss { get; set; }
        public string Header { get; set; }
        public string SubHeader { get; set; }
        public string FooterInstruction1 { get; set; }
        public string FooterInstruction2 { get; set; }
        public string SelectedBackground { get; set; }
        public string TintColor { get; set; }
        public string BadgeCount { get; set; }
        public string FormattedText { get; set; }
        public string FormattedTextCss { get; set; }

        public string ProgramColor { get; set; }

        /// <summary>
        /// is un read header
        /// </summary>
        public bool IsUnreadHeader { get; set; }

        /// <summary>
        /// is un read dot
        /// </summary>
        public bool ShowUnreadBadge { get; set; }

        /// <summary>
        /// Control Type
        /// </summary>
        public FieldTypes ControlType { get; set; }

        /// <summary>
        /// Image Circle or not
        /// </summary>
        public bool IsCircle { get; set; }

        /// <summary>
        /// Image Size
        /// </summary>
        public AppImageSize ImageSize { get; set; } = AppImageSize.ImageSizeL;

        /// <summary>
        /// IsFileType
        /// </summary>
        public bool IsFileType { get; set; }

        public bool ShowAccept { get; set; }

        public bool IsHTMLContent { get; set; } = false;

        /// <summary>
        /// Unique key
        /// </summary>
        public string UniqueKey { get; set; }
    }
}