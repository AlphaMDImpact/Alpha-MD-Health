using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ReasonService")]
    [ApiAuthorize]
    public class ReasonController : BaseController
    {
        /// <summary>
        /// Get Reasons
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="reasonID">get records modified after last modified date</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <returns>List of reasons and operation status</returns>
        [Route("GetReasonsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetReasonsAsync(byte languageID, long permissionAtLevelID, byte reasonID, long recordCount)
        {
            return HttpActionResult(await new ReasonServiceBussinessLayer(HttpContext).GetReasonsAsync(languageID, permissionAtLevelID, reasonID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Reason To Database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="reasonDTO">Organisation id</param>
        /// <returns>Operation status</returns>
        [Route("SaveReasonAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveReasonAsync(byte languageID, long permissionAtLevelID, [FromBody] ReasonDTO reasonDTO)
        {
            return HttpActionResult(await new ReasonServiceBussinessLayer(HttpContext).SaveReasonAsync(reasonDTO, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets Program Reasons Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programReasonID">Program Reason ID</param>
        /// <returns>Return Program Reason Data and Operation status</returns>
        [Route("GetProgramReasonsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramReasonsAsync(byte languageID, long permissionAtLevelID, long recordCount, long programReasonID)
        {
            return HttpActionResult(await new ReasonServiceBussinessLayer(HttpContext).GetProgramReasonsAsync(languageID, permissionAtLevelID,  recordCount, programReasonID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Program reason
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program reason Data</param>
        /// <returns>operation status</returns>
        [Route("SaveProgramReasonAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramReasonAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ReasonServiceBussinessLayer(HttpContext).SaveProgramReasonAsync(permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }
    }
}
