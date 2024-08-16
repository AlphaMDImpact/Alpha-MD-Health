using SQLite;
using AlphaMDHealth.Utility;
namespace AlphaMDHealth.Model
{
    public class ProgramNoteModel
    {
        [PrimaryKey]
        public long ProgramNoteID { get; set; }
        public long ProgramID { get; set; }
        public long QuestionnaireID { get; set; }
        public long OrganisationID { get; set; }
        public bool IsActive { get; set; }
        [Ignore]
        public string Questionnaire { get; set; }
        
        [MyCustomAttributes(ResourceConstants.R_NOTE_TYPE_KEY)]
        public string NoteText { get; set; }

        [MyCustomAttributes(ResourceConstants.R_NOTE_DESCRIPTION_KEY)]
        public string NoteDescription { get; set; }
        [Ignore]
        public string LanguageName { get; set; }
        [Ignore]
        public byte LanguageID { get; set; }
    }
}
