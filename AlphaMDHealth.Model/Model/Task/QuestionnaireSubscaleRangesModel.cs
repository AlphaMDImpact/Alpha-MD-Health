using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireSubscaleRangesModel
    {
        [PrimaryKey]
        public long SubScaleRangeID { get; set; }
        public long SubScaleID { get; set; }
        [MyCustomAttributes(ResourceConstants.R_MIN_TEXT_KEY)]
        public float MinValue { get; set; }
        [MyCustomAttributes(ResourceConstants.R_MAX_TEXT_KEY)]
        public float MaxValue { get; set; }
        public bool IsActive { get; set; }
        [Ignore]
        public string PageHeading { get; set; }
    }
}
