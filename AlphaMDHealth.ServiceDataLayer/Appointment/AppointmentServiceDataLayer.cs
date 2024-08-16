using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer;

public class AppointmentServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Appointments
    /// </summary>
    /// <param name="appointmentData">Reference object to return list of Appointments</param>
    /// <returns>List of Appointments</returns>
    public async Task GetAppointmentsAsync(AppointmentDTO appointmentData)
    {
        GetAppointmentPermissionCheck(appointmentData.RecordCount, appointmentData.SelectedUserID, out string permissionCheck, out string permissionRequest);
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), appointmentData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.SelectedUserID), appointmentData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.AppointmentID), appointmentData.Appointment.AppointmentID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), appointmentData.LastModifiedON.Value, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(appointmentData, parameter, permissionCheck, permissionRequest);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_APPOINTMENTS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            await MapAppointmentRecordsAsync(appointmentData, result).ConfigureAwait(false);
            await MapReturnPermissionsAsync(appointmentData, result).ConfigureAwait(false);
        }
        appointmentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Save Appointment
    /// </summary>
    /// <param name="appointmentData">Object which contains Appointment to save</param>
    /// <returns>Operation Status</returns>
    public async Task SaveAppointmentAsync(AppointmentDTO appointmentData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), appointmentData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.IsRepeatRequest), appointmentData.Appointment.IsRepeatRequest, DbType.Boolean, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentDTO.IsExternalParticipant), appointmentData.IsExternalParticipant, DbType.Boolean, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapParticipantsToTable(appointmentData).AsTableValuedParameter());
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_2, MapAppointmentsToTable(appointmentData).AsTableValuedParameter());
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.AppointmentID), appointmentData.Appointment.AppointmentID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.FromDateTime), appointmentData.Appointment.FromDateTime, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.ToDateTime), appointmentData.Appointment.ToDateTime, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.AppointmentTypeID), appointmentData.Appointment.AppointmentTypeID, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.AppointmentAddress), appointmentData.Appointment.AppointmentAddress, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.AppointmentStatusID), appointmentData.Appointment.AppointmentStatusID, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AddedON), appointmentData.AddedON, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ParticipantsModel.FirstName), appointmentData.ExternalParticipant.FirstName, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ParticipantsModel.MobileNo), appointmentData.ExternalParticipant.MobileNo, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(ParticipantsModel.EmailID), appointmentData.ExternalParticipant.EmailID?.ToLowerInvariant(), DbType.String, ParameterDirection.Input);
        MapCommonSPParameters(appointmentData, parameter, AppPermissions.AppointmentAddEdit.ToString());
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_APPOINTMENT, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            List<ParticipantsModel> duplicateParticipant = (await result.ReadAsync<ParticipantsModel>().ConfigureAwait(false))?.ToList();
            if (duplicateParticipant?.Count > 0)
            {
                appointmentData.AppointmentParticipants = duplicateParticipant;
            }
            if (!result.IsConsumed)
            {
                appointmentData.InviteParticipants = (await result.ReadAsync<ParticipantsModel>().ConfigureAwait(false))?.ToList();
            }
        }
        appointmentData.Appointment.AppointmentID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(AppointmentModel.AppointmentID));
        appointmentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    /// <summary>
    /// Update appointment status
    /// </summary>
    /// <param name="appointmentData">Object which contains Appointment to update</param>
    /// <returns>Operation Status</returns>
    public async Task UpdateAppointmentStatusAsync(AppointmentDTO appointmentData)
    {
        UpdateAppointmentPermissionCheck(appointmentData.Appointment.AppointmentStatusID, out string permissionCheck);
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), appointmentData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(AppointmentModel.AppointmentID)), appointmentData.Appointment.AppointmentID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(AppointmentModel.AppointmentStatusID)), appointmentData.Appointment.AppointmentStatusID, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), appointmentData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), appointmentData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(appointmentData, parameter, permissionCheck);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_UPDATE_APPOINTMENT_STATUS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            appointmentData.AppointmentParticipants = (await result.ReadAsync<ParticipantsModel>().ConfigureAwait(false))?.ToList();
        }
        appointmentData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
    }

    private async Task MapAppointmentRecordsAsync(AppointmentDTO appointmentData, SqlMapper.GridReader result)
    {
        switch (appointmentData.RecordCount)
        {
            case -2:
                appointmentData.Appointment = (await result.ReadAsync<AppointmentModel>().ConfigureAwait(false)).FirstOrDefault();
                appointmentData.AppointmentParticipants = (await result.ReadAsync<ParticipantsModel>().ConfigureAwait(false))?.ToList();
                break;
            case -1:
                if (appointmentData.Appointment.AppointmentID > 0)
                {
                    appointmentData.Appointment = (await result.ReadAsync<AppointmentModel>().ConfigureAwait(false)).FirstOrDefault();
                }
                appointmentData.AppointmentDetails = (await result.ReadAsync<ContentDetailModel>().ConfigureAwait(false))?.ToList();
                appointmentData.AppointmentParticipants = (await result.ReadAsync<ParticipantsModel>().ConfigureAwait(false))?.ToList();

                appointmentData.ExternalParticipant = (await result.ReadAsync<ParticipantsModel>().ConfigureAwait(false)).FirstOrDefault();
                break;
            default:
                appointmentData.Appointments = (await result.ReadAsync<AppointmentModel>().ConfigureAwait(false))?.ToList();
                break;
        }
    }

    private DataTable MapParticipantsToTable(AppointmentDTO appointmentData)
    {
        DataTable dataTable = CreateGenericTypeTable();
        if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentParticipants))
        {
            foreach (ParticipantsModel record in appointmentData.AppointmentParticipants)
            {
                dataTable.Rows.Add(record.ParticipantID, Guid.Empty, (long)0, appointmentData.Appointment.AppointmentStatusID, string.Empty, string.Empty);
            }
        }
        return dataTable;
    }

    private DataTable MapAppointmentsToTable(AppointmentDTO appointmentData)
    {
        DataTable dataTable = CreateGenericTypeTable();
        if (GenericMethods.IsListNotEmpty(appointmentData.AppointmentDetails))
        {
            foreach (ContentDetailModel record in appointmentData.AppointmentDetails)
            {
                dataTable.Rows.Add(record.PageID, Guid.Empty, record.LanguageID, record.PageHeading, record.PageData, string.Empty);
            }
        }
        return dataTable;
    }

    private void GetAppointmentPermissionCheck(long recordCount, long selectedUserID, out string permissionCheck, out string permissionRequest)
    {
        if (recordCount == -1)
        {
            permissionCheck = AppPermissions.AppointmentAddEdit.ToString();
            permissionRequest = $"{AppPermissions.AppointmentAddEdit},{AppPermissions.AppointmentCancelMeeting}";
        }
        else if (recordCount == -2)
        {
            permissionCheck = AppPermissions.AppointmentView.ToString();
            permissionRequest = $"{AppPermissions.AppointmentAddEdit},{AppPermissions.AppointmentJoin},{AppPermissions.AppointmentDecline}";
        }
        else
        {
            permissionCheck = selectedUserID > 0 ? AppPermissions.PatientAppointmentsView.ToString() : AppPermissions.AppointmentsView.ToString();
            permissionRequest = $"{AppPermissions.AppointmentView},{AppPermissions.AppointmentAddEdit}";
        }
    }

    private void UpdateAppointmentPermissionCheck(string statusKey, out string permissionCheck)
    {
        if (statusKey == ResourceConstants.R_REJECTED_STATUS_KEY)
        {
            permissionCheck = AppPermissions.AppointmentDecline.ToString();
        }
        else if (statusKey == ResourceConstants.R_CANCELLED_STATUS_KEY)
        {
            permissionCheck = AppPermissions.AppointmentCancelMeeting.ToString();
        }
        else
        {
            permissionCheck = string.Empty;
        }
    }
}
