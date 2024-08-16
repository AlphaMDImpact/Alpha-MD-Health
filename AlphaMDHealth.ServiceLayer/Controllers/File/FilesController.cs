using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/FilesService")]
    [ApiAuthorize]
    public class FilesController : BaseController
    {
        /// <summary>
        /// Get patient files from database
        /// </summary>
        /// <param name="languageID">user's selected language</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="selectedUserID">user id</param>
        /// <param name="fileID">document id</param>
        /// <param name="recordCount">number of record count to fetch</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
        /// <returns>Operation status and list of patient files</returns>
        [Route("GetFilesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetFilesAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, Guid fileID, long recordCount, string fromDate, string toDate)
        {
            return HttpActionResult(await new FilesServiceBusinessLayer(HttpContext).GetFilesAndDocumentsAsync(languageID, permissionAtLevelID, organisationID, selectedUserID, fileID, recordCount, fromDate, toDate).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save files to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="documentsData">document data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveFilesAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveFilesAsync(byte languageID, long permissionAtLevelID, [FromBody] FileDTO documentsData)
        {
            return HttpActionResult(await new FilesServiceBusinessLayer(HttpContext).SaveFilesAsync(permissionAtLevelID, documentsData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Update document status from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="documentFileID">Document file id</param>
        /// <returns>Operation status</returns>
        [Route("UpdateDocumentStatusAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateDocumentStatusAsync(byte languageID, long permissionAtLevelID, Guid documentFileID)
        {
            return HttpActionResult(await new FilesServiceBusinessLayer(HttpContext).UpdateDocumentStatusAsync(permissionAtLevelID, documentFileID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Delete patient files from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="fileID">File Id to be deleted</param>
        /// <returns>Operation status</returns>
        [Route("DeleteFilesAsync")]
        [HttpPost]
        public async Task<IActionResult> DeleteFilesAsync(byte languageID, long permissionAtLevelID, Guid fileID , DateTimeOffset lastModifiedOn)
        {
            return HttpActionResult(await new FilesServiceBusinessLayer(HttpContext).DeleteFilesAsync(permissionAtLevelID, fileID, lastModifiedOn).ConfigureAwait(false), languageID);
        }
    }
}