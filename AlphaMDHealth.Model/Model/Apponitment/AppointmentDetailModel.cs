namespace AlphaMDHealth.Model
{
    public class AppointmentDetailModel
    {
        public long AppointmentID { get; set; }
        public string AppointmentHeader { get; set; }
        public string AppointmentInfo { get; set; }
        public byte LanguageID { get; set; }
        public string LanguageName { get; set; }
        public bool IsActive { get; set; }
    }
}
