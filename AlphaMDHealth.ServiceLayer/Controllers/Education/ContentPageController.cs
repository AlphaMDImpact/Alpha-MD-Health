using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ContentPageService")]
    [ApiAuthorize]
    public class ContentPageController : BaseController
    {
        /// <summary>
        /// Get Content Pages data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">id of organization</param>
        /// <param name="lastModifiedOn">last modified date time</param>
        /// <param name="pageID">Content Page ID</param>
        /// <param name="selectedUserID">User's ID</param>
        /// <param name="isEducation">Is Education is parameter to deside the page is education or Static content page</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="forProvider">Is Education is for provider or all educations of organisation</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
        /// <returns>List of Content Pages and operation status</returns>
        [Route("GetBasicContentPagesAsync")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetBasicContentPagesAsync(byte languageID, long permissionAtLevelID, long organisationID, DateTimeOffset lastModifiedOn, long pageID, long selectedUserID, bool isEducation, long recordCount, bool forProvider, string fromDate, string toDate)
        {
            return HttpActionResult(await new ContentPageServiceBusinessLayer(HttpContext).GetContentPagesAsync(languageID, permissionAtLevelID, organisationID, lastModifiedOn, pageID, selectedUserID, isEducation, recordCount, forProvider, true, fromDate, toDate).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Content Pages data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">id of organization</param>
        /// <param name="lastModifiedOn">last modified date time</param>
        /// <param name="pageID">Content Page ID</param>
        /// <param name="selectedUserID">User's ID</param>
        /// <param name="isEducation">Is Education is parameter to deside the page is education or Static content page</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="forProvider">Is Education is for provider or all educations of organisation</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
        /// <returns>List of Content Pages and operation status</returns>
        [Route("GetContentPagesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetContentPagesAsync(byte languageID, long permissionAtLevelID, long organisationID, DateTimeOffset lastModifiedOn, long pageID, long selectedUserID, bool isEducation, long recordCount, bool forProvider, string fromDate, string toDate)
        {
            return HttpActionResult(await new ContentPageServiceBusinessLayer(HttpContext).GetContentPagesAsync(languageID, permissionAtLevelID, organisationID, lastModifiedOn, pageID, selectedUserID, isEducation, recordCount, forProvider, false, fromDate, toDate).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save ContentPages to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">Organisation id</param>
        /// <param name="contentPage">ContentPages data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveContentPagesAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveContentPagesAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] ContentPageDTO contentPage)
        {
            return HttpActionResult(await new ContentPageServiceBusinessLayer(HttpContext).SaveContentPagesAsync(permissionAtLevelID, organisationID, contentPage).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Publish/ UnPublish ContentPage
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="pageID">Page id that need to published or unpublished</param>
        /// <param name="isPublished">Publish or Unpublish</param>
        /// <returns>Operation Status</returns>
        [Route("PublishContentPageAsync")]
        [HttpPost]
        public async Task<IActionResult> PublishContentPageAsync(byte languageID, long permissionAtLevelID, long pageID, bool isPublished)
        {
            return HttpActionResult(await new ContentPageServiceBusinessLayer(HttpContext).PublishContentPageAsync(permissionAtLevelID, pageID, isPublished).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save ContentPages status to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">Organisation id</param>
        /// <param name="contentPage">ContentPages data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveEducationStatusAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveEducationStatusAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] ContentPageDTO contentPage)
        {
            return HttpActionResult(await new ContentPageServiceBusinessLayer(HttpContext).SaveEducationStatusAsync(permissionAtLevelID, organisationID, contentPage).ConfigureAwait(false), languageID);
        }
    }
}