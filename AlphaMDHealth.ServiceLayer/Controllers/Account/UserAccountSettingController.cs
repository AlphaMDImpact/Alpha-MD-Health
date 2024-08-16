using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/UserAccountSettingService")]
    [ApiAuthorize]
    public class UserAccountSettingController : BaseController
    {
        /// <summary>
        /// To get User Account settings
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Operation status</returns>
        [Route("GetUserAccountSettingsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetUserAccountSettingsAsync(byte languageID, long permissionAtLevelID, short recordCount)
        {
            return HttpActionResult(await new UserAccountSettingsServiceBusinessLayer(HttpContext).GetUserAccountSettingsAsync(languageID, permissionAtLevelID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save User Account Settings
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="userAccountSettingData">User account settig data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveUserAccountSettingsAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveUserAccountSettingsAsync(byte languageID, long permissionAtLevelID, [FromBody] UserAccountSettingDTO userAccountSettingData)
        {
            return HttpActionResult(await new UserAccountSettingsServiceBusinessLayer(HttpContext).SaveUserAccountSettingsAsync(permissionAtLevelID, userAccountSettingData).ConfigureAwait(false), languageID);
        }
    }
}
