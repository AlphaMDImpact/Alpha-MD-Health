using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class BranchModel : LanguageModel
    {
        public long BranchID { get; set; }
        public long DepartmentsCount { get; set; }

        [MyCustomAttributes(ResourceConstants.R_BRANCH_NAME_KEY)]
        public string BranchName { get; set; }
        public DateTimeOffset AddedON { get; set; }
    }
}