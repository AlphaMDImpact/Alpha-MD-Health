using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public partial class FileService : BaseService
    {
        /// <summary>
        /// Map and Save Document Data From Sever
        /// </summary>
        /// <param name="result">Data Sync Result</param>
        /// <param name="data">Jtoken Data From Sync call</param>
        /// <returns>Operation Status and No of records saved</returns>
        internal async Task MapAndSaveDocumentsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                FileDTO fileData = new FileDTO
                {
                    Files = MapFiles(data, nameof(DataSyncDTO.Files)),
                    FileDocuments = MapFileDocuments(data, nameof(DataSyncDTO.FileDocuments))
                };
                if (GenericMethods.IsListNotEmpty(fileData.Files) || GenericMethods.IsListNotEmpty(fileData.FileDocuments))
                {
                    await new FilesDatabase().SaveFilesAndDocumentsAsync(fileData).ConfigureAwait(false);
                    result.RecordCount = fileData.Files?.Count ?? 0 + fileData.FileDocuments?.Count ?? 0;
                }
                _ = ImageMappingAsync().ConfigureAwait(false);
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Sync Files from service
        /// </summary>
        /// <param name="fileData">fileData reference to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Patient Document Data received from server</returns>
        private async Task SyncFilesFromServerAsync(FileDTO fileData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_FILES_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(fileData.SelectedUserID, CultureInfo.InvariantCulture) },
                        { Constants.SE_FILE_ID_QUERY_KEY, Convert.ToString(fileData.File?.FileID, CultureInfo.InvariantCulture) },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(fileData.RecordCount, CultureInfo.InvariantCulture) },
                        { nameof(BaseDTO.FromDate), fileData.FromDate },
                        { nameof(BaseDTO.ToDate), fileData.ToDate },
                    }
                };
                await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
                fileData.ErrCode = httpData.ErrCode;
                if (fileData.ErrCode == ErrorCode.OK)
                {
                    MapGetFilesServiceResponse(fileData, httpData.Response);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                fileData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        internal object MapFilesHistoryData(MedicalHistoryDTO medicalHistoryData, MedicalHistoryViewModel historyView, string jsonResponse)
        {
            FileDTO fileData = new FileDTO
            {
                FromDate = medicalHistoryData.FromDate,
                ToDate = medicalHistoryData.ToDate,
                RecordCount = medicalHistoryData.RecordCount,
                ErrCode = historyView.ErrorCode,
                File = new FileModel { FileID = Guid.Empty }
            };
            MapGetFilesServiceResponse(fileData, jsonResponse);
            fileData.FeaturePermissions = medicalHistoryData.FeaturePermissions;
            GetFilesUIData(fileData);
            historyView.HasData = GenericMethods.IsListNotEmpty(fileData.Files);
            return fileData;
        }

        private void MapGetFilesServiceResponse(FileDTO fileData, string jsonResponse)
        {
            JToken data = JToken.Parse(jsonResponse);
            if (data != null && data.HasValues)
            {
                MapCommonData(fileData, data);
                MapDocumentRecords(data, fileData);
            }
        }

        /// <summary>
        /// Sync File data to server
        /// </summary>
        /// <param name="requestData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        public async Task SyncFilesToServerAsync(FileDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    await new FilesDatabase().GetFilesAndDocumentsForSyncAsync(requestData).ConfigureAwait(false);
                }
                if (GenericMethods.IsListNotEmpty(requestData.Files) || GenericMethods.IsListNotEmpty(requestData.FileDocuments))
                {
                    var httpData = new HttpServiceModel<FileDTO>
                    {
                        CancellationToken = cancellationToken,
                        PathWithoutBasePath = UrlConstants.SAVE_FILES_ASYNC_PATH,
                        ContentToSend = requestData,
                        QueryParameters = new NameValueCollection
                        {
                            { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }
                        }
                    };
                    await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                    requestData.ErrCode = httpData.ErrCode;
                    if (requestData.ErrCode == ErrorCode.OK)
                    {
                        JToken data = JToken.Parse(httpData.Response);
                        if (data?.HasValues == true)
                        {
                            requestData.SaveFiles = MapSaveResponse(data, nameof(FileDTO.SaveFiles));
                            requestData.SaveFileDocuments = MapSaveResponse(data, nameof(FileDTO.SaveFileDocuments));
                            await SaveDocumentsResultsAsync(requestData, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync Deleted File to server
        /// </summary>
        /// <param name="requestData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status call</returns>
        public async Task SyncDeletedFileToServerAsync(FileDTO requestData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.DELETE_FILES_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_FILE_ID_QUERY_KEY, Convert.ToString(requestData.File.FileID, CultureInfo.InvariantCulture)},
                        { Constants.SE_LAST_MODIFIED_ON_QUERY_KEY, Convert.ToString(requestData.LastModifiedON, CultureInfo.InvariantCulture)}
                    },
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                requestData.ErrCode = httpData.ErrCode;
            }
            catch (Exception ex)
            {
                requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync File Document Read status to server
        /// </summary>
        /// <param name="fileData">Object containing Document ID to update status</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Operation status call</returns>
        public async Task UpdateDocumentStatusAsync(BaseDTO fileData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.UPDATE_DOCUMENT_READ_STATUS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_DOCUMENT_FILE_ID_QUERY_KEY, fileData.ErrorDescription }
                    }
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                fileData.ErrCode = httpData.ErrCode;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                fileData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        private async Task SaveDocumentsResultsAsync(FileDTO requestData, CancellationToken cancellationToken)
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new FilesDatabase().UpdateDocumentsSyncStatusAsync(requestData).ConfigureAwait(false);
            }
            else
            {
                // Map error result to main object as web will call save for single record
                requestData.ErrCode = requestData.SaveFiles.FirstOrDefault().ErrCode;
                if (requestData.ErrCode == ErrorCode.DuplicateGuid)
                {
                    requestData.Files.FirstOrDefault().FileID = GenericMethods.GenerateGuid();
                    requestData.FileDocuments.FirstOrDefault().FileID = requestData.Files.FirstOrDefault().FileID;
                }
                else if (requestData.ErrCode == ErrorCode.OK && requestData.SaveFileDocuments != null)
                {
                    foreach (var file in requestData.SaveFileDocuments)
                    {
                        if (file.ErrCode == ErrorCode.DuplicateGuid)
                        {
                            requestData.FileDocuments.Find(x => x.FileDocumentID == file.ClientGuid).FileDocumentID = GenericMethods.GenerateGuid();
                        }
                    }
                    requestData.ErrCode = requestData.SaveFileDocuments.Any(x => x.ErrCode == ErrorCode.DuplicateGuid)
                        ? ErrorCode.DuplicateGuid
                        : ErrorCode.OK;
                }
                else
                {
                    //Empty case to handle duplicate Data Error.
                }
            }
            if (requestData.ErrCode == ErrorCode.DuplicateGuid)
            {
                requestData.ErrCode = ErrorCode.OK;
                await SyncFilesToServerAsync(requestData, cancellationToken).ConfigureAwait(false);
            }
            requestData.RecordCount = requestData.Files?.Count ?? 0;
        }

        private void FormatDocumentsData(FileDTO documentData)
        {
            if (GenericMethods.IsListNotEmpty(documentData.FileDocuments))
            {
                foreach (var document in documentData.FileDocuments)
                {
                    document.AddedOn = GenericMethods.GetUtcDateTime;
                    document.DocumentName = document.DocumentName.Split(Constants.SYMBOL_QUESTIONMARK).First();
                    if (document.FileDocumentID == Guid.Empty)
                    {
                        FormatDocumentData(documentData.File.FileID, document);
                    }
                    if (!MobileConstants.IsMobilePlatform)
                    {
                        //todo:
                        //document.ImageSource = null;
                    }
                }
            }
        }

        private void FormatDocumentData(Guid fileID, FileDocumentModel document)
        {
            document.FileDocumentID = GenericMethods.GenerateGuid();
            document.DocumentStatus = ResourceConstants.R_NEW_STATUS_KEY;
            document.FileID = fileID;
            document.AddedByID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
            document.IsSynced = false;
            document.IsDownloaded = true;
            document.AddedForAccountID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0) == (int)RoleName.Patient
                ? document.AddedByID
                : GetLoginUserID();
            document.AddedByUserName = ResourceConstants.R_DOCUMENT_FILE_YOU_TEXT_KEY;
        }

        private async Task ImageMappingAsync()
        {
            FileDTO documentDTO = new FileDTO();
            await new FilesDatabase().GetImagesToDownloadAsync(documentDTO).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(documentDTO.FileDocuments))
            {
                foreach (FileDocumentModel file in documentDTO.FileDocuments)
                {
                    var base64str = !string.IsNullOrWhiteSpace(file.DocumentName) && file.DocumentName.Contains(Constants.HTTP_TAG_PREFIX)
                        ? await GetImageAsBase64Async(file.DocumentName).ConfigureAwait(false)
                        : string.Empty;
                    file.DocumentName = string.IsNullOrWhiteSpace(base64str)
                        ? file.DocumentName
                        : base64str;
                    file.IsDownloaded = !string.IsNullOrWhiteSpace(base64str);
                }
                await new FilesDatabase().UpdateImageDataAsync(documentDTO).ConfigureAwait(false);
            }
        }

        protected void MapDocumentRecords(JToken data, FileDTO fileData)
        {
            if (fileData.RecordCount == -1)
            {
                fileData.CategoryOptions = MapCategoriesOptions(data, fileData);
            }
            fileData.Files = MapFiles(data, nameof(FileDTO.Files));
            if (fileData.RecordCount == -1 && GenericMethods.IsListNotEmpty(fileData.Files))
            {
                fileData.File = fileData.Files.FirstOrDefault();
                fileData.Files.Clear();
            }
            fileData.FileDocuments = MapFileDocuments(data, nameof(FileDTO.FileDocuments));
        }

        private List<OptionModel> MapCategoriesOptions(JToken data, FileDTO fileData)
        {
            return (data[nameof(fileData.CategoryOptions)].Any())
                ? (from dataItem in data[nameof(FileDTO.CategoryOptions)]
                   select new OptionModel
                   {
                       GroupName = (string)dataItem[nameof(OptionModel.GroupName)],
                       OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                       OptionID = (int)dataItem[nameof(OptionModel.OptionID)],
                       IsSelected = (bool)dataItem[nameof(OptionModel.IsSelected)],
                   }).ToList()
                : new List<OptionModel>();
        }

        /// <summary>
        /// Map Files documents
        /// </summary>
        /// <param name="data">jtoken data</param>
        /// <param name="mapKey">key</param>
        /// <returns>List of documents</returns>
        public List<FileDocumentModel> MapFileDocuments(JToken data, string mapKey)
        {
            return (data[mapKey].Any())
                ? (from dataItem in data[mapKey]
                   select new FileDocumentModel
                   {
                       FileID = (Guid)dataItem[nameof(FileDocumentModel.FileID)],
                       FileDocumentID = (Guid)dataItem[nameof(FileDocumentModel.FileDocumentID)],
                       DocumentName = (string)dataItem[nameof(FileDocumentModel.DocumentName)],
                       FileDocumentName = GetFileNameWithExtension((string)dataItem[nameof(FileDocumentModel.DocumentName)]),
                       DocumentDescription = (string)dataItem[nameof(FileDocumentModel.DocumentDescription)],
                       AddedOn = (DateTimeOffset)dataItem[nameof(FileDocumentModel.AddedOn)],
                       DocumentStatus = (string)dataItem[nameof(FileDocumentModel.DocumentStatus)],
                       DocumentSourceID = (string)dataItem[nameof(FileDocumentModel.DocumentSourceID)],
                       IsActive = (bool)dataItem[nameof(FileDocumentModel.IsActive)],
                       AddedByID = (long)dataItem[nameof(FileDocumentModel.AddedByID)],
                       AddedByUserName = string.Concat((string)dataItem[nameof(FileDocumentModel.UserFirstName)], Constants.STRING_SPACE, (string)dataItem[nameof(FileDocumentModel.UserLastName)]),
                       AddedForAccountID = (long)dataItem[nameof(FileDocumentModel.AddedForAccountID)],
                       IsSynced = true
                   }).ToList()
                : new List<FileDocumentModel>();
        }

        /// <summary>
        /// Map Files
        /// </summary>
        /// <param name="data">data</param>
        /// <param name="mapKey">key</param>
        /// <returns>List of Files</returns>
        public List<FileModel> MapFiles(JToken data, string mapKey)
        {
            return (data[mapKey].Any())
                ? (from dataItem in data[mapKey]
                   select new FileModel
                   {
                       FileID = (Guid)dataItem[nameof(FileModel.FileID)],
                       UserID = (long)dataItem[nameof(FileModel.UserID)],
                       FileName = (string)dataItem[nameof(FileModel.FileName)],
                       FileImage = (string)dataItem[nameof(FileModel.FileImage)],
                       FileCategoryID = (long)dataItem[nameof(FileModel.FileCategoryID)],
                       FormattedNumberOfFiles = (string)dataItem[nameof(FileModel.FormattedNumberOfFiles)],
                       NumberOfFiles = (string)dataItem[nameof(FileModel.NumberOfFiles)],
                       IsActive = (bool)dataItem[nameof(FileModel.IsActive)],
                       AddedOn = (DateTimeOffset)dataItem[nameof(FileModel.AddedOn)],
                       AddedByID = (long)dataItem[nameof(FileModel.AddedByID)],
                       ErrCode = ErrorCode.OK,
                       IsSynced = true
                   }).ToList()
                : new List<FileModel>();
        }
    }
}