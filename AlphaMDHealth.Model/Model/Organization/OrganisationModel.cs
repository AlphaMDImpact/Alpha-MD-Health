using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model
{
    public class OrganisationModel
    {
        [PrimaryKey]
        public long OrganisationID { get; set; }
        public long ParentID { get; set; }
        public long DepartmentID { get; set; }
        public string OrganisationName { get; set; }

        [MyCustomAttributes(ResourceConstants.R_DOMAIN_KEY)]
        public string OrganisationDomain { get; set; }

        [MyCustomAttributes(ResourceConstants.R_TAX_NUMBER_KEY)]
        public string TaxNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset AddedON { get; set; }
        public long NoOfEmployee { get; set; }
        public long NoOfPatient { get; set; }
        public string CurrentStatus { get; set; }
        public string AddedOnString { get; set; }
        public short PlanID { get; set; }
        public bool IsManageOrganisation {  get; set; }
    }
}
