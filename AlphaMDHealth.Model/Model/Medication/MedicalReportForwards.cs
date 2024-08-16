namespace AlphaMDHealth.Model
{
    public class MedicalReportForwards
    {
        public long PatientID { get; set; }
        public string DoctorName { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public DateTimeOffset? ReportForDate { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset AddedOn { get; set; }
        public DateTimeOffset LastModifiedON { get; set; }
    }
}
