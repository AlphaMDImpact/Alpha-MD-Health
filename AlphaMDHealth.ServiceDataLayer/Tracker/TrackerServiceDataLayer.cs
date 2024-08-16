using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class TrackerServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Trackers assigned to patient
        /// </summary>
        /// <param name="trackerData">Object in which data will be passed</param>
        /// <returns>Patient Tracker Data</returns>
        public async Task GetPatientTrackersAsync(TrackerDTO trackerData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), trackerData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(PatientTrackersModel.PatientTrackerID)), trackerData.PatientTracker?.PatientTrackerID, DbType.Guid, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(PatientTrackersModel.TrackerName)), trackerData.PatientTracker?.TrackerName, DbType.String, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.SelectedUserID)), trackerData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
            AddDateTimeParameter(nameof(BaseDTO.FromDate), trackerData.FromDate, parameters, ParameterDirection.Input);
            AddDateTimeParameter(nameof(BaseDTO.ToDate), trackerData.ToDate, parameters, ParameterDirection.Input);
            MapCommonSPParameters(trackerData, parameters, AppPermissions.PatientTrackersView.ToString(),
                 $"{AppPermissions.PatientTrackerDelete},{AppPermissions.PatientTrackerAddEdit}");
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_TRACKERS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                await MapPatientTrackersViewDataAsync(trackerData, result).ConfigureAwait(false);
                await MapReturnPermissionsAsync(trackerData, result).ConfigureAwait(false);
            }
            trackerData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        internal async Task MapPatientTrackersViewDataAsync(TrackerDTO trackerData, SqlMapper.GridReader result)
        {
            if (trackerData.RecordCount == -1)
            {
                trackerData.TrackerTypes = (await result.ReadAsync<OptionModel>().ConfigureAwait(false)).ToList();
                trackerData.PatientTracker = (await result.ReadAsync<PatientTrackersModel>().ConfigureAwait(false)).FirstOrDefault();
            }
            else if (trackerData.RecordCount == -3)
            {
                trackerData.PatientTracker = (await result.ReadAsync<PatientTrackersModel>().ConfigureAwait(false)).FirstOrDefault();
                trackerData.TrackerRanges = (await result.ReadAsync<TrackerRangeModel>().ConfigureAwait(false)).ToList();
            }
            else
            {
                trackerData.PatientTrackers = (await result.ReadAsync<PatientTrackersModel>().ConfigureAwait(false)).ToList();
            }
        }

        /// <summary>
        /// Save patient tracker Data
        /// </summary>
        /// <param name="trackerData">Reference object which holds patient tracker Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SavePatientTrackerAsync(TrackerDTO trackerData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), trackerData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.PatientTracker.PatientTrackerID)), trackerData.PatientTracker.PatientTrackerID, DbType.Guid, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.PatientTracker.UserID)), trackerData.PatientTracker.UserID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.PatientTracker.TrackerID)), trackerData.PatientTracker.TrackerID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.PatientTracker.FromDate)), GenericMethods.ApplyUtcDateTimeFormatToUtcValue(trackerData.PatientTracker.FromDate.Value), DbType.DateTimeOffset, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.PatientTracker.ToDate)), GenericMethods.ApplyUtcDateTimeFormatToUtcValue(trackerData.PatientTracker.ToDate.Value), DbType.DateTimeOffset, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.IsActive)), trackerData.PatientTracker.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), trackerData.PatientTracker.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(trackerData, parameter, AppPermissions.PatientTrackerAddEdit.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PATIENT_TRACKERS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            trackerData.PatientTracker.PatientTrackerID = parameter.Get<Guid>(ConcateAt(nameof(TrackerDTO.PatientTracker.PatientTrackerID)));
            trackerData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }

        /// <summary>
        /// Save patient tracker Value
        /// </summary>
        /// <param name="trackerData">Reference object which holds patient tracker Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SavePatientTrackerValueAsync(TrackerDTO trackerData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), trackerData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.PatientTracker.PatientTrackerID)), trackerData.PatientTrackerValue.PatientTrackerID, DbType.Guid, ParameterDirection.InputOutput);
            parameter.Add(ConcateAt(nameof(TrackerDTO.PatientTracker.CurrentValue)), trackerData.PatientTrackerValue.CurrentValue, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TrackerDTO.IsActive)), trackerData.PatientTrackerValue.IsActive, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), trackerData.PatientTrackerValue.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            MapCommonSPParameters(trackerData, parameter, AppPermissions.PatientTrackerAddEdit.ToString());
            await connection.QueryAsync(SPNameConstants.USP_SAVE_PATIENT_TRACKER_VALUE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            trackerData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }

        /// <summary>
        /// Get Trackers Data
        /// </summary>
        /// <param name="trackerData">Reference object which holds tracker Data</param>
        /// <returns>Tracker Data</returns>
        public async Task GetTrackersAsync(TrackerDTO trackerData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), trackerData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(TrackersModel.TrackerID)), trackerData?.Tracker?.TrackerID, DbType.Int16, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(TrackerRangeModel.TrackerRangeID)), trackerData?.TrackerRange?.TrackerRangeID, DbType.Int16, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), DBNull.Value, DbType.DateTimeOffset, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.ErrCode)), trackerData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            MapCommonSPParameters(trackerData, parameters, AppPermissions.TrackersView.ToString(), 
                $"{AppPermissions.TrackerDelete},{AppPermissions.TrackersAddEdit},{AppPermissions.TrackerRangesView},{AppPermissions.TrackerRangesAddEdit},{AppPermissions.TrackerRangesDelete}"
            );
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_TRACKERS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                switch (trackerData.RecordCount)
                {
                    case -1:
                        trackerData.Tracker = (await result.ReadAsync<TrackersModel>().ConfigureAwait(false)).FirstOrDefault();
                        trackerData.TrackersI18N = (await result.ReadAsync<TrackersI18NModel>().ConfigureAwait(false)).ToList();
                        trackerData.TrackerRanges = (await result.ReadAsync<TrackerRangeModel>().ConfigureAwait(false)).ToList();
                        break;
                    case -2:
                        trackerData.TrackerRange = (await result.ReadAsync<TrackerRangeModel>().ConfigureAwait(false))?.FirstOrDefault();
                        trackerData.TrackerRangesI18N = (await result.ReadAsync<TrackerRangesI18N>().ConfigureAwait(false))?.ToList();
                        break;
                    default:
                        trackerData.Trackers = (await result.ReadAsync<TrackersModel>().ConfigureAwait(false))?.ToList();
                        break;
                }
                await MapReturnPermissionsAsync(trackerData, result).ConfigureAwait(false);
            }
            trackerData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }


        /// <summary>
        ///  Save tracker Data
        /// </summary>
        /// <param name="trackerData">Reference object which holds tracker Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SaveTrackerAsync(TrackerDTO trackerData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), trackerData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackersModel.TrackerID), trackerData.Tracker.TrackerID, DbType.Int16, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackersModel.TrackerName), trackerData.Tracker.TrackerIdentifier, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackersModel.TrackerTypeID), trackerData.Tracker.TrackerTypeID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapTrackerI18NToTable(trackerData.Tracker.TrackerID, trackerData.TrackersI18N).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackersModel.IsActive), trackerData.Tracker.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(trackerData, parameter, AppPermissions.TrackersAddEdit.ToString());
            await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_TRACKER, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            trackerData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (trackerData.ErrCode == ErrorCode.OK)
            {
                trackerData.Tracker.TrackerID = parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(trackerData.Tracker.TrackerID));
            }
        }

        /// <summary>
        /// Save tracker ranges Data
        /// </summary>
        /// <param name="trackerData">Reference object which holds tracker Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SaveTrackerRangesAsync(TrackerDTO trackerData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), trackerData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackerRangeModel.TrackerRangeID), trackerData.TrackerRange.TrackerRangeID, DbType.Int16, ParameterDirection.InputOutput);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackerRangeModel.TrackerID), trackerData.TrackerRange.TrackerID, DbType.Int16, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackerRangeModel.FromValue), trackerData.TrackerRange.FromValue, DbType.Int32, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackerRangeModel.ToValue), trackerData.TrackerRange.ToValue, DbType.Int32, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(TrackerRangeModel.ImageName), trackerData.TrackerRange.ImageName, DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapTrackerRangesI18NToTable(trackerData.TrackerRange.TrackerRangeID, trackerData.TrackerRangesI18N).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), trackerData.TrackerRange.IsActive, DbType.Boolean, ParameterDirection.Input);
            MapCommonSPParameters(trackerData, parameter, AppPermissions.TrackerRangesAddEdit.ToString());
            await connection.ExecuteAsync(SPNameConstants.USP_SAVE_TRACKER_RANGES, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            trackerData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            trackerData.TrackerRange.TrackerRangeID = parameter.Get<short>(ConcateAt(nameof(TrackerDTO.TrackerRange.TrackerRangeID)));
        }

        protected DataTable MapTrackerI18NToTable(short TrackerId, List<TrackersI18NModel> trackeri18N)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if(GenericMethods.IsListNotEmpty(trackeri18N))
            {
                foreach (TrackersI18NModel record in trackeri18N)
                {
                    if (TrackerId > 0)
                    {
                        dataTable.Rows.Add(TrackerId, Guid.Empty, record.LanguageID, record.TrackerName, string.Empty);
                    }
                    else
                    {
                        dataTable.Rows.Add(TrackerId, Guid.Empty, record.LanguageID, record.TrackerName, string.Empty);
                    }
                }
            }
            return dataTable;
        }

        protected DataTable MapTrackerRangesI18NToTable(short TrackerRangeId, List<TrackerRangesI18N> programNotes)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(programNotes))
            {
                foreach (TrackerRangesI18N record in programNotes)
                {
                    if (TrackerRangeId > 0)
                    {
                        dataTable.Rows.Add(TrackerRangeId, Guid.Empty, record.LanguageID, record.CaptionText, record.InstructionsText, string.Empty);
                    }
                    else
                    {
                        dataTable.Rows.Add(TrackerRangeId, Guid.Empty, record.LanguageID, Guid.Empty, Guid.Empty, string.Empty);
                    }
                }
            }
            return dataTable;
        }
    }
}