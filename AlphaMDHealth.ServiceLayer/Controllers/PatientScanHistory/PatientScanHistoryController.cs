using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/PatientScanHistoryService")]
    [ApiAuthorize]
    public class PatientScanHistoryController : BaseController
    {
        /// <summary>
        /// Get Organisation Tags
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="selectedUserID">get records modified after last modified date</param>
        /// <returns>List of reasons and operation status</returns>
        [Route("GetPatientScanHistoryAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetPatientScanHistoryAsync(byte languageID, long permissionAtLevelID,  long recordCount, long selectedUserID)
        {
            return HttpActionResult(await new PatientScanHistoryServiceBuisnessLayer(HttpContext).GetPatientScanHistoryAsync(languageID, permissionAtLevelID, recordCount, selectedUserID).ConfigureAwait(false), languageID);
        }
    }
}
