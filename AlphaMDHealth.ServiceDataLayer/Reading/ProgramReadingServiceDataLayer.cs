using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class ProgramReadingServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Program Reading Data
        /// </summary>
        /// <param name="programData">object to get data</param>
        /// <returns>operation status and list of reading category and reading types</returns>
        public async Task GetProgramReadingAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ProgramReadingID)), programData.ProgramReading.ProgramReadingID, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, AppPermissions.ProgramReadingsView.ToString(), $"{AppPermissions.ProgramReadingAddEdit},{AppPermissions.ProgramReadingDelete}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_READINGS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                programData.Items = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed && programData.ProgramReading.ProgramReadingID > 0)
                {
                    programData.ProgramReading = await result.ReadFirstAsync<ReadingModel>().ConfigureAwait(false);
                }
                await MapReturnPermissionsAsync(programData, result).ConfigureAwait(false);
            }
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Save Program Reading Data
        /// </summary>
        /// <param name="programData">Reference object which holds Program Reading Data</param>
        /// <returns>Operation Status Code And ProgramReadingID</returns>
        public async Task SaveProgramReadingAsync(ProgramDTO programData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), programData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ProgramReadingID)), programData.ProgramReading.ProgramReadingID, DbType.Int64, ParameterDirection.InputOutput);
            parameter.Add(ConcateAt(nameof(ReadingModel.ProgramID)), programData.ProgramReading.ProgramID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ReadingID)), programData.ProgramReading.ReadingID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.SequenceNo)), programData.ProgramReading.SequenceNo, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.IsActive)), programData.ProgramReading.IsActive, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.IsCritical)), programData.ProgramReading.IsCritical, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(programData, parameter, programData.ProgramReading.IsActive ? AppPermissions.ProgramReadingAddEdit.ToString() : AppPermissions.ProgramReadingDelete.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_READING, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            programData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (programData.ErrCode == ErrorCode.OK)
            {
                programData.ProgramReading.ProgramReadingID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(ReadingModel.ProgramReadingID));
            }
        }

        /// <summary>
        /// To get program reading metadata from database
        /// </summary>
        /// <param name="readingRangeData">Reference object</param>
        /// <returns>program Reading ranges data</returns>
        public async Task GetProgramReadingMetadataAsync(ReadingMasterDataDTO readingRangeData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), readingRangeData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(ReadingRangeModel.ProgramReadingID)), readingRangeData.ReadingMetadata.ProgramReadingID, DbType.Int32, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), 0, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(readingRangeData, parameters, AppPermissions.ProgramReadingAddEdit.ToString(), AppPermissions.ProgramReadingAddEdit.ToString());
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_READING_METADATA, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                readingRangeData.ReadingMetadatas = (await result.ReadAsync<ReadingModel>().ConfigureAwait(false))?.ToList();
                await MapReturnPermissionsAsync(readingRangeData, result).ConfigureAwait(false);
            }
            readingRangeData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Saves program reading metadata in Database
        /// </summary>
        /// <param name="readingMetadata">Object that contains reading metadata to be saved in database</param>
        /// <returns>operation status</returns>
        public async Task SaveProgramReadingMetadataAsync(ReadingMasterDataDTO readingMetadata)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), readingMetadata.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ProgramReadingID)), readingMetadata.ReadingMetadata.ProgramReadingID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.DigitsAfterDecimalPoint)), readingMetadata.ReadingMetadata.DigitsAfterDecimalPoint, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ReadingFrequency)), readingMetadata.ReadingMetadata.ReadingFrequency, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ValueAddedBy)), readingMetadata.ReadingMetadata.ValueAddedBy, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.AllowManualAdd)), readingMetadata.ReadingMetadata.AllowManualAdd, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.AllowHealthKitData)), readingMetadata.ReadingMetadata.AllowHealthKitData, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.AllowDeviceData)), readingMetadata.ReadingMetadata.AllowDeviceData, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ShowInGraph)), readingMetadata.ReadingMetadata.ShowInGraph, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ShowInData)), readingMetadata.ReadingMetadata.ShowInData, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.AllowDelete)), readingMetadata.ReadingMetadata.AllowDelete, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ShowInDifferentLines)), readingMetadata.ReadingMetadata.ShowInDifferentLines, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.SummaryRecordCount)), readingMetadata.ReadingMetadata.SummaryRecordCount, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ChartType)), readingMetadata.ReadingMetadata.ChartType, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.ReadingFilters)), readingMetadata.ReadingMetadata.ReadingFilters, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingModel.DaysOfPastRecordsToSync)), readingMetadata.ReadingMetadata.DaysOfPastRecordsToSync, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.RecordCount)), readingMetadata.RecordCount, DbType.Int64, ParameterDirection.Input);
            MapCommonSPParameters(readingMetadata, parameter, AppPermissions.ProgramReadingAddEdit.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PROGRAM_READING_METADATA, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            readingMetadata.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }

        /// <summary>
        /// To get program reading ranges from database
        /// </summary>
        /// <param name="rangeData">Reference object</param>
        /// <returns>program Reading ranges data</returns>
        public async Task GetProgramReadingRangesAsync(ReadingMasterDataDTO rangeData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), rangeData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(ReadingRangeModel.ProgramReadingID)), rangeData.ReadingRange.ProgramReadingID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(ReadingRangeModel.ReadingRangeID)), rangeData.ReadingRange.ReadingRangeID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), 0, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(rangeData, parameters, AppPermissions.ProgramReadingRangesView.ToString(),
                 $"{AppPermissions.ProgramReadingRangeAddEdit},{AppPermissions.ProgramReadingRangeDelete}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PROGRAM_READING_RANGES, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                rangeData.ReadingRanges = (await result.ReadAsync<ReadingRangeModel>().ConfigureAwait(false))?.ToList();
                await MapReturnPermissionsAsync(rangeData, result).ConfigureAwait(false);
            }
            rangeData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Saves program reading range in Database
        /// </summary>
        /// <param name="rangeData">Object that contains reading range to be saved in database</param>
        /// <returns>operation status</returns>
        public async Task SaveProgramReadingRangeAsync(ReadingMasterDataDTO rangeData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), rangeData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.ProgramReadingID)), rangeData.ReadingRange.ProgramReadingID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.ReadingRangeID)), rangeData.ReadingRange.ReadingRangeID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.ForGender)), rangeData.ReadingRange.ForGender, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.ForAgeGroup)), rangeData.ReadingRange.ForAgeGroup, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.FromAge)), rangeData.ReadingRange.FromAge, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.ToAge)), rangeData.ReadingRange.ToAge, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.AbsoluteMinValue)), rangeData.ReadingRange.AbsoluteMinValue, DbType.Decimal, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.AbsoluteMaxValue)), rangeData.ReadingRange.AbsoluteMaxValue, DbType.Decimal, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.AbsoluteBandColor)), rangeData.ReadingRange.AbsoluteBandColor, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.NormalMinValue)), rangeData.ReadingRange.NormalMinValue, DbType.Decimal, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.NormalMaxValue)), rangeData.ReadingRange.NormalMaxValue, DbType.Decimal, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.NormalBandColor)), rangeData.ReadingRange.NormalBandColor, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(ReadingRangeModel.TargetBandColor)), rangeData.ReadingRange.TargetBandColor, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.IsActive)), rangeData.ReadingRange.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(rangeData, parameter, rangeData.ReadingRange.IsActive ? AppPermissions.ProgramReadingAddEdit.ToString() : AppPermissions.ProgramReadingDelete.ToString());
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PROGRAM_READING_RANGE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                rangeData.ReadingRange.ReadingRangeID = (await result.ReadAsync<ReadingRangeModel>().ConfigureAwait(false)).FirstOrDefault().ReadingRangeID;
            }
            rangeData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }
    }
}