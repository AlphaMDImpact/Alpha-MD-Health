using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ProgramServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Program Subflow(s) Data
        /// </summary>
        /// <param name="programData">Reference object to save and return Program Subflows Data</param>
        /// <returns>Program Subflows Data with Operation Status</returns>
        public async Task GetSubFlowsAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.SubFlowID), programData.SubFlow.SubFlowID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.SubflowsView.ToString(), $"{AppPermissions.SubflowDelete},{AppPermissions.SubflowAddEdit}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_SUBFLOWS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapProgramSubFlowsAsync(programData, result).ConfigureAwait(false);
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Subflow Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Subflow Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SaveSubFlowAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.SubFlowID), programData.SubFlow.SubFlowID, DbType.Int64, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), programData.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.OperationType), programData.SubFlow.OperationType, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TaskType), programData.SubFlow.TaskType, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.ItemID), programData.SubFlow.ItemID, DbType.Int64, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TemplateID), GetTemplateID(programData), DbType.Int16, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.Value1), programData.SubFlow.Value1, DbType.Decimal, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.Value2), programData.SubFlow.Value2, DbType.Decimal, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertProgramDetailsToTable(programData).AsTableValuedParameter());
            MapCommonSPParameters(programData, parameter, programData.IsActive ? AppPermissions.SubflowAddEdit.ToString() : AppPermissions.SubflowDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_SUBFLOW, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Get Program Task(s) Data
        /// </summary>
        /// <param name="programData">Reference object to save and return Program Tasks Data</param>
        /// <returns>Program Tasks Data with Operation Status</returns>
        public async Task GetTasksAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.TaskID), programData.Task.TaskID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, AppPermissions.TasksView.ToString(), $"{AppPermissions.TaskDelete},{AppPermissions.TaskAddEdit},{AppPermissions.SubflowsView},{AppPermissions.SubflowAddEdit}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_TASKS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapProgramTasksAsync(programData, result).ConfigureAwait(false);
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Task Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Task Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SaveTaskAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.TaskID), programData.Task.TaskID, DbType.Int64, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertLanguageDetailsToTable(programData, true).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.TaskType), programData.Task.TaskType, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ItemID), programData.Task.ItemID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), programData.Task.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.IsActive ? AppPermissions.TaskAddEdit.ToString() : AppPermissions.TaskDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_TASK, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Get Task Subflow(s) Data
        /// </summary>
        /// <param name="programData">Reference object to save and return Task Subflows Data</param>
        /// <returns>Task Subflows Data with Operation Status</returns>
        public async Task GetTaskSubFlowAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TaskSubFlowID), programData.SubFlow.TaskSubFlowID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, AppPermissions.SubflowsView.ToString(), $"{AppPermissions.SubflowAddEdit},{AppPermissions.SubflowDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_TASK_SUBFLOWS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.SubFlows = (await result.ReadAsync<SubFlowModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed && programData.SubFlow.TaskSubFlowID > 0)
                {
                    programData.SubFlow = await result.ReadFirstAsync<SubFlowModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Task Subflow Data
        /// </summary>
        /// <param name="programData">Reference object which holds Task Subflow Data</param>
        /// <returns>Operation Status Code And TaskSubFlowID</returns>
        public async Task SaveTaskSubflowAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TaskSubFlowID), programData.SubFlow.TaskSubFlowID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TaskID), programData.SubFlow.TaskID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.SubFlowID), programData.SubFlow.SubFlowID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.AssignAfterDays), programData.SubFlow.AssignAfterDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.AssignForDays), programData.SubFlow.AssignForDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.IsActive), programData.SubFlow.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.SubFlow.IsActive ? AppPermissions.SubflowAddEdit.ToString() : AppPermissions.SubflowDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_TASK_SUBFLOW, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.SubFlow.TaskSubFlowID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TaskSubFlowID));
            }
        }

        /// <summary>
        /// Get Program(s) Data
        /// </summary>
        /// <param name="programData">Reference object to return Programs</param>
        /// <returns>Programs Data with Operation Status</returns>
        public async Task GetProgramsAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(ProgramModel.ProgramID)), programData.Program.ProgramID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, CheckPermission(programData), ReturnPermission(programData));
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAMS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                if (programData.RecordCount == -2)
                {
                    programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false)).ToList();
                }
                else if (programData.RecordCount == -1)
                {
                    programData.LanguageDetails = (await result.ReadAsync<ProgramDetails>().ConfigureAwait(false)).ToList();
                    if (programData.Program.ProgramID > 0)
                    {
                        await MapProgramDetailsAsync(programData, result).ConfigureAwait(false);
                    }
                }
                else
                {
                    programData.Programs = (await result.ReadAsync<ProgramModel>().ConfigureAwait(false))?.ToList();
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private string CheckPermission(ProgramDTO programData)
        {
            if (programData.RecordCount == -2)
            {
                return AppPermissions.ProgramSubscribeView.ToString();
            }
            else
            {
                return $"{AppPermissions.ProgramPublish},{AppPermissions.ProgramsView}";
            }
        }

        private string ReturnPermission(ProgramDTO programData)
        {
            return programData.RecordCount switch
            {
                -2 => AppPermissions.ProgramSubscribeView.ToString(),
                -1 => $"{AppPermissions.ProgramTasksView},{AppPermissions.SubflowsView},{AppPermissions.ProgramCaregiversView},{AppPermissions.ProgramReadingsView}" +
                    $",{AppPermissions.ProgramEducationsView},{AppPermissions.ProgramMedicationsView},{AppPermissions.ProgramTrackersView},{AppPermissions.ProgramServicesView},{AppPermissions.PatientProviderNoteTypesView}" +
                    $",{AppPermissions.ProgramBillingItemsView},{AppPermissions.ProgramReasonsView},{AppPermissions.ProgramConfigurationView}" +
                    $",{AppPermissions.ProgramAddEdit},{AppPermissions.ProgramTaskAddEdit},{AppPermissions.SubflowAddEdit}" +
                    $",{AppPermissions.ProgramCaregiverAddEdit},{AppPermissions.ProgramPublish},{AppPermissions.ProgramReadingAddEdit},{AppPermissions.ProgramServiceAddEdit},{AppPermissions.ProgramEducationAddEdit}," +
                    $"{AppPermissions.ProgramMedicationAddEdit},{AppPermissions.ProgramTrackerAddEdit},{AppPermissions.PatientProviderNoteTypeAddEdit}," +
                    $"{AppPermissions.BillingItemAddEdit},{AppPermissions.ProgramReasonAddEdit},{AppPermissions.ProgramReadingRangesView} ",
                _ => $"{AppPermissions.ProgramsView},{AppPermissions.ProgramSubscribeView},{AppPermissions.ProgramAddEdit}",
            };
        }

        /// <summary>
        /// Save Program Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SaveProgramAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.ProgramID), programData.Program.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.AllowSelfRegistration), programData.Program.AllowSelfRegistration, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.AllowProviderToScan), programData.Program.AllowProviderToScan, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.AllowPatientToScan), programData.Program.AllowPatientToScan, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.AllowPatientToBuyCredits), programData.Program.AllowPatientToBuyCredits, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.AllowProgramToBuyCredits), programData.Program.AllowProgramToBuyCredits, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.ProgramGroupIdentifier), programData.Program.ProgramGroupIdentifier, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.ProgramTypeID), programData.Program.ProgramTypeID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.ProgramDuration), programData.Program.ProgramDuration, DbType.Int32, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, ConvertProgramDetailsToTable(programData).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.CodeSystemID), programData.Program.CodeSystemID, DbType.Int16, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramAddEdit.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Save Program publish Unpublish program Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task PublishProgramAsync(BaseDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.ProgramID), programData.RecordCount, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.IsPublished), programData.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, AppPermissions.ProgramPublish.ToString());
            await connection.QueryAsync(SPNameConstants.USP_PUBLISH_PROGRAM, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Save subscribed program
        /// </summary>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SubscribeProgramAsync(BaseDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.ProgramID), programData.RecordCount, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramSubscribeView.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SUBSCRIBE_PROGRAM, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Get Program Task(s) Data
        /// </summary>
        /// <param name="programData">Reference object to save and return Programs Task Data</param>
        /// <returns>Program Task Data with Operation Status</returns>
        public async Task GetProgramTaskAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ProgramTaskID), programData.Task.ProgramTaskID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(TaskModel.IsSynced)), programData.Task.IsSynced, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, AppPermissions.ProgramTasksView.ToString(), $"{AppPermissions.ProgramTaskAddEdit},{AppPermissions.ProgramTaskDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_TASKS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.Tasks = (await result.ReadAsync<TaskModel>().ConfigureAwait(false))?.ToList();
                if (programData.Task.ProgramTaskID > 0 && !result.IsConsumed)
                {
                    programData.Task = await result.ReadFirstAsync<TaskModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Task Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Task Data</param>
        /// <returns>Operation Status Code And ProgramTaskID</returns>
        public async Task SaveProgramTaskAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ProgramTaskID), programData.Task.ProgramTaskID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ProgramID), programData.Task.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.TaskID), programData.Task.TaskID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.AssignAfterDays), programData.Task.AssignAfterDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.AssignForDays), programData.Task.AssignForDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.IsActive), programData.Task.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ExecuteOnLogin), programData.Task.ExecuteOnLogin, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.Task.IsActive ? AppPermissions.TaskAddEdit.ToString() : AppPermissions.TaskDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_TASK, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.Task.ProgramTaskID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ProgramTaskID));
            }
        }

        /// <summary>
        /// Get Program Subflow(s) Data
        /// </summary>
        /// <param name="programData">Reference object to save and return Programs Subflows Data</param>
        /// <returns>Program Subflows Data with Operation Status</returns>
        public async Task GetProgramSubFlowAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.ProgramSubFlowID), programData.SubFlow.ProgramSubFlowID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(SubFlowModel.IsSynced)), programData.SubFlow.IsSynced, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, AppPermissions.SubflowsView.ToString(), $"{AppPermissions.SubflowAddEdit},{AppPermissions.SubflowDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_SUBFLOWS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.SubFlows = (await result.ReadAsync<SubFlowModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed && programData.SubFlow.ProgramSubFlowID > 0)
                {
                    programData.SubFlow = await result.ReadFirstAsync<SubFlowModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Subflow Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Subflow Data</param>
        /// <returns>Operation Status Code And ProgramSubFlowID</returns>
        public async Task SaveProgramSubflowAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.ProgramSubFlowID), programData.SubFlow.ProgramSubFlowID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.ProgramID), programData.SubFlow.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TaskType), programData.SubFlow.TaskType, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.ItemID), programData.SubFlow.ItemID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.SubFlowID), programData.SubFlow.SubFlowID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.AssignAfterDays), programData.SubFlow.AssignAfterDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.AssignForDays), programData.SubFlow.AssignForDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.IsActive), programData.SubFlow.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.SubFlow.IsActive ? AppPermissions.SubflowAddEdit.ToString() : AppPermissions.SubflowDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_SUBFLOW, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.SubFlow.ProgramSubFlowID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.ProgramSubFlowID));
            }
        }

        /// <summary>
        /// Get program caregivers
        /// </summary>
        /// <param name="programData">object to get data</param>
        /// <returns>operation status and list of caregivers</returns>
        public async Task GetProgramCaregiverAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.ProgramCareGiverID), programData.ProgramCareGiver.ProgramCareGiverID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), default(DateTimeOffset), DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramCaregiversView.ToString(), $"{AppPermissions.ProgramCaregiverAddEdit},{AppPermissions.ProgramCaregiverDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_CARE_GIVERS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed && programData.ProgramCareGiver.ProgramCareGiverID > 0)
                {
                    programData.ProgramCareGiver = await result.ReadFirstAsync<CaregiverModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Caregiver Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Caregiver Data</param>
        /// <returns>Operation Status Code And PatientCareGiverID</returns>
        public async Task SaveProgramCaregiverAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_PROGRAM_CARE_GIVER_ID, programData.ProgramCareGiver.ProgramCareGiverID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.ProgramID), programData.ProgramCareGiver.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserModel.UserID), programData.ProgramCareGiver.CareGiverID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.AssignAfterDays), programData.ProgramCareGiver.AssignAfterDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.AssignForDays), programData.ProgramCareGiver.AssignForDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.IsActive), programData.ProgramCareGiver.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.ProgramCareGiver.IsActive ? AppPermissions.ProgramCaregiverAddEdit.ToString() : AppPermissions.ProgramCaregiverDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_CAREGIVER, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.ProgramCareGiver.ProgramCareGiverID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_PROGRAM_CARE_GIVER_ID);
            }
        }

        /// <summary>
        /// Get program Education
        /// </summary>
        /// <param name="programData">object to get data</param>
        /// <returns>operation status and list of Educations</returns>
        public async Task GetProgramEducationAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.ProgramEducationID)), programData.ProgramEducation.ProgramEducationID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.IsSynced)), programData.ProgramEducation.IsSynced, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramEducationsView.ToString(), $"{AppPermissions.ProgramEducationAddEdit},{AppPermissions.ProgramEducationDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_EDUCATIONS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.TaskTypes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed)
                {
                    programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                }
                if (!result.IsConsumed && programData.ProgramEducation.ProgramEducationID > 0)
                {
                    programData.ProgramEducation = await result.ReadFirstAsync<PatientEducationModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Education Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Education Data</param>
        /// <returns>Operation Status Code And PatientEducationID</returns>
        public async Task SaveProgramEducationAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.ProgramEducationID)), programData.ProgramEducation.ProgramEducationID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.ProgramID)), programData.ProgramEducation.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.PageID)), programData.ProgramEducation.PageID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.AssignAfterDays)), programData.ProgramEducation.AssignAfterDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.AssignForDays)), programData.ProgramEducation.AssignForDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.IsActive)), programData.ProgramEducation.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(PatientEducationModel.ForProviders)), programData.ProgramEducation.ForProviders, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.ProgramEducation.IsActive ? AppPermissions.ProgramEducationAddEdit.ToString() : AppPermissions.ProgramEducationDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_EDUCATION, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.ProgramEducation.ProgramEducationID = parameter.Get<long>(ConcateAt(nameof(PatientEducationModel.ProgramEducationID)));
            }
        }

        /// <summary>
        /// Get items for selected Task Type
        /// </summary>
        /// <param name="programData">Reference object to save and return Items</param>
        /// <returns>Items with Operation Status</returns>
        public async Task GetItemsBasedOnTaskTypeAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SubFlowModel.TaskType), programData.Program.Name, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramModel.ProgramID), programData.Program.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), programData.LanguageID, DbType.Byte, direction: ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.SelectedUserID), programData.SelectedUserID, DbType.Int64, direction: ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, string.Empty);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_ITEMS_FOR_TASK_TYPE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        private DataTable ConvertLanguageDetailsToTable(ProgramDTO programData, bool isUpdateFlow)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(programData.LanguageDetails))
            {
                foreach (ProgramDetails record in programData.LanguageDetails)
                {
                    dataTable.Rows.Add(0, Guid.Empty, record.LanguageID, record.Name, isUpdateFlow ? record.Description : string.Empty, string.Empty);
                }
            }
            return dataTable;
        }

        private DataTable MapProgramReasonConfigurationsToTable(List<ProgramConfigurationModel> prcModel, long OrganisationID, long programID)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(ProgramConfigurationModel.ProgramReasonConfigurationID), typeof(long)),
                    new DataColumn(nameof(ProgramConfigurationModel.ProgramID), typeof(long)),
                    new DataColumn(nameof(BaseDTO.OrganisationID), typeof(long)),
                    new DataColumn(nameof(ProgramConfigurationModel.FeatureID), typeof(int)),
                    new DataColumn(nameof(ProgramConfigurationModel.IsReasonRequired), typeof(bool)),
                    new DataColumn(nameof(ProgramConfigurationModel.IsSignatureRequired), typeof(bool)),
                    new DataColumn(nameof(ProgramConfigurationModel.IsActive), typeof(bool)),
                }
            };
            foreach (ProgramConfigurationModel record in prcModel)
            {
                dataTable.Rows.Add(
                    record.ProgramReasonConfigurationID, programID, OrganisationID, record.FeatureID,
                    record.IsReasonRequired, record.IsSignatureRequired, record.IsActive);
            }
            return dataTable;
        }

        private DataTable ConvertProgramDetailsToTable(ProgramDTO programData)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(programData.LanguageDetails))
            {
                foreach (ProgramDetails item in programData.LanguageDetails)
                {
                    dataTable.Rows.Add(0, Guid.Empty, item.LanguageID, item.Name, item.Description, string.Empty);
                }
            }
            return dataTable;
        }

        private async Task MapProgramSubFlowsAsync(ProgramDTO programData, SqlMapper.GridReader result)
        {
            if (programData.RecordCount == -1)
            {
                programData.ProgramSubFlows = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed)
                {
                    programData.LanguageDetails = (await result.ReadAsync<ProgramDetails>().ConfigureAwait(false))?.ToList();
                }
                if (programData.SubFlow.SubFlowID > 0)
                {
                    if (!result.IsConsumed)
                    {
                        programData.SubFlow = await result.ReadFirstAsync<SubFlowModel>().ConfigureAwait(false);
                    }
                    if (!result.IsConsumed)
                    {
                        programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                    }
                }
            }
            else
            {
                programData.SubFlows = (await result.ReadAsync<SubFlowModel>().ConfigureAwait(false))?.ToList();
            }
        }

        private async Task MapProgramTasksAsync(ProgramDTO programData, SqlMapper.GridReader result)
        {
            if (programData.RecordCount == -1)
            {
                await MapProgramTaskDetailsAsync(programData, result).ConfigureAwait(false);
            }
            if (!result.IsConsumed && (programData.RecordCount != -1 || programData.Task.TaskID > 0))
            {
                programData.Tasks = (await result.ReadAsync<TaskModel>().ConfigureAwait(false))?.ToList();
            }
        }

        private async Task MapProgramTaskDetailsAsync(ProgramDTO programData, SqlMapper.GridReader result)
        {
            if (!result.IsConsumed)
            {
                programData.LanguageDetails = (await result.ReadAsync<ProgramDetails>().ConfigureAwait(false))?.ToList();
            }
            if (programData.Task.TaskID > 0)
            {
                if (!result.IsConsumed)
                {
                    programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                }
                if (!result.IsConsumed)
                {
                    programData.SubFlows = (await result.ReadAsync<SubFlowModel>().ConfigureAwait(false))?.ToList();
                }
            }
        }

        private async Task MapProgramDetailsAsync(ProgramDTO programData, SqlMapper.GridReader result)
        {
            programData.Program = (await result.ReadAsync<ProgramModel>().ConfigureAwait(false))?.FirstOrDefault();
            if (!result.IsConsumed)
            {
                programData.Tasks = (await result.ReadAsync<TaskModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.SubFlows = (await result.ReadAsync<SubFlowModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramCareGivers = (await result.ReadAsync<CaregiverModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramReadings = (await result.ReadAsync<ReadingModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramEducations = (await result.ReadAsync<PatientEducationModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramMedications = (await result.ReadAsync<PatientMedicationModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramTrackers = (await result.ReadAsync<ProgramTrackerModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramBillingItems = (await result.ReadAsync<PatientBillModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramNotes = (await result.ReadAsync<ProgramNoteModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.Reasons = (await result.ReadAsync<ReasonModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramConfigurations = (await result.ReadAsync<ProgramConfigurationModel>().ConfigureAwait(false))?.ToList();
            }
            if (!result.IsConsumed)
            {
                programData.ProgramServices = (await result.ReadAsync<ProgramServiceModel>().ConfigureAwait(false))?.ToList();
            }
        }

        private object GetTemplateID(ProgramDTO opType)
        {
            if (opType.SubFlow.TaskType == ResourceConstants.R_EMAIL_KEY || opType.SubFlow.TaskType == ResourceConstants.R_SMS_KEY || opType.SubFlow.TaskType == ResourceConstants.R_NOTIFICATION_KEY)
            {
                return opType.SubFlow.TemplateID;
            }
            return DBNull.Value;
        }

        /// <summary>
        /// Gets Program billing Items data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Billing Items Data</param>
        /// <returns>Program Billing Items Data with operation status</returns>
        public async Task GetProgramBillingItemsAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientBillModel.ProgramBillingItemID), programData.ProgramBillItem.ProgramBillingItemID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), default(DateTimeOffset), DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramBillingItemsView.ToString(), $"{AppPermissions.ProgramBillingItemDelete},{AppPermissions.ProgramBillingItemAddEdit}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_BILLING_ITEMS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.BillingItemOptionList = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed && programData.ProgramBillItem.ProgramBillingItemID > 0)
                {
                    programData.ProgramBillItem = await result.ReadFirstAsync<PatientBillModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Program Reason Configurations
        /// </summary>
        /// <param name="programData">Reference object which holds Program Reason Configurations Data</param>
        /// <returns>Program Reason Configurations Data with operation status</returns>
        public async Task GetProgramReasonConfigurationsAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramConfigurationModel.ProgramID), programData.ProgramConfiguration.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), default(DateTimeOffset), DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, AppPermissions.ProgramConfigurationView.ToString());
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_REASON_CONFIGURATIONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed)
                {
                    programData.ProgramConfigurations = (await result.ReadAsync<ProgramConfigurationModel>().ConfigureAwait(false))?.ToList();
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }



        /// <summary>
        /// Saves Progem Billing Item Data
        /// </summary>
        /// <param name="programData">Object Which holds program billing item data</param>
        /// <returns>Operation status</returns>
        public async Task SaveProgramBillingItemAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientBillModel.ProgramBillingItemID), programData.ProgramBillItem.ProgramBillingItemID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientBillModel.ProgramID), programData.ProgramBillItem.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientBillModel.BillingItemID), programData.ProgramBillItem.BillingItemID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientBillModel.Amount), programData.ProgramBillItem.Amount, DbType.Double, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(PatientBillModel.IsActive), programData.ProgramBillItem.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.ProgramBillItem.IsActive ? AppPermissions.ProgramBillingItemAddEdit.ToString() : AppPermissions.ProgramBillingItemDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_BILLING_ITEM, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.ProgramBillItem.ProgramBillingItemID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_PROGRAM_BILLING_ITEM_ID);
            }
        }

        /// <summary>
        /// Saves Progem Reason Configuration
        /// </summary>
        /// <param name="programData">Object Which holds program reason configuration data</param>
        /// <returns>Operation status</returns>
        public async Task SaveProgramReasonConfigurationsAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapProgramReasonConfigurationsToTable(programData.ProgramConfigurations, programData.OrganisationID, programData.CreatedByID).AsTableValuedParameter());
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramConfigurationView.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_REASON_CONFIGURATION, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Get Program Instruction(s) Data
        /// </summary>
        /// <param name="programData">Reference object to save and return Program Instructions Data</param>
        /// <returns>Program Instruction Data with Operation Status</returns>
        public async Task GetInstructionsAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(InstructionModel.InstructionID), programData.Instruction.InstructionID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), default(DateTimeOffset), DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.InstructionsView.ToString(), $"{AppPermissions.InstructionDelete},{AppPermissions.InstructionAddEdit}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_INSTRUCTIONS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapProgramInstructionsAsync(programData, result).ConfigureAwait(false);
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Instruction Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Instruction Data</param>
        /// <param name="isUpdateFlow">Flag to decide description contains image data or not</param>
        /// <returns>Operation Status Code</returns>
        public async Task SaveInstructionsAsync(ProgramDTO programData, bool isUpdateFlow)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(InstructionModel.InstructionID)), programData.Instruction.InstructionID, DbType.Int64, direction: ParameterDirection.InputOutput);
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), ConvertLanguageDetailsToTable(programData, isUpdateFlow).AsTableValuedParameter());
            parameter.Add(ConcateAt(nameof(InstructionModel.IsActive)), programData.Instruction.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.Instruction.IsActive
                ? AppPermissions.InstructionAddEdit.ToString()
                : AppPermissions.InstructionDelete.ToString()
            );
            await connection.QueryAsync(SPNameConstants.USP_SAVE_INSTRUCTIONS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.Instruction.InstructionID = parameter.Get<long>(ConcateAt(nameof(InstructionModel.InstructionID)));
            }
        }


        private async Task MapProgramInstructionsAsync(ProgramDTO programData, SqlMapper.GridReader result)
        {
            if (!result.IsConsumed)
            {
                if (programData.RecordCount == -1)
                {
                    programData.LanguageDetails = (await result.ReadAsync<ProgramDetails>().ConfigureAwait(false))?.ToList();
                }
                else
                {
                    programData.Instructions = (await result.ReadAsync<InstructionModel>().ConfigureAwait(false))?.ToList();
                }
            }
        }

        /// <summary>
        /// Gets Program Tracker(s) Data
        /// </summary>
        /// <param name="programData">DTO to return Program Trackers Data</param>
        /// <returns>Program Trackers Data with operation status</returns>
        public async Task GetProgramTrackers(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.ProgramTrackerID), programData.ProgramTracker.ProgramTrackerID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameters, AppPermissions.ProgramTrackersView.ToString(),
                 $"{AppPermissions.ProgramTrackerDelete},{AppPermissions.ProgramTrackerAddEdit}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_TRACKERS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.TrackerTypes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed && programData.ProgramTracker.ProgramTrackerID > 0)
                {
                    programData.ProgramTracker = await result.ReadFirstAsync<ProgramTrackerModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Tracker Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Tracker Data</param>
        /// <returns>Operation Status Code And ProgramTrackerID</returns>
        public async Task SaveProgramTrackerAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.PatientTrackerID), programData.ProgramTracker.PatientTrackerID, DbType.Guid, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.ProgramTrackerID), programData.ProgramTracker.ProgramTrackerID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.ProgramID), programData.ProgramTracker.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.ValueAddedBy), programData.ProgramTracker.ValueAddedBy, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.TrackerID), programData.ProgramTracker.TrackerID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.AssignAfterDays), programData.ProgramTracker.AssignAfterDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.AssignForDays), programData.ProgramTracker.AssignForDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramTrackerModel.IsActive), programData.ProgramTracker.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.ProgramTracker.IsActive ? AppPermissions.ProgramTrackerAddEdit.ToString() : AppPermissions.ProgramTrackerDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_TRACKERS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.ProgramTracker.ProgramTrackerID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_PROGRAM_Tracker_ID);
            }
        }

        /// <summary>
        /// Get program notes
        /// </summary>
        /// <param name="programData">object to get data</param>
        /// <returns>operation status and list of notes</returns>
        public async Task GetProgramNotesAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramNoteModel.ProgramNoteID), programData.ProgramNote.ProgramNoteID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramNoteModel.ProgramID), programData.ProgramNote.ProgramID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.PatientProviderNoteTypesView.ToString(), $"{AppPermissions.PatientProviderNoteTypeAddEdit},{AppPermissions.PatientProviderNoteTypeDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_NOTES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed)
                {
                    programData.ProgramNotes = (await result.ReadAsync<ProgramNoteModel>().ConfigureAwait(false))?.ToList();
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Note Data
        /// </summary>
        /// <param name="programData">Program Note data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SaveProgramNoteAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramNoteModel.ProgramNoteID), programData.ProgramNote.ProgramNoteID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramNoteModel.ProgramID), programData.ProgramNote.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramNoteModel.QuestionnaireID), programData.ProgramNote.QuestionnaireID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapProgramNotesToTable(programData.ProgramNote.ProgramNoteID, programData.ProgramNotes).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), programData.ProgramNote.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.IsActive ? AppPermissions.PatientProviderNoteTypeAddEdit.ToString() : AppPermissions.PatientProviderNoteTypeDelete.ToString());
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_PROGRAM_NOTE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.ProgramNote.ProgramNoteID = parameter.Get<Int64>(Constants.SYMBOL_AT_THE_RATE + nameof(programData.ProgramNote.ProgramNoteID));
            }
        }

        protected DataTable MapProgramNotesToTable(long ProgramNoteId, List<ProgramNoteModel> programNotes)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if(GenericMethods.IsListNotEmpty(programNotes))
            {
                foreach (ProgramNoteModel record in programNotes)
                {
                    dataTable.Rows.Add(ProgramNoteId, Guid.Empty, record.LanguageID, record.NoteText, record.NoteDescription, string.Empty);
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Get program services
        /// </summary>
        /// <param name="programData">object to get data</param>
        /// <returns>operation status and list of notes</returns>
        public async Task GetProgramServicesAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.ProgramExternalServiceID), programData.ProgramService.ProgramExternalServiceID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.ProgramID), programData.ProgramService.ProgramID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramServicesView.ToString(), $"{AppPermissions.ProgramServiceAddEdit},{AppPermissions.ProgramServiceDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_SERVICES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed && programData.ProgramService.ProgramExternalServiceID > 0)
                {
                    programData.ProgramService = await result.ReadFirstAsync<ProgramServiceModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Service Data
        /// </summary>
        /// <param name="programData">Program Service data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task SaveProgramServiceAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.ProgramExternalServiceID), programData.ProgramService.ProgramExternalServiceID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.ProgramID), programData.ProgramService.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.ExternalServiceID), programData.ProgramService.ExternalServiceID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.AssignAfterDays), programData.ProgramService.AssignAfterDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.AssignForDays), programData.ProgramService.AssignForDays, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.Quantity), programData.ProgramService.Quantity, DbType.Int32, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ProgramServiceModel.IsActive), programData.ProgramService.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.ProgramService.IsActive ? AppPermissions.ProgramServiceAddEdit.ToString() : AppPermissions.ProgramServiceDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_SERVICE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.ProgramService.ProgramExternalServiceID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_PROGRAM_EXTERNAL_SERVICE_ID);
            }
        }
    }
}