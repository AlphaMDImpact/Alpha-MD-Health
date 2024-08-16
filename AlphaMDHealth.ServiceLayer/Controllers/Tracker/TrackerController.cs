using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/TrackerService")]
    [ApiAuthorize]
    public class TrackerController : BaseController
    {
        /// <summary>
        /// Get Trackers
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="trackerID">Tracker ID</param>
        ///  <param name="trackerRangeID">Tracker RangeID</param>
        /// <returns>Tracker Data</returns>
        [Route("GetTrackersAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetTrackersAsync(byte languageID, long permissionAtLevelID, long recordCount, short trackerID, short trackerRangeID)
        {
            return HttpActionResult(await new TrackerServiceBusinessLayer(HttpContext).GetTrackersAsync(languageID, permissionAtLevelID, recordCount, trackerID, trackerRangeID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Tracker Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="trackerDTO">Reference object which holds Tracker Data</param>
        /// <returns>Operation Status</returns>
        [Route("SaveTrackerAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveTrackerAsync(byte languageID, long permissionAtLevelID, [FromBody] TrackerDTO trackerDTO)
        {
            return HttpActionResult(await new TrackerServiceBusinessLayer(HttpContext).SaveTrackerAsync(permissionAtLevelID, trackerDTO).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Tracker Ranges Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="trackerDTO">Reference object which holds Tracker Data</param>
        /// <returns>Operation Status</returns>
        [Route("SaveTrackerRangesAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveTrackerRangesAsync(byte languageID, long permissionAtLevelID, [FromBody] TrackerDTO trackerDTO)
        {
            return HttpActionResult(await new TrackerServiceBusinessLayer(HttpContext).SaveTrackerRangesAsync(trackerDTO, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Trackers assigned to patient
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="patientTrackerID">Patient Tracker ID</param>
        /// <param name="recordCount">Record Count</param>
        /// <param name="trackerName">Name of tracker</param>
        /// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
        /// <returns>Tracker Data</returns>
        [Route("GetPatientTrackersAsync")]
        [HttpGet]
        public async Task<IActionResult> GetPatientTrackersAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, Guid patientTrackerID, long recordCount, string trackerName, string fromDate, string toDate)
        {
            return HttpActionResult(await new TrackerServiceBusinessLayer(HttpContext).GetPatientTrackersAsync(languageID, permissionAtLevelID, organisationID, selectedUserID, patientTrackerID, recordCount, trackerName, fromDate, toDate).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Patient Tracker Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="trackerData">Reference object which holds Patient Tracker Data</param>
        /// <returns>Operation Status</returns>
        [Route("SavePatientTrackerAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientTrackerAsync(byte languageID, long permissionAtLevelID, long selectedUserID, [FromBody] TrackerDTO trackerData)
        {
            return HttpActionResult(await new TrackerServiceBusinessLayer(HttpContext).SavePatientTrackerAsync(languageID, permissionAtLevelID, selectedUserID, trackerData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Patient Tracker Value
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="trackerData">Reference object which holds Patient Tracker Value</param>
        /// <returns>Operation Status</returns>
        [Route("SavePatientTrackerValueAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientTrackerValueAsync(byte languageID, long permissionAtLevelID, long selectedUserID, [FromBody] TrackerDTO trackerData)
        {
            return HttpActionResult(await new TrackerServiceBusinessLayer(HttpContext).SavePatientTrackerValueAsync(languageID, permissionAtLevelID, selectedUserID, trackerData).ConfigureAwait(false), languageID);
        }
    }
}