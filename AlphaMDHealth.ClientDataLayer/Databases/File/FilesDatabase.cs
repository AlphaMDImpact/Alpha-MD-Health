using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer
{
    public class FilesDatabase : BaseDatabase
    {
        /// <summary>
        /// Save Files and Documents to local database
        /// </summary>
        /// <param name="fileData">Reference object of Document</param>
        /// <returns>operation status</returns>
        public async Task SaveFilesAndDocumentsAsync(FileDTO fileData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                SaveFiles(fileData, transaction);
                SaveFileDocuments(fileData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Save File and its documents to local database
        /// </summary>
        /// <param name="fileData">Reference object of Document</param>
        /// <returns>operation status</returns>
        public async Task SaveFileAndDocumentsAsync(FileDTO fileData)
        {
            if (GenericMethods.IsListNotEmpty(fileData.Files))
            {
                FileModel file = fileData.Files.FirstOrDefault();
                if (!file.IsSynced && file.IsActive
                    && await SqlConnection.FindWithQueryAsync<FileModel>(
                        "SELECT 1 FROM FileModel WHERE UserID = ? AND IsActive = 1 AND FileCategoryID = ? AND FileID <> ? "
                        , file.UserID, file.FileCategoryID, file.FileID
                    ).ConfigureAwait(false) != null)
                {
                    fileData.ErrCode = ErrorCode.DuplicateData;
                    return;
                }
            }
            await SaveFilesAndDocumentsAsync(fileData).ConfigureAwait(false);
        }

        public async Task<ErrorCode> SaveQuestionFileAndDocumentAsync(List<FileDocumentModel> documents, long userID)
        {
            ErrorCode error = ErrorCode.ErrorWhileSavingRecords;
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (var document in documents)
                {
                    bool isFileActive = true;
                    if (document.FileID == Guid.Empty)
                    {
                        Guid existingFileID = transaction.ExecuteScalar<Guid>("SELECT FileID FROM FileModel WHERE UserID = ? AND IsActive = 1 AND FileCategoryID = ?",
                            userID, document.FileCategoryID);

                        if (existingFileID == Guid.Empty)
                        {
                            existingFileID = GenericMethods.GenerateGuid();

                            transaction.Execute("INSERT INTO FileModel (FileID, UserID, FileCategoryID, IsSynced, IsActive, AddedOn, AddedByID, ErrCode) " +
                                "VALUES (?, ?, ?, ?, ?, ?, ?, ?)", existingFileID, userID, document.FileCategoryID, 0, 1, GenericMethods.GetUtcDateTime, document.AddedByID, ErrorCode.OK);
                        }
                        document.FileID = existingFileID;
                    }
                    transaction.InsertOrReplace(document);
                    if (!document.IsActive)
                    {
                        int fileDocumentModelsCount = transaction.ExecuteScalar<int>($"SELECT COUNT(*) FROM FileDocumentModel WHERE FileID = ? AND IsActive = 1",
                          document.FileID);

                        if (fileDocumentModelsCount == 0)
                        {
                            isFileActive = false;
                        }
                    }
                    transaction.Execute($"UPDATE FileModel SET IsSynced = 0, IsActive = ? WHERE FileID = ?"
                        , isFileActive, document.FileID);
                }
            }).ConfigureAwait(false);
            error = ErrorCode.OK;
            return error;
        }

        /// <summary>
        /// Get files from database
        /// </summary>
        /// <param name="fileData">Reference object to return files records</param>
        /// <returns>Chats in reference object</returns>
        public async Task GetFilesAsync(FileDTO fileData)
        {
            string limit = fileData.RecordCount > 0 ? $" LIMIT {fileData.RecordCount}" : "";
            fileData.Files = await SqlConnection.QueryAsync<FileModel>(
                "SELECT FM.FileID, B.Name AS FileName, A.ImageName AS FileImage, FM.AddedOn, FM.ErrCode, " +
                    "(SELECT COUNT(1) FROM FileDocumentModel FDM WHERE FM.FileID = FDM.FileID AND FDM.IsActive = 1) AS FormattedNumberOfFiles," +
                    "(SELECT Count(1) FROM FileDocumentModel FDM WHERE FDM.IsActive = 1 AND FDM.DocumentStatus = 'NewStatusKey' AND FDM.AddedByID <> ? AND FDM.FileID = FM.FileID) AS NumberOfFiles " +
                "FROM FileModel FM " +
                "JOIN FileCategoryModel A ON A.FileCategoryID = FM. FileCategoryID AND A.IsActive = 1 " +
                "JOIN FileCategoryDetailModel B ON B.FileCategoryID = A.FileCategoryID AND B.LanguageID = ? AND B.IsActive = 1 " +
                "WHERE FM.UserID = ? AND FM.IsActive = 1 " +
                $"ORDER BY FM.AddedOn DESC {limit}",
          fileData.AccountID, fileData.LanguageID, fileData.SelectedUserID).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Files data for which base64 data is not downloaded
        /// </summary>
        /// <param name="fileData">Reference object to return Document records</param>
        /// <returns>operation status</returns>
        public async Task GetImagesToDownloadAsync(FileDTO fileData)
        {
            fileData.FileDocuments = await SqlConnection.QueryAsync<FileDocumentModel>(
                "SELECT FileDocumentID, FileID, DocumentName, IsActive FROM FileDocumentModel WHERE IsActive = 1 AND IsDownloaded = 0"
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Document List and Files List to sync
        /// </summary>
        /// <param name="requestData">Reference object to return Document records</param>
        /// <returns>operation status</returns>
        public async Task GetFilesAndDocumentsForSyncAsync(FileDTO requestData)
        {
            requestData.Files = await SqlConnection.QueryAsync<FileModel>(
                "SELECT * FROM FileModel WHERE IsSynced = 0"
            ).ConfigureAwait(false);
            requestData.FileDocuments = await SqlConnection.QueryAsync<FileDocumentModel>(
                "SELECT * FROM FileDocumentModel WHERE IsSynced = 0"
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get File and its related documents data
        /// </summary>
        /// <param name="fileData">Object containing document ID</param>
        /// <returns>return File and Documents Data</returns>
        public async Task GetFileAsync(FileDTO fileData)
        {
            var files = await SqlConnection.FindWithQueryAsync<FileModel>(
                "SELECT * FROM FileModel WHERE FileID = ?", fileData.File.FileID
            ).ConfigureAwait(false);
            if (files != null)
            {
                fileData.File = files;
                fileData.FileDocuments = await SqlConnection.QueryAsync<FileDocumentModel>(
                  "SELECT * FROM FileDocumentModel WHERE FileID = ? AND IsActive = 1 ORDER BY AddedOn DESC", fileData.File.FileID
              ).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// delete file from database
        /// </summary>
        /// <param name="fileID">ID to delete files and documents </param>
        /// <returns>Operation status in FilesDTO</returns>
        public async Task DeleteFileAsync(Guid fileID, DateTimeOffset? lastModifiedON)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute("UPDATE FileDocumentModel SET IsActive = 0, AddedOn = ?, IsSynced = 0 WHERE FileID = ?", lastModifiedON, fileID);
                transaction.Execute("UPDATE FileModel SET IsActive = 0, AddedOn = ?, IsSynced = 0 WHERE FileID = ?",  lastModifiedON, fileID);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update File base64 data 
        /// <param name="documentData">Reference object to return Document records</param>
        /// <returns>operation status</returns>
        public async Task UpdateImageDataAsync(FileDTO fileData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                foreach (var file in fileData.FileDocuments)
                {
                    transaction.Execute(
                        "UPDATE FileDocumentModel SET IsDownloaded = 1, DocumentName = ? WHERE FileDocumentID = ? AND FileID = ?"
                        , file.DocumentName, file.FileDocumentID, file.FileID);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update Documents sync status
        /// </summary>
        /// <param name="requestData">data to update sync status</param>
        public async Task UpdateDocumentsSyncStatusAsync(FileDTO requestData)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                UpdateDocumentsData(requestData, transaction);
                UpdateFilesData(requestData, transaction);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update document read status to database
        /// </summary>
        /// <param name="fileDocument">fileDocumentID to delete</param>
        /// <returns>Operation status</returns>
        public async Task UpdateDocumentStatusAsync(FileDocumentModel fileDocument)
        {
            await SqlConnection.RunInTransactionAsync(transaction =>
            {
                transaction.Execute("UPDATE FileModel SET IsSynced = 0 WHERE FileID = ?", fileDocument.FileID);
                transaction.Execute(
                    "UPDATE FileDocumentModel SET DocumentStatus = ?, IsSynced = 0 WHERE FileDocumentID = ?"
                    , ResourceConstants.R_COMPLETED_STATUS_KEY, fileDocument.FileDocumentID
                );
            }).ConfigureAwait(false);
        }

        private void UpdateFilesData(FileDTO fileData, SQLiteConnection transaction)
        {
            foreach (FileDocumentModel file in fileData.FileDocuments)
            {
                SaveResultModel result = fileData.SaveFileDocuments?.FirstOrDefault(x => x.ClientGuid == file.FileDocumentID);
                file.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                switch (file.ErrCode)
                {
                    case ErrorCode.OK:
                        // Data is successfully synced, so only update sync flag and EmpID received from server
                        transaction.Execute(
                            "UPDATE FileDocumentModel SET IsSynced = 1, IsDownloaded = 1, ErrCode = ? WHERE FileDocumentID = ?"
                            , fileData.ErrCode, file.FileDocumentID);
                        break;
                    case ErrorCode.DuplicateGuid:
                        // Mark document as not synced
                        transaction.Execute("UPDATE FileModel SET IsSynced = 0 WHERE FileID = ?", file.FileID);
                        // Update with new Guid
                        transaction.Execute(
                            "UPDATE FileDocumentModel SET FileDocumentID = ? WHERE FileDocumentID = ?"
                            , GenerateNewGuid(transaction, false), file.FileDocumentID);
                        fileData.ErrCode = file.ErrCode;
                        break;
                    default:
                        // Mark record with the received error code
                        transaction.Execute(
                            "UPDATE FileDocumentModel SET ErrCode = ? WHERE FileDocumentID = ?"
                            , file.ErrCode, file.FileDocumentID);
                        break;
                }
            }

        }

        private void UpdateDocumentsData(FileDTO fileData, SQLiteConnection transaction)
        {
            foreach (FileModel document in fileData.Files)
            {
                SaveResultModel result = fileData.SaveFiles?.FirstOrDefault(x => x.ClientGuid == document.FileID);
                document.ErrCode = result == null ? ErrorCode.OK : result.ErrCode;
                switch (document.ErrCode)
                {
                    case ErrorCode.OK:
                        // Data is successfully synced, so only update sync flag and EmpID received from server
                        transaction.Execute("UPDATE FileModel SET IsSynced = 1, ErrCode = ? WHERE FileID = ?", fileData.ErrCode, document.FileID);
                        break;
                    case ErrorCode.DuplicateGuid:
                        // Update with new Guid
                        Guid newGuid = GenerateNewGuid(transaction, true);
                        transaction.Execute("UPDATE FileModel SET FileID = ?, IsSynced = 0 WHERE FileID = ?", newGuid, document.FileID);
                        transaction.Execute("UPDATE FileDocumentModel SET FileID = ?, IsSynced = 0 WHERE FileID = ?", newGuid, document.FileID);
                        fileData.ErrCode = document.ErrCode;
                        break;
                    default:
                        // Mark record with the received error code
                        transaction.Execute("UPDATE FileModel SET ErrCode = ? WHERE FileID = ?", document.ErrCode, document.FileID);
                        break;
                }
            }
        }

        private void SaveFileDocuments(FileDTO fileData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(fileData.FileDocuments))
            {
                foreach (var document in fileData.FileDocuments)
                {
                    transaction.InsertOrReplace(document);
                }
            }
        }

        private void SaveFiles(FileDTO fileData, SQLiteConnection transaction)
        {
            if (GenericMethods.IsListNotEmpty(fileData.Files))
            {
                foreach (var file in fileData.Files)
                {
                    transaction.InsertOrReplace(file);
                }
            }
        }

        private Guid GenerateNewGuid(SQLiteConnection transaction, bool forfileID)
        {
            Guid newGuid = GenericMethods.GenerateGuid();
            if (forfileID)
            {
                while (transaction.ExecuteScalar<int>("SELECT 1 FROM FileModel WHERE FileID = ?", newGuid) > 0)
                {
                    newGuid = GenericMethods.GenerateGuid();
                }
            }
            else
            {
                while (transaction.ExecuteScalar<int>("SELECT 1 FROM FileDocumentModel WHERE FileDocumentID = ?", newGuid) > 0)
                {
                    newGuid = GenericMethods.GenerateGuid();
                }
            }
            return newGuid;
        }
    }
}
