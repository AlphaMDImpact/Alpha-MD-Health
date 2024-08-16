using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireSubscaleModel
    {
        [PrimaryKey]
        public long SubscaleID { get; set; }
        public long QuestionnaireID { get; set; }
        public QuestionnaireSubscaleScoreType ScoreTypeID { get; set; }
        public bool IsActive { get; set; }
    }
}
