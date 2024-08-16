using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/UserService")]
    [ApiAuthorize]
    public class UserController : BaseController
    {
        /// <summary>
        /// Register user 
        /// </summary>
        /// <param name="languageID">user's selected language</param>
        /// <param name="organisationID">user's organisation id</param>
        /// <param name="userData">object to save user data</param>
        /// <returns>Operation status and token data</returns>
        [Route("RegisterPatientAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPatientAsync(byte languageID, long organisationID, [FromBody] UserDTO userData)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).RegisterPatientAsync(languageID, organisationID, userData, Request.Headers).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Users Temp Token
        /// </summary>
        /// <param name="languageID">user's selected language</param>
        /// <param name="organisationID">permission at level id</param>
        /// <param name="ID">user account id</param>
        /// <returns>Temp Token based on ID</returns>
        [Route("GetPatientTempTokenByIDAsync")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPatientTempTokenByIDAsync(byte languageID, long organisationID, long ID)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).GetPatientTempTokenByIDAsync(languageID, organisationID, ID, Request.Headers).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Register user 
        /// </summary>
        /// <param name="languageID">user's selected language</param>
        /// <param name="organisationID">user's organisation id</param>
        /// <param name="userData">object to save user data</param>
        /// <returns>Operation status and token data</returns>
        [Route("RegisterUserAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUserAsync(byte languageID, long organisationID, [FromBody] UserDTO userData)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).RegisterUserAsync(languageID, organisationID, userData, Request.Headers).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get users from database
        /// </summary>
        /// <param name="languageID">user's selected language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="userID">user id</param>
        /// <param name="selectedOrganisationID">selected organisation id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="viewFor">view for which data needs to be fetched</param>
        /// <returns>Operation status and list of users</returns>
        [Route("GetUsersAsync")]
        [HttpGet]
        public async Task<IActionResult> GetUsersAsync(byte languageID, long permissionAtLevelID, long userID, long selectedOrganisationID, long recordCount, AppPermissions viewFor)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).GetUsersAsync(languageID, permissionAtLevelID, userID, selectedOrganisationID, recordCount, viewFor).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save users to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="userData">user data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveUserAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveUserAsync(byte languageID, long permissionAtLevelID, [FromBody] UserDTO userData)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).SaveUserAsync(languageID, permissionAtLevelID, userData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Delete users from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="userData">user data to be delete</param>
        /// <returns>Operation status</returns>
        [Route("DeleteUserAsync")]
        [HttpPost]
        public async Task<IActionResult> DeleteUserAsync(byte languageID, long permissionAtLevelID, [FromBody] UserDTO userData)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).DeleteUserAsync(permissionAtLevelID, userData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Resend activation link to selected user
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="userData">user data</param>
        /// <returns>Operation status</returns>
        [Route("ResendActivationAsync")]
        [HttpPost]
        public async Task<IActionResult> ResendActivationAsync(byte languageID, long permissionAtLevelID, [FromBody] UserDTO userData)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).ResendActivationAsync(languageID, permissionAtLevelID, userData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Patient Caregiver(s)
        /// </summary>
        /// <param name="languageID">user's selected language</param>
        /// <param name="organisationID">user's organisation id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="selectedUserID">patient id</param>
        /// <param name="patientCareGiverID">user id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Operation status and caregiver data</returns>
        [Route("GetPatientCaregiversAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetPatientCaregiversAsync(byte languageID, long organisationID, long permissionAtLevelID, long selectedUserID, long patientCareGiverID, long recordCount)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).GetPatientCaregiversAsync(languageID, organisationID, permissionAtLevelID, selectedUserID, patientCareGiverID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save patient caregiver
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="caregiverData">caregiver data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SavePatientCaregiverAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SavePatientCaregiverAsync(byte languageID, long permissionAtLevelID, [FromBody] CaregiverDTO caregiverData)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).SavePatientCaregiverAsync(languageID, permissionAtLevelID, caregiverData).ConfigureAwait(false), languageID);
        }
        
        /// <summary>
        /// Save users to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="userData">user data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveUsersFromExcelAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveUsersFromExcelAsync(byte languageID, long permissionAtLevelID, [FromBody] UserDTO userData)
        {
            return HttpActionResult(await new UserServiceBusinessLayer(HttpContext).SaveUsersFromExcelAsync(languageID, permissionAtLevelID, userData).ConfigureAwait(false), languageID);
        }
    }
}