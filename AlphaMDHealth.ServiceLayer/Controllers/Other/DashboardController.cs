using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/DashboardService")]
    public class DashboardController : BaseController
    {
        /// <summary>
        /// Get Dashboard Configurations
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
        /// <param name="dashboardSettingID">Id of Dashboard Setting</param>
        /// <param name="roleID">Id of Role</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Dashboard Configurations</returns>
        [Route("GetDashboardConfigurationsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetDashboardConfigurationsAsync(byte languageID, long permissionAtLevelID, long selectedUserID, long dashboardSettingID, byte roleID, long recordCount)
        {
            return HttpActionResult(await new DashboardServiceBusinessLayer(HttpContext).GetDashboardConfigurationsAsync(languageID, permissionAtLevelID, selectedUserID, dashboardSettingID, roleID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Dashboard Configuration
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="dashboardData">Dashboard Configuration data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveDashboardConfigurationAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveDashboardConfigurationAsync(byte languageID, long permissionAtLevelID, [FromBody] DashboardDTO dashboardData)
        {
            return HttpActionResult(await new DashboardServiceBusinessLayer(HttpContext).SaveDashboardConfigurationAsync(dashboardData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }
    }
}