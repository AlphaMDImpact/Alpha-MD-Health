using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    public class MedicineModel
	{
		[MyCustomAttributes(ResourceConstants.R_MEDICINE_NAME_KEY)]
		public string ShortName { get; set; }

		public string FullName { get; set; }

		[MyCustomAttributes(ResourceConstants.R_UNIT_KEY)]
		public string UnitIdentifier { get; set; }

		public string MedicineType { get; set; }
        public bool IsCritical { get; set; }

        public bool IsActive { get; set; }
	}
}