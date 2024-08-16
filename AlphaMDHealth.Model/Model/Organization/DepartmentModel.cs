using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class DepartmentModel : LanguageModel
    {
        public byte DepartmentID { get; set; }

		[MyCustomAttributes(ResourceConstants.R_DEPARTMENT_NAME_TEXT_KEY)]
		public string DepartmentName { get; set; }
    }
}