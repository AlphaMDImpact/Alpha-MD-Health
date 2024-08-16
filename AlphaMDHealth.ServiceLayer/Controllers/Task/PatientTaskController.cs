using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/PatientTaskService")]
    [ApiAuthorize]
    public class PatientTaskController : BaseController
    {
        /// <summary>
        /// Get patient tasks
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's Organisation ID</param>
        /// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="recordCount">Record count to decide how much data to retrive</param>
        /// <param name="patientTaskID">Patient task ID to retrive specific task data</param>
        /// <returns>List of tasks with operation status</returns>
        [Route("GetPatientTasksAsync")]
        [HttpGet]
        public async Task<IActionResult> GetPatientTasksAsync(byte languageID, long organisationID, long selectedUserID, long permissionAtLevelID, long recordCount, long patientTaskID)
        {
            return HttpActionResult(await new PatientTaskServiceBussinessLayer(HttpContext).GetPatientTasksAsync(languageID, organisationID, selectedUserID, permissionAtLevelID, recordCount, patientTaskID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save patient task
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="taskData">Task data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SavePatientTaskAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientTaskAsync(byte languageID, long organisationID, long permissionAtLevelID, [FromBody] ProgramDTO taskData)
        {
            return HttpActionResult(await new PatientTaskServiceBussinessLayer(HttpContext).SavePatientTaskAsync(languageID, organisationID, taskData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Update patient task status
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="taskData">Task status data to be updated</param>
        /// <returns>Operation status</returns>
        [Route("UpdatePatientTaskStatusAsync")]
        [HttpPost]
        public async Task<IActionResult> UpdatePatientTaskStatusAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO taskData)
        {
            return HttpActionResult(await new PatientTaskServiceBussinessLayer(HttpContext).UpdatePatientTaskStatusAsync(taskData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get next/previous question
        /// </summary>
        /// <param name="languageID">languageID</param>
        /// <param name="permissionAtLevelID">permissionAtLevelID</param>
        /// <param name="questionnaireData">questionnaireData</param>
        /// <returns>next/previous question with operation status</returns>
        [Route("GetNextQuestionAsync")]
        [HttpPost]
        public async Task<IActionResult> GetNextQuestionAsync(byte languageID, long organizationID, long permissionAtLevelID, [FromBody] QuestionnaireDTO questionnaireData, short readingCategoryID)
        {
            return HttpActionResult(await new PatientTaskServiceBussinessLayer(HttpContext).GetNextQuestionAsync(languageID, organizationID, permissionAtLevelID, questionnaireData, readingCategoryID).ConfigureAwait(false), languageID);
        }
    }
}