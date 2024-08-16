namespace AlphaMDHealth.Model
{
    public class MedicationReminderModel
    {
        public Guid PatientMedicationID { get; set; }
        public string ReminderId { get; set; }
        public DateTimeOffset ReminderDateTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsSynced { get; set; }
    }
}