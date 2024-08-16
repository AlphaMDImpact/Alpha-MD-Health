using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/FileCategoryService")]
    [ApiAuthorize]
    public class FileCategoryController : BaseController
    {
        /// <summary>
        /// Get File Categories
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="lastModifiedOn">get records modified after last modified date</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="fileCategoryID">File Category Id</param>
        /// <returns>File Categories Data With Operation Status</returns>
        [Route("GetFileCategoriesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetFileCategoriesAsync(byte languageID, long permissionAtLevelID, DateTimeOffset lastModifiedOn, long recordCount, long fileCategoryID)
        {
            return HttpActionResult(await new FileCategoryServiceBusinessLayer(HttpContext).GetFileCategoriesAsync(languageID, permissionAtLevelID, lastModifiedOn, recordCount, fileCategoryID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save File Category To Database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">Organisation id</param>
        /// <param name="categoryDTO">File Category Data</param>
        /// <returns>Operation Status</returns>
        [Route("SaveFileCategoryAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveFileCategoryAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] FileCategoryDTO categoryDTO)
        {
            return HttpActionResult(await new FileCategoryServiceBusinessLayer(HttpContext).SaveFileCategoryAsync(permissionAtLevelID, organisationID, categoryDTO).ConfigureAwait(false), languageID);
        }
    }
}
