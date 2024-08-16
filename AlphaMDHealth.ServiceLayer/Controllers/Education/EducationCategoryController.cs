using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/EducationCategoryService")]
    [ApiAuthorize]
    public class EducationCategoryController : BaseController
    {
        /// <summary>
        /// Get Education Categories
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="lastModifiedOn">get records modified after last modified date</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="educationCategoryID">Education Category Id</param>
        /// <returns>Education Categories Data with operation status</returns>
        [Route("GetEducationCategoriesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetEducationCategoriesAsync(byte languageID, long permissionAtLevelID, DateTimeOffset lastModifiedOn, long recordCount, long educationCategoryID)
        {
            return HttpActionResult(await new EducationCategoryServiceBusinessLayer(HttpContext).GetEducationCategoriesAsync(languageID, permissionAtLevelID, lastModifiedOn, recordCount, educationCategoryID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Education Category to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">Organisation id</param>
        /// <param name="categoryDTO">Education Category Data</param>
        /// <returns>Result of operation</returns>
        [Route("SaveEducationCategoryAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveEducationCategoryAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] EducationCategoryDTO categoryDTO)
        {
            return HttpActionResult(await new EducationCategoryServiceBusinessLayer(HttpContext).SaveEducationCategoryAsync(permissionAtLevelID, organisationID, categoryDTO).ConfigureAwait(false), languageID);
        }
    }
}