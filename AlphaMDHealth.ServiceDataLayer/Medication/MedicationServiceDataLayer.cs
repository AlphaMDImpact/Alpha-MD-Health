using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class MedicationServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get patient medicines data for searched name
    /// </summary>
    /// <param name="medicationData">Reference object having input data </param>
    /// <returns>List of medicines with operation status</returns>
    public async Task GetMedicinesAsync(PatientMedicationDTO medicationData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), medicationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(MedicineModel.ShortName)), medicationData.Medication.ShortName, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
        MapCommonSPParameters(medicationData, parameter, string.Empty);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR), string.Empty, DbType.String, ParameterDirection.Input);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MEDICINES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            medicationData.Medicines = (await result.ReadAsync<MedicineModel>().ConfigureAwait(false))?.ToList();
            await MapReturnPermissionsAsync(medicationData, result).ConfigureAwait(false);
        }
        medicationData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Get medications for program/patient
    /// </summary>
    /// <param name="medicationData">Reference object to return medications data with operation status</param>
    /// <returns>Medications data with operation status</returns>
    public async Task GetMedicationsAsync(PatientMedicationDTO medicationData)
    {
        GetPatientMedicationPermissionData(medicationData.RecordCount, medicationData.Medication?.ProgramID > 0, out string permissionCheck, out string permissionRequest);
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), medicationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), medicationData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientMedicationModel.ProgramID)), medicationData.Medication.ProgramID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientMedicationModel.ProgramMedicationID)), medicationData.Medication.ProgramMedicationID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientMedicationModel.PatientMedicationID)), medicationData.Medication.PatientMedicationID, DbType.Guid, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), medicationData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientMedicationDTO.IsMedicalHistory)), medicationData.IsMedicalHistory, DbType.Boolean, ParameterDirection.Input);
        AddDateTimeParameter(nameof(BaseDTO.FromDate), medicationData.FromDate, parameters, ParameterDirection.Input);
        AddDateTimeParameter(nameof(BaseDTO.ToDate), medicationData.ToDate, parameters, ParameterDirection.Input);
        MapCommonSPParameters(medicationData, parameters, permissionCheck, permissionRequest);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_MEDICATIONS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            await MapPatientMedicationsViewDataAsync(medicationData, result).ConfigureAwait(false);
            await MapReturnPermissionsAsync(medicationData, result).ConfigureAwait(false);
        }
        medicationData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    internal async Task MapPatientMedicationsViewDataAsync(PatientMedicationDTO medicationData, SqlMapper.GridReader result)
    {
        medicationData.Medications = (await result.ReadAsync<PatientMedicationModel>().ConfigureAwait(false))?.ToList();
        
        // Map Patient Medication for Add/Edit page
        if (medicationData.RecordCount == -1)
        {
            if (medicationData.Medication?.ProgramID < 1 && !result.IsConsumed)
            {
                medicationData.Reminders = (await result.ReadAsync<MedicationReminderModel>().ConfigureAwait(false))?.ToList();
            }
            medicationData.UnitOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
        }
    }

    /// <summary>
    /// Save medication for patient/program
    /// </summary>
    /// <param name="medicationData">Reference object which holds medication data</param>
    /// <returns>Operation Status Code</returns>
    public async Task SaveMedicationAsync(PatientMedicationDTO medicationData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), medicationData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapMedicationsToTable(medicationData.Medications).AsTableValuedParameter());
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS_2), MapMedicationRemindersTypeToTable(medicationData.Reminders).AsTableValuedParameter());
        parameter.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), medicationData?.Medications[0].AddedByID != null ? medicationData?.Medications[0].AddedByID : medicationData?.SelectedUserID.ToString(), DbType.Int64, ParameterDirection.Input);
        var checkPermission = medicationData.Medications?.FirstOrDefault()?.ProgramID > 0 && medicationData.FeatureFor == 2 ? AppPermissions.ProgramMedicationAddEdit.ToString() : AppPermissions.PatientMedicationAddEdit.ToString();
        MapCommonSPParameters(medicationData, parameter, checkPermission);
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_MEDICATIONS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            medicationData.SaveMedications = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
        }
        medicationData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
    }

    private void GetPatientMedicationPermissionData(long recordCount, bool isProgramFlow, out string permissionCheck, out string permissionRequest)
    {
        // For Patient/Program Medication Add/Edit page
        permissionCheck = isProgramFlow
                ? AppPermissions.ProgramMedicationsView.ToString()
                : AppPermissions.PatientMedicationsView.ToString();
        if (recordCount == -1)
        {
            permissionRequest = isProgramFlow
                ? $"{AppPermissions.ProgramMedicationAddEdit},{AppPermissions.ProgramMedicationDelete}"
                : $"{AppPermissions.PatientMedicationAddEdit},{AppPermissions.PatientMedicationDelete}";
        }
        // For Patient/Program Medication list page
        else
        {
            permissionRequest = isProgramFlow
                ? AppPermissions.ProgramMedicationAddEdit.ToString()
                : AppPermissions.PatientMedicationAddEdit.ToString();
        }
    }

    private DataTable MapMedicationsToTable(List<PatientMedicationModel> documents)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(PatientMedicationModel.ProgramID), typeof(long)),
                new DataColumn(nameof(PatientMedicationModel.ProgramMedicationID), typeof(long)),
                new DataColumn(nameof(PatientMedicationModel.AssignAfterDays), typeof(long)),
                new DataColumn(nameof(PatientMedicationModel.AssignForDays), typeof(long)),
                new DataColumn(nameof(PatientMedicationModel.PatientMedicationID), typeof(Guid)),
                new DataColumn(nameof(PatientMedicationModel.PatientID), typeof(long)),
                new DataColumn(nameof(PatientMedicationModel.ShortName), typeof(string)),
                new DataColumn(nameof(PatientMedicationModel.UnitIdentifier), typeof(string)),
                new DataColumn(nameof(PatientMedicationModel.Doses), typeof(decimal)),
                new DataColumn(nameof(PatientMedicationModel.Frequency), typeof(string)),
                new DataColumn(nameof(PatientMedicationModel.HowOften), typeof(int)),
                new DataColumn(nameof(PatientMedicationModel.AfterDays), typeof(byte)),
                new DataColumn(nameof(PatientMedicationModel.StartDate), typeof(DateTimeOffset)),
                new DataColumn(nameof(PatientMedicationModel.EndDate), typeof(DateTimeOffset)),
                new DataColumn(nameof(PatientMedicationModel.Note), typeof(string)),
                new DataColumn(nameof(PatientMedicationModel.AdditionalNotes), typeof(string)),
                new DataColumn(nameof(PatientMedicationModel.IsCritical), typeof(bool)),
                new DataColumn(nameof(PatientMedicationModel.Reminder), typeof(bool)),
                new DataColumn(nameof(PatientMedicationModel.IsActive), typeof(bool)),
                new DataColumn(nameof(PatientMedicationModel.AddedOn), typeof(DateTimeOffset)),
                new DataColumn(nameof(PatientMedicationModel.AddedByID), typeof(string)),
                new DataColumn(nameof(PatientMedicationModel.IsReadOnly), typeof(bool)),
                new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(short)),
            }
        };
        short count = 1;
        if (GenericMethods.IsListNotEmpty(documents))
        {
            foreach (PatientMedicationModel record in documents)
            {
                dataTable.Rows.Add(record.ProgramID, record.ProgramMedicationID, record.AssignAfterDays, record.AssignForDays,
                    record.PatientMedicationID, record.PatientID, record.ShortName, record.UnitIdentifier, record.Doses, record.Frequency, record.HowOften,
                    record.AfterDays, record.StartDate, record.EndDate, record.Note, record.AdditionalNotes, record.IsCritical, record.Reminder, record.IsActive, record.AddedOn, record.AddedByID, record.IsReadOnly, count++);
            }
        }
        return dataTable;
    }

    private DataTable MapMedicationRemindersTypeToTable(List<MedicationReminderModel> reminders)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(MedicationReminderModel.PatientMedicationID), typeof(Guid)),
                new DataColumn(nameof(MedicationReminderModel.ReminderDateTime), typeof(DateTimeOffset)),
                new DataColumn(nameof(MedicationReminderModel.IsActive), typeof(bool)),
                new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(short)),
            }
        };
        if (GenericMethods.IsListNotEmpty(reminders))
        {
            short count = 1;
            foreach (MedicationReminderModel record in reminders)
            {
                dataTable.Rows.Add(record.PatientMedicationID, record.ReminderDateTime, record.IsActive, count++);
            }
        }
        return dataTable;
    }
}
