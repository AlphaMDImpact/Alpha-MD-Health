using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class ContentPageModel : BaseListItemModel
    {
         [PrimaryKey]
        public long PageID { get; set; }

        [MyCustomAttributes(ResourceConstants.R_CATEGORY_KEY)]
        public long EducationCategoryID { get; set; }
        public long EducationID { get; set; }
        public bool IsEducation { get; set; }

        [MyCustomAttributes(ResourceConstants.R_TAGS_KEY)]
        public string PageTags { get; set; }
        public bool IsLink { get; set; }
        public bool IsPdf { get; set; }
        public bool IsPublished { get; set; }
        public string Title { get; set; }
        public string PageData { get; set; }
        public string Description { get; set; }
        [Ignore]
        public string PageType { get; set; }

        public string Status { get; set; }
        public long ProgramEducationID { get; set; }
        public DateTimeOffset AddedOn { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public string DateStyle { get; set; }
        public string ToDateString { get; set; }
        public string FromDateString { get; set; }
        public string ImageBase64 { get; set; }

        [MyCustomAttributes(ResourceConstants.R_SELECT_ICON_KEY)]
        public string ImageName { get; set; }

        public string PDFName { get; set; }

        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        public string CategoryDetails { get; set; }
        [Ignore]
        public string Name { get; set; }
        [Ignore]
        public string StatusColor { get; set; }
        public bool IsDataDownloaded { get; set; }
        public bool IsActive { get; set; }
        [Ignore]
        public string EducationIDs { get; set; }
        public string ProgramColor { get; set; }
        public string ProgramName { get; set; }

        /// <summary>
        /// ProgramID
        /// </summary>
        public long ProgramID { get; set; }
        public string IsPatientPage { get; set; }
    }
}