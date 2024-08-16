namespace AlphaMDHealth.Model
{
    public class ProgramNoteI18NModel
    {
        public long ProgramNoteID { get; set; }
        public byte LanguageID { get; set; }
        public string NoteText { get; set; }
        public bool IsActive { get; set; }
    }
}
