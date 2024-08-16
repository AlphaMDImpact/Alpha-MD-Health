namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Object for mediccal history details
    /// </summary>
    public class MedicalHistoryModel
	{
		/// <summary>
		/// Date for which Medical history needs to load
		/// </summary>
		public DateTimeOffset HistoryDate { get; set; }

		/// <summary>
		/// Sequence No of Datw
		/// </summary>
		public short SequenceNo { get; set; }

		/// <summary>
		/// Flag representing data is loaded for this range or not
		/// </summary>
		public bool IsLoaded { get; set; }

		/// <summary>
		/// List of feature views to load in medical history
		/// </summary>
		public string HistoryFromDate { get; set; } = null;

		/// <summary>
		/// List of feature views to load in medical history
		/// </summary>
		public string HistoryToDate { get; set; } = null;

		/// <summary>
		/// List of feature views to load in medical history
		/// </summary>
		public List<MedicalHistoryViewModel> MedicalHistoryViews { get; set; }

	}
}