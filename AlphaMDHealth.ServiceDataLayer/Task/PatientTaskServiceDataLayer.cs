using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class PatientTaskServiceDataLayer : BaseServiceDataLayer
	{
		/// <summary>
		/// Check and update missed task status
		/// </summary>
		/// <returns>Operation Status Code</returns>
		public async Task<ErrorCode> UpdateMissedTasksAsync()
		{
			using var connection = ConnectDatabase();
			DynamicParameters parameter = new DynamicParameters();
			parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), 200, DbType.Int16, direction: ParameterDirection.Output);
			await connection.QueryAsync(SPNameConstants.USP_CHECK_AND_UPDATE_MISSED_TASKS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			return GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
		}

		/// <summary>
		/// Get patient tasks
		/// </summary>
		/// <param name="taskData">Reference object to return list of patient tasks with operation status</param>
		/// <returns>List of patient tasks with operation status</returns>
		public async Task GetPatientTasksAsync(ProgramDTO taskData)
		{
            GetPatientTaskPermissionData(taskData.RecordCount, taskData.Task.PatientTaskID > 0, out string permissionCheck, out string permissionRequest);
            using var connection = ConnectDatabase();
			connection.Open();
			DynamicParameters parameters = new DynamicParameters();
			parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), taskData.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.PatientTaskID), taskData.Task.PatientTaskID, DbType.Int64, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.SelectedUserID), taskData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
			parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), taskData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
			AddDateTimeParameter(nameof(BaseDTO.FromDate), taskData.FromDate, parameters, ParameterDirection.Input);
			AddDateTimeParameter(nameof(BaseDTO.ToDate), taskData.ToDate, parameters, ParameterDirection.Input);
			MapCommonSPParameters(taskData, parameters, permissionCheck, permissionRequest);
			SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIRNT_TASKS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			if (result.HasRows())
			{
				await MapPatientTasksViewDataAsync(taskData, result).ConfigureAwait(false);
				await MapReturnPermissionsAsync(taskData, result).ConfigureAwait(false);
			}
			taskData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
		}

		internal async Task MapPatientTasksViewDataAsync(ProgramDTO taskData, SqlMapper.GridReader result)
		{
			// Map Patient Task for Add/Edit page
			if (taskData.RecordCount == -1)
			{
				// Map Patient Task Details
				taskData.Task = taskData.Task?.PatientTaskID > 0 ? (await result.ReadFirstAsync<TaskModel>().ConfigureAwait(false)) : null;
				if (taskData.Task?.TaskType == TaskType.QuestionnaireKey.ToString() && taskData.Task?.Status == ResourceConstants.R_COMPLETED_STATUS_KEY)
				{
					taskData.QuestionnaireQuestions = (await result.ReadAsync<QuestionnaireQuestionModel>().ConfigureAwait(false))?.ToList();
					taskData.QuestionnaireQuestionOptions = (await result.ReadAsync<QuestionnaireQuestionOptionModel>().ConfigureAwait(false))?.ToList();
				}
			}
			// For Patient Task list page
			else
			{
				taskData.Tasks = (await result.ReadAsync<TaskModel>().ConfigureAwait(false))?.ToList();
			}
		}

		/// <summary>
		/// Save patient task
		/// </summary>
		/// <param name="programData">Reference object which holds task data</param>
		/// <returns>Operation Status Code And TaskID</returns>
		public async Task SavePatientTaskAsync(ProgramDTO programData)
		{
			using var connection = ConnectDatabase();
			DynamicParameters parameter = new DynamicParameters();
			parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.PatientTaskID), programData.Task.PatientTaskID, DbType.Int64, ParameterDirection.InputOutput);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.SelectedUserID), programData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.TaskType), programData.Task.TaskType, DbType.String, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.Status), programData.Task.Status, DbType.String, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ItemID), programData.Task.ItemID, DbType.Int64, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.FromDate), programData.Task.FromDate.Value.Date, DbType.DateTimeOffset, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.ToDate), programData.Task.ToDate.Value.Date, DbType.DateTimeOffset, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.IsActive), programData.Task.IsActive, DbType.Boolean, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.AddedOn), programData.Task.AddedOn, DbType.DateTimeOffset, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.LastModifiedON), programData.Task.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
			MapCommonSPParameters(programData, parameter, programData.Task.IsActive ? AppPermissions.PatientAssignTaskView.ToString() : AppPermissions.PatientTaskDelete.ToString());
			await connection.QueryAsync(SPNameConstants.USP_SAVE_PATIENT_TASK, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
			if (programData.ErrCode == ErrorCode.OK && programData.Task.IsActive)
			{
				programData.Task.PatientTaskID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(TaskModel.PatientTaskID));
			}
		}

		private void GetPatientTaskPermissionData(long recordCount, bool isViewPage, out string permissionCheck, out string permissionRequest)
		{
			// For Patient Task Detail page
			if (isViewPage)
			{
                permissionCheck = AppPermissions.PatientTaskView.ToString();
                permissionRequest = AppPermissions.PatientTaskDelete.ToString();
			}
			// For Patient Task Add page
			else if (recordCount == -1)
            {
                permissionCheck = AppPermissions.PatientAssignTaskView.ToString();
                permissionRequest = string.Empty;
            }
			else
			{
				permissionCheck = AppPermissions.PatientTasksView.ToString();
				permissionRequest = $"{AppPermissions.PatientAssignTaskView},{AppPermissions.PatientTaskView},{AppPermissions.QuestionnaireTaskView}";
			}
		}

		/// <summary>
		/// Update Patient task Status
		/// </summary>
		/// <param name="programData">Reference object which holds task status</param>
		/// <returns>Operation Status</returns>
		public async Task UpdatePatientTaskStatusAsync(ProgramDTO programData)
		{
			using var connection = ConnectDatabase();
			DynamicParameters parameter = new DynamicParameters();
			parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
			parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapPatientTasksToTable(programData.Tasks).AsTableValuedParameter());
			MapCommonSPParameters(programData, parameter, AppPermissions.PatientTaskStatusEdit.ToString());
			await connection.QueryAsync(SPNameConstants.USP_UPDATE_PATIENT_TASK_STATUS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
			programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
		}

		private DataTable MapPatientTasksToTable(List<TaskModel> tasks)
		{
			DataTable dataTable = new DataTable
			{
				Locale = CultureInfo.InvariantCulture,
				Columns =
				{
					new DataColumn(nameof(TaskModel.PatientTaskID), typeof(long)),
					new DataColumn(nameof(TaskModel.Status), typeof(string)),
				}
			};
			foreach (TaskModel record in tasks)
			{
				dataTable.Rows.Add(record.PatientTaskID, record.Status);
			}
			return dataTable;
		}
	}
}
