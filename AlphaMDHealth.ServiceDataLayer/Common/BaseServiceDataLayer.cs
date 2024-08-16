using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class BaseServiceDataLayer : IDisposable
    {
        /// <summary>
        /// Create a connection to Database
        /// </summary>
        /// <returns>returns a new Database instance.</returns>
        protected IDbConnection ConnectDatabase()
        {
            var myConfig = MyConfiguration.GetInstance;
            //todo
            // string sqlConnection = "Server=tcp:sqldevalphamdhealth.database.windows.net,1433;Initial Catalog=AlphaMDHealth-Dev;Persist Security Info=False;User ID=balvvant;Password=Amd2024FL--;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string sqlConnection = string.Format(
                myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_CONNECTION_STRING),
                myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_SERVER_NAME),
                myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_DATABASE_NAME),
                myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_USER_NAME),
                myConfig.GetConfigurationValue(ConfigurationConstants.CS_DB_SETTINGS_PASSWORD));
            return new SqlConnection(sqlConnection);
        }

        /// <summary>
        /// Method which maps common sp parameters
        /// </summary>
        /// <param name="pageData">Reference object containing data</param>
        /// <param name="parameter">sp parameter object</param>
        /// <param name="checkPermission">permission to check</param>
        /// <param name="returnPermission">permission to return</param>
        protected void MapCommonSPParameters(BaseDTO pageData, DynamicParameters parameter, string checkPermission, string returnPermission)
        {
            parameter.Add(ConcateAt(nameof(BaseDTO.RecordCount)), pageData.RecordCount, DbType.Int16, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.LanguageID)), pageData.LanguageID, DbType.Byte, direction: ParameterDirection.Input);
            MapCommonSPParameters(pageData, parameter, checkPermission);
            if (returnPermission != null)
            {
                parameter.Add(ConcateAt(SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR), returnPermission, DbType.String, ParameterDirection.Input);
            }
        }

        /// <summary>
        /// Method which maps common sp parameters
        /// </summary>
        /// <param name="pageData">Reference object containing data</param>
        /// <param name="parameter">sp parameter object</param>
        /// <param name="checkPermission">permission to check</param>
        protected void MapCommonSPParameters(BaseDTO pageData, DynamicParameters parameter, string checkPermission)
        {
            parameter.Add(ConcateAt(nameof(BaseDTO.AccountID)), pageData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.PermissionAtLevelID)), pageData.PermissionAtLevelID, DbType.Int64, direction: ParameterDirection.Input);
            if (checkPermission != null)
            {
                //parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), pageData.FeatureFor, DbType.Byte, ParameterDirection.Input);
                parameter.Add(ConcateAt(SPFieldConstants.FIELD_CHECK_PERMISSION), checkPermission, DbType.String, ParameterDirection.Input);
            }
            parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), pageData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        }

        protected void AddDateTimeParameter(string colName, string datetime, DynamicParameters parameter, ParameterDirection direction)
        {
            if (!string.IsNullOrWhiteSpace(datetime))
            {
                parameter.Add(ConcateAt(colName), datetime, DbType.DateTimeOffset, direction);
            }
            else
            {
                parameter.Add(ConcateAt(colName), DBNull.Value, DbType.DateTimeOffset, direction);
            }
        }

        protected void AddDateTimeParameter(string colName, DateTimeOffset? datetime, DynamicParameters parameter, ParameterDirection direction)
        {
            if (datetime.HasValue)
            {
                parameter.Add(ConcateAt(colName), GenericMethods.ApplyUtcDateTimeFormatToUtcValue(datetime.Value), DbType.DateTimeOffset, direction);
            }
            else
            {
                parameter.Add(ConcateAt(colName), DBNull.Value, DbType.DateTimeOffset, direction);
            }
        }

        /// <summary>
        /// Concates @ before string
        /// </summary>
        /// <param name="name">string in which @ need to concate as prefix</param>
        /// <returns>string with @ prefix</returns>
        protected string ConcateAt(string name)
        {
            return Constants.SYMBOL_AT_THE_RATE + name;
        }

        /// <summary>
        /// Map Return Permission data
        /// </summary>
        /// <param name="pageData">Reference object to store data</param>
        /// <param name="result">Object from where data needs to map</param>
        /// <returns>List of Permissions</returns>
        protected async Task MapReturnPermissionsAsync(BaseDTO pageData, SqlMapper.GridReader result)
        {
            if (!result.IsConsumed)
            {
                pageData.FeaturePermissions = (await result.ReadAsync<OrganizationFeaturePermissionModel>().ConfigureAwait(false))?.ToList();
            }
        }

        /// <summary>
        /// Check and return data in given list of model 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        protected async Task<List<T>> MapTableDataAsync<T>(SqlMapper.GridReader result)
        {
            return result.IsConsumed
                ? null
                : (await result.ReadAsync<T>().ConfigureAwait(false))?.ToList();
        }

        /// <summary>
        /// Save ErrorLogs into DB
        /// </summary>
        /// <param name="errorLogData">Ref object which holds data to store on server and returns status</param>
        /// <returns>Opertaion Status</returns>
        public async Task SaveErrorLogsAsync(ErrorLogDTO errorLogData)
        {
            using var connection = ConnectDatabase();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapToErrorLogTable(errorLogData.ErrorLogs).AsTableValuedParameter());
            parameter.Add(ConcateAt(nameof(BaseDTO.AccountID)), errorLogData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), errorLogData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_ERROR_LOGGING, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            errorLogData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }

        /// <summary>
        /// Get communication template data
        /// </summary>
        /// <param name="template">data to get template</param>
        /// <returns>Communication template data</returns>
        public async Task GetTemplateDataAsync(TemplateDTO template)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(TemplateModel.DataRecordPrimaryKey)), template.TemplateData.DataRecordPrimaryKey, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.ToId)), template.EmailID, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.ToPhoneNo)), template.PhoneNumber, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.TemplateName)), template.TemplateData.TemplateName.ToString().Substring(1), DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateDTO.OrganisationID)), template.OrganisationID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.IsExternal)), template.TemplateData.IsExternal, DbType.Boolean, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateDTO.LanguageID)), template.LanguageID, DbType.String, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateDTO.AccountID)), template.AccountID, DbType.Int64, ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), template.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            template.TemplateData = await connection.QueryFirstOrDefaultAsync<TemplateModel>(SPNameConstants.USP_GET_TEMPLATE_WITH_DATA, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            template.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Get pending notifications to send
        /// </summary>
        /// <param name="communicationData">Reference object to hold pending notifications</param>
        /// <returns>Operation status with pending notifications</returns>
        public async Task GetPendingCommunicationAsync(TemplateDTO communicationData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), communicationData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PENDING_COMMUNICATIONS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            if (result.HasRows())
            {
                communicationData.Templates = (await result.ReadAsync<TemplateModel>().ConfigureAwait(false))?.ToList();
            }
            communicationData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
        }

        /// <summary>
        /// Get pending notifications to send
        /// </summary>
        /// <param name="communicationData">Reference object to hold pending notifications</param>
        /// <returns>Operation status with pending notifications</returns>
        public async Task SavePendingCommunicationAsync(TemplateDTO communicationData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add(ConcateAt(nameof(TemplateModel.CommunicationID)), communicationData.TemplateData.CommunicationID, DbType.Int64, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.IsActive)), true, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.TemplateID)), communicationData.TemplateData.TemplateID, DbType.Int16, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_USER_ID), communicationData.SelectedUserID, DbType.Int64, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.CommunicationType)), communicationData.TemplateData.CommunicationType ?? string.Empty, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.EmailHeader)), communicationData.TemplateData.EmailHeader, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.EmailBody)), communicationData.TemplateData.EmailBody, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.SmsHeader)), communicationData.TemplateData.SmsHeader, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.SmsBody)), communicationData.TemplateData.SmsBody, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.AlertHeader)), communicationData.TemplateData.AlertHeader, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.AlertBody)), communicationData.TemplateData.AlertBody, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.WhatsAppHeader)), communicationData.TemplateData.WhatsAppHeader, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.WhatsAppBody)), communicationData.TemplateData.WhatsAppBody, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.ToId)), communicationData.TemplateData.ToId, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.ToPhoneNo)), communicationData.TemplateData.ToPhoneNo, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.Attachments)), communicationData.TemplateData.Attachments, DbType.String, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.IsEmailSent)), communicationData.TemplateData.IsEmailSent, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.IsSMSSent)), communicationData.TemplateData.IsSMSSent, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.IsWhatsAppSent)), communicationData.TemplateData.IsWhatsAppSent, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.IsNotificationSent)), communicationData.TemplateData.IsNotificationSent, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(TemplateModel.CommunicationDateTime)), DateTime.UtcNow, DbType.DateTimeOffset, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.AccountID)), communicationData.AccountID, DbType.Boolean, direction: ParameterDirection.Input);
            parameter.Add(ConcateAt(nameof(BaseDTO.ErrCode)), communicationData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
            SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_SAVE_PENDING_COMMUNICATION, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            communicationData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
        }

        /// <summary>
        /// Map device information to dynamic parameters
        /// </summary>
        /// <param name="sessionData">Session data containing the device information</param>
        /// <param name="parameters">Dynamic parameters in which device information is to be mapped</param>
        protected void MapDeviceDataInSpInputParams(SessionModel sessionData, DynamicParameters parameters)
        {
            parameters.Add(ConcateAt(nameof(SessionModel.DeviceID)), sessionData.DeviceID, DbType.String, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(SessionModel.DeviceType)), sessionData.DeviceType, DbType.String, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(SessionModel.DeviceOS)), sessionData.DeviceOS, DbType.String, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(SessionModel.DeviceOSVersion)), sessionData.DeviceOSVersion, DbType.String, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(SessionModel.DeviceModel)), sessionData.DeviceModel, DbType.String, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(SessionModel.DeviceDetail)), sessionData.DeviceDetail, DbType.String, ParameterDirection.Input);
            parameters.Add(ConcateAt(nameof(SessionModel.ClientIdentifier)), sessionData.ClientIdentifier, DbType.String, ParameterDirection.Input);
        }

        private DataTable MapToErrorLogTable(List<ErrorLogModel> errorLogs)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(SPFieldConstants.FIELD_ERROR_LOG_DATE, typeof(DateTimeOffset)),
                    new DataColumn(SPFieldConstants.FIELD_ERROR_THREAD_INFO, typeof(string)),
                    new DataColumn(nameof(ErrorLogModel.ErrorMessage), typeof(string)),
                    new DataColumn(SPFieldConstants.FIELD_IS_DB_ERROR, typeof(bool)),
                    new DataColumn(nameof(ErrorLogModel.AccountID), typeof(long)),
                }
            };
            if (errorLogs?.Count > 0)
            {
                foreach (ErrorLogModel item in errorLogs)
                {
                    dataTable.Rows.Add(item.CreatedOn, item.ErrorFunction, item.ErrorMessage, false, item.AccountID);
                }
            }
            return dataTable;
        }

        protected DataTable CreateGenericTypeTable()
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(SPFieldConstants.FIELD_BIG_INT_ID, typeof(long)),
                    new DataColumn(SPFieldConstants.FIELD_UNIQ_ID, typeof(Guid)),
                    new DataColumn(nameof(LanguageModel.LanguageID), typeof(byte)),
                    new DataColumn(SPFieldConstants.FIELD_LANGUAGE_DATA_1, typeof(string)),
                    new DataColumn(SPFieldConstants.FIELD_LANGUAGE_DATA_2, typeof(string)),
                    new DataColumn(SPFieldConstants.FIELD_LANGUAGE_DATA_3, typeof(string))
                }
            };
            return dataTable;
        }

        protected DataTable MapDepartmentsToTable(List<DepartmentModel> departments)
        {
            DataTable dataTable = CreateGenericTypeTable();
            if (GenericMethods.IsListNotEmpty(departments))
            {
                foreach (DepartmentModel record in departments)
                {
                    dataTable.Rows.Add(record.DepartmentID, Guid.Empty, record.LanguageID, record.DepartmentName, string.Empty, string.Empty);
                }
            }
            return dataTable;
        }

        protected DataTable MapFileDocumentsToTable(List<FileDocumentModel> fileDocuments)
        {
            DataTable dataTable = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                Columns =
                {
                    new DataColumn(nameof(FileDocumentModel.FileDocumentID), typeof(Guid)),
                    new DataColumn(nameof(FileDocumentModel.FileID), typeof(Guid)),
                    new DataColumn(nameof(FileDocumentModel.FileCategoryID), typeof(long)),
                    new DataColumn(nameof(FileDocumentModel.DocumentName), typeof(string)),
                    new DataColumn(nameof(FileDocumentModel.DocumentDescription), typeof(string)),
                    new DataColumn(nameof(FileDocumentModel.DocumentStatus), typeof(string)),
                    new DataColumn(nameof(FileDocumentModel.DocumentSourceID),typeof(string)),
                    new DataColumn(nameof(FileDocumentModel.IsActive), typeof(bool)),
                    new DataColumn(nameof(FileDocumentModel.AddedOn), typeof(DateTimeOffset)),
                    new DataColumn(SPFieldConstants.FIELD_TEMP_ID, typeof(short)),
                }
            };
            if (GenericMethods.IsListNotEmpty(fileDocuments))
            {
                short count = 1;
                foreach (FileDocumentModel record in fileDocuments)
                {
                    dataTable.Rows.Add(
                        record.FileDocumentID, record.FileID, record.FileCategoryID, record.DocumentName.Contains(Constants.HTTP_TAG_PREFIX) ? record.DocumentName : string.Empty,
                        record.DocumentDescription ?? string.Empty, record.DocumentStatus, record.DocumentSourceID, record.IsActive, record.AddedOn, count++);
                }
            }
            return dataTable;
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}