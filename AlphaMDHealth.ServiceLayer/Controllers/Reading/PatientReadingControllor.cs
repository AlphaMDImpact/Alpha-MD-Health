using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/PatientReadingService")]
    [ApiAuthorize]
    public class PatientReadingControllor : BaseController
    {
        /// <summary>
        ///Save user reading targets data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="readingData">Reading targets data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SavePatientReadingTargetsAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientReadingTargetsAsync(byte languageID, long permissionAtLevelID, [FromBody] PatientReadingDTO readingData)
        {
            return HttpActionResult(await new PatientReadingServiceBusinessLayer(HttpContext).SavePatientReadingTargetsAsync(languageID, permissionAtLevelID, readingData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Organisation Reading ranges and Reading Detail
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
		/// <param name="organisationID">Id of organization</param>
        /// <param name="selectedUserID">User id for which data needs to fetch</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="readingCategoryID">Category for which data needs to fetch</param>
        /// <param name="patientReadingID">Id of patient reading for which data needs to fetch</param>
        /// <param name="readingID">Reading ID for which data needs to fetch</param>
        /// <param name="fromDate">Start date from where data needs to fetch</param>
        /// <param name="toDate">End date, till data needs to fetched</param>
        /// <returns>Organisation Reading detail and reading ranges data</returns>
        [Route("GetPatientReadingsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetPatientReadingsAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, long recordCount, short readingCategoryID, Guid patientReadingID, short readingID, string fromDate, string toDate, bool isMedicalHistory, bool isCommingFromQuestionnaireTaskPage)
        {
            return HttpActionResult(await new PatientReadingServiceBusinessLayer(HttpContext).GetPatientReadingsAsync(languageID, permissionAtLevelID, organisationID, selectedUserID, recordCount, readingCategoryID, patientReadingID, readingID, fromDate, toDate, isMedicalHistory, isCommingFromQuestionnaireTaskPage).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Patient Scan Vitals Data 
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="selectedUserID">User id for which data needs to fetch</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>Patient Scan Vitals Data</returns>
        [Route("GetPatientScanVitalsDataAsync")]
        [HttpGet]
        public async Task<IActionResult> GetPatientScanVitalsDataAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, long recordCount)
        {
            return HttpActionResult(await new PatientReadingServiceBusinessLayer(HttpContext).GetPatientScanVitalsDataAsync(languageID, permissionAtLevelID, organisationID, selectedUserID, recordCount).ConfigureAwait(false), languageID);
        }
        
        /// <summary>
        /// Save Patient readings data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="readingData">Patient reading data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SavePatientReadingsAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientReadingsAsync(byte languageID, long permissionAtLevelID, [FromBody] PatientReadingDTO readingData)
        {
            return HttpActionResult(await new PatientReadingServiceBusinessLayer(HttpContext).SavePatientReadingsAsync(languageID, permissionAtLevelID, readingData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Search food item
        /// </summary>
        /// <param name="languageID">Language id</param>
        /// <param name="searchItem">Search food item</param>
        /// <param name="organisationID">Organisation id</param>
        /// <returns>List of food items</returns>
        [Route("SearchFoodItemAsync")]
        [HttpGet]
        public async Task<IActionResult> SearchFoodItemAsync(byte languageID, string search, long organisationID)
        {
            return HttpActionResult(await new PatientReadingServiceBusinessLayer(HttpContext).SearchFoodItemAsync(languageID, search, organisationID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get food nutritions data
        /// </summary>
        /// <param name="languageID">Language id</param>
        /// <param name="foodIdentifier">Food identifier</param>
        /// <param name="organisationID">Organisation id</param>
        /// <returns>Food nutrition data</returns>
        [Route("GetFoodNutritionDataAsync")]
        [HttpGet]
        public async Task<IActionResult> GetFoodNutritionDataAsync(byte languageID, string foodIdentifier, long organisationID)
        {
            return HttpActionResult(await new PatientReadingServiceBusinessLayer(HttpContext).GetFoodNutritionDataAsync(languageID, foodIdentifier, organisationID).ConfigureAwait(false), languageID);
        }
    }
}
