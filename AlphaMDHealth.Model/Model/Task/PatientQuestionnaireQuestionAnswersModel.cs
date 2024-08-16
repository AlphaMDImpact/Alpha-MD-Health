using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class PatientQuestionnaireQuestionAnswersModel
    {
        [PrimaryKey]
        public Guid PatientAnswerID { get; set; }       
        public byte? TaskType { get; set; }
        public long QuestionID { get; set; }
        public string PatientTaskID { get; set; }
        public string AnswerValue { get; set; }
        public long? PreviousQuestionID { get; set; }
        public long? NextQuestionID { get; set; }
        public bool IsSynced { get; set; }
        public decimal? ScoreValue { get; set; }
        public ErrorCode ErrCode { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset LastModifiedON { get; set; }
    }
}
