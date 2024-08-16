using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/HealthScansService")]
    [ApiAuthorize]
    public class HealthScansController : BaseController
    {
        /// <summary>
        /// Get HealthScans
        /// </summary>
        /// <param name="languageID">Id of current language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="selectedOrganisationID">Selected Organisation ID</param>
        /// <param name="transacationID">transacation ID</param>
        /// <returns>HealthScans Data with operation status</returns>
        [Route("GetHealthScansAsync")]
        [HttpGet]
        public async Task<IActionResult> GetHealthScansAsync(byte languageID, long permissionAtLevelID, long recordCount, long selectedOrganisationID, long transactionID)
        {
            return HttpActionResult(await new CareFlixHealthScanServiceBussinessLayer(HttpContext).GetHealthScansAsync(languageID, permissionAtLevelID, recordCount, selectedOrganisationID, transactionID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Health Scan
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's organisation ID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="healthScanData">healthScan data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveHealthScanAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveHealthScanAsync(byte languageID, long permissionAtLevelID, [FromBody] HealthScanDTO healthScanData)
        {
            return HttpActionResult(await new CareFlixHealthScanServiceBussinessLayer(HttpContext).SaveHealthScanAsync(languageID,permissionAtLevelID, healthScanData, false).ConfigureAwait(false), languageID);
        }
    }
}