using AlphaMDHealth.Utility;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// data transfer object for mediccal history
    /// </summary>
    public class MedicalHistoryDTO : BaseDTO
	{
		/// <summary>
		/// List of feature views to load in medical history
		/// </summary>
		public List<MedicalHistoryViewModel> AllMedicalHistoryViews { get; set; }

		/// <summary>
		/// List of medical history date
		/// </summary>
		[DataMember]
		public List<MedicalHistoryModel> MedicalHistory { get; set; }

		/// <summary>
		/// List of Add medical history feature options
		/// </summary>
		[DataMember]
		public List<OptionModel> AddHistoryFor { get; set; }

		public MedicalReportForwards MedicalReportForwards { get; set; }

		public string OrganisationAddress { get; set; }

		public string OrganisationContact { get; set; } 

		public string HtmlString { get; set; }

		public double DateTimeDifference { get; set; }
		public ErrorCode Error { get; set; }
		public Byte[] PdfBytes { get; set; }
	}
}