using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class FilesServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Files from database
        /// </summary>
        /// <param name="fileData">object to hold patients data</param>
        /// <returns>Patient files data and operation status</returns>
        public async Task GetFilesAsync(FileDTO fileData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), fileData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(FileModel.FileID), fileData.File.FileID, DbType.Guid, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(FileModel.UserID), fileData.File.UserID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), fileData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.RecordCount), fileData.RecordCount, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LanguageID), fileData.LanguageID, DbType.Int64, ParameterDirection.Input);
            AddDateTimeParameter(nameof(BaseDTO.FromDate), fileData.FromDate, parameters, ParameterDirection.Input);
            AddDateTimeParameter(nameof(BaseDTO.ToDate), fileData.ToDate, parameters, ParameterDirection.Input);
            //MapCommonSPParameters(fileData, parameters, fileData.RecordCount == -1 ? AppPermissions.PatientFileAddEdit.ToString() : AppPermissions.PatientFilesView.ToString());
            MapCommonSPParameters(fileData, parameters,
                AppPermissions.PatientFilesView.ToString(), $"{AppPermissions.PatientFileDelete},{AppPermissions.PatientFileAddEdit}"
            );

            //parameters.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR, fileData.RecordCount == -1 ? AppPermissions.PatientFileDelete.ToString() : AppPermissions.PatientFileAddEdit.ToString(), DbType.String, ParameterDirection.Input);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_FILES_AND_DOCUMENTS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
			{
				await MapPatientFilesViewDataAsync(fileData, result).ConfigureAwait(false);
				await MapReturnPermissionsAsync(fileData, result).ConfigureAwait(false);
			}
			fileData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        }

		internal async Task MapPatientFilesViewDataAsync(FileDTO fileData, SqlMapper.GridReader result)
		{
			if (fileData.RecordCount == -1 && !result.IsConsumed)
			{
				fileData.CategoryOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
				fileData.Files = (await result.ReadAsync<FileModel>().ConfigureAwait(false))?.ToList();
				fileData.FileDocuments = (await result.ReadAsync<FileDocumentModel>().ConfigureAwait(false))?.ToList();
			}
			else
			{
				fileData.Files = (await result.ReadAsync<FileModel>().ConfigureAwait(false))?.ToList();
			}
		}

        /// <summary>
        /// Save Files data in database
        /// </summary>
        /// <param name="filesData">object to save data</param>
        /// <returns>Operation status</returns>
        public async Task SaveFilesAsync(FileDTO filesData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), filesData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS, MapFilesToTable(filesData.Files).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_DETAIL_RECORDS_2, MapFileDocumentsToTable(filesData.FileDocuments).AsTableValuedParameter());
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), filesData.OrganisationID, DbType.Int64, ParameterDirection.InputOutput);
            MapCommonSPParameters(filesData, parameter, AppPermissions.PatientFileAddEdit.ToString());
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PATIENT_FILES_AND_DOCUMENTS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                filesData.SaveFiles = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
                if (!result.IsConsumed)
                {
                    filesData.SaveFileDocuments = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
                }
                if (!result.IsConsumed)
                {
                    filesData.NewFiles = (await result.ReadAsync<FileModel>().ConfigureAwait(false))?.ToList();
                }
            }
            filesData.OrganisationID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID));
            filesData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), filesData.Files[0].IsActive ? OperationType.Save : OperationType.Delete);
        }

        /// <summary>
        /// Mark file document as read 
        /// </summary>
        /// <param name="accountData">object containing document Id</param>
        /// <returns>Operation status</returns>
        public async Task UpdateDocumentStatusAsync(BaseDTO accountData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), accountData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(FileDocumentModel.FileDocumentID), new Guid(accountData.ErrorDescription), DbType.Guid, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), accountData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), accountData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, AppPermissions.PatientFileAddEdit.ToString(), DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), accountData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_UPDATE_DOCUMENT_STATUS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            accountData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Delete file document as read 
        /// </summary>
        /// <param name="accountData">object containing file ID</param>
        /// <returns>Operation status</returns>
        public async Task DeleteFileAsync(BaseDTO accountData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), accountData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(FileDocumentModel.FileID), new Guid(accountData.ErrorDescription), DbType.Guid, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), accountData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.PermissionAtLevelID), accountData.PermissionAtLevelID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + SPFieldConstants.FIELD_CHECK_PERMISSION, AppPermissions.PatientFileDelete.ToString(), DbType.String, ParameterDirection.Input);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), accountData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.LastModifiedON), accountData.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
            await connection.ExecuteAsync(SPNameConstants.USP_DELETE_PATIENT_FILE, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            accountData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        ///Save Note/Task Documents Data
        /// </summary>
        /// <param name="fileData">Reference object which holds patient provider note data</param>
        /// <returns>Operation Status Code</returns>
        public async Task SaveNoteDocumentsAsync(FileDTO fileData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), fileData.FeatureFor, DbType.Byte, ParameterDirection.Input);
            parameters.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapFileDocumentsToTable(fileData.FileDocuments).AsTableValuedParameter());
            parameters.Add(ConcateAt(nameof(PatientProviderNoteModel.PatientID)), fileData.File.UserID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(BaseDTO.OrganisationID)), fileData.OrganisationID, DbType.Int64, ParameterDirection.InputOutput);
            MapCommonSPParameters(fileData, parameters, string.Empty);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_NOTE_DOCUMENTS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                fileData.SaveFileDocuments = (await result.ReadAsync<SaveResultModel>().ConfigureAwait(false))?.ToList();
            }
            fileData.OrganisationID = parameters.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID));
            fileData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }


        private DataTable MapFilesToTable(List<FileModel> documents)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(FileModel.FileID), typeof(Guid)),
                    new DataColumn(nameof(FileModel.UserID), typeof(long)),
                    new DataColumn(nameof(FileModel.FileCategoryID), typeof(string)),
                    new DataColumn(nameof(FileModel.IsActive), typeof(bool)),
                    new DataColumn(nameof(FileModel.AddedOn), typeof(DateTimeOffset)),
                    new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(short)),
                }
            };
            if (GenericMethods.IsListNotEmpty(documents))
            {
                short count = 1;
                foreach (FileModel record in documents)
                {
                    dataTable.Rows.Add(record.FileID, record.UserID, record.FileCategoryID
                        , record.IsActive, record.AddedOn, count++);
                }
            }
            return dataTable;
        }
    }
}
