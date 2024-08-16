using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/MasterService")]
    [ApiAuthorize(ErrorCode.RefreshMasterPage)]
    public class MasterController : BaseController
    {
        /// <summary>
        /// Get master Page data for anonymous users
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="selectedUserID">Selected User's ID </param>
        /// <param name="organisationDomain">Client Domain</param>
        /// <returns>Master page data with operation status</returns>
        [Route("GetMasterDataAsync")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetMasterDataAsync(byte languageID, long selectedUserID, string organisationDomain, long accountID)
        {
            return HttpActionResult(await new MasterServiceBusinessLayer(HttpContext).GetMasterDataAsync(languageID, selectedUserID, organisationDomain, accountID).ConfigureAwait(false), languageID);
        }
    }
}