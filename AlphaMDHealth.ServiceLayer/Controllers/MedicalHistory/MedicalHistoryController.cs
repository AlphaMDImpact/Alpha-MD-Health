using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    /// <summary>
    /// Medical history service
    /// </summary>
    [Route("api/MedicalHistoryService")]
	[ApiAuthorize]
	public class MedicalHistoryController : BaseController
	{
		/// <summary>
		/// Get Medical History data for patient/program
		/// </summary>
		/// <param name="languageID">User's language ID</param>
		/// <param name="organisationID">User's Organisation ID</param>
		/// <param name="permissionAtLevelID">User's permission level</param>
		/// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <param name="dateTimeDifference">utc and local date time difference</param>
		/// <returns>Medical History data with operation status</returns>
		[Route("GetMedicalHistoryAsync")]
		[HttpGet]
		public async Task<IActionResult> GetMedicalHistoryAsync(byte languageID, long organisationID, long permissionAtLevelID, long selectedUserID, string fromDate, string toDate, double dateTimeDifference, bool showAllData = false)
		{
			return HttpActionResult(await new MedicalHistoryServiceBusinessLayer(HttpContext).GetMedicalHistoryAsync(languageID, organisationID, permissionAtLevelID, selectedUserID, fromDate, toDate, dateTimeDifference,showAllData).ConfigureAwait(false), languageID);
		}

		/// <summary>
		/// Get Medical Report Forwards
		/// </summary>
		/// <param name="permissionAtLevelID">level at which permission is required</param>
		/// <param name="organisationID">organisation ID</param>
		/// <param name="languageID">Language ID</param>
		/// <returns>Resources and settings</returns>
		[Route("GetMedicalReportForwardsAsync")]
		[HttpGet]
		public async Task<IActionResult> GetMedicalReportForwardsAsync(long permissionAtLevelID, long organisationID, byte languageID)
		{
			return HttpActionResult(await new MedicalHistoryServiceBusinessLayer(HttpContext).GetMedicalReportForwardsAsync(permissionAtLevelID, organisationID, languageID).ConfigureAwait(false), languageID);
		}

		/// <summary>
		/// Save Medical Report Forwards
		/// </summary>
		/// <param name="languageID">Selected language id</param>
		/// <param name="permissionAtLevelID">Level at which permission is required</param>
		/// <param name="organisationID">organisation ID</param>
		/// <param name="medicalHistoryData">Reference object which holds Data</param>
		/// <returns>Result of operation</returns>
		[Route("SaveMedicalReportForwardsAsync")]
		[HttpPost]
		[ApiAuthorize]
		public async Task<IActionResult> SaveMedicalReportForwardsAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] MedicalHistoryDTO medicalHistoryData)
		{
			return HttpActionResult(await new MedicalHistoryServiceBusinessLayer(HttpContext).SaveMedicalReportForwardsAsync(medicalHistoryData, permissionAtLevelID, organisationID).ConfigureAwait(false), languageID);
		}
	}
}