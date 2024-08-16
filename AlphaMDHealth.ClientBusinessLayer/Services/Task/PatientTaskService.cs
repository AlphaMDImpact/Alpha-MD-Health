using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class PatientTaskService : BaseService
    {
        QuestionnaireService _questionnaireService;

        /// <summary>
        /// Task service
        /// </summary>
        public PatientTaskService(IEssentials essentials) : base(essentials)
        {
            _questionnaireService = new QuestionnaireService(essentials);
        }

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveTasksAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var taskData = new ProgramDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                };
                taskData.Tasks = MapPatientTasks(data, nameof(DataSyncDTO.PatientTasks), taskData);
                if (GenericMethods.IsListNotEmpty(taskData.Tasks))
                {
                    await new PatientTaskDatabase().SavePatientTasksAsync(taskData).ConfigureAwait(false);
                    result.RecordCount = taskData.Tasks.Count;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Update the patient task Status
        /// </summary>
        /// <param name="taskID">Task id for which status needs to update</param>
        /// <param name="statusKey">Status key of that task to update</param>
        /// <returns>Operation status</returns>
        public async Task<ErrorCode> UpdateTaskStatusAsync(long taskID, string statusKey)
        {
            try
            {
                await new PatientTaskDatabase().UpdateTaskStatusAsync(taskID, statusKey).ConfigureAwait(false);
                return ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return ErrorCode.ErrorWhileSavingRecords;
            }
        }


        /// <summary>
        /// Get patient tasks
        /// </summary>
        /// <param name="taskData">Reference object to hold list of tasks</param>
        /// <param name="selectedTabKey">Selected tab</param>
        /// <returns>List of patient tasks with operation status</returns>
        public async Task GetTasksAsync(ProgramDTO taskData, string selectedTabKey)
        {
            try
            {
                taskData.SelectedUserID = GetUserID();
                if (MobileConstants.IsMobilePlatform)
                {
                    taskData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    taskData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                    using (PatientTaskDatabase patientTaskDB = new PatientTaskDatabase())
                    {
                        await Task.WhenAll(
                             GetFeaturesAsync(new[] { AppPermissions.PatientTasksView.ToString(), AppPermissions.PatientAssignTaskView.ToString(), AppPermissions.PatientTaskView.ToString() }),
                             GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                             GetResourcesAsync(GroupConstants.RS_TASK_GROUP, GroupConstants.RS_TASK_TYPE_GROUP, GroupConstants.RS_TASK_STATUS_GROUP, GroupConstants.RS_COMMON_GROUP),
                             patientTaskDB.GetTasksAsync(taskData, selectedTabKey),
                             patientTaskDB.GetOpenTasksCountAsync(taskData)
                         ).ConfigureAwait(false);
                    }
                    MapTaskData(taskData, selectedTabKey);
                    taskData.Tasks = taskData.Tasks.OrderByDescending(x => x.FromDate.Value.Date).ToList();
                }
                else
                {
                    await SyncPatientTasksFromServerAsync(taskData, CancellationToken.None).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                taskData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Get patient tasks
        /// </summary>
        /// <param name="taskData">Reference object to hold list of tasks</param>
        /// <returns>List of patient tasks with operation status</returns>
        public async Task<int> IsTaskCompletionRequiredAsync()
        {
            try
            {
                ProgramDTO taskData = new ProgramDTO
                {
                    LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0),
                    IsActive = true, // Fetch only those tasks, for execute on login is true
                    AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0),
                    SelectedUserID = GetUserID()
                };
                var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);

                using (PatientTaskDatabase patientTaskDB = new PatientTaskDatabase())
                {
                    await patientTaskDB.GetTasksAsync(taskData, string.Empty).ConfigureAwait(false);
                }
                if (roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker)
                {
                    return 0;
                }
                else
                {
                    return taskData.Tasks?.Count > 0 ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return 2;
            }
        }

        /// <summary>
        /// Get open task count for chat menu
        /// </summary>
        /// <returns>badge count with operation status</returns>
        public async Task<string> GetUnreadCountAsync()
        {
            ProgramDTO taskData = new ProgramDTO();
            try
            {
                taskData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                taskData.SelectedUserID = GetUserID();
                taskData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                await new PatientTaskDatabase().GetOpenTasksCountAsync(taskData).ConfigureAwait(false);
                return taskData.BadgeCount;
            }
            catch (Exception ex)
            {
                taskData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
                return string.Empty;
            }
        }

        private void MapTaskData(ProgramDTO taskData, string selectedTabKey)
        {
            LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            taskData.Tasks.ForEach(task =>
            {
                MapFormattedDate(dayFormat, monthFormat, yearFormat, task, false);
                task.SelectedItemName = string.IsNullOrWhiteSpace(task.SelectedItemName) ? string.Empty : task.SelectedItemName;
                //Todo:
                //task.ProgramColor = string.IsNullOrWhiteSpace(task.ProgramColor) ? Color.Transparent.ToHex() : task.ProgramColor;
                task.StatusValue = LibResources.GetResourceValueByKey(PageData?.Resources, task.Status);
                task.LeftDefaultIcon = GetImageBasedOnTaskType(task.TaskType);
                task.FromDateString = string.IsNullOrWhiteSpace(selectedTabKey)
                ? $"{task.FromDateString} {Constants.SYMBOL_DASH} {task.ToDateString}" : LibResources.GetResourceValueByKey(PageData?.Resources, task.TaskType);
                if (string.IsNullOrWhiteSpace(selectedTabKey))
                {
                    task.Status = DateTime.Now.Date > _essentials.ConvertToLocalTime(task.ToDate.Value) && task.Status != ResourceConstants.R_COMPLETED_STATUS_KEY ? ResourceConstants.R_MISSED_STATUS_KEY : task.Status;
                    task.StatusColor = StatusColor(task.Status);
                    task.Name = string.IsNullOrEmpty(task.Name) ? LibResources.GetResourceValueByKey(PageData?.Resources, task.TaskType) : $"{LibResources.GetResourceValueByKey(PageData?.Resources, task.TaskType)} {Constants.SYMBOL_DASH} {task.Name}";
                }
                //Code for Taking Task ValueAddedBy
                //ReadingService _serviceObject = new ReadingService();
                //PatientReadingDTO readingsData = new PatientReadingDTO();
                //readingsData.ReadingID = (short)task.ItemID;
                //if (task.TaskType == TaskType.MeasurementKey.ToString())
                //{
                //    await _serviceObject.GetMetadataAsync(readingsData).ConfigureAwait(false);
                //    if (selectedTabKey == ResourceConstants.R_OPEN_TASK_KEY)
                //    {
                //        task.TaskRespondent = readingsData.Readings.FirstOrDefault(x => x.ReadingID == task.ItemID && x.IsActive && x.UserID == taskData.SelectedUserID)?.ValueAddedBy.ToString();
                //    }
                //}
            });
            // For Provifer, display open task first then display history task in the list
            if (string.IsNullOrEmpty(selectedTabKey))
            {
                SortPatientTasks(taskData);
            }
            taskData.ErrCode = ErrorCode.OK;
        }


        private void SortPatientTasks(ProgramDTO taskData)
        {
            List<TaskModel> openTasks = new List<TaskModel>();
            List<TaskModel> historyTasks = new List<TaskModel>();
            foreach (var categoryGroup in taskData.Tasks.GroupBy(x => x.Status).ToList())
            {
                if (GenericMethods.IsListNotEmpty(categoryGroup.ToList()) && (categoryGroup.Key == ResourceConstants.R_NEW_STATUS_KEY || categoryGroup.Key == ResourceConstants.R_INPROGRESS_STATUS_KEY))
                {
                    openTasks.AddRange(categoryGroup.ToList());
                }
                else
                {
                    historyTasks.AddRange(categoryGroup.ToList());
                }
            }
            var open = openTasks.OrderBy(x => x.ToDate).ToList();
            var history = historyTasks.OrderByDescending(x => x.CompletionDate).ToList();
            taskData.Tasks = new List<TaskModel>();
            taskData.Tasks.AddRange(open);
            taskData.Tasks.AddRange(history);
        }

        /// <summary>
        /// Get patient task based on PatientTaskID
        /// </summary>
        /// <param name="taskData">Reference object to hold list of tasks</param>
        /// <returns>Patient task based on PatientTaskID with operation status</returns>
        public async Task GetPatientTaskAsync(ProgramDTO taskData)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform && taskData.Task.PatientTaskID > 0)
                {
                    taskData.LanguageID = (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
                    taskData.SelectedUserID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
                    await Task.WhenAll(
                        GetFeaturesAsync(new[] { AppPermissions.PatientTaskView.ToString(), AppPermissions.PatientTaskDelete.ToString() }),
                        GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                        GetResourcesAsync(GroupConstants.RS_TASK_GROUP, GroupConstants.RS_TASK_TYPE_GROUP, GroupConstants.RS_TASK_STATUS_GROUP, GroupConstants.RS_COMMON_GROUP),
                        new PatientTaskDatabase().GetTaskAsync(taskData)
                    ).ConfigureAwait(false);
                    if (taskData.Task != null)
                    {
                        LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                        MapFormattedDate(dayFormat, monthFormat, yearFormat, taskData.Task, true);
                        taskData.Task.Status = DateTime.Now.Date > _essentials.ConvertToLocalTime(taskData.Task.ToDate.Value) && taskData.Task.Status != ResourceConstants.R_COMPLETED_STATUS_KEY ? ResourceConstants.R_MISSED_STATUS_KEY : taskData.Task.Status;
                        taskData.Task.StatusValue = LibResources.GetResourceValueByKey(PageData?.Resources, taskData.Task.Status);
                        taskData.Task.TaskType = LibResources.GetResourceValueByKey(PageData?.Resources, taskData.Task.TaskType);
                        if (GenericMethods.IsListNotEmpty(taskData.QuestionnaireQuestions))
                        {
                            _questionnaireService.MapQuestionDetails(taskData.QuestionnaireQuestions, taskData.QuestionnaireQuestiosDetails);
                        }
                        taskData.ErrCode = ErrorCode.OK;
                    }
                    else
                    {
                        taskData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                    }
                }
                else
                {
                    await SyncPatientTasksFromServerAsync(taskData, CancellationToken.None).ConfigureAwait(false);
                }
                if (taskData.ErrCode == ErrorCode.OK && taskData.RecordCount == -1)
                {
                    var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                    if (roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker)
                    {
                        if ((taskData.Task.TaskType == TaskType.QuestionnaireKey.ToString() || taskData.Task.TaskType == LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_QUESTIONNAIRE_KEY).ResourceValue)
                            && taskData.Task.Status == ResourceConstants.R_COMPLETED_STATUS_KEY)
                        {
                            if (GenericMethods.IsListNotEmpty(taskData.QuestionnaireQuestionOptions) || GenericMethods.IsListNotEmpty(taskData.QuestionnaireQuestions))
                            {
                                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                                string childrenUI = _questionnaireService.GetQuestionAnswerList(taskData.QuestionnaireQuestions, taskData.QuestionnaireQuestionOptions, dayFormat, monthFormat, yearFormat, true);
                                if (!string.IsNullOrEmpty(childrenUI))
                                {
                                    taskData.Items = new List<OptionModel>
                                    {
                                        new OptionModel { OptionID = 0, OptionText = childrenUI }
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                taskData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        ///  Sync patient task to server
        /// </summary>
        /// <param name="programData">Reference object to hold program task data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncPatientTaskToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                if (programData.Task != null)
                {
                    programData.Task.AddedOn = GenericMethods.GetUtcDateTime;
                    programData.Task.LastModifiedON = GenericMethods.GetUtcDateTime;
                }
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PATIENT_TASK_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (httpData.ErrCode == ErrorCode.OK && MobileConstants.IsMobilePlatform)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data?.HasValues == true)
                    {
                        programData.Task.PatientTaskID = (long)data[nameof(ProgramDTO.Task)][nameof(TaskModel.PatientTaskID)];
                    }
                    programData.Tasks = new List<TaskModel> { programData.Task };
                    await new PatientTaskDatabase().SavePatientTasksAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Patient Task Status To server
        /// </summary>
        /// <param name="taskData">Object containing Patient Task ID and Status to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status call</returns>
        public async Task SyncPatientTaskStatusToServerAsync(ProgramDTO taskData, CancellationToken cancellationToken)
        {
            try
            {
                await new PatientTaskDatabase().GetPatientTaskStatusToSyncWithServerAsync(taskData).ConfigureAwait(false);
                if (GenericMethods.IsListNotEmpty(taskData.Tasks))
                {
                    var httpData = new HttpServiceModel<ProgramDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.UPDATE_PATIENT_TASK_STATUS_ASYNC_PATH,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        },
                        ContentToSend = taskData
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    taskData.ErrCode = httpData.ErrCode;
                    if (taskData.ErrCode == ErrorCode.OK)
                    {
                        taskData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
                        taskData.Tasks.ForEach(x =>
                        {
                            x.IsSynced = true;
                            x.LastModifiedByID = taskData.AccountID;
                        });
                        await new PatientTaskDatabase().UpdateTaskStatusAsync(taskData).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                taskData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        /// <summary>
        /// Sync patient task from service
        /// </summary>
        /// <param name="taskData">taskData reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Patient tasks received from server</returns>
        private async Task SyncPatientTasksFromServerAsync(ProgramDTO taskData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PATIENT_TASKS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { nameof(TaskModel.PatientTaskID), Convert.ToString(taskData.Task?.PatientTaskID ?? 0, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.RecordCount), Convert.ToString(taskData.RecordCount, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.SelectedUserID), Convert.ToString(taskData.SelectedUserID, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                taskData.ErrCode = httpData.ErrCode;
                if (taskData.ErrCode == ErrorCode.OK)
                {
                    await MapGetPatientTasksServiceResponseAsync(taskData, httpData.Response).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                taskData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private async Task MapGetPatientTasksServiceResponseAsync(ProgramDTO taskData, string jsonResponse)
        {
            JToken data = JToken.Parse(jsonResponse);
            if (data != null && data.HasValues)
            {
                if (!MobileConstants.IsMobilePlatform)
                {
                    MapCommonData(taskData, data);
                    SetResourcesAndSettings(taskData);
                }
                await MapPatientTasksAsync(data, taskData).ConfigureAwait(false);
                var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                if (roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker)
                {
                    //SortPatientMedications(taskData);
                    GenericMethods.SortByDate(taskData.Tasks, x => x.FromDate, x => x.ToDate);
                }
            }
        }

        internal async Task<object> MapPatientTasksHistoryData(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string jsonResponse)
        {
            ProgramDTO taskData = new ProgramDTO
            {
                FromDate = medicalHistoryData.FromDate,
                ToDate = medicalHistoryData.ToDate,
                RecordCount = medicalHistoryData.RecordCount,
                ErrCode = historyView.ErrorCode
            };
            await MapGetPatientTasksServiceResponseAsync(taskData, jsonResponse);
            taskData.FeaturePermissions = medicalHistoryData.FeaturePermissions;
            historyView.HasData = GenericMethods.IsListNotEmpty(taskData.Tasks);
            return taskData;
        }

        private async Task MapPatientTasksAsync(JToken data, ProgramDTO taskData)
        {
            LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            // Add Edit Case
            if (taskData.RecordCount == -1)
            {
                //Detail case
                if (taskData.Task.PatientTaskID > 0)
                {
                    JToken taskJData = data[nameof(ProgramDTO.Task)];
                    if (taskJData.HasValues)
                    {
                        taskData.Task = MapPatientTask(taskData, taskJData);
                        MapFormattedDate(dayFormat, monthFormat, yearFormat, taskData.Task, true);
                    }
                    taskData.QuestionnaireQuestions = _questionnaireService.MapQuestionnaireQuestions(data, nameof(taskData.QuestionnaireQuestions));
                    taskData.QuestionnaireQuestionOptions = _questionnaireService.MapQuestionOptions(data, nameof(taskData.QuestionnaireQuestionOptions));
                }
                //Add case
                else
                {
                    if (MobileConstants.IsMobilePlatform)
                    {
                        await GetResourcesAsync(GroupConstants.RS_TASK_TYPE_GROUP).ConfigureAwait(false);
                    }
                    taskData.TaskTypes = MapResourcesIntoOptions(GroupConstants.RS_TASK_TYPE_GROUP, string.Empty, "1", false);
                    taskData.Items = GetPickerSource(data, nameof(ProgramDTO.Items), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), taskData.RecordCount, false, null);
                }
            }
            // List case
            else
            {
                taskData.Tasks = MapPatientTasks(data, nameof(ProgramDTO.Tasks), taskData);
                taskData.Task = new TaskModel { FromDateString = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat) };
                taskData.Tasks.ForEach(task =>
                {
                    task.ToDateValue = GenericMethods.GetLocalDateTimeBasedOnCulture(task.ToDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    task.FromDateValue = GenericMethods.GetLocalDateTimeBasedOnCulture(task.FromDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    task.DateStyle = GetDateTimeStyle(task.FromDate, task.ToDate);
                    task.StatusColor = StatusColor(task.Status);
                });
            }
        }

        //todo: to change once table control will be reviewed and corrected properly to support all type of labels
        private string GetDateTimeStyle(DateTimeOffset? fromDate, DateTimeOffset? toDate)
        {
            var currentDate = GenericMethods.GetUtcDateTime;
            string dateStyle;
            if (!fromDate.HasValue || !toDate.HasValue || (fromDate < currentDate && toDate > currentDate))
            {
                dateStyle = MobileConstants.IsMobilePlatform
                    ? StyleConstants.SUCCESS_COLOR
                    : "date-present ";
            }
            else if (fromDate > currentDate && toDate > currentDate)
            {
                dateStyle = MobileConstants.IsMobilePlatform
                    ? StyleConstants.SECONDARY_APP_COLOR
                    : "date-future ";
            }
            else
            {
                dateStyle = MobileConstants.IsMobilePlatform
                    ? StyleConstants.ERROR_COLOR
                    : "date-past ";
            }
            return dateStyle;
        }

        private List<TaskModel> MapPatientTasks(JToken data, string collectionName, ProgramDTO taskData)
        {
            return data[collectionName].Any()
                ? (from dataItem in data[collectionName]
                   select MapPatientTask(taskData, dataItem)).ToList()
                : new List<TaskModel>();
        }

        private TaskModel MapPatientTask(ProgramDTO taskData, JToken dataItem)
        {
            return new TaskModel
            {
                PatientTaskID = GetDataItem<long>(dataItem, nameof(TaskModel.PatientTaskID)),
                UserID = GetDataItem<long>(dataItem, nameof(TaskModel.UserID)),
                Name = string.IsNullOrWhiteSpace(GetDataItem<string>(dataItem, nameof(TaskModel.Name))) && !MobileConstants.IsMobilePlatform
                    ? Constants.SYMBOL_DOUBLE_HYPHEN
                    : GetDataItem<string>(dataItem, nameof(TaskModel.Name)),
                SelectedItemName = GetDataItem<string>(dataItem, nameof(TaskModel.SelectedItemName)),
                ItemID = GetDataItem<long>(dataItem, nameof(TaskModel.ItemID)),
                IsActive = GetDataItem<bool>(dataItem, nameof(TaskModel.IsActive)),
                TaskType = MobileConstants.IsMobilePlatform || taskData.RecordCount == -1
                    ? GetDataItem<string>(dataItem, nameof(TaskModel.TaskType))
                    : LibResources.GetResourceValueByKey(PageData?.Resources, GetDataItem<string>(dataItem, nameof(TaskModel.TaskType))),
                FromDate = GetDataItem<DateTimeOffset>(dataItem, nameof(TaskModel.FromDate)),
                ToDate = GetDataItem<DateTimeOffset>(dataItem, nameof(TaskModel.ToDate)),
                ProgramID = GetDataItem<long>(dataItem, nameof(TaskModel.ProgramID)),
                CompletionDate = GetDataItem<DateTimeOffset>(dataItem, nameof(TaskModel.CompletionDate)),
                Status = GetDataItem<string>(dataItem, nameof(TaskModel.Status)),
                StatusValue = GetTaskStatus((DateTimeOffset)dataItem[nameof(TaskModel.ToDate)], (string)dataItem[nameof(TaskModel.Status)], taskData.RecordCount >= 0),
                ResultValue = (string)dataItem[nameof(TaskModel.ResultValue)],
                Recommendation = (string)dataItem[nameof(TaskModel.Recommendation)],
                ProgramColor = (string)dataItem[nameof(TaskModel.ProgramColor)],
                ExecuteOnLogin = GetDataItem<bool>(dataItem, nameof(TaskModel.ExecuteOnLogin)),
                IsSynced = true,
                LastActionBy = (string)dataItem[nameof(TaskModel.LastActionBy)],
                LastModifiedByID = GetDataItem<long>(dataItem, nameof(TaskModel.LastModifiedByID)),
                TaskRespondent = (string)dataItem[nameof(TaskModel.TaskRespondent)],
            };
        }

        private void MapFormattedDate(string dayFormat, string monthFormat, string yearFormat, TaskModel task, bool isDetailPage)
        {
            if (!string.IsNullOrWhiteSpace(dayFormat) && !string.IsNullOrWhiteSpace(monthFormat) && !string.IsNullOrWhiteSpace(yearFormat))
            {
                task.ToDateString = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(task.ToDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                task.FromDateString = isDetailPage
                    ? $"{GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(task.FromDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat)} - {task.ToDateString}"
                    : GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(task.FromDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                task.CompletionDateString = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(task.CompletionDate), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            }
        }

        private string GetTaskStatus(DateTimeOffset toDateTime, string status, bool isListPage)
        {
            if (MobileConstants.IsMobilePlatform || !isListPage)
            {
                return (DateTime.Now.Date > _essentials.ConvertToLocalTime(toDateTime) && status != ResourceConstants.R_COMPLETED_STATUS_KEY)
                    ? LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_MISSED_STATUS_KEY)
                    : LibResources.GetResourceValueByKey(PageData?.Resources, status);
            }
            else
            {
                status = (DateTime.Now.Date > _essentials.ConvertToLocalTime(toDateTime) && status != ResourceConstants.R_COMPLETED_STATUS_KEY) ? ResourceConstants.R_MISSED_STATUS_KEY : status;
                return LibResources.GetResourceValueByKey(PageData?.Resources, status);
            }
        }

        private string StatusColor(string status)
        {
            return status switch
            {
                ResourceConstants.R_NEW_STATUS_KEY => FieldTypes.SecondaryBadgeControl.ToString(),
                ResourceConstants.R_INPROGRESS_STATUS_KEY => FieldTypes.BadgeCountControl.ToString(),
                ResourceConstants.R_COMPLETED_STATUS_KEY => FieldTypes.SuccessBadgeControl.ToString(),
                ResourceConstants.R_MISSED_STATUS_KEY => FieldTypes.DangerBadgeControl.ToString(),
                _ => string.Empty,
            };
        }

        private string GetImageBasedOnTaskType(string taskType)
        {
            return taskType.ToEnum<TaskType>() switch
            {
                TaskType.MeasurementKey => ImageConstants.I_MEASUREMENT_TASK_ICON_SVG,
                TaskType.EducationKey => ImageConstants.I_EDUCATION_TASK_ICON_SVG,
                TaskType.QuestionnaireKey => ImageConstants.I_QUESTIONNAIRE_TASK_ICON_SVG,
                TaskType.InstructionKey => ImageConstants.I_QUESTIONNAIRE_TASK_ICON_SVG,
                _ => "",
            };
        }
    }
}