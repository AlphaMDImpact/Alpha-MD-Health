using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ProgramService")]
    [ApiAuthorize]
    public class ProgramController : BaseController
    {
        /// <summary>
        /// Get Program Subflow(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="subFlowID">SubFlow ID of which data has to be fetched</param>
        /// <returns>Program Subflow Data with operation status</returns>
        [Route("GetSubFlowsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetSubFlowsAsync(byte languageID, long permissionAtLevelID, long recordCount, long subFlowID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetSubFlowsAsync(languageID, permissionAtLevelID, recordCount, subFlowID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Subflow Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Subflow Data</param>
        /// <returns>Operation Status Code</returns>
        [Route("SaveSubFlowAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveSubFlowAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveSubFlowAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Program Task(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="taskID">Task ID of which data has to be fetched</param>
        /// <returns>Program Task Data with operation status</returns>
        [Route("GetTasksAsync")]
        [HttpGet]
        public async Task<IActionResult> GetTasksAsync(byte languageID, long permissionAtLevelID, long recordCount, long taskID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetTasksAsync(languageID, permissionAtLevelID, recordCount, taskID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Task Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Task Data</param>
        /// <returns>Operation Status Code</returns>
        [Route("SaveTaskAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveTaskAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveTaskAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Tasks Subflow(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="taskSubFlowID">Task SubFlow ID of which data has to be fetched</param>
        /// <returns>Task Subflow Data with operation status</returns>
        [Route("GetTaskSubFlowAsync")]
        [HttpGet]
        public async Task<IActionResult> GetTaskSubFlowAsync(byte languageID, long permissionAtLevelID, long taskSubFlowID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetTaskSubFlowAsync(languageID, permissionAtLevelID, taskSubFlowID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Tasks Subflow Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Task Subflow Data</param>
        /// <returns>Operation Status Code And TaskSubFlowID</returns>
        [Route("SaveTaskSubFlowAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveTaskSubFlowAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveTaskSubFlowAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Program(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="organisationID">Organisation ID</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="programID">Program ID of which data has to be fetched</param>
        /// <returns>Program Data with operation status</returns>
        [Route("GetProgramsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramsAsync(byte languageID, long permissionAtLevelID, long organisationID, long recordCount, long programID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramsAsync(languageID, permissionAtLevelID, organisationID, recordCount, programID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code</returns>
        [Route("SaveProgramAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramAsync(permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Publish/Unpublish Program Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programID">Id of progrm</param>
        /// <param name="isPublished">Publish= true / Unpublish Flag= false</param>
        /// <returns>Operation Status Code</returns>
        [Route("PublishProgramAsync")]
        [HttpPost]
        public async Task<IActionResult> PublishProgramAsync(byte languageID, long permissionAtLevelID, long programID, bool isPublished)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).PublishProgramAsync(languageID, permissionAtLevelID, programID, isPublished).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Subscribe a Program
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programID">Id of progrm</param>
        /// <returns>Operation Status Code</returns>
        [Route("SubscribeProgramAsync")]
        [HttpPost]
        public async Task<IActionResult> SubscribeProgramAsync(byte languageID, long permissionAtLevelID, long programID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SubscribeProgramAsync(permissionAtLevelID, programID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Program Task(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programTaskID">Program Task ID of which data has to be fetched</param>
        /// <returns>Program Task Data with operation status</returns>
        [Route("GetProgramTaskAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramTaskAsync(byte languageID, long permissionAtLevelID, long programTaskID, bool isSynced)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramTaskAsync(languageID, permissionAtLevelID, programTaskID, isSynced).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Task Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Task Data</param>
        /// <returns>Operation Status Code And ProgramTaskID</returns>
        [Route("SaveProgramTaskAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramTaskAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramTaskAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Program Subflow(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programSubFlowID">Program SubFlow ID of which data has to be fetched</param>
        /// <returns>Program Subflow Data with operation status</returns>
        [Route("GetProgramSubFlowAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramSubFlowAsync(byte languageID, long permissionAtLevelID, long programSubFlowID, bool isSynced)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramSubFlowAsync(languageID, permissionAtLevelID, programSubFlowID, isSynced).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Subflow Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Subflow Data</param>
        /// <returns>Operation Status Code And ProgramSubFlowID</returns>
        [Route("SaveProgramSubflowAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramSubflowAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramSubflowAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get program education data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programEducationID">Program EducationID used to fetch data</param>
        /// <returns>Program Education data with operation status</returns>
        [Route("GetProgramEducationAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramEducationAsync(byte languageID, long permissionAtLevelID, long programEducationID, bool isSynced)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramEducationAsync(languageID, permissionAtLevelID, programEducationID, isSynced).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Education Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Education Data</param>
        /// <returns>Operation Status Code And ProgramEducationID</returns>
        [Route("SaveProgramEducationAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramEducationAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramEducationAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get program caregiver data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programCareGiverID">Program caregiverID used to fetch data</param>
        /// <returns>Program caregivers data with operation status</returns>
        [Route("GetProgramCaregiverAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramCaregiverAsync(byte languageID, long permissionAtLevelID, long programCareGiverID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramCaregiverAsync(languageID, permissionAtLevelID, programCareGiverID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Caregiver Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Caregiver Data</param>
        /// <returns>Operation Status Code And PatientCareGiverID</returns>
        [Route("SaveProgramCaregiverAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramCaregiverAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramCaregiverAsync(permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Items based on selected task type
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="taskType">Type of task whose items has to be feteched</param>
        /// <param name="programID">Program ID from which request is sent</param>
        /// <param name="selectedUserID">selectedUserID from which request is sent</param>
        /// <returns>List of items with operation status</returns>
        [Route("GetItemsBasedOnTaskTypeAsync")]
        [HttpGet]
        public async Task<IActionResult> GetItemsBasedOnTaskTypeAsync(byte languageID, long permissionAtLevelID, string taskType, long programID, long selectedUserID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetItemsBasedOnTaskTypeAsync(languageID, permissionAtLevelID, taskType, programID, selectedUserID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Program Instruction(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="instructionID">Instruction ID of which data has to be fetched</param>
        /// <returns>Program Instruction Data with operation status</returns>
        [Route("GetInstructionsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetInstructionsAsync(byte languageID, long permissionAtLevelID, long recordCount, long instructionID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetInstructionsAsync(languageID, permissionAtLevelID, recordCount, instructionID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Instruction Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Instruction Data</param>
        /// <returns>Operation Status Code</returns>
        [Route("SaveInstructionsAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveInstructionsAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveInstructionsAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets Program Tracker(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programTrackerID">Program Trackers ID of which data has to be fetched</param>
        /// <returns>Program Trackers Data with operation status</returns>
        [Route("GetProgramTrackersAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramsTrackersAsync(byte languageID, long permissionAtLevelID , long programTrackerID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramTrackersAsync(languageID, permissionAtLevelID, programTrackerID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Tracker Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Trackers Data</param>
        /// <returns>Operation Status Code</returns>
        [Route("SaveProgramTrackerAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramTrackerAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramTrackerAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get program notes
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programNoteID">Program noteID used to fetch data</param>
        /// <returns>Program notes data with operation status</returns>
        [Route("GetProgramNotesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramNotesAsync(byte languageID, long permissionAtLevelID, long programNoteID,long programID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramNotesAsync(languageID, permissionAtLevelID, programNoteID, programID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Note Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Note Data</param>
        /// <returns>Operation Status Code And ProgramNoteID</returns>
        [Route("SaveProgramNoteAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramNoteAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramNoteAsync(permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets Program Billing Item(s)
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programBillingItemID">Program Billing Item ID</param>
        /// <returns>Program Billing Item(s) Data with operation status</returns>
        [Route("GetProgramBillingItemsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramBillingItemsAsync(byte languageID, long permissionAtLevelID, long programBillingItemID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramBillingItemsAsync(languageID, permissionAtLevelID, programBillingItemID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Program Billing Item
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Billing Item Data</param>
        /// <returns>operation status</returns>
        [Route("SaveProgramBillingItemAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramBillingItemAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramBillingItemAsync(permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets Program Reason Configuration Item(s)
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programID">Program ID</param>
        /// <returns>Program configuration Item(s) Data with operation status</returns>
        [Route("GetProgramReasonConfigurationsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramReasonConfigurationsAsync(byte languageID, long permissionAtLevelID, long programID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramReasonConfigurationsAsync(languageID, permissionAtLevelID, programID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Program Reason Configuration Item(s)
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Reason Configuration data</param>
        /// <returns>operation status</returns>
        [Route("SaveProgramReasonConfigurationsAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramReasonConfigurationsAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramReasonConfigurationsAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Program Services Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programExternalServiceID">program external service id</param>
        /// <param name="programID">Program ID</param>
        /// <returns></returns>
        [Route("GetProgramServicesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetProgramServicesAsync(byte languageID, long permissionAtLevelID, long programExternalServiceID, long programID)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).GetProgramServicesAsync(languageID, permissionAtLevelID, programExternalServiceID, programID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Program Service Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Note Data</param>
        /// <returns>Operation Status Code</returns>
        [Route("SaveProgramServiceAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveProgramServiceAsync(byte languageID, long permissionAtLevelID, [FromBody] ProgramDTO programData)
        {
            return HttpActionResult(await new ProgramServiceBusinessLayer(HttpContext).SaveProgramServiceAsync(languageID, permissionAtLevelID, programData).ConfigureAwait(false), languageID);
        }
    }
}