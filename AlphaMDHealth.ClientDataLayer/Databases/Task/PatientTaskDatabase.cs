using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.ClientDataLayer
{
    public class PatientTaskDatabase : BaseDatabase
    {
        /// <summary>
        /// Insert or update record in the database
        /// </summary>
        /// <param name="programData">Task data for save into DB</param>
        /// <returns>Operation Status</returns>
        public async Task SavePatientTasksAsync(ProgramDTO programData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (TaskModel task in programData.Tasks)
                {
                    transaction.InsertOrReplace(task);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get patient tasks
        /// </summary>
        /// <param name="taskData">Reference object to hold list of tasks</param>
        /// <param name="selectedTabKey">Selected tab name to retrieve the particular list</param>
        /// <returns>List of patient tasks</returns>
        public async Task GetTasksAsync(ProgramDTO taskData, string selectedTabKey)
        {
            string limit = taskData.RecordCount > 0 ? $" LIMIT {taskData.RecordCount}" : "";
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            string condition;
            if (taskData.IsActive)
            {
                condition = " AND TM.IsActive = 1 AND TM.ExecuteOnLogin = 1 " +
                    $"AND TM.Status IN ('{ResourceConstants.R_NEW_STATUS_KEY}','{ResourceConstants.R_INPROGRESS_STATUS_KEY}') " +
                    "AND CAST(strftime('%s', FromDateString)  AS  INTEGER) <= CAST(strftime('%s', 'now')  AS  INTEGER) " +
                    $"AND TM.ToDate >= '{GenericMethods.GetUtcDateTime.Date.Ticks}' " +
                    "ORDER BY TM.ToDate ";
            }
            else
            {
                switch (selectedTabKey)
                {
                    case "":
                        condition = "AND (TM.IsActive = 1 OR (TM.IsActive = 0 AND TM.ProgramID IS NOT NULL " +
                            $"AND TM.Status IN ('{ResourceConstants.R_COMPLETED_STATUS_KEY}','{ResourceConstants.R_MISSED_STATUS_KEY}'))) " +
                            "ORDER BY TM.ToDate DESC";
                        break;
                    case ResourceConstants.R_OPEN_TASK_KEY:
                        condition = $"AND TM.IsActive = 1 AND TM.Status IN ('{ResourceConstants.R_NEW_STATUS_KEY}','{ResourceConstants.R_INPROGRESS_STATUS_KEY}') " +
                            "AND CAST(strftime('%s', FromDateString)  AS  INTEGER) <= CAST(strftime('%s', 'now')  AS  INTEGER) " +
                            $"AND TM.ToDate >= '{GenericMethods.GetUtcDateTime.Date.Ticks}' " +
                            "ORDER BY TM.ToDate ";
                        break;
                    default:
                        condition = "AND (TM.IsActive = 1 AND ( TM.ProgramID IS NOT NULL)) " +
                            $"AND (TM.Status IN ('{ResourceConstants.R_COMPLETED_STATUS_KEY}','{ResourceConstants.R_MISSED_STATUS_KEY}') " +
                            $"OR TM.ToDate < '{GenericMethods.GetUtcDateTime.Date.Ticks}' ) " +
                            "ORDER BY TM.CompletionDate DESC ";
                        break;
                }
            }

            string pColor;
            if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
            {
                pColor = "PPM.ProgramGroupIdentifier";
            }
            else
            {
                pColor = "PM.ProgramGroupIdentifier";
            }

            taskData.Tasks = await SqlConnection.QueryAsync<TaskModel>(
                "SELECT DISTINCT strftime('%Y-%m-%d',TM.FromDate/10000000 - 62135596800,'unixepoch') As FromDateString, TM.PatientTaskID, TM.ItemID, PM.Name, " +
                $"{pColor} AS ProgramColor, TM.TaskType, TM.FromDate, TM.ToDate, TM.Status, TM.ProgramID, TM.ExecuteOnLogin, " +
                "CASE WHEN TM.TaskType = ? THEN CDM.PageHeading WHEN TM.TaskType = ? THEN QDM.CaptionText WHEN TM.TaskType = ? THEN IM.Name ELSE RSM.ResourceValue END AS SelectedItemName, " +
                "CASE WHEN TM.TaskType = ? THEN QM.DefaultRespondentID " +
                    $"WHEN TM.TaskType = ? THEN (SELECT ValueAddedBy FROM ReadingModel RM WHERE TM.TaskType = ? AND RM.ReadingID = TM.ItemID AND RM.UserID = {taskData.SelectedUserID} AND RM.IsActive =1 LIMIT 1) " +
                    "ELSE 'PatientKey' END AS TaskRespondent " +
                "FROM TaskModel TM " +
                "LEFT JOIN ProgramModel PM ON TM.ProgramID = PM.ProgramID AND PM.IsActive = 1 " +
                "LEFT JOIN PatientProgramModel PPM ON PPM.ProgramID = PM.ProgramID AND PPM.PatientID = ? " +
                "LEFT JOIN ContentDetailModel CDM ON TM.TaskType = ? AND TM.ItemID = CDM.PageID AND CDM.LanguageID = ? AND CDM.IsActive = 1 " +
                "LEFT JOIN QuestionnaireModel QM ON TM.TaskType = ? AND TM.ItemID = QM.QuestionnaireID " +
                "LEFT JOIN QuestionnaireDetailsModel QDM ON TM.TaskType = ? AND TM.ItemID = QDM.QuestionnaireID AND QDM.LanguageID = ? " +
                "LEFT JOIN ResourceModel RSM ON TM.TaskType = ? AND TM.ItemID = RSM.ResourceKeyID AND RSM.LanguageID = ? " +
                "LEFT JOIN InstructionI18NModel IM ON TM.TaskType = ? AND TM.ItemID = IM.InstructionID AND IM.LanguageID = ? " +
                $"WHERE TM.UserID = ? {condition} {limit}",
                ResourceConstants.R_EDUCATION_KEY, ResourceConstants.R_QUESTIONNAIRE_KEY, ResourceConstants.R_INSTRUCTION_KEY, ResourceConstants.R_QUESTIONNAIRE_KEY,
                ResourceConstants.R_MEASUREMENT_KEY, ResourceConstants.R_MEASUREMENT_KEY, taskData.SelectedUserID, ResourceConstants.R_EDUCATION_KEY,
                taskData.LanguageID, ResourceConstants.R_QUESTIONNAIRE_KEY, ResourceConstants.R_QUESTIONNAIRE_KEY, taskData.LanguageID, ResourceConstants.R_MEASUREMENT_KEY,
                taskData.LanguageID, ResourceConstants.R_INSTRUCTION_KEY, taskData.LanguageID, taskData.SelectedUserID
            ).ConfigureAwait(false);
            if (roleID == (int)RoleName.CareTaker && taskData.Tasks?.Count > 0)
            {
                List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(taskData).ConfigureAwait(false);
                taskData.Tasks = sharedPrograms?.Count > 0
                    ? taskData.Tasks.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID))?.ToList()
                    : null;
            }
            if (taskData.RecordCount > 0)
            {
                taskData.Tasks = taskData.Tasks?.Take((int)taskData.RecordCount)?.ToList();
            }
        }

        /// <summary>
        /// Return the count of open and inprogress tasks
        /// </summary>
        /// <param name="taskData">Task data instance</param>
        /// <returns>open and inprogress task count</returns>
        public async Task GetOpenTasksCountAsync(ProgramDTO taskData)
        {
            ProgramDTO taskCountData = new ProgramDTO { SelectedUserID = taskData.SelectedUserID, AccountID = taskData.AccountID, LanguageID = taskData.LanguageID, IsActive = false };
            await GetTasksAsync(taskCountData, ResourceConstants.R_OPEN_TASK_KEY).ConfigureAwait(false);
            taskData.BadgeCount = GenericMethods.IsListNotEmpty(taskCountData.Tasks)
                ? taskCountData.Tasks.Count.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        /// <summary>
        /// Get patient task based on PatientTaskID
        /// </summary>
        /// <param name="taskData">Reference object to hold patient task</param>
        /// <returns>Patient task based on PatientTaskID</returns>
        public async Task GetTaskAsync(ProgramDTO taskData)
        {
            taskData.Task = await SqlConnection.FindWithQueryAsync<TaskModel>(
               $"SELECT TM.PatientTaskID, TM.ProgramID, PM.Name, TM.TaskType, TM.CompletionDate, TM.FromDate, TM.ToDate, TM.Status, TM.ItemID, TM.LastModifiedByID, (SELECT FirstName || ' ' || LastName FROM UserModel AS FirstName WHERE AccountID = TM.LastModifiedByID) AS LastActionBy, " +
               $"CASE WHEN TM.TaskType = ? THEN CDM.PageHeading " +
                    $"WHEN TM.TaskType = ? THEN QM.CaptionText " +
                    $"WHEN TM.TaskType = ? THEN IM.Name " +
                    $"ELSE RSM.ResourceValue END SelectedItemName, " +
               $"CASE WHEN TM.TaskType = ? THEN QSD.CaptionText END Recommendation, " +
               $"CASE WHEN TM.TaskType = ? THEN PQS.UserScore " +
                    $"WHEN TM.TaskType = ? THEN PRM.ReadingValue END ResultValue " +
               $"FROM TaskModel TM " +
               $"LEFT JOIN ProgramModel PM ON TM.ProgramID = PM.ProgramID AND PM.IsActive = 1 " +
               $"LEFT JOIN ContentDetailModel CDM ON TM.TaskType = ? AND TM.ItemID = CDM.PageID AND CDM.LanguageID = ? " +
               $"LEFT JOIN QuestionnaireDetailsModel QM ON TM.TaskType = ? AND TM.ItemID = QM.QuestionnaireID " +
               $"LEFT JOIN ResourceModel RSM ON RSM.ResourceKeyID = TM.ItemID AND TM.TaskType = ? AND RSM.LanguageID = ? " +
               $"LEFT JOIN InstructionI18NModel IM ON TM.TaskType = ? AND TM.ItemID = IM.InstructionID AND IM.LanguageID = ? " +
               $"LEFT JOIN PatientQuestionnaireScoresModel PQS ON TM.PatientTaskID = PQS.PatientTaskID " +
               $"LEFT JOIN QuestionnaireSubscaleRangeDetailsModel QSD ON PQS.SubScaleRangeID = QSD.SubScaleRangeID " +
               $"LEFT JOIN PatientReadingModel PRM ON TM.TaskType = ? AND TM.PatientTaskID = PRM.PatientTaskID " +
               $"WHERE TM.PatientTaskID = ? ",
               ResourceConstants.R_EDUCATION_KEY, ResourceConstants.R_QUESTIONNAIRE_KEY, ResourceConstants.R_INSTRUCTION_KEY, ResourceConstants.R_QUESTIONNAIRE_KEY, ResourceConstants.R_QUESTIONNAIRE_KEY, ResourceConstants.R_MEASUREMENT_KEY,
               ResourceConstants.R_EDUCATION_KEY, taskData.LanguageID, ResourceConstants.R_QUESTIONNAIRE_KEY, ResourceConstants.R_MEASUREMENT_KEY, taskData.LanguageID, ResourceConstants.R_INSTRUCTION_KEY, taskData.LanguageID, ResourceConstants.R_MEASUREMENT_KEY, taskData.Task.PatientTaskID
            ).ConfigureAwait(false);
            var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
            bool isPatientData = roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker;
            if (!isPatientData)
            {
                if (taskData.Task.TaskType == ResourceConstants.R_QUESTIONNAIRE_KEY && taskData.Task.Status == ResourceConstants.R_COMPLETED_STATUS_KEY)
                {
                    await new PatientProviderNoteDatabase().GetTaskQuestionDetailsAsync(taskData).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Fetch task by its id
        /// </summary>
        /// <param name="patientTaskID">patient task id to fetch task</param>
        /// <returns>Patient task for given id</returns>
        public async Task<TaskModel> GetTaskByPatientTaskIDAsync(long patientTaskID)
        {
            return await SqlConnection.FindWithQueryAsync<TaskModel>(
                "SELECT * FROM TaskModel WHERE PatientTaskID = ? AND IsActive = 1", patientTaskID
            );
        }

        /// <summary>
        /// Get All the patient taskID and Status to sync
        /// </summary>
        /// <param name="taskData">Reference object to hold patient task ID and Status</param>
        /// <returns>Operation Status</returns>
        public async Task GetPatientTaskStatusToSyncWithServerAsync(ProgramDTO taskData)
        {
            taskData.Tasks = await SqlConnection.QueryAsync<TaskModel>("SELECT PatientTaskID, Status FROM TaskModel WHERE IsSynced = 0");
        }
        /// <summary>
        /// Update the patient task Status
        /// </summary>
        /// <param name="patientTaskID">Task id for which status needs to update</param>
        /// <param name="statusKey">Status key of that task to update</param>
        /// <returns>Operation status</returns>
        public async Task UpdateTaskStatusAsync(long patientTaskID, string statusKey)
        {
            await UpdateTaskStatusAsync(new ProgramDTO
            {
                Tasks = new List<TaskModel>
                {
                    new TaskModel {
                        PatientTaskID = Convert.ToInt64(patientTaskID),
                        Status = statusKey,
                        IsSynced = false
                    }
                }
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// Update Task Status on PatientTaskID
        /// </summary>
        /// <param name="taskData">Reference object to hold patient task</param>
        /// <returns>Operation Status</returns>
        public async Task UpdateTaskStatusAsync(ProgramDTO taskData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (TaskModel task in taskData.Tasks)
                {
                    if (task.IsSynced)
                    {
                        transaction.Execute(
                            "UPDATE TaskModel SET IsSynced = ?, LastModifiedByID = ?  WHERE PatientTaskID = ? AND IsSynced <> ?"
                            , task.IsSynced, task.LastModifiedByID, task.PatientTaskID, task.IsSynced);
                    }
                    else
                    {
                        var existingTasks = transaction.FindWithQuery<TaskModel>("SELECT * FROM TaskModel WHERE PatientTaskID = ?", task.PatientTaskID);
                        if (existingTasks != null && existingTasks.Status != ResourceConstants.R_COMPLETED_STATUS_KEY)
                        {
                            string completionDate = task.Status == ResourceConstants.R_COMPLETED_STATUS_KEY
                                ? $" , CompletionDate = '{GenericMethods.GetUtcDateTime.Ticks}' "
                                : "";
                            transaction.Execute(
                                $"UPDATE TaskModel SET Status = ?, IsSynced = ? {completionDate} WHERE PatientTaskID = ? AND Status <> ?"
                                , task.Status, task.IsSynced, task.PatientTaskID, task.Status);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}