using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/PatientEducationService")]
    [ApiAuthorize]
    public class PatientEducationController : BaseController
    {
        /// <summary>
        /// Save Assigned education to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>        
        /// <param name="educationData">Assigned Education data to be saved</param>
        /// <returns>Operation Status</returns>
        [Route("SavePatientEducationAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientEducationAsync(byte languageID, long permissionAtLevelID, [FromBody] ContentPageDTO educationData)
        {
            return HttpActionResult(await new PatientEducationServiceBusinessLayer(HttpContext).SavePatientEducationAsync(permissionAtLevelID, educationData).ConfigureAwait(false), languageID);
        }
    }
}