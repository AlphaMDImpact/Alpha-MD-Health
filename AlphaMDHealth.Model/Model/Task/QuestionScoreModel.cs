using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class QuestionScoreModel
    {
        
        public long QuestionID { get; set; }
        public long OptionID { get; set; }
        public string OptionText { get; set; }
        public string InstructionText { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double? ScoreValue { get; set; }
        public bool IsActive { get; set; }
    }
}
