namespace AlphaMDHealth.Model
{
    public class BranchDepartmentModel
    {
        public long BranchID { get; set; }
        public string BranchName { get; set; }
        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public DateTimeOffset AddedON { get; set; }
    }
}
