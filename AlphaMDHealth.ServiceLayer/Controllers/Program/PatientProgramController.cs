using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/PatientProgramService")]
    [ApiAuthorize]
    public class PatientProgramController : BaseController
    {
        /// <summary>
        /// Get Programs assigned to patient
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="patientProgramID">Patient program id</param>
        /// <param name="recordCount">Record Count</param>
        /// <returns>Program Data</returns>
        [Route("GetPatientProgramsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetPatientProgramsAsync(byte languageID, long permissionAtLevelID, long selectedUserID, long patientProgramID, long recordCount)
        {
            return HttpActionResult(await new PatientProgramServiceBusinessLayer(HttpContext).GetPatientProgramsAsync(languageID, permissionAtLevelID, selectedUserID, patientProgramID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Patient Program Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="programData">Reference object which holds Program Task Data</param>
        /// <returns>Operation Status</returns>
        [Route("SavePatientProgramsAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientProgramsAsync(byte languageID, long permissionAtLevelID, long selectedUserID, [FromBody] PatientProgramDTO programData)
        {
            return HttpActionResult(await new PatientProgramServiceBusinessLayer(HttpContext).SavePatientProgramsAsync(languageID, permissionAtLevelID, selectedUserID, programData).ConfigureAwait(false), languageID);
        }
    }
}