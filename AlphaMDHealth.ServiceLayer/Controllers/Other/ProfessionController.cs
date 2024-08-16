using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ProfessionService")]
    [ApiAuthorize]
    public class ProfessionController : BaseController
    {
        /// <summary>
        /// Get Profession data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="professionID">Profession ID</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Proessions</returns>
        [Route("GetProfessionsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetProfessionsAsync(byte languageID, long permissionAtLevelID, byte professionID, long recordCount)
        {
            return HttpActionResult(await new ProfessionServiceBusinessLayer(HttpContext).GetProfessionsAsync(languageID, permissionAtLevelID, professionID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Profession data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="profession">Profession data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveProfessionAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveProfessionAsync(byte languageID, long permissionAtLevelID, [FromBody] ProfessionDTO profession)
        {
            return HttpActionResult(await new ProfessionServiceBusinessLayer(HttpContext).SaveProfessionAsync(languageID, permissionAtLevelID, profession).ConfigureAwait(false), languageID);
        }
    }
}
