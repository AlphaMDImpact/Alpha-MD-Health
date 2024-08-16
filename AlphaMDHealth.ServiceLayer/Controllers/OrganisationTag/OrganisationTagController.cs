using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/OrganisationTagService")]
    [ApiAuthorize]
    public class OrganisationTagController : BaseController
    {
        /// <summary>
        /// Get Organisation Tags
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="reasonID">get records modified after last modified date</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <returns>List of reasons and operation status</returns>
        [Route("GetOrganisationTagsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetOrganisationTagsAsync(byte languageID, long permissionAtLevelID, long organisationTagID, long recordCount)
        {
            return HttpActionResult(await new OrganisationTagServiceBusinessLayer(HttpContext).GetOrganisationTagsAsync(languageID, permissionAtLevelID, organisationTagID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Organisation Tag To Database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationTagDTO">Organisation id</param>
        /// <returns>Operation status</returns>
        [Route("SaveOrganisationTagAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveOrganisationTagAsync(byte languageID, long permissionAtLevelID, [FromBody] OrganisationTagDTO organisationTagDTO)
        {
            return HttpActionResult(await new OrganisationTagServiceBusinessLayer(HttpContext).SaveOrganisationTagAsync(organisationTagDTO, permissionAtLevelID).ConfigureAwait(false), languageID);
        }
    }
}
