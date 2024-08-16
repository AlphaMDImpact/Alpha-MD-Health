using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class QuestionnaireQuestionsLinkModel
    {
       public long QuestionnaireID { get; set; }
       public long QuestionID { get; set; }
       public bool IsStartingQuestion { get; set; }
       public string CaptionText { get; set; }
       public QuestionType QuestionTypeID { get; set; }
       public int Flows { get; set; }
       public bool IsActive { get; set; }
    }
}
