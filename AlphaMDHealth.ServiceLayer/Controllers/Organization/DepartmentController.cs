using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/DepartmentService")]
    [ApiAuthorize]
    public class DepartmentController : BaseController
    {
        /// <summary>
        /// Get departments from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="departmentID">Id of Department</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of departments and operation status</returns>
        [Route("GetDepartmentsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetDepartmentsAsync(byte languageID, long permissionAtLevelID, byte departmentID, long recordCount)
        {
            return HttpActionResult(await new DepartmentServiceBusinessLayer(HttpContext).GetDepartmentsAsync(languageID, permissionAtLevelID, departmentID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save department to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="departmentData">Organisation department data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveDepartmentAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveDepartmentAsync(byte languageID, long permissionAtLevelID, [FromBody] DepartmentDTO departmentData)
        {
            return HttpActionResult(await new DepartmentServiceBusinessLayer(HttpContext).SaveDepartmentAsync(departmentData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }
    }
}