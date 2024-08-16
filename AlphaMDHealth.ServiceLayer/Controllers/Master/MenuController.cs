using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/MenuService")]
    [ApiAuthorize]
    public class MenuController : BaseController
    {
        /// <summary>
        /// Get Mobile menu groups from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuGroupID">Menu Group Id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Mobile Menu groups</returns>
        [Route("GetMobileMenuGroupsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetMobileMenuGroupsAsync(byte languageID, long permissionAtLevelID, long menuGroupID, long recordCount)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).GetMobileMenuGroupsAsync(languageID, permissionAtLevelID, menuGroupID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Mobile Menu Groups to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="menuGroupData">Mobile Menu Group data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveMobileMenuGroupAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveMobileMenuGroupAsync(byte languageID, long permissionAtLevelID, [FromBody] MenuGroupDTO menuGroupData)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).SaveMobileMenuGroupAsync(menuGroupData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets list of mobile menu nodes
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="mobileMenuNodeID">ID of Mobile menu Node</param>
        /// <returns>Mobile menu nodes</returns>
        [Route("GetMobileMenuNodesAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetMobileMenuNodesAsync(byte languageID, long permissionAtLevelID, long recordCount, long mobileMenuNodeID)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).GetMobileMenuNodesAsync(languageID, permissionAtLevelID, recordCount, mobileMenuNodeID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Mobile menu node
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="mobileMenuNodeData">Object which holds Data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveMobileMenuNodeAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveMobileMenuNodeAsync(byte languageID, long permissionAtLevelID, [FromBody] MobileMenuNodeDTO mobileMenuNodeData)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).SaveMobileMenuNodeAsync(mobileMenuNodeData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get web menu groups
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuGroupID">web menu group id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of web menu groups</returns>
        [Route("GetWebMenuGroupsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetWebMenuGroupsAsync(byte languageID, long permissionAtLevelID, long menuGroupID, long recordCount)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).GetWebMenuGroupsAsync(languageID, permissionAtLevelID, menuGroupID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save web menu group to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="menuGroup">Web menu group data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveWebMenuGroupAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveWebMenuGroupAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] MenuGroupDTO menuGroup)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).SaveWebMenuGroupAsync(permissionAtLevelID, organisationID, menuGroup).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Mobile menus
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuID">menu id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of mobile menus</returns>
        [Route("GetMobileMenusAsync")]
        [HttpGet]
        public async Task<IActionResult> GetMobileMenusAsync(byte languageID, long permissionAtLevelID, long menuID, long recordCount, bool isPatientMenu)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).GetMobileMenusAsync(languageID, permissionAtLevelID, menuID, recordCount, isPatientMenu).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save mobile menus
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="menu">mobile menus data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveMobileMenusAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveMobileMenusAsync(byte languageID, long permissionAtLevelID, [FromBody] MenuDTO menu)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).SaveMobileMenusAsync(menu, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Web menus
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="menuID">menu id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of web menus</returns>
        [Route("GetWebMenusAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetWebMenusAsync(byte languageID, long permissionAtLevelID, long menuID, long recordCount)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).GetWebMenusAsync(languageID, permissionAtLevelID, menuID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save web menus
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="menu">web menus data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveWebMenusAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveWebMenusAsync(byte languageID, long permissionAtLevelID, [FromBody] MenuDTO menu)
        {
            return HttpActionResult(await new MenuServiceBusinessLayer(HttpContext).SaveWebMenuAsync(permissionAtLevelID, menu).ConfigureAwait(false), languageID);
        }
    }
}