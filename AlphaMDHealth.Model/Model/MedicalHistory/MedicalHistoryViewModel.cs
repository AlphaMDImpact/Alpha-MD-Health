using AlphaMDHealth.Utility;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model to store Medical History information
    /// </summary>
    public class MedicalHistoryViewModel
	{
		/// <summary>
		/// Feature code
		/// </summary>
		public AppPermissions FeatureCode { get; set; }

		/// <summary>
		/// Operation status
		/// </summary>
		public ErrorCode ErrorCode { get; set; }

		/// <summary>
		/// SequenceNo of view
		/// </summary>
		public short SequenceNo { get; set; }

		/// <summary>
		/// Flag representing show all data in one go or show it based on date ranges of view
		/// </summary>
		public bool ShowAllData { get; set; }

		/// <summary>
		/// Flag representing data is present for the view or not
		/// </summary>
		public bool HasData { get; set; }

		/// <summary>
		/// Data to load in the view
		/// </summary>
		public object PageData { get; set; }
	}
}