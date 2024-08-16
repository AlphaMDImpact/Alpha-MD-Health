using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ConsentService")]
    [ApiAuthorize]
    public class ConsentController : BaseController
    {
        /// <summary>
        /// Get Consent(s)
        /// </summary>
        /// <param name="languageID">Id of current language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="consentId">Consent Id</param>
        /// <param name="recordCount">Record Count</param>
        /// <returns>Consent Data with operation status</returns>
        [Route("GetConsentsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetConsentsAsync(byte languageID, long permissionAtLevelID, long consentId, long recordCount)
        {
            return HttpActionResult(await new ConsentServiceBusinessLayer(HttpContext).GetConsentsAsync(languageID, permissionAtLevelID, consentId, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save consent
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="consentData">reference object which holds consent data</param>
        /// <returns>Operation status</returns>
        [Route("SaveConsentAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveConsentAsync(byte languageID, long permissionAtLevelID, [FromBody] ConsentDTO consentData)
        {
            return HttpActionResult(await new ConsentServiceBusinessLayer(HttpContext).SaveConsentAsync(permissionAtLevelID, consentData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get User Consent(s)
        /// </summary>
        /// <param name="languageID">Id of current language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <returns>User Consent Data with operation status</returns>
        [Route("GetUserConsentsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetUserConsentsAsync(byte languageID, long permissionAtLevelID, long recordCount)
        {
            return HttpActionResult(await new ConsentServiceBusinessLayer(HttpContext).GetUserConsentsAsync(languageID, permissionAtLevelID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save user consents
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="consentData">reference object which holds consent data</param>
        /// <returns>Operation status</returns>
        [Route("SaveUserConsentAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveUserConsentAsync(byte languageID, [FromBody] ConsentDTO consentData)
        {
            return HttpActionResult(await new ConsentServiceBusinessLayer(HttpContext).SaveUserConsentAsync(consentData).ConfigureAwait(false), languageID);
        }
    }
}