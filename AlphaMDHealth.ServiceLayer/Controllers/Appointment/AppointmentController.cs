using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/AppointmentService")]
    [ApiAuthorize]
    public class AppointmentController : BaseController
    {
        /// <summary>
        /// Get Appointments
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
        /// <param name="organisationID">User's organisationID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="recordCount">Record count to decide how much data to retrive</param>
        /// <param name="appointmentID">Appointment ID to retrive specific contact</param>
        /// <param name="lastModifiedOn">Last Modified on DateTime</param>
        /// <returns>Appointment List</returns>
        [Route("GetAppointmentsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetAppointmentsAsync(byte languageID, long selectedUserID, long organisationID, long permissionAtLevelID, long recordCount, long appointmentID, DateTimeOffset lastModifiedOn)
        {
            return HttpActionResult(await new AppointmentServiceBusinessLayer(HttpContext).GetAppointmentsAsync(languageID, selectedUserID, organisationID, permissionAtLevelID, recordCount, appointmentID, lastModifiedOn).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Appointment
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's organisationID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="appointmentData">Appointment data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveAppointmentAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveAppointmentAsync(byte languageID, long organisationID, long permissionAtLevelID, [FromBody] AppointmentDTO appointmentData)
        {
            return HttpActionResult(await new AppointmentServiceBusinessLayer(HttpContext).SaveAppointmentAsync(languageID, organisationID, permissionAtLevelID, appointmentData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Update Appointment status
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's organisationID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="appointmentData">Appointment data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("UpdateAppointmentStatusAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatusAsync(byte languageID, long organisationID, long permissionAtLevelID, [FromBody] AppointmentDTO appointmentData)
        {
            return HttpActionResult(await new AppointmentServiceBusinessLayer(HttpContext).UpdateAppointmentStatusAsync(organisationID, appointmentData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }
    }
}