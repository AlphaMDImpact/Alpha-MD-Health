using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ReportsService")]
    [ApiAuthorize]
    public class ReportsController : BaseController
    {        
        /// <summary>
        /// Get Bills 
        /// </summary>
        /// <param name="permissionAtLevelID">Permission At</param>
        /// <param name="fromDate">From Date</param>
        /// <param name="toDate">To Date</param>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="recordCount">No of Records to return</param>
        /// <returns>List of Bills and operation status</returns>
        [Route("GetBillsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetBillsAsync(long permissionAtLevelID, string fromDate, string toDate, byte languageID, long recordCount)
        {
            return HttpActionResult(await new ReportsServiceBusinessLayer(HttpContext).GetBillsAsync(permissionAtLevelID, fromDate, toDate, languageID, recordCount).ConfigureAwait(false), languageID);
        }
    }
}