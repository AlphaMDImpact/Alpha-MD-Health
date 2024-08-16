namespace AlphaMDHealth.Model
{
    public class QuestionConditionModel
    {
        public long QuestionID { get; set; }

        /// <summary>
        /// Stores Option Id 
        /// </summary>
        public int OptionID { get; set; }

        /// <summary>
        /// Stores Option Text
        /// </summary>
        public string OptionText { get; set; }


        public double Value1 { get; set; }
        public double Value2 { get; set; }

        public int UIID { get; set; } = 0;
        public long? TargetQuestionID { get; set; }
    }
}
