using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ProgramServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Program service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ProgramServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Program Subflow(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="subFlowID">SubFlow ID of which data has to be fetched</param>
        /// <returns>Program Subflow Data with operation status</returns>
        public async Task<ProgramDTO> GetSubFlowsAsync(byte languageID, long permissionAtLevelID, long recordCount, long subFlowID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, recordCount, programData).ConfigureAwait(false))
                {
                    programData.SubFlow = new SubFlowModel { SubFlowID = subFlowID };
                    await new ProgramServiceDataLayer().GetSubFlowsAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Subflow Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Subflow Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task<ProgramDTO> SaveSubFlowAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || programData.SubFlow == null || AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}, {GroupConstants.RS_TASK_TYPE_GROUP}").ConfigureAwait(false))
                    {
                        if (programData.SubFlow.OperationType == ResourceConstants.R_BETWEEN_KEY && programData.SubFlow.Value2 <= programData.SubFlow.Value1)
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                        if(LibResources.IsRequired(programData.Resources, ResourceConstants.R_SHOW_FOR_DAYS_KEY))
                        {
                            if (programData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SHOW_FOR_DAYS_KEY) != null)
                            {
                                programData.SubFlow.AssignForDays = (short)(programData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_SHOW_FOR_DAYS_KEY).MinLength);
                            }
                        }
                        if (!await ValidateDataAsync(programData.SubFlow, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                        else if (!await ValidateDataAsync(programData.LanguageDetails, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.LanguageID = languageID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveSubFlowAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get Program Task(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="taskID">Task ID of which data has to be fetched</param>
        /// <returns>Program Task Data with operation status</returns>
        public async Task<ProgramDTO> GetTasksAsync(byte languageID, long permissionAtLevelID, long recordCount, long taskID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, recordCount, programData).ConfigureAwait(false))
                {
                    programData.Task = new TaskModel { TaskID = taskID };
                    await new ProgramServiceDataLayer().GetTasksAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Task Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Task Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task<ProgramDTO> SaveTaskAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || languageID < 1 || programData == null || programData.Task == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.Task.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(programData.LanguageDetails))
                    {
                        programData.ErrCode = ErrorCode.InvalidData;
                        return programData;
                    }
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_TASK_TYPE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.Task, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                        else if (!await ValidateDataAsync(programData.LanguageDetails, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.LanguageID = languageID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveTaskAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get Tasks Subflow(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="taskSubFlowID">Task SubFlow ID of which data has to be fetched</param>
        /// <returns>Task Subflow Data with operation status</returns>
        public async Task<ProgramDTO> GetTaskSubFlowAsync(byte languageID, long permissionAtLevelID, long taskSubFlowID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.SubFlow = new SubFlowModel { TaskSubFlowID = taskSubFlowID };
                    await new ProgramServiceDataLayer().GetTaskSubFlowAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Tasks Subflow Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Task Subflow Data</param>
        /// <returns>Operation Status Code And TaskSubFlowID</returns>
        public async Task<ProgramDTO> SaveTaskSubFlowAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (ValidateTaskSubflow(languageID, permissionAtLevelID, programData))
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveTaskSubflowAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        private bool ValidateTaskSubflow(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            return permissionAtLevelID < 1 || languageID < 1 || programData?.SubFlow?.TaskID < 1 || (programData.SubFlow.IsActive && (programData.SubFlow.AssignAfterDays < 0 || programData.SubFlow.AssignForDays < 1 || programData.SubFlow.SubFlowID < 1));
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
        public async Task<ProgramDTO> GetProgramsAsync(byte languageID, long permissionAtLevelID, long organisationID, long recordCount, long programID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.OrganisationID = organisationID;
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                programData.Settings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP,
                    languageID, default, 0, organisationID, false).ConfigureAwait(false)).Settings;
                if (GenericMethods.IsListNotEmpty(programData.Settings)
                    && await UpdatePageDataAsync(languageID, permissionAtLevelID, recordCount, programData).ConfigureAwait(false))
                {
                    programData.Program = new ProgramModel { ProgramID = programID };
                    await new ProgramServiceDataLayer().GetProgramsAsync(programData).ConfigureAwait(false);
                    if (programData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(programData.ProgramCareGivers))
                    {
                        programData.ProgramCareGivers.ForEach(item =>
                        {
                            item.FullName = $"{item.FirstName} {item.LastName}";
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task<ProgramDTO> SaveProgramAsync(long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || programData.Program == null || AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(programData.LanguageDetails))
                    {
                        programData.ErrCode = ErrorCode.InvalidData;
                        return programData;
                    }
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_PROGRAM_TYPE_GROUP}").ConfigureAwait(false))
                    {
                        if (programData.Program.ProgramTypeID != LibResources.GetResourceKeyIDByKey(programData.Resources, ResourceConstants.R_TIME_BOUNDED_PROGRAM_KEY))
                        {
                            if ((programData.Program.ProgramDuration <= (int)programData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PROGRAM_DURATION_KEY).MinLength) &&
                                    (programData.Program.ProgramDuration >= (int)programData.Resources.FirstOrDefault(x => x.ResourceKey == ResourceConstants.R_PROGRAM_DURATION_KEY).MaxLength))
                            {
                                programData.ErrCode = ErrorCode.InvalidData;
                                return programData;
                            }
                        }
                        if (!await ValidateDataAsync(programData.Program, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                        else if (!await ValidateDataAsync(programData.LanguageDetails, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Publish/Unpublish Program Data
        /// </summary>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programID">Id of progrm</param>
        /// <param name="isPublished">Publish / Unpublish Flag</param>
        /// <returns>Operation Status Code</returns>
        public async Task<BaseDTO> PublishProgramAsync(byte languageID, long permissionAtLevelID, long programID, bool isPublished)
        {
            BaseDTO programData = new BaseDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || programID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.RecordCount = programID;
                programData.IsActive = isPublished;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().PublishProgramAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Subscribe Program Data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programID">Id of progrm</param>
        /// <returns>Operation Status Code</returns>
        public async Task<BaseDTO> SubscribeProgramAsync(long permissionAtLevelID, long programID)
        {
            BaseDTO programData = new BaseDTO();
            try
            {
                if (permissionAtLevelID < 1 || programID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.RecordCount = programID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SubscribeProgramAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get Program Task(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programTaskID">Program Task ID of which data has to be fetched</param>
        /// <returns>Program Task Data with operation status</returns>
        public async Task<ProgramDTO> GetProgramTaskAsync(byte languageID, long permissionAtLevelID, long programTaskID, bool isSynced)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.Task = new TaskModel { ProgramTaskID = programTaskID, IsSynced = isSynced };
                    await new ProgramServiceDataLayer().GetProgramTaskAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Task Data
        /// </summary>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code And ProgramTaskID</returns>
        public async Task<ProgramDTO> SaveProgramTaskAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || programData?.Task?.ProgramID < 1 || IsProgramTaskNotValid(programData))
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramTaskAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get Program Subflow(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programSubFlowID">Program SubFlow ID of which data has to be fetched</param>
        /// <returns>Program Subflow Data with operation status</returns>
        public async Task<ProgramDTO> GetProgramSubFlowAsync(byte languageID, long permissionAtLevelID, long programSubFlowID, bool isSynced)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.SubFlow = new SubFlowModel { ProgramSubFlowID = programSubFlowID, IsSynced = isSynced };
                    await new ProgramServiceDataLayer().GetProgramSubFlowAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        private void ValidateProgramSubflowData(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            programData.ErrCode = permissionAtLevelID < 1 || languageID < 1 || programData?.SubFlow?.ProgramID < 1 || (programData.SubFlow.IsActive && (programData.SubFlow.AssignAfterDays < 0 || programData.SubFlow.AssignForDays < 1 || programData.SubFlow.ItemID < 1)) ? ErrorCode.InvalidData : ErrorCode.OK;
        }

        /// <summary>
        /// Save Program Subflow Data
        /// </summary>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code And ProgramSubFlowID</returns>
        public async Task<ProgramDTO> SaveProgramSubflowAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || AccountID < 1 || programData?.SubFlow?.ProgramID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.SubFlow.IsActive)
                {
                    programData.LanguageID = languageID;
                    if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.SubFlow, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramSubflowAsync(programData).ConfigureAwait(false);



            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get program education data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programEducationID">Program EducationID used to fetch data</param>
        /// <returns>Program Education data with operation status</returns>
        public async Task<ProgramDTO> GetProgramEducationAsync(byte languageID, long permissionAtLevelID, long programEducationID, bool isSynced)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.ProgramEducation = new PatientEducationModel { ProgramEducationID = programEducationID, IsSynced = isSynced };
                    await new ProgramServiceDataLayer().GetProgramEducationAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Education Data
        /// </summary>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code And ProgramEducationID</returns>
        public async Task<ProgramDTO> SaveProgramEducationAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || languageID < 1 || programData.ProgramEducation == null || programData?.ProgramEducation?.ProgramID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    programData.LanguageID = languageID;
                    if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                    {
                        programData.Resources.RemoveAll(x =>
                        x.ResourceKey == ResourceConstants.R_START_DATE_KEY ||
                        x.ResourceKey == ResourceConstants.R_END_DATE_KEY ||
                        x.ResourceKey == ResourceConstants.R_FOR_PROVIDERS_KEY);
                        if (!await ValidateDataAsync(programData.ProgramEducation, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramEducationAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get program caregiver data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programEducationID">Program caregiverID used to fetch data</param>
        /// <returns>Program caregivers data with operation status</returns>
        public async Task<ProgramDTO> GetProgramCaregiverAsync(byte languageID, long permissionAtLevelID, long programCareGiverID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.ProgramCareGiver = new CaregiverModel { ProgramCareGiverID = programCareGiverID };
                    await new ProgramServiceDataLayer().GetProgramCaregiverAsync(programData).ConfigureAwait(false);
                    if (programData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(programData.Items))
                    {
                        programData.Items.ForEach(option =>
                        {
                            string[] optionText = option.OptionText.Split(Constants.SYMBOL_PIPE_SEPERATOR);
                            option.OptionText = $"{optionText[2]} - {optionText[0]} {optionText[1]}";
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Care giver Data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code And PatientCareGiverID</returns>
        public async Task<ProgramDTO> SaveProgramCaregiverAsync(long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || programData?.ProgramCareGiver == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.ProgramCareGiver.IsActive)
                {
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.ProgramCareGiver, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramCaregiverAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get Items based on Selected task type
        /// </summary>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="taskType">Selected Task Type</param>
        /// <param name="programID">Selected Program ID</param>
        /// <param name="selectedUserID">Selected User ID</param>
        /// <returns>Items that can be selected for given task type</returns>
        public async Task<ProgramDTO> GetItemsBasedOnTaskTypeAsync(byte languageID, long permissionAtLevelID, string taskType, long programID, long selectedUserID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                programData.AccountID = AccountID;
                programData.LanguageID = languageID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.Program = new ProgramModel { ProgramID = programID, Name = taskType };
                programData.SelectedUserID = selectedUserID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().GetItemsBasedOnTaskTypeAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        private async Task<bool> UpdatePageDataAsync(byte languageID, long permissionAtLevelID, long recordCount, BaseDTO data)
        {
            return await GetResourcesAndMapDataAsync(languageID, permissionAtLevelID, recordCount, data,
                $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_READINGS_GROUP},{GroupConstants.RS_READING_CATEGORY_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}," +
                $"{GroupConstants.RS_OPERATION_TYPE_GROUP},{GroupConstants.RS_TASK_STATUS_GROUP},{GroupConstants.RS_TASK_TYPE_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP}," +
                $"{GroupConstants.RS_MEDICATION_GROUP},{GroupConstants.RS_MEDICATION_FREQUENCY_GROUP},{GroupConstants.RS_CODE_SYSTEM_GROUP},{GroupConstants.RS_TRACKERS_GROUP}," +
                $"{GroupConstants.RS_BILLING_GROUP},{GroupConstants.RS_USER_TYPE_GROUP},{GroupConstants.RS_REASONS_GROUP},{GroupConstants.RS_PROGRAM_TYPE_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP}");
        }

        private bool IsProgramTaskNotValid(ProgramDTO programData)
        {
            return programData.Task.IsActive && (programData.Task.AssignAfterDays < 0 || programData.Task.AssignForDays < 1 || programData.Task.TaskID < 1 || string.IsNullOrEmpty(programData.Task.TaskType));
        }

        /// <summary>
        /// Get Program Instruction(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Number of record count to fetch</param>
        /// <param name="instructionID">Instruction ID of which data has to be fetched</param>
        /// <returns>Program Instruction Data with operation status</returns>
        public async Task<ProgramDTO> GetInstructionsAsync(byte languageID, long permissionAtLevelID, long recordCount, long instructionID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, $"{GroupConstants.RS_COMMON_GROUP}",
                     languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
                programData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_COMMON_GROUP}",
                    languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (GenericMethods.IsListNotEmpty(programData.Settings) && GenericMethods.IsListNotEmpty(programData.Resources))
                {
                    programData.AccountID = AccountID;
                    programData.LanguageID = languageID;
                    programData.PermissionAtLevelID = permissionAtLevelID;
                    programData.RecordCount = recordCount;
                    programData.Instruction = new InstructionModel { InstructionID = instructionID };
                    programData.FeatureFor = FeatureFor;
                    await new ProgramServiceDataLayer().GetInstructionsAsync(programData).ConfigureAwait(false);
                    if (programData.ErrCode == ErrorCode.OK)
                    {
                        await ReplaceInstructionImageCdnLinkAsync(programData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Instruction Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Instruction Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task<ProgramDTO> SaveInstructionsAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || programData == null || programData.Instruction == null || AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(programData.LanguageDetails))
                    {
                        programData.ErrCode = ErrorCode.InvalidData;
                        return programData;
                    }
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_PROGRAMS_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.LanguageDetails, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.LanguageID = languageID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveInstructionsAsync(programData, false).ConfigureAwait(false);
                if (programData.ErrCode == ErrorCode.OK && programData.Instruction.IsActive)
                {
                    await UploadImagesAsync(programData).ConfigureAwait(false);
                    if (programData.ErrCode == ErrorCode.OK)
                    {
                        await new ProgramServiceDataLayer().SaveInstructionsAsync(programData, true).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }


        internal async Task ReplaceInstructionImageCdnLinkAsync(List<InstructionI18NModel> instructionDetails)
        {
            if (GenericMethods.IsListNotEmpty(instructionDetails))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var detail in instructionDetails)
                {
                    if (!string.IsNullOrWhiteSpace(detail.Description))
                    {
                        detail.Description = await ReplaceCDNLinkAsync(detail.Description, cdnCacheData);
                    }
                }
            }
        }

        private async Task ReplaceInstructionImageCdnLinkAsync(ProgramDTO programData)
        {
            if (GenericMethods.IsListNotEmpty(programData.LanguageDetails))
            {
                BaseDTO cdnCacheData = new BaseDTO();
                foreach (var detail in programData.LanguageDetails)
                {
                    if (!string.IsNullOrWhiteSpace(detail.Description))
                    {
                        detail.Description = await ReplaceCDNLinkAsync(detail.Description, cdnCacheData);
                    }
                }
            }
        }

        private async Task UploadImagesAsync(ProgramDTO programData)
        {
            FileUploadDTO files = CreateFileDataObject(FileTypeToUpload.InstructionImages, programData.Instruction.InstructionID.ToString(CultureInfo.InvariantCulture));
            files.FileContainers[0].FileData.AddRange(from detail in programData.LanguageDetails
                                                      select CreateFileObject($"{detail.LanguageID}_{programData.Instruction.InstructionID}", detail.Description, true));
            files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
            programData.ErrCode = files.ErrCode;
            if (programData.ErrCode == ErrorCode.OK)
            {
                foreach (var contentDetail in programData.LanguageDetails)
                {
                    contentDetail.Description = GetBase64FileFromFirstContainer(files, $"{contentDetail.LanguageID}_{programData.Instruction.InstructionID}");
                }
            }
        }

        /// <summary>
        /// Gets Program Tracker(s) Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programTrackerID">Reference object which holds Program Tracker Data</param>
        /// <returns>Program Trackers Data with operation status</returns>
        public async Task<ProgramDTO> GetProgramTrackersAsync(byte languageID, long permissionAtLevelID, long programTrackerID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.ProgramTracker = new ProgramTrackerModel { ProgramTrackerID = programTrackerID };
                    await new ProgramServiceDataLayer().GetProgramTrackers(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Gets Program Billing Item(s)
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programBillingItemID">Program Billing Item ID</param>
        /// <returns>Program Billing Item(s) Data with operation status</returns>
        public async Task<ProgramDTO> GetProgramBillingItemsAsync(byte languageID, long permissionAtLevelID, long programBillingItemID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.ProgramBillItem = new PatientBillModel { ProgramBillingItemID = programBillingItemID };
                    await new ProgramServiceDataLayer().GetProgramBillingItemsAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Gets Program Reason configuration Item(s)
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="ProgramID">Program ID</param>
        /// <returns>Program Reason Configuration Data with operation status</returns>
        public async Task<ProgramDTO> GetProgramReasonConfigurationsAsync(byte languageID, long permissionAtLevelID, long programID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.ProgramConfiguration = new ProgramConfigurationModel { ProgramID = programID };
                    await new ProgramServiceDataLayer().GetProgramReasonConfigurationsAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Saves Program billing item data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Billing item Data</param>
        /// <returns>Operation Status</returns>
        public async Task<ProgramDTO> SaveProgramBillingItemAsync(long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || programData.ProgramBillItem == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.ProgramBillItem, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramBillingItemAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Saves Program Reason Configurations data
        /// </summary>
        /// <param name="languageID">Selected language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Reason Configurations Data</param>
        /// <returns>Operation Status</returns>
        public async Task<ProgramDTO> SaveProgramReasonConfigurationsAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || programData?.ProgramConfigurations == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramReasonConfigurationsAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Tracker Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Tracker Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task<ProgramDTO> SaveProgramTrackerAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || languageID < 1 || programData.ProgramTracker == null || programData?.ProgramTracker?.ProgramID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    programData.LanguageID = languageID;
                    if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.ProgramTracker, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramTrackerAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Get program notes
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programNoteID">Program NoteID used to fetch data</param>
        /// <returns>Program note data with operation status</returns>
        public async Task<ProgramDTO> GetProgramNotesAsync(byte languageID, long permissionAtLevelID, long programNoteID, long programID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdatePageDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.ProgramNote = new ProgramNoteModel { ProgramNoteID = programNoteID, ProgramID = programID };
                    await new ProgramServiceDataLayer().GetProgramNotesAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }

        /// <summary>
        /// Save Program Note Data
        /// </summary>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code And ProgramNoteId</returns>
        public async Task<ProgramDTO> SaveProgramNoteAsync(long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || programData.ProgramNote == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(programData.ProgramNotes))
                    {
                        programData.ErrCode = ErrorCode.InvalidData;
                        return programData;
                    }
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.ProgramNotes, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramNoteAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        public async Task<BaseDTO> GetProgramServicesAsync(byte languageID, long permissionAtLevelID, long programExternalServiceID, long programID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await UpdateProgramServiceDataAsync(languageID, permissionAtLevelID, -1, programData).ConfigureAwait(false))
                {
                    programData.ProgramService = new ProgramServiceModel { ProgramExternalServiceID = programExternalServiceID, ProgramID = programID };
                    await new ProgramServiceDataLayer().GetProgramServicesAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }
        private async Task<bool> UpdateProgramServiceDataAsync(byte languageID, long permissionAtLevelID, long recordCount, BaseDTO data)
        {
            return await GetResourcesAndMapDataAsync(languageID, permissionAtLevelID, recordCount, data,
                $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP}");
        }
        public async Task<BaseDTO> SaveProgramServiceAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || languageID < 1 || programData.ProgramService == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    programData.LanguageID = languageID;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_CARE_FLIX_SERVICE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.ProgramService, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramServiceDataLayer().SaveProgramServiceAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }
    }
}