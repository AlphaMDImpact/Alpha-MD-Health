using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireModel
    {
        [PrimaryKey]
        public long QuestionnaireID { get; set; }
        public bool IsPublished { get; set; }
        public short QuestionnaireTypeID { get; set; }
        public short DefaultRespondentID { get; set; }

        [Ignore]
        public string QuestionnaireCode { get; set; }
        [Ignore]
        public string CaptionText { get; set; }
        [Ignore]
        public string InstructionsText { get; set; }
        [Ignore]
        public QuestionnaireAction QuestionnaireAction { get; set; }
        [Ignore]
        public DateTimeOffset AddedOn { get; set; }
        [Ignore]
        public int NoOfQuestions { get; set; }
        [Ignore]
        public byte NoOfSubscales { get; set; }
        [Ignore]
        public long SubscaleID { get; set; }
        [Ignore]
        public String CreatedOn { get; set; }
        [Ignore]
        public string PublisheUnpublishText { get; set; }
        [Ignore]
        public FieldTypes StatusStyle { get; set; }
    }
}
