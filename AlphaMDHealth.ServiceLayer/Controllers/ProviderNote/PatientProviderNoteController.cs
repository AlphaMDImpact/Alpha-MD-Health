using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/PatientProviderNoteService")]
	[ApiAuthorize]
	public class PatientProviderNoteController : BaseController
	{
		/// <summary>
		/// Get patient provider note(s)
		/// </summary>
		/// <param name="languageID">language ID</param>
		/// <param name="selectedUserID">patient ID </param>
		/// <param name="permissionAtLevelID">level at which permission is required </param>
		/// <param name="organisationID">Id of organization</param>
		/// <param name="recordCount">number of record count to fetch</param>
		/// <param name="providerNoteID">providerNote ID</param>
		/// <param name="programID">Program ID</param>
		/// <param name="questionnaireID">Questionnaire ID</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <returns>provider note(s) with operation status</returns>
		[Route("GetPatientProviderNotesAsync")]
		[HttpGet]
		[ApiAuthorize]
		public async Task<IActionResult> GetPatientProviderNotesAsync(byte languageID, long selectedUserID, long permissionAtLevelID, long organisationID, long recordCount, Guid providerNoteID, long programID, long questionnaireID, string fromDate, string toDate)
		{
			return HttpActionResult(await new PatientProviderNoteServiceBusinessLayer(HttpContext).GetPatientProviderNotesAsync(languageID, selectedUserID, permissionAtLevelID, organisationID, recordCount, providerNoteID, programID, questionnaireID, fromDate, toDate).ConfigureAwait(false), languageID);
		}

		/// <summary>
		/// Save Provider Note Data
		/// </summary>
		/// <param name="languageID">language ID</param>
		/// <param name="permissionAtLevelID">level at which permission is required </param>
		/// <param name="patientProviderNoteData">Reference object which holds Patient Provider Note Data</param>
		/// <returns>Operation status<</returns>
		[Route("SavePatientProviderNoteAsync")]
		[HttpPost]
		public async Task<IActionResult> SavePatientProviderNoteAsync(byte languageID, long permissionAtLevelID, [FromBody] PatientProviderNoteDTO patientProviderNoteData)
		{
			return HttpActionResult(await new PatientProviderNoteServiceBusinessLayer(HttpContext).SavePatientProviderNoteAsync(permissionAtLevelID, patientProviderNoteData).ConfigureAwait(false), languageID);
		}
	}
}
