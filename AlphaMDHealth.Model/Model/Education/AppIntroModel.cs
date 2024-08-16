using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model;

public class AppIntroModel
{
    [PrimaryKey]
    public long IntroSlideID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_SEQUENCE_KEY)]
    public int SequenceNo { get; set; }

    [MyCustomAttributes(ResourceConstants.R_UPLOAD_IMAGE_TEXT_KEY)]
    public string ImageName { get; set; }

    [MyCustomAttributes(ResourceConstants.R_HEADER_TEXT_KEY)]
    public string HeaderText { get; set; }

    [MyCustomAttributes(ResourceConstants.R_BODY_TEXT_KEY)]
    public string SubHeaderText { get; set; }
    public bool IsActive { get; set; }
    public byte LanguageID { get; set; }

    [Ignore]
    public string LanguageName { get; set; }

}