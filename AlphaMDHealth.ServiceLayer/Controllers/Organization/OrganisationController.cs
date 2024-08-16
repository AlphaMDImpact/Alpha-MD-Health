using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/OrganisationService")]
    [ApiAuthorize]
    public class OrganisationController : BaseController
    {
        /// <summary>
        /// Get organisation profile data
        /// </summary>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationIdForSetup">organisation ID</param>
        /// <param name="languageID">Language ID</param>
        /// <returns>organisation profile data with operation status</returns>
        [Route("GetOrganisationProfileAsync")]
        [HttpGet]
        public async Task<IActionResult> GetOrganisationProfileAsync(long permissionAtLevelID, long organisationIdForSetup, byte languageID)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).GetOrganisationProfileAsync(permissionAtLevelID, organisationIdForSetup, languageID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves organisation profile data
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationData">reference object which holds organisation data</param>
        /// <returns>organisation ID with operation status</returns>
        [Route("SaveOrganisationProfileAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveOrganisationProfileAsync(byte languageID, long permissionAtLevelID, [FromBody] OrganisationDTO organisationData)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).SaveOrganisationProfileAsync(permissionAtLevelID, organisationData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Organisation Settings
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="groupName">Name of group to be fetched</param>
        /// <returns>Organisation data</returns>
        [Route("GetOrganisationSettingsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetOrganisationSettingsAsync(byte languageID, long permissionAtLevelID, string groupName)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).GetOrganisationSettingsAsync(languageID, permissionAtLevelID, groupName).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Updates Organisation Settings
        /// </summary>
        /// <param name="languageID">language Id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="groupName">Name of group that is to be saved</param>
        /// <param name="settings">List of settings that needed to be updated</param>
        /// <returns>Result of operation</returns>
        [Route("UpdateOrganisationSettingsAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrganisationSettingsAsync(byte languageID, long permissionAtLevelID, string groupName, [FromBody] BaseDTO settings)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).UpdateOrganisationSettingsAsync(permissionAtLevelID, groupName, settings).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Organisation Branches
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="branchID">Id of Branch</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Organisation Branches data</returns>
        [Route("GetOrganisationBranchesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetOrganisationBranchesAsync(byte languageID, long permissionAtLevelID, long branchID, long recordCount)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).GetOrganisationBranchesAsync(languageID, permissionAtLevelID, branchID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Organisation Branch data to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="branchData">Organisation Branches data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveOrganisationBranchAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveOrganisationBranchAsync(byte languageID, long permissionAtLevelID, [FromBody] BranchDTO branchData)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).SaveOrganisationBranchAsync(branchData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets organisation view data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">Selected user OrganisationID</param>
        ///  <param name="recordCount">For organisation data decision</param>
        /// <returns>Organisation view data</returns>
        [Route("GetOrganisationViewAsync")]
        [HttpGet]
        public async Task<IActionResult> GetOrganisationViewAsync(long permissionAtLevelID, byte languageID, long organisationID, long recordCount)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).GetOrganisationViewAsync(languageID, permissionAtLevelID, organisationID, recordCount).ConfigureAwait(false), languageID);
        }
        /// <summary>
        /// Get organisations
        /// </summary>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationIdForSetup">organisation ID</param>
        /// <param name="languageID">Language ID</param>
        /// <param name="recordCount">No of records to return</param>
        /// <returns>Matching Organisation records</returns>
        [Route("GetOrganisationsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetOrganisationsAsync(long permissionAtLevelID, long organisationIdForSetup, byte languageID, long recordCount)
        {
            return HttpActionResult(await new OrganisationServiceBusinessLayer(HttpContext).GetOrganisationsAsync(permissionAtLevelID, organisationIdForSetup, languageID, recordCount).ConfigureAwait(false), languageID);
        }
    }
}
