using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ScheduleJobService")]
    public class ScheduleJobController : BaseController
    {
        /// <summary>
        /// Trigger jobs for anonymous users
        /// </summary>
        /// <param name="languageID">user's selected language</param>
        /// <param name="organisationID">user's organisation id</param>
        /// <param name="selectedUserID">patient id</param>
        /// <param name="JobAction">Job action to execute</param> 
        /// <returns>Operation status</returns>
        [Route("TriggerJobsAsync")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TriggerJobsAsync(byte languageID, long organisationID, long selectedUserID, string JobAction)
        {
            return HttpActionResult(await new ScheduleJobServiceBusinessLayer(HttpContext).TriggerJobsAsync(JobAction).ConfigureAwait(false), languageID);
        }
    }
}