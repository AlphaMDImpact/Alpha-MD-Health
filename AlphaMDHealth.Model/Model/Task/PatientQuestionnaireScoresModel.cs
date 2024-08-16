using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class PatientQuestionnaireScoresModel
    {
        public Guid PatientScoreID { get; set; }
        public long SubScaleRangeID { get; set; }
        public string PatientTaskID { get; set; }
        public double UserScore { get; set; }
        public bool IsSynced { get; set; }
        public ErrorCode ErrCode { get; set; }
    }
}
