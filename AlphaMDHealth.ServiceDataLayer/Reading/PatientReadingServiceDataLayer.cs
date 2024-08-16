using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class PatientReadingServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// To get Organisation reading ranges and reading detail from database
    /// </summary>
    /// <param name="readingData">Reference object</param>
    /// <returns>Organisation Reading detail and reading ranges data</returns>
    public async Task GetPatientReadingsAsync(PatientReadingDTO readingData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), readingData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        if (readingData.PatientReadingID == Guid.Empty)
        {
            parameters.Add(ConcateAt(nameof(PatientReadingDTO.PatientReadingID)), DBNull.Value, DbType.Guid, ParameterDirection.Input);
        }
        else
        {
            parameters.Add(ConcateAt(nameof(PatientReadingDTO.PatientReadingID)), readingData.PatientReadingID, DbType.Guid, ParameterDirection.Input);
        }
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.ReadingID)), readingData.ReadingID, DbType.Int16, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.SelectedListItemReadingID)), readingData.SelectedListItemReadingID, DbType.Int16, ParameterDirection.Input);
        AddDateTimeParameter(nameof(BaseDTO.FromDate), readingData.FromDate, parameters, ParameterDirection.InputOutput);
        AddDateTimeParameter(nameof(BaseDTO.ToDate), readingData.ToDate, parameters, ParameterDirection.InputOutput);
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.IsMedicalHistory)), readingData.IsMedicalHistory, DbType.Boolean, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.IsCommingFromQuestionnaireTaskPage)), readingData.IsCommingFromQuestionnaireTaskPage, DbType.Boolean, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.SelectedUserID)), readingData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.ReadingID)), readingData.ReadingID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientReadingDTO.ReadingCategoryID)), readingData.ReadingCategoryID, DbType.Int16, direction: ParameterDirection.InputOutput, 20);
        MapCommonSPParameters(readingData, parameters, CheckPermission(readingData.RecordCount), ReturnPermissions(readingData.RecordCount));
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_READINGS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            await MapPatientReadingsViewDataAsync(readingData, result).ConfigureAwait(false);
            await MapReturnPermissionsAsync(readingData, result).ConfigureAwait(false);
        }
        readingData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    /// <summary>
    /// To get Patient's Scan Vitals Data from database
    /// </summary>
    /// <param name="vitalData">Reference object</param>
    /// <returns>Patient's Scan Vitals Data</returns>
    public async Task GetPatientScanVitalsDataAsync(PatientScanVitalDTO vitalsData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), vitalsData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientScanVitalDTO.AccountID)), vitalsData.AccountID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientScanVitalDTO.SelectedUserID)), vitalsData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.ErrCode)), vitalsData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        MapCommonSPParameters(vitalsData, parameters, CheckPermission(vitalsData.RecordCount), ReturnPermissions(vitalsData.RecordCount));
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_SCAN_VITALS_DATA, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            var userResult = await result.ReadAsync<PatientScanVital>().ConfigureAwait(false);
            var user = userResult.FirstOrDefault();
            if (user != null)
            {
                vitalsData.PatientScanVital.DateOfBirth = user.DateOfBirth;
                vitalsData.PatientScanVital.GenderID = user.GenderID;
            }

            var heightValue = (await result.ReadAsync<decimal?>().ConfigureAwait(false))?.FirstOrDefault();
            if (heightValue.HasValue)
            {
                vitalsData.PatientScanVital.Height = (double)heightValue.Value;
            }
            var weightValue = (await result.ReadAsync<decimal?>().ConfigureAwait(false))?.FirstOrDefault();
            if (weightValue.HasValue)
            {
                vitalsData.PatientScanVital.Weight = (double)weightValue.Value;
            }
        }
        vitalsData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    /// <summary>
    /// Save Patient readings data to database
    /// </summary>
    /// <param name="readingData">Patient readings to be saved</param>
    /// <returns>operation status</returns>
    public async Task SavePatientScanVitalsDataAsync(PatientScanVitalDTO vitalData)
    {
       // using var connection = ConnectDatabase();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), vitalData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientScanVitalDTO.AccountID)), vitalData.AccountID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientScanVitalDTO.SelectedUserID)), vitalData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientScanVitalDTO.PatientScanVital.Weight)), vitalData.PatientScanVital.Weight, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(PatientScanVitalDTO.PatientScanVital.Height)), vitalData.PatientScanVital.Height, DbType.Int64, ParameterDirection.Input);
        MapCommonSPParameters(vitalData, parameters, AppPermissions.PatientReadingAddEdit.ToString());
       // SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_SCAN_VITALS_DATA, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        vitalData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);

        using var connection = ConnectDatabase();
        //DynamicParameters parameter = new DynamicParameters();
        //parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), readingData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        //parameter.Add(ConcateAt(nameof(BaseDTO.RecordCount)), readingData.RecordCount, DbType.Int16, direction: ParameterDirection.Input);
        //parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapPatientReadingsToTable(readingData).AsTableValuedParameter());
        //parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS_2), MapPatientReadingSourcesToTable(readingData).AsTableValuedParameter());
        //MapCommonSPParameters(readingData, parameter, AppPermissions.PatientReadingAddEdit.ToString());
        //SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PATIENT_READINGS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        //if (result.HasRows())
        //{
        //    readingData.SaveReadingSources = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
        //    if (result.HasRows())
        //    {
        //        readingData.SaveReadings = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
        //    }
        //}
        //readingData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
    }

    /// <summary>
    /// Save Patient readings data to database
    /// </summary>
    /// <param name="readingData">Patient readings to be saved</param>
    /// <returns>operation status</returns>
    public async Task SavePatientReadingsAsync(PatientReadingDTO readingData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), readingData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.RecordCount)), readingData.RecordCount, DbType.Int16, direction: ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapPatientReadingsToTable(readingData).AsTableValuedParameter());
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS_2), MapPatientReadingSourcesToTable(readingData).AsTableValuedParameter());
        MapCommonSPParameters(readingData, parameter, AppPermissions.PatientReadingAddEdit.ToString());
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PATIENT_READINGS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            readingData.SaveReadingSources = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
            if (result.HasRows())
            {
                readingData.SaveReadings = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
            }
        }
        readingData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
    }

    /// <summary>
    /// Save user reading target data to database
    /// </summary>
    /// <param name="targetData">Reading target to be saved</param>
    /// <returns>operation status</returns>
    public async Task SavePatientReadingTargetsAsync(PatientReadingDTO targetData)
    {
        using var connection = ConnectDatabase();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), targetData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapToPatientTargetTable(targetData.PatientReadingTargets).AsTableValuedParameter());
        MapCommonSPParameters(targetData, parameter, AppPermissions.PatientReadingTargetAddEdit.ToString());
        await connection.QueryAsync(SPNameConstants.USP_SAVE_PATIENT_READING_TARGETS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        targetData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
    }

    private DataTable MapToPatientTargetTable(List<ReadingTargetModel> targets)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(ReadingTargetModel.ReadingID), typeof(Int64)),
                new DataColumn(nameof(ReadingTargetModel.UserID), typeof(Int64)),
                new DataColumn(nameof(ReadingTargetModel.TargetMinValue), typeof(Decimal)),
                new DataColumn(nameof(ReadingTargetModel.TargetMaxValue), typeof(Decimal)),
                new DataColumn(nameof(ReadingTargetModel.IsActive), typeof(Boolean)),
            }
        };
        foreach (ReadingTargetModel item in targets)
        {
            dataTable.Rows.Add(item.ReadingID, item.UserID, item.TargetMinValue, item.TargetMaxValue, item.IsActive);
        }
        return dataTable;
    }

    internal async Task MapPatientReadingsViewDataAsync(PatientReadingDTO readingData, SqlMapper.GridReader result)
    {
        //// RecordCount = -3 : Get Data for Patient Reading Target Add Edit Page
        if (readingData.RecordCount == -3)
        {
            readingData.ChartMetaData = await MapTableDataAsync<ReadingMetadataUIModel>(result).ConfigureAwait(false);
        }
        
        //// RecordCount = -1 : Get Data for Patient Reading Add Edit Page
        //// RecordCount = -2 : Get Data for Patient Reading Details View
        //// RecordCount = 0 : Get Data for Patient Readings List Page
        //// RecordCount > 0 : Get Data for Patient Readings Dashboard List View
        else
        {
            if (readingData.RecordCount == -4)
            {
                readingData.User = (await result.ReadAsync<UserModel>().ConfigureAwait(false)).FirstOrDefault();
                readingData.ChartMetaData = await MapTableDataAsync<ReadingMetadataUIModel>(result).ConfigureAwait(false);
            }
            if (readingData.RecordCount > -2)
            {
                readingData.FilterOptions = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
            }
            readingData.ChartMetaData = await MapTableDataAsync<ReadingMetadataUIModel>(result).ConfigureAwait(false);
            if (readingData.RecordCount < 0)
            {
                readingData.ListData = await MapTableDataAsync<PatientReadingUIModel>(result).ConfigureAwait(false);
            }
        }
        readingData.ReadingUnits = await MapTableDataAsync<UnitModel>(result).ConfigureAwait(false);
        readingData.UserAccountSettings = await MapTableDataAsync<UserAccountSettingsModel>(result).ConfigureAwait(false);
    }

    private DataTable MapPatientReadingsToTable(PatientReadingDTO readingData)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(PatientReadingModel.PatientReadingID), typeof(Guid)),
                new DataColumn(nameof(PatientReadingModel.UserID), typeof(Int64)),
                new DataColumn(nameof(PatientReadingModel.PatientTaskID), typeof(Int64)),
                new DataColumn(nameof(PatientReadingModel.ReadingID), typeof(Int16)),
                new DataColumn(nameof(PatientReadingModel.ReadingDateTime), typeof(DateTimeOffset)),
                new DataColumn(nameof(PatientReadingModel.ReadingFrequency), typeof(Int64)),
                new DataColumn(nameof(PatientReadingModel.ReadingValue), typeof(double)),
                new DataColumn(nameof(PatientReadingModel.ReadingValue2), typeof(string)),
                new DataColumn(nameof(PatientReadingModel.ReadingNotes), typeof(string)),
                new DataColumn(nameof(PatientReadingModel.ReadingSourceType), typeof(string)),
                new DataColumn(nameof(PatientReadingModel.ReadingSourceID), typeof(Guid)),
                new DataColumn(nameof(PatientReadingModel.SourceName), typeof(string)),
                new DataColumn(nameof(PatientReadingModel.SourceQuantity), typeof(float)),
                new DataColumn(nameof(PatientReadingModel.IsActive), typeof(bool)),
                new DataColumn(nameof(PatientReadingModel.AddedON), typeof(DateTimeOffset)),
                new DataColumn(nameof(PatientReadingModel.LastModifiedON), typeof(DateTimeOffset)),
                new DataColumn(nameof(PatientReadingModel.AddedByID), typeof(Int64)),
                new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(Int16))
            }
        };
        int count = 1;
        foreach (PatientReadingModel record in readingData.PatientReadings)
        {
            dataTable.Rows.Add(
                record.PatientReadingID,
                record.UserID,
                record.PatientTaskID,
                record.ReadingID,
                record.ReadingDateTime,
                record.ReadingFrequency,
                record.ReadingValue.HasValue ? record.ReadingValue : null,
                record.ReadingValue2,
                record.ReadingNotes, record.ReadingSourceType,
                record.ReadingSourceID == default(Guid) ? null : record.ReadingSourceID,
                record.SourceName, record.SourceQuantity,
                record.IsActive, record.AddedON, record.LastModifiedON, record.AddedByID, count++);
        }
        return dataTable;
    }

    private string CheckPermission(long recordCount)
    {
        return recordCount switch
        {
            -1 => AppPermissions.PatientReadingsView.ToString(),
            -2 => AppPermissions.PatientReadingDetailsView.ToString(),
            -3 => AppPermissions.PatientReadingTargetAddEdit.ToString(),
            -4 => AppPermissions.ReadingScanVitals.ToString(),
            _ => AppPermissions.PatientReadingsView.ToString(),
        };
    }

    private string ReturnPermissions(long recordCount)
    {
        return recordCount switch
        {
            -1 => $"{AppPermissions.PatientReadingAddEdit},{AppPermissions.PatientReadingDelete}",
            -2 => $"{AppPermissions.PatientReadingAddEdit},{AppPermissions.PatientReadingTargetAddEdit}",
            -3 => AppPermissions.PatientReadingTargetAddEdit.ToString(),
            -4 => AppPermissions.ReadingScanVitals.ToString(),
            _ => $"{AppPermissions.PatientReadingAddEdit},{AppPermissions.PatientReadingDetailsView},{AppPermissions.PatientReadingTargetAddEdit}",
        };
    }

    private DataTable MapPatientReadingSourcesToTable(PatientReadingDTO readingSources)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(PatientDeviceModel.ReadingSourceID), typeof(Guid)),
                new DataColumn(nameof(PatientDeviceModel.UserID), typeof(Int64)),
                new DataColumn(nameof(PatientDeviceModel.DeviceIdentifier), typeof(string)),
                new DataColumn(nameof(PatientDeviceModel.DeviceSerialNo), typeof(string)),
                new DataColumn(nameof(PatientDeviceModel.DeviceFirmwareVersion), typeof(string)),
                new DataColumn(nameof(PatientDeviceModel.LastReadingID), typeof(string)),
                new DataColumn(nameof(PatientReadingModel.IsActive), typeof(bool)),
                new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(int))
            }
        };
        int count = 1;
        if (readingSources?.PatientReadingDevices != null)
        {
            foreach (PatientDeviceModel source in readingSources?.PatientReadingDevices)
            {
                dataTable.Rows.Add(source.ReadingSourceID, source.UserID, source.DeviceIdentifier, source.DeviceSerialNo, source.DeviceFirmwareVersion,
                    source.LastReadingID, source.IsActive, count++);
            }
        }
        return dataTable;
    }
}