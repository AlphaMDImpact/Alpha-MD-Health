using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/MedicationService")]
    [ApiAuthorize]
    public class MedicationController : BaseController
    {
        /// <summary>
        /// Get patient medicines data for searched name
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="shortName">Patient medicine name to retrive list of medicines data</param>
        /// <returns>List of medicines with operation status</returns>
        [Route("GetMedicinesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetMedicinesAsync(byte languageID, long permissionAtLevelID, string shortName)
        {
            return HttpActionResult(await new MedicationServiceBusinessLayer(HttpContext).GetMedicinesAsync(languageID, permissionAtLevelID, shortName).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get medications for patient/program
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's Organisation ID</param>
        /// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="recordCount">Record count to decide how much data to retrive</param>
        /// <param name="programID">Program ID to retrive program specific medication data</param>
        /// <param name="programMedicationID">Program medication ID to retrive specific medication data</param>
        /// <param name="patientMedicationID">Patient medication ID to retrive specific medication data</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <param name="isMedicalHistory">flag representing data is requested for medical history or not</param>
        /// <returns>Medications data with operation status</returns>
        [Route("GetMedicationsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetMedicationsAsync(byte languageID, long organisationID, long selectedUserID, long permissionAtLevelID, long recordCount, long programID, long programMedicationID, Guid patientMedicationID, string fromDate, string toDate, bool isMedicalHistory)
        {
            return HttpActionResult(await new MedicationServiceBusinessLayer(HttpContext).GetMedicationsAsync(languageID, organisationID, selectedUserID, permissionAtLevelID, recordCount, programID, programMedicationID, patientMedicationID, fromDate, toDate, isMedicalHistory).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save medications for patient/program
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="medicationData">Medication data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveMedicationAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveMedicationAsync(byte languageID, long permissionAtLevelID, [FromBody] PatientMedicationDTO medicationData)
        {
            return HttpActionResult(await new MedicationServiceBusinessLayer(HttpContext).SaveMedicationAsync(medicationData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }
    }
}