using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public partial class ProgramService : BaseService
    {
        public ProgramService(IEssentials essentials):base(essentials) 
        { }
        /// <summary>
        ///  Get program's subflows
        /// </summary>
        /// <param name="programData">Reference object contains data</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        /// <returns>Subflows data with operation status</returns>
        public async Task SyncSubFlowsFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_SUBFLOWS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(SubFlowModel.SubFlowID), Convert.ToString(programData.SubFlow.SubFlowID, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.RecordCount), Convert.ToString(programData.RecordCount, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        MapSubFlowsServiceResponse(data, programData);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync subflow data to server
        /// </summary>
        /// <param name="programData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncSubFlowToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_SUBFLOW_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData,
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                programData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Get program tasks from server
        /// </summary>
        /// <param name="programData">Reference object to hold program task data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Program tasks with operation status</returns>
        public async Task SyncTasksFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_TASKS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(TaskModel.TaskID), Convert.ToString(programData.Task.TaskID, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.RecordCount), Convert.ToString(programData.RecordCount, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        MapTasksServiceResponse(data, programData);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Sync task to server
        /// </summary>
        /// <param name="programData">Reference object to hold program task data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncTaskToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_TASK_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                programData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get subflows list that can be added for task
        /// </summary>
        /// <param name="programData">object to get subflow data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>task sublow and list of subflows</returns>
        public async Task SyncTaskSubflowFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_TASK_SUBFLOW_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(SubFlowModel.TaskSubFlowID), Convert.ToString(programData.SubFlow.TaskSubFlowID, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        MapSubflows(data, programData);
                        if (programData.SubFlow.TaskSubFlowID > 0 && data[nameof(ProgramDTO.SubFlow)].Any())
                        {
                            programData.SubFlow = new SubFlowModel
                            {
                                SubFlowID = (long)data[nameof(ProgramDTO.SubFlow)][nameof(SubFlowModel.SubFlowID)],
                                AssignAfterDays = (short)data[nameof(ProgramDTO.SubFlow)][nameof(SubFlowModel.AssignAfterDays)],
                                AssignForDays = (short)data[nameof(ProgramDTO.SubFlow)][nameof(SubFlowModel.AssignForDays)],
                            };
                        }
                        programData.ProgramSubFlows = GetPickerSource(data, nameof(ProgramDTO.SubFlows), nameof(SubFlowModel.SubFlowID), Constants.CNT_NAME_TEXT, programData.SubFlow?.SubFlowID ?? 0, false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Sync task subflow to server
        /// </summary>
        /// <param name="programData">Reference object to hold task subflow data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncTaskSubflowToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_TASK_SUBFLOW_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                       { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.SubFlow.TaskSubFlowID = (long)data[nameof(ProgramDTO.SubFlow)][nameof(SubFlowModel.TaskSubFlowID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }


        /// <summary>
        ///  Get program's Reason Configuration flow subflows
        /// </summary>
        /// <param name="programData">Reference object to hold task subflow data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramReasonConfigToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_REASON_CONFIGURATIION_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                       { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);

                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync programs details from server
        /// </summary>
        /// <param name="programData">object to get progarm data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramsFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAMS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(programData.RecordCount, CultureInfo.InvariantCulture)},
                        { Constants.SE_PROGRAM_ID_QUERY_KEY, Convert.ToString(programData.Program.ProgramID, CultureInfo.InvariantCulture)}
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        MapProgramsServiceResponse(programData, data);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Progrm details to server
        /// </summary>
        /// <param name="programData">Program data to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                programData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Progrm details to server
        /// </summary>
        /// <param name="programData">Program data to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncPublishProgramToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PUBLISH_PROGRAM_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(ProgramModel.ProgramID), Convert.ToString(programData.Program.ProgramID, CultureInfo.InvariantCulture)  },
                        { nameof(ProgramModel.IsPublished), Convert.ToString(programData.Program.IsPublished, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Progrm details to server
        /// </summary>
        /// <param name="programData">Program data to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncSubscribeProgramToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_SUBSCRIBE_PROGRAM_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(ProgramModel.ProgramID), Convert.ToString(programData.Program.ProgramID, CultureInfo.InvariantCulture)  },
                    }
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }


        /// <summary>
        /// Validates the assign after and show for date.
        /// </summary>
        /// <param name="programDuration">Duration of the program.</param>
        /// <param name="assignAfterDay">The assign after day.</param>
        /// <param name="showForAfterDay">The show for after day.</param>
        /// <returns>
        /// A tuple containing a boolean value indicating if the assign after day is valid, a boolean value indicating if the show for day is valid, and a string containing an error code.
        /// </returns>
        public (bool isValidAssignAfterDay, bool isValidShowForAfterDay, string errorCode) ValidateAssingAfterAndShowForDate(int programDuration, double assignAfterDay, double showForAfterDay, List<ResourceModel> resources)
        {
            bool isValidForAssignAfterDay = true;
            bool isValidShowForDay = true;
            string errorCode = string.Empty;
            SetPageResources(resources);
            if (programDuration > 0)
            {
                if (assignAfterDay > programDuration)
                {
                    isValidForAssignAfterDay = false;
                    errorCode += LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY) + " ";
                    errorCode += string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY), 0, programDuration) + " ";
                }
                if (showForAfterDay > programDuration)
                {
                    errorCode += LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SHOW_FOR_DAYS_KEY) + " ";
                    errorCode += string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY), 0, programDuration) + " ";
                    isValidShowForDay = false;
                }
                else if ((assignAfterDay + showForAfterDay) > programDuration)
                {
                    isValidShowForDay = false;
                    if (isValidForAssignAfterDay)
                    {
                        errorCode += LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SHOW_FOR_DAYS_KEY) + " ";
                        errorCode += string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY), 0, (programDuration - assignAfterDay)) + " ";
                    }
                    else
                    {
                        errorCode += LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_SHOW_FOR_DAYS_KEY) + " ";
                        errorCode += string.Format(CultureInfo.InvariantCulture, LibResources.GetResourceValueByKey(PageData?.Resources, ResourceConstants.R_RANGE_VALUE_VALIDATION_KEY), 0, programDuration) + " ";
                    }
                }
            }
            return (isValidForAssignAfterDay, isValidShowForDay, errorCode);
        }


        /// <summary>
        ///  Get program tasks from server
        /// </summary>
        /// <param name="programData">Reference object to hold program task data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Program tasks with operation status</returns>
        public async Task SyncProgramTaskFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_TASK_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(TaskModel.ProgramTaskID), Convert.ToString(programData.Task.ProgramTaskID, CultureInfo.InvariantCulture) },
                        { nameof(TaskModel.IsSynced),Convert.ToString(programData.Task.IsSynced, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.RecordCount), Convert.ToString(programData.RecordCount, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        MapTasksServiceResponse(data, programData);
                        if (programData.Task.ProgramTaskID > 0)
                        {
                            programData.Task = MapTask(data[nameof(ProgramDTO.Task)]);
                            var selectedItem = programData.Tasks.FirstOrDefault(x => x.TaskID == programData.Task.TaskID);
                            if (selectedItem != null)
                            {
                                programData.Task.SelectedItemName = selectedItem.SelectedItemName;
                                programData.Task.Name = selectedItem.Name;
                                programData.Task.TaskType = selectedItem.TaskType;
                                programData.Task.NoOfSubflows = selectedItem.NoOfSubflows;
                            }
                            else
                            {
                                programData.Tasks.Add(programData.Task);
                            }
                        }
                    }
                    SetPageResources(programData.Resources);
                    if (programData.Tasks != null)
                    {
                        programData.Items = GetPickerSource(programData.Tasks, nameof(TaskModel.TaskID), Constants.CNT_NAME_TEXT, programData.Task.TaskID, false, null);
                    }
                    else
                    {
                        programData.Items = new List<OptionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Sync program task to server
        /// </summary>
        /// <param name="programData">Reference object to hold program task data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramTaskToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_TASK_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.Task.ProgramTaskID = (long)data[nameof(ProgramDTO.Task)][nameof(TaskModel.ProgramTaskID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get subflows list that can be added for program
        /// </summary>
        /// <param name="programData">object to get readings data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>list of subflows</returns>
        public async Task SyncProgramSubflowFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_SUBFLOW_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(SubFlowModel.ProgramSubFlowID), Convert.ToString(programData.SubFlow.ProgramSubFlowID, CultureInfo.InvariantCulture) },
                        { nameof(SubFlowModel.ProgramID), Convert.ToString(programData.SubFlow.ProgramID, CultureInfo.InvariantCulture) },
                        { nameof(SubFlowModel.IsSynced),Convert.ToString(programData.SubFlow.IsSynced, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        MapSubflows(data, programData);
                        if (programData.SubFlow.ProgramSubFlowID > 0)
                        {
                            programData.SubFlow = MapSubFlow(data[nameof(ProgramDTO.SubFlow)]);
                        }
                        var selectedItem = programData.SubFlows.FirstOrDefault(x => x.SubFlowID == programData.SubFlow.SubFlowID);
                        if(selectedItem == null)
                        {
                            programData.SubFlows.Add(programData.SubFlow);
                        }
                        programData.ProgramSubFlows = GetPickerSource(programData.SubFlows, nameof(SubFlowModel.SubFlowID), Constants.CNT_NAME_TEXT, programData.SubFlow.SubFlowID, false, null);

                      //  programData.ProgramSubFlows = GetPickerSource(data, nameof(ProgramDTO.SubFlows), nameof(SubFlowModel.SubFlowID), Constants.CNT_NAME_TEXT, programData.SubFlow.SubFlowID, true, null);
                        programData.TaskTypes = MapResourcesIntoOptions(GroupConstants.RS_TASK_TYPE_GROUP, programData.SubFlow.TaskType, "1", true);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Sync program subflow to server
        /// </summary>
        /// <param name="programData">Reference object to hold program subflow data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramSubflowToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_SUBFLOW_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.SubFlow.ProgramSubFlowID = (long)data[nameof(ProgramDTO.SubFlow)][nameof(SubFlowModel.ProgramSubFlowID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get caregivers list that can be added for program
        /// </summary>
        /// <param name="programData">object to get caregiver data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramCaregiversFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_CAREGIVER_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        {nameof(CaregiverModel.ProgramCareGiverID),Convert.ToString(programData.ProgramCareGiver.ProgramCareGiverID, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        programData.ProgramCareGiver = data[nameof(ProgramDTO.ProgramCareGiver)].Any()
                            ? MapCaregiver(data[nameof(ProgramDTO.ProgramCareGiver)], string.Empty, string.Empty, string.Empty)
                            : new CaregiverModel();
                        programData.Items = GetPickerSource(data, nameof(ProgramDTO.Items), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), programData.ProgramCareGiver?.CareGiverID??-1, false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Save program caregiver
        /// </summary>
        /// <param name="programData">object to hold caregiver data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramCaregiverToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_CAREGIVER_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.ProgramCareGiver.ProgramCareGiverID = (long)data[nameof(ProgramDTO.ProgramCareGiver)][nameof(CaregiverModel.ProgramCareGiverID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get education list that can be added for program
        /// </summary>
        /// <param name="programData">object to get education data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramEducationFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_EDUCATION_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(PatientEducationModel.ProgramEducationID),Convert.ToString(programData.ProgramEducation.ProgramEducationID, CultureInfo.InvariantCulture) },
                        { nameof(PatientEducationModel.IsSynced),Convert.ToString(programData.ProgramEducation.IsSynced, CultureInfo.InvariantCulture) }
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        programData.ProgramEducation = data[nameof(ProgramDTO.ProgramEducation)].Any()
                            ? MapEducation(data[nameof(ProgramDTO.ProgramEducation)])
                            : new PatientEducationModel();
                        programData.Items = GetPickerSource(data, nameof(ProgramDTO.Items), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), programData.ProgramEducation.PageID, false, nameof(OptionModel.ParentOptionID));
                        programData.TaskTypes = GetPickerSource(data, nameof(ProgramDTO.TaskTypes), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), programData.ProgramEducation.PatientEducationID, false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private List<OptionModel> MapPatientMedicationUnits(JToken data, string collectionName)
        {
            return data[collectionName].Any()
               ? (from dataItem in data[collectionName]
                  select MapPatientMedicationUnit(dataItem)).ToList()
               : new List<OptionModel>();
        }

        private List<OptionModel> MapOptions(JToken data, string collectionName, long selectedID)
        {
            return GetPickerSource(data, collectionName, nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), selectedID, false, null, nameof(OptionModel.ParentOptionText));
        }

        private List<ProgramTrackerModel> MapProgramTracker(JToken data, string collectionName)
        {
            return data[collectionName].Any()
               ? (from dataItem in data[collectionName]
                  select MapProgramTracker(dataItem)).ToList()
               : new List<ProgramTrackerModel>();
        }
        private List<ProgramServiceModel> MapProgramService(JToken data, string collectionName)
        {
            return data[collectionName].Any()
               ? (from dataItem in data[collectionName]
                  select MapProgramServiceList(dataItem)).ToList()
               : new List<ProgramServiceModel>();
        }

        private List<PatientBillModel> MapProgramBillingItem(JToken data, string collectionName)
        {
            return data[collectionName].Any()
               ? (from dataItem in data[collectionName]
                  select MapProgramBillingItem(dataItem)).ToList()
               : new List<PatientBillModel>();
        }

        private List<ReasonModel> MapProgramReasons(JToken data, string collectionName)
        {
            return data[collectionName].Any()
               ? (from dataItem in data[collectionName]
                  select new ReasonService(_essentials).MapReasons(dataItem)
                  ).ToList()
               : new List<ReasonModel>();
        }

        private List<ProgramConfigurationModel> MapProgramConfigurations(JToken data, string collectionName)
        {
            return data[collectionName].Any()
              ? (from dataItem in data[collectionName]
                 select MapProgramConfigurations(dataItem)).ToList()
              : new List<ProgramConfigurationModel>();
        }

        private ProgramConfigurationModel MapProgramConfigurations(JToken dataItem)
        {
            return new ProgramConfigurationModel
            {
                ProgramReasonConfigurationID = GetDataItem<long>(dataItem, nameof(ProgramConfigurationModel.ProgramReasonConfigurationID)),
                ProgramID = GetDataItem<long>(dataItem, nameof(ProgramConfigurationModel.ProgramID)),
                FeatureID = GetDataItem<long>(dataItem, nameof(ProgramConfigurationModel.FeatureID)),
                FeatureCode = GetDataItem<string>(dataItem, nameof(ProgramConfigurationModel.FeatureCode)),
                FeatureText = GetDataItem<string>(dataItem, nameof(ProgramConfigurationModel.FeatureText)),
                IsReasonRequired = GetDataItem<bool>(dataItem, nameof(ProgramConfigurationModel.IsReasonRequired)),
                IsSignatureRequired = GetDataItem<bool>(dataItem, nameof(ProgramConfigurationModel.IsSignatureRequired))
            };
        }
        private PatientBillModel MapProgramBillingItem(JToken dataItem)
        {
            return new PatientBillModel
            {
                ProgramBillingItemID = GetDataItem<long>(dataItem, nameof(PatientBillModel.ProgramBillingItemID)),
                ProgramID = GetDataItem<long>(dataItem, nameof(PatientBillModel.ProgramID)),
                BillingItemID = GetDataItem<short>(dataItem, nameof(PatientBillModel.BillingItemID)),
                Amount = GetDataItem<double>(dataItem, nameof(PatientBillModel.Amount)),
                Item = GetDataItem<string>(dataItem, nameof(PatientBillModel.Item)),
                IsActive = GetDataItem<bool>(dataItem, nameof(PatientBillModel.IsActive))
            };
        }

        private ProgramTrackerModel MapProgramTracker(JToken dataItem)
        {
            return new ProgramTrackerModel
            {
                ProgramTrackerID = GetDataItem<long>(dataItem, nameof(ProgramTrackerModel.ProgramTrackerID)),
                ProgramID = GetDataItem<long>(dataItem, nameof(ProgramTrackerModel.ProgramID)),
                TrackerID = GetDataItem<short>(dataItem, nameof(ProgramTrackerModel.TrackerID)),
                AssignAfterDays = GetDataItem<int>(dataItem, nameof(ProgramTrackerModel.AssignAfterDays)),
                AssignForDays = GetDataItem<int>(dataItem, nameof(ProgramTrackerModel.AssignForDays)),
                TrackerName = GetDataItem<string>(dataItem, nameof(ProgramTrackerModel.TrackerName)),
                IsActive = GetDataItem<bool>(dataItem, nameof(ProgramTrackerModel.IsActive))
            };
        }

        private OptionModel MapPatientMedicationUnit(JToken dataItem)
        {
            return new OptionModel
            {
                ParentOptionID = GetDataItem<long>(dataItem, nameof(OptionModel.ParentOptionID)),
                OptionID = GetDataItem<long>(dataItem, nameof(OptionModel.OptionID)),
                GroupName = GetDataItem<string>(dataItem, nameof(OptionModel.GroupName)),
                OptionText = GetDataItem<string>(dataItem, nameof(OptionModel.OptionText)),
                ParentOptionText = GetDataItem<string>(dataItem, nameof(OptionModel.ParentOptionText)),
            };
        }



        /// <summary>
        /// Save program education
        /// </summary>
        /// <param name="programData">object to hold education data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramEducationToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_EDUCATION_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.ProgramEducation.ProgramEducationID = (long)data[nameof(ProgramDTO.ProgramEducation)][nameof(PatientEducationModel.ProgramEducationID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get program tasks from server
        /// </summary>
        /// <param name="programData">Reference object to hold program task data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Program tasks with operation status</returns>
        public async Task SyncTaskItemsFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_ITEMS_BASED_ON_TASK_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(TaskModel.TaskType), programData.Program.Name },
                        { nameof(ProgramModel.ProgramID), Convert.ToString(programData.Program.ProgramID, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.SelectedUserID), Convert.ToString(programData.SelectedUserID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.Items = GetPickerSource(data, nameof(ProgramDTO.Items), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), programData.RecordCount, false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Get instruction(s) from server
        /// </summary>
        /// <param name="programData">Reference object to hold instruction(s) data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Program instruction with operation status</returns>
        public async Task SyncInstructionsFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_INSTRUCTIONS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(InstructionModel.InstructionID), Convert.ToString(programData.Instruction.InstructionID, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.RecordCount), Convert.ToString(programData.RecordCount, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        MapInstructionsServiceResponse(data, programData);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Sync instruction to server
        /// </summary>
        /// <param name="programData">Reference object to hold program instruction data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncInstructionsToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_INSTRUCTIONS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                programData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapSubFlowsServiceResponse(JToken data, ProgramDTO programData)
        {
            SetPageResources(programData.Resources);
            if (programData.RecordCount == -1)
            {
                if (programData.SubFlow.SubFlowID > 0 && data[nameof(ProgramDTO.SubFlow)].Any())
                {
                    programData.SubFlow = MapSubFlow(data[nameof(ProgramDTO.SubFlow)]);
                }
                programData.ProgramSubFlows = GetPickerSource(data, nameof(ProgramDTO.ProgramSubFlows), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), programData.SubFlow.ItemID, false, null);
                programData.TaskTypes = MapResourcesIntoOptions(GroupConstants.RS_TASK_TYPE_GROUP, programData.SubFlow.TaskType, string.Empty, false);
                programData.OperationTypes = MapResourcesIntoOptions(GroupConstants.RS_OPERATION_TYPE_GROUP, programData.SubFlow.OperationType, string.Empty, false);
                programData.OperationTypes.AddRange(MapResourcesIntoOptions(GroupConstants.RS_TASK_STATUS_GROUP, programData.SubFlow.OperationType, "1", false));
                programData.Items = GetPickerSource(data, nameof(ProgramDTO.Items), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText),
                    programData.SubFlow.TaskType == ResourceConstants.R_EMAIL_KEY || programData.SubFlow.TaskType == ResourceConstants.R_SMS_KEY || programData.SubFlow.TaskType == ResourceConstants.R_NOTIFICATION_KEY ? programData.SubFlow.TemplateID : programData.SubFlow.ItemID,
                    false, null
                );
                MapLanguageDetails(data, programData);
            }
            else
            {
                MapSubflows(data, programData);
            }
        }

        private void MapTasksServiceResponse(JToken data, ProgramDTO programData)
        {
            SetPageResources(programData.Resources);
            MapTasks(data, programData);
            if (programData.RecordCount == -1)
            {
                MapLanguageDetails(data, programData);
                programData.Items = GetPickerSource(data, nameof(ProgramDTO.Items), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), programData.Tasks?.FirstOrDefault().ItemID ?? 0, false, null);
                MapSubflows(data, programData);
                if (programData.Tasks != null)
                {
                    programData.Task = programData.Tasks.FirstOrDefault();
                    programData.Tasks.Clear();
                }
                programData.TaskTypes = MapResourcesIntoOptions(GroupConstants.RS_TASK_TYPE_GROUP, programData.Task.TaskType, "1", false);
            }
        }

        private void MapProgramsServiceResponse(ProgramDTO programData, JToken data)
        {
            if (programData.RecordCount >= 0)
            {
                programData.Programs = MapPrograms(data, nameof(ProgramDTO.Programs));
            }
            else if (programData.RecordCount == -2)
            {
                programData.Items = data[nameof(ProgramDTO.Items)].Any()
                    ? (from dataItem in data[nameof(ProgramDTO.Items)]
                       select new OptionModel
                       {
                           OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                           OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                           ParentOptionText = (string)dataItem[nameof(OptionModel.ParentOptionText)],
                       }).ToList()
                    : new List<OptionModel>();
            }
            else
            {
                var programDetail = data[nameof(ProgramDTO.Program)];
                if (programDetail.HasValues)
                {
                    programData.Program = MapProgram(programDetail);
                }
                long selectedID = 0;
                List<ResourceModel> dataSource = programData.Resources.Where(x => x.GroupName == GroupConstants.RS_CODE_SYSTEM_GROUP).OrderBy(x => x.ResourceValue).ToList();
                if (programData.Program?.ProgramID > 0)
                {
                    selectedID = dataSource.FirstOrDefault(x => x.ResourceKeyID == programData.Program.CodeSystemID)?.ResourceKeyID ?? 0;
                }
                programData.OrganisationID = (long)data[nameof(BaseDTO.OrganisationID)];
                programData.SupportedCodes = GetPickerSource(dataSource, nameof(ResourceModel.ResourceKeyID), Constants.CNT_RESOURCE_VALUE_TEXT, selectedID, false, null);
                List<ResourceModel> ProgramTypeDataSource = programData.Resources.Where(x => x.GroupName == GroupConstants.RS_PROGRAM_TYPE_GROUP).OrderBy(x => x.ResourceValue).ToList();
                long selectedProgramTypeID = ProgramTypeDataSource.FirstOrDefault(x => x.ResourceKeyID == programData.Program?.ProgramTypeID)?.ResourceKeyID ?? 0;
                programData.ProgramTypes = GetPickerSource(ProgramTypeDataSource, nameof(ResourceModel.ResourceKeyID), Constants.CNT_RESOURCE_VALUE_TEXT, selectedProgramTypeID, false, null);
                SetResourcesAndSettings(programData);
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                MapLanguageDetails(data, programData);
                MapTasks(data, programData);
                MapSubflows(data, programData);
                MapProgramNotes(data, programData);
                programData.ProgramMedications = new MedicationSevice(_essentials).MapMedications(data, nameof(ProgramDTO.ProgramMedications));
                programData.ProgramCareGivers = data[nameof(ProgramDTO.ProgramCareGivers)].Any()
                    ? (from dataItem in data[nameof(ProgramDTO.ProgramCareGivers)]
                       select MapCaregiver(dataItem, dayFormat, monthFormat, yearFormat)).ToList()
                       : null;
                programData.ProgramReadings = new ReadingService(_essentials).MapReadings(data, nameof(ProgramDTO.ProgramReadings));
                if (GenericMethods.IsListNotEmpty(programData.ProgramReadings))
                {
                    foreach (var reading in programData.ProgramReadings)
                    {
                        reading.ReadingCategory = LibResources.GetResourceValueByKeyID(PageData?.Resources, reading.ReadingCategoryID);
                        reading.Reading = LibResources.GetResourceValueByKeyID(PageData?.Resources, reading.ReadingID);
                    }
                }
                programData.UnitOptions = MapPatientMedicationUnits(data, nameof(PatientMedicationDTO.UnitOptions));
                programData.ProgramEducations = data[nameof(ProgramDTO.ProgramEducations)].Any()
                   ? (from dataItem in data[nameof(ProgramDTO.ProgramEducations)]
                      select MapEducation(dataItem)).ToList()
                      : null;
                programData.ProgramTrackers = MapProgramTracker(data, nameof(ProgramDTO.ProgramTrackers));
                if (GenericMethods.IsListNotEmpty(programData.ProgramMedications))
                {
                    MapMedicationUnitWithDoses(programData);
                }
                programData.ProgramBillingItems = MapProgramBillingItem(data, nameof(ProgramDTO.ProgramBillingItems));
                programData.Reasons = MapProgramReasons(data, nameof(ProgramDTO.Reasons));
                programData.ProgramConfigurations = MapProgramConfigurations(data, nameof(ProgramDTO.ProgramConfigurations));
                programData.ProgramServices = MapProgramService(data, nameof(ProgramDTO.ProgramServices));
            }
        }

        /// <summary>
        /// Map programs data
        /// </summary>
        /// <param name="data">programs json data</param>
        /// <param name="collectionName">collection name</param>
        /// <returns>Program list</returns>
        internal List<ProgramModel> MapPrograms(JToken data, string collectionName)
        {
            return data[collectionName].Any()
                ? (from dataItem in data[collectionName]
                   select MapProgram(dataItem)).ToList()
                : null;
        }

        /// <summary>
        /// Map program data
        /// </summary>
        /// <param name="dataItem">program json object</param>
        /// <returns>Program data</returns>
        private ProgramModel MapProgram(JToken dataItem)
        {
            return new ProgramModel
            {
                ProgramID = GetDataItem<long>(dataItem, nameof(ProgramModel.ProgramID)),
                PatientID = GetDataItem<long>(dataItem, nameof(ProgramModel.PatientID)),
                Name = GetDataItem<string>(dataItem, nameof(ProgramModel.Name)),
                AddedOnString = GetSubscribedValue(dataItem),
                ProgramGroupIdentifier = GetDataItem<string>(dataItem, nameof(ProgramModel.ProgramGroupIdentifier)),
                AllowSelfRegistration = GetDataItem<bool>(dataItem, nameof(ProgramModel.AllowSelfRegistration)),
                AllowProviderToScan = GetDataItem<bool>(dataItem, nameof(ProgramModel.AllowProviderToScan)),
                AllowPatientToScan = GetDataItem<bool>(dataItem, nameof(ProgramModel.AllowPatientToScan)),
                AllowPatientToBuyCredits = GetDataItem<bool>(dataItem, nameof(ProgramModel.AllowPatientToBuyCredits)),
                AllowProgramToBuyCredits = GetDataItem<bool>(dataItem, nameof(ProgramModel.AllowProgramToBuyCredits)),
                CodeSystemID = GetDataItem<long>(dataItem, nameof(ProgramModel.CodeSystemID)),
                ProgramTypeID = GetDataItem<short>(dataItem, nameof(ProgramModel.ProgramTypeID)),
                ProgramDuration = GetDataItem<int>(dataItem, nameof(ProgramModel.ProgramDuration)),
                IsPublished = GetDataItem<bool>(dataItem, nameof(ProgramModel.IsPublished)),
                IsActive = GetDataItem<bool>(dataItem, nameof(ProgramModel.IsActive)),
                IsSynced = GetDataItem<bool>(dataItem, nameof(ProgramModel.IsSynced)),
                NoOfTasks = GetDataItem<int>(dataItem, nameof(ProgramModel.NoOfTasks)),
                NoOfSubflows = GetDataItem<int>(dataItem, nameof(ProgramModel.NoOfSubflows)),
                NoOfDefaultCareGivers = GetDataItem<int>(dataItem, nameof(ProgramModel.NoOfDefaultCareGivers)),
                NoOfReadings = GetDataItem<int>(dataItem, nameof(ProgramModel.NoOfReadings)),
                NoOfEducations = GetDataItem<int>(dataItem, nameof(ProgramModel.NoOfEducations)),
                NoOfMedications = GetDataItem<int>(dataItem, nameof(ProgramModel.NoOfMedications))
            };
        }

        private string GetSubscribedValue(JToken dataItem)
        {
            string subscribedValue = GetDataItem<string>(dataItem, nameof(ProgramModel.AddedOnString));
            return string.IsNullOrEmpty(subscribedValue) ? string.Empty : $"<span class= 'badge-done-fill truncate'>{subscribedValue}</span>";
        }

        private CaregiverModel MapCaregiver(JToken dataItem, string dayFormat, string monthFormat, string yearFormat)
        {
            var caregiverData = new UserService(_essentials).MapProgramCaregiver(dataItem);
            if (!string.IsNullOrWhiteSpace(dayFormat) && !string.IsNullOrWhiteSpace(monthFormat) && !string.IsNullOrWhiteSpace(yearFormat))
            {
                caregiverData.FromDateValue = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(caregiverData.FromDate.Value),
                    DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                caregiverData.ToDateValue = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(caregiverData.ToDate.Value),
                    DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            }
            return caregiverData;
        }

        private void MapLanguageDetails(JToken data, ProgramDTO programData)
        {
            programData.LanguageDetails = data[nameof(ProgramDTO.LanguageDetails)].Any()
                ? (from dataItem in data[nameof(ProgramDTO.LanguageDetails)]
                   select new ProgramDetails
                   {
                       Name = (string)dataItem[nameof(ProgramDetails.Name)],
                       Description = (string)dataItem[nameof(ProgramDetails.Description)],
                       LanguageID = (byte)dataItem[nameof(ProgramDetails.LanguageID)],
                       LanguageName = (string)dataItem[nameof(ProgramDetails.LanguageName)],
                   }).ToList()
                : null;
        }

        private void MapTasks(JToken data, ProgramDTO programData)
        {
            programData.Tasks = data[nameof(ProgramDTO.Tasks)].Any()
                ? (from dataItem in data[nameof(ProgramDTO.Tasks)]
                   select MapTask(dataItem)).ToList()
                : null;
        }

        private TaskModel MapTask(JToken dataItem)
        {
            return new TaskModel
            {
                TaskID = (long)dataItem[nameof(TaskModel.TaskID)],
                ItemID = (long)dataItem[nameof(TaskModel.ItemID)],
                ProgramID = GetDataItem<long>(dataItem, nameof(TaskModel.ProgramID)),
                SelectedItemName = GetDataItem<string>(dataItem, nameof(TaskModel.SelectedItemName)),
                NoOfSubflows = (int)dataItem[nameof(TaskModel.NoOfSubflows)],
                ProgramTaskID = (long)dataItem[nameof(TaskModel.ProgramTaskID)],
                Name = GetDataItem<string>(dataItem, nameof(TaskModel.Name)),
                TaskType = GetDataItem<string>(dataItem, nameof(TaskModel.TaskType)),
                AssignAfterDays = (short)dataItem[nameof(TaskModel.AssignAfterDays)],
                AssignForDays = (short)dataItem[nameof(TaskModel.AssignForDays)],
                ExecuteOnLogin = (bool)dataItem[nameof(TaskModel.ExecuteOnLogin)],
                IsActive = GetDataItem<bool>(dataItem, nameof(TaskModel.IsActive))
            };
        }

        private void MapSubflows(JToken data, ProgramDTO programData)
        {
            programData.SubFlows = data[nameof(ProgramDTO.SubFlows)].Any()
                                    ? (from dataItem in data[nameof(ProgramDTO.SubFlows)]
                                       select MapSubFlow(dataItem)).ToList()
                                    : null;
        }

        private SubFlowModel MapSubFlow(JToken dataItem)
        {
            return new SubFlowModel
            {
                ProgramSubFlowID = GetDataItem<long>(dataItem, nameof(SubFlowModel.ProgramSubFlowID)),
                TaskSubFlowID = GetDataItem<long>(dataItem, nameof(SubFlowModel.TaskSubFlowID)),
                SubFlowID = (long)dataItem[nameof(SubFlowModel.SubFlowID)],
                ProgramID = GetDataItem<long>(dataItem, nameof(SubFlowModel.ProgramID)),
                TaskID = GetDataItem<long>(dataItem, nameof(SubFlowModel.TaskID)),
                Name = (string)dataItem[nameof(SubFlowModel.Name)],
                TaskType = (string)dataItem[nameof(SubFlowModel.TaskType)],
                Item = (string)dataItem[nameof(SubFlowModel.Item)],
                ItemID = (long)dataItem[nameof(SubFlowModel.ItemID)],
                OperationType = (string)dataItem[nameof(SubFlowModel.OperationType)],
                TemplateID = GetDataItem<short>(dataItem, nameof(SubFlowModel.TemplateID)),
                Value1 = GetDataItem<int>(dataItem, nameof(SubFlowModel.Value1)),
                Value2 = GetDataItem<int>(dataItem, nameof(SubFlowModel.Value2)),
                AssignAfterDays = GetDataItem<short>(dataItem, nameof(SubFlowModel.AssignAfterDays)),
                AssignForDays = GetDataItem<short>(dataItem, nameof(SubFlowModel.AssignForDays)),
                IsActive = GetDataItem<bool>(dataItem, nameof(SubFlowModel.IsActive))
            };
        }

        private PatientEducationModel MapEducation(JToken dataItem)
        {
            var educationData = new PatientEducationModel
            {
                PatientEducationID = (long)dataItem[nameof(PatientEducationModel.PatientEducationID)],
                ProgramEducationID = (long)dataItem[nameof(PatientEducationModel.ProgramEducationID)],
                PageID = (long)dataItem[nameof(PatientEducationModel.PageID)],
                ProgramID = GetDataItem<long>(dataItem, nameof(PatientEducationModel.ProgramID)),
                AssignAfterDays = GetDataItem<short>(dataItem, nameof(PatientEducationModel.AssignAfterDays)),
                AssignForDays = GetDataItem<short>(dataItem, nameof(PatientEducationModel.AssignForDays)),
                PageHeading = GetDataItem<string>(dataItem, nameof(PatientEducationModel.PageHeading)),
                ForProviders = GetDataItem<bool>(dataItem, nameof(PatientEducationModel.ForProviders)),
                IsActive = GetDataItem<bool>(dataItem, nameof(PatientEducationModel.IsActive))
            };
            return educationData;
        }

        /// <summary>
        /// Map medication unit with doses
        /// </summary>
        /// <param name="programData">Program medications data</param>
        public void MapMedicationUnitWithDoses(ProgramDTO programData)
        {
            foreach (var medication in programData.ProgramMedications)
            {
                medication.MedicationDosesString = string.Concat(CultureInfo.InvariantCulture,
                    $"{medication.Doses} {medication.ShortUnitName}");
            }
        }

        private void MapInstructionsServiceResponse(JToken data, ProgramDTO programData)
        {
            if (programData.RecordCount == -1)
            {
                MapLanguageDetails(data, programData);
            }
            else
            {
                programData.Instructions = data[nameof(ProgramDTO.Instructions)].Any()
                   ? (from dataItem in data[nameof(ProgramDTO.Instructions)]
                      select new InstructionModel
                      {
                          InstructionID = (long)dataItem[nameof(InstructionModel.InstructionID)],
                          Name = (string)dataItem[nameof(InstructionModel.Name)],
                          IsActive = GetDataItem<bool>(dataItem, nameof(InstructionModel.IsActive))
                      }).ToList()
                   : null;
            }
        }

        ///// <summary>
        ///// Get madicine based on searched text
        ///// </summary>
        ///// <param name="medicationData">Reference object to return medicine data</param>
        ///// <param name="searchText">test to search medicine</param>
        ///// <returns>Operation status with searched medicines in reference object</returns>
        //public async Task SearchMedicineAsync(PatientMedicationDTO medicationData, string searchText)
        //{
        //    try
        //    {
        //        if (MobileConstants.IsMobilePlatform)
        //        {
        //            /*await new PatientMedicationDatabase().GetMedicinesAsync(medicationData, searchText).ConfigureAwait(false);
        //            medicationData.Medicines = medicationData.Medicines ?? new List<MedicineModel>();
        //            medicationData.Medicines.AddRange(MedicineData.Medicines.Where(x => (x.FullName.ToLowerInvariant().Contains(searchText.ToLowerInvariant()) || x.ShortName.ToLowerInvariant().Contains(searchText.ToLowerInvariant())) && x.IsActive && !medicationData.Medicines.Any(m => m.ShortName == x.ShortName)));*/
        //        }
        //        else
        //        {
        //            await SyncPatientMedicinesFromServerAsync(medicationData, CancellationToken.None, searchText).ConfigureAwait(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex.Message, ex);
        //        medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        //    }
        //}

        //private async Task SyncPatientMedicinesFromServerAsync(PatientMedicationDTO medicationData, CancellationToken cancellationToken, string searchText)
        //{
        //    try
        //    {
        //        var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
        //        {
        //            CancellationToken = cancellationToken,
        //            PathWithoutBasePath = UrlConstants.GET_MEDICINES_ASYNC_PATH,
        //            QueryParameters = new NameValueCollection
        //            {
        //                { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
        //                { nameof(PatientMedicationModel.ShortName), searchText },
        //            }
        //        };
        //        await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
        //        medicationData.ErrCode = httpData.ErrCode;
        //        if (medicationData.ErrCode == ErrorCode.OK)
        //        {
        //            JToken data = JToken.Parse(httpData.Response);
        //            if (data != null && data.HasValues)
        //            {
        //                medicationData.Medicines = MapPatientMedicines(data, nameof(PatientMedicationDTO.Medicines));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex.Message, ex);
        //        medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        //    }
        //}

        /// <summary>
        /// Sync Program Billing Items To server 
        /// </summary>
        /// <param name="programData">Object To get data</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Program Billing Items Data with Operation status</returns>
        public async Task SyncProgramBillingItemsFromServer(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_BILLING_ITEM_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(PatientBillModel.ProgramBillingItemID),Convert.ToString(programData.ProgramBillItem.ProgramBillingItemID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        programData.ProgramBillItem = data[nameof(ProgramDTO.ProgramBillItem)].Any()
                            ? MapProgramBillingItem(data[nameof(ProgramDTO.ProgramBillItem)])
                            : new PatientBillModel();
                        programData.BillingItemOptionList = MapOptions(data, nameof(ProgramDTO.BillingItemOptionList), programData.ProgramBillItem?.BillingItemID ?? - 1);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Saves Program Billing Item To server
        /// </summary>
        /// <param name="programData">Object To pass Billing Item data to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramBillingItemToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_BILLING_ITEM_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData,
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                programData.Response = httpData.Response;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.ProgramBillItem.ProgramBillingItemID = (long)data[nameof(ProgramDTO.ProgramBillItem)][nameof(PatientBillModel.ProgramBillingItemID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }


        //private List<MedicineModel> MapPatientMedicines(JToken data, string collectionName)
        //{
        //    return data[collectionName].Any()
        //       ? (from dataItem in data[collectionName]
        //          select MapPatientMedicine(dataItem)).ToList()
        //       : new List<MedicineModel>();
        //}

        //private MedicineModel MapPatientMedicine(JToken dataItem)
        //{
        //    return new MedicineModel
        //    {
        //        UnitIdentifier = GetDataItem<string>(dataItem, nameof(MedicineModel.UnitIdentifier)),
        //        ShortName = GetDataItem<string>(dataItem, nameof(MedicineModel.ShortName)),
        //        FullName = GetDataItem<string>(dataItem, nameof(MedicineModel.FullName)),
        //        IsActive = GetDataItem<bool>(dataItem, nameof(MedicineModel.IsActive))

        //    };
        //}

        /// <summary>
        /// Get ProgramTracker Data 
        /// </summary>
        /// <param name="programData">Reference object contains data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation Status</returns>
        public async Task SyncProgramTrackersFromServer(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_TRACKER_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                        { nameof(ProgramTrackerModel.ProgramTrackerID),Convert.ToString(programData.ProgramTracker.ProgramTrackerID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        programData.ProgramTracker = data[nameof(ProgramDTO.ProgramTracker)].Any()
                            ? MapTrackerList(data[nameof(ProgramDTO.ProgramTracker)])
                            : new ProgramTrackerModel();
                        programData.TrackerTypes = MapOptions(data, nameof(ProgramDTO.TrackerTypes), programData.ProgramTracker?.TrackerID ?? -1);
                        programData.ValueAddedByType = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_USER_TYPE_GROUP, "", false, programData.ProgramTracker?.ValueAddedBy??-1);
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private ProgramTrackerModel MapTrackerList(JToken dataItem)
        {
            var trackerData = new ProgramTrackerModel
            {
                ProgramTrackerID = (long)dataItem[nameof(ProgramTrackerModel.ProgramTrackerID)],
                TrackerID = (short)dataItem[nameof(ProgramTrackerModel.TrackerID)],
                TrackerName = (string)dataItem[nameof(ProgramTrackerModel.TrackerName)],
                ValueAddedBy = GetDataItem<short>(dataItem, nameof(ProgramTrackerModel.ValueAddedBy)),
                ProgramID = GetDataItem<long>(dataItem, nameof(PatientEducationModel.ProgramID)),
                AssignAfterDays = GetDataItem<short>(dataItem, nameof(PatientEducationModel.AssignAfterDays)),
                AssignForDays = GetDataItem<short>(dataItem, nameof(PatientEducationModel.AssignForDays)),
                IsActive = GetDataItem<bool>(dataItem, nameof(PatientEducationModel.IsActive))
            };
            return trackerData;
        }

        /// <summary>
        /// Sync ProgramTracker to server
        /// </summary>
        /// <param name="programData">Reference object to hold program tracker data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramTrackerToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.Save_PROGRAM_TRACKER_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData,
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                programData.Response = httpData.Response;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.ProgramTracker.ProgramTrackerID = (long)data[nameof(ProgramDTO.ProgramTracker)][nameof(ProgramTrackerModel.ProgramTrackerID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapProgramNotes(JToken data, ProgramDTO programData)
        {
            programData.ProgramNotes = data[nameof(ProgramDTO.ProgramNotes)].Any()
                ? (from dataItem in data[nameof(ProgramDTO.ProgramNotes)]
                   select MapProgramNote(dataItem)).ToList()
                : null;
        }

        private ProgramNoteModel MapProgramNote(JToken dataItem)
        {
            return new ProgramNoteModel
            {
                ProgramNoteID = (long)dataItem[nameof(ProgramNoteModel.ProgramNoteID)],
                NoteText = (string)dataItem[nameof(ProgramNoteModel.NoteText)],
                NoteDescription = (string)dataItem[nameof(ProgramNoteModel.NoteDescription)],
                Questionnaire = (string)dataItem[nameof(ProgramNoteModel.Questionnaire)],
                QuestionnaireID = (long)dataItem[nameof(ProgramNoteModel.QuestionnaireID)],
                LanguageID = (byte)dataItem[nameof(ProgramNoteModel.LanguageID)],
                LanguageName = (string)dataItem[nameof(ProgramNoteModel.LanguageName)],
                IsActive = (bool)dataItem[nameof(ProgramNoteModel.IsActive)],
                OrganisationID = (long)dataItem[nameof(ProgramNoteModel.OrganisationID)],
            };
        }

        /// <summary>
        /// Get note(s)
        /// </summary>
        /// <param name="programData">object to get program note data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>program note(s)</returns>
        public async Task SyncProgramNotesFromServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_NOTES_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        {
                            Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()
                        },
                        {
                            nameof(ProgramNoteModel.ProgramNoteID),Convert.ToString(programData.ProgramNote.ProgramNoteID, CultureInfo.InvariantCulture)
                        },
                        {
                            nameof(ProgramNoteModel.ProgramID),Convert.ToString(programData.ProgramNote.ProgramID, CultureInfo.InvariantCulture)
                        }
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        MapProgramNotes(data, programData);
                        programData.Items = (data[nameof(ProgramDTO.Items)]?.Count() > 0) ?
                            (from dataItem1 in data[nameof(ProgramDTO.Items)]
                             select new OptionModel
                             {
                                 OptionID = (long)dataItem1[nameof(OptionModel.OptionID)],
                                 OptionText = (string)dataItem1[nameof(OptionModel.OptionText)],
                                 IsSelected = (bool)dataItem1[nameof(OptionModel.IsSelected)],
                                 GroupName = (string)dataItem1[nameof(OptionModel.GroupName)]
                             }).ToList()
                             : new List<OptionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Save program note
        /// </summary>
        /// <param name="programData">object to hold program note data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramNoteToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_NOTE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                    },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.ProgramNote.ProgramNoteID = (long)data[nameof(ProgramDTO.ProgramNote)][nameof(ProgramNoteModel.ProgramNoteID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Save program services
        /// </summary>
        /// <param name="programData">object to hold program note data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramServiceToServerAsync(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_SERVICE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
               {
                   { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
               },
                    ContentToSend = programData
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        programData.ProgramService.ProgramExternalServiceID = (long)data[nameof(ProgramDTO.ProgramService)][nameof(ProgramServiceModel.ProgramExternalServiceID)];
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get Program Services
        /// </summary>
        /// <param name="programData">object to hold program note data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status</returns>
        public async Task SyncProgramServicesFromServer(ProgramDTO programData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<ProgramDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_PROGRAM_SERVICES_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
               {
                   { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID()},
                   { nameof(ProgramServiceModel.ProgramExternalServiceID),Convert.ToString(programData.ProgramService.ProgramExternalServiceID, CultureInfo.InvariantCulture) },
                   { nameof(ProgramServiceModel.ProgramID),Convert.ToString(programData.ProgramService.ProgramID, CultureInfo.InvariantCulture) },
               }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                programData.ErrCode = httpData.ErrCode;
                if (programData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(programData, data);
                        SetPageResources(programData.Resources);
                        programData.ProgramService = data[nameof(ProgramDTO.ProgramService)].Any()
                            ? MapProgramServiceList(data[nameof(ProgramDTO.ProgramService)])
                            : new ProgramServiceModel();
                        programData.Items = MapOptions(data, nameof(ProgramDTO.Items), (long)(programData.ProgramService?.ExternalServiceID == 0 ? -1 : programData.ProgramService?.ExternalServiceID));
                    }
                }
            }
            catch (Exception ex)
            {
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        private ProgramServiceModel MapProgramServiceList(JToken dataItem)
        {
            var serviceData = new ProgramServiceModel
            {
                ProgramExternalServiceID = (long)dataItem[nameof(ProgramServiceModel.ProgramExternalServiceID)],
                ServiceName = GetDataItem<string>(dataItem, nameof(ProgramServiceModel.ServiceName)),
                ExternalServiceID = (short)dataItem[nameof(ProgramServiceModel.ExternalServiceID)],
                Quantity = (int)dataItem[nameof(ProgramServiceModel.Quantity)],
                ProgramID = GetDataItem<long>(dataItem, nameof(ProgramServiceModel.ProgramID)),
                AssignAfterDays = GetDataItem<short>(dataItem, nameof(ProgramServiceModel.AssignAfterDays)),
                AssignForDays = GetDataItem<short>(dataItem, nameof(ProgramServiceModel.AssignForDays)),
                IsActive = GetDataItem<bool>(dataItem, nameof(ProgramServiceModel.IsActive))
            };
            return serviceData;
        }
    }
}