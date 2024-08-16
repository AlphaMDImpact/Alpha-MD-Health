using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ProgramReadingService")]
    [ApiAuthorize]
    public class ProgramReadingControllor : BaseController
    {
        /// <summary>
        /// Get Program Reading Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programReadingID">Program reading ID for which data is to be fetched</param>
        /// <returns>List of reading category and reading type with operation status</returns>
        [Route("GetProgramReadingAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramReadingAsync(byte languageID, long permissionAtLevelID, long programReadingID)
        {
            return HttpActionResult(await new ProgramReadingServiceBusinessLayer(HttpContext).GetProgramReadingAsync(languageID, permissionAtLevelID, programReadingID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Reading Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Reading Data</param>
        /// <returns>Operation Status Code And ProgramReadingID</returns>
        [Route("SaveProgramReadingAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramReadingAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramReadingServiceBusinessLayer(HttpContext).SaveProgramReadingAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get program reading metadata
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="programReadingID">Id of Program Reading</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Organisation Reading detail and reading ranges data</returns>
        [Route("GetReadingMetadataAsync")]
        [HttpGet]
        public async Task<IActionResult> GetReadingMetadataAsync(byte languageID, long permissionAtLevelID, int programReadingID, long recordCount)
        {
            return HttpActionResult(await new ProgramReadingServiceBusinessLayer(HttpContext).GetReadingMetadataAsync(languageID, permissionAtLevelID, programReadingID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save program reading metadata
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="readingsMetadata">Program reading metadata to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveReadingMetadataAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramReadingMetadataAsync(byte languageID, long permissionAtLevelID, [FromBody] ReadingMasterDataDTO readingsMetadata)
        {
            return HttpActionResult(await new ProgramReadingServiceBusinessLayer(HttpContext).SaveProgramReadingMetadataAsync(languageID, permissionAtLevelID, readingsMetadata).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get program reading ranges
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="programReadingID">Id of Program Reading</param>
        /// <param name="readingRangeID">Id of Reading Range</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Organisation Reading detail and reading ranges data</returns>
        [Route("GetProgramReadingRangesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramReadingRangesAsync(byte languageID, long permissionAtLevelID, int programReadingID, long readingRangeID, long recordCount)
        {
            return HttpActionResult(await new ProgramReadingServiceBusinessLayer(HttpContext).GetProgramReadingRangesAsync(languageID, permissionAtLevelID, programReadingID, readingRangeID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save program reading range data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="rangeData">Program Reading ranges data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveProgramReadingRangeAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramReadingRangeAsync(byte languageID, long permissionAtLevelID, [FromBody] ReadingMasterDataDTO rangeData)
        {
            return HttpActionResult(await new ProgramReadingServiceBusinessLayer(HttpContext).SaveProgramReadingRangeAsync(languageID, permissionAtLevelID, rangeData).ConfigureAwait(false), languageID);
        }

        ///// <summary>
        ///// Save program reading range data
        ///// </summary>
        ///// <param name="languageID">language id</param>
        ///// <param name="permissionAtLevelID">permission at level id</param>
        ///// <param name="rangeData">Program Reading ranges data to be saved</param>
        ///// <returns>Result of operation</returns>
        //[Route("GetReadingMasterDataAsync")]
        //[HttpPost]
        //public async Task<IActionResult> GetReadingMasterDataAsync(byte languageID, long permissionAtLevelID, [FromBody] ReadingMasterDataDTO readingData)
        //{
        //    return HttpActionResult(await new ProgramReadingServiceBusinessLayer(HttpContext).GetReadingMasterDataAsync(languageID, permissionAtLevelID, readingData).ConfigureAwait(false), languageID);
        //}
    }
}