using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class FileStorageService : LibraryService
    {
        private readonly IFileStorage _fileStorage;

        /// <summary>
        /// File storage service business layer
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public FileStorageService(HttpContext httpContext) : base(httpContext)
        {
            //_headers = httpContext.Request.Headers;
            switch (_libraryServiceData.LibraryInfo.ServiceType)
            {
                case ServiceType.AzureBlobStorage:
                    _fileStorage = new AzureBlobOperations(_libraryServiceData.LibraryInfo);
                    break;
                case ServiceType.GoogleStorage:
                    // Implmentation of Google Storage
                    break;
                case ServiceType.AmazonS3:
                    // Implementation of Aws S3
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Upload file to file Storage
        /// </summary>
        /// <param name="fileData">upload file details</param>
        /// <returns>Uploaded data</returns>
        public async Task<FileUploadDTO> UploadFilesAsync(FileUploadDTO fileData)
        {
            try
            {
                await _fileStorage.UploadFilesAsync(fileData).ConfigureAwait(true);
                LogCalls(fileData);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex.StackTrace);
                fileData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                fileData.ErrorDescription = ex.Message;
            }
            return fileData;
        }

        /// <summary>
        /// Fetch dummy cdn link with actual cdn link
        /// </summary>
        /// <param name="fileData">Object to store links</param>
        /// <returns>Dummy cdn link with actual cdn link</returns>
        public async Task<FileUploadDTO> GetReplacementCdnLinkAsync(FileUploadDTO fileData)
        {
            try
            {
                await _fileStorage.GetReplacementCdnLinkAsync(fileData).ConfigureAwait(true);
                LogCalls(fileData);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex.StackTrace);
                fileData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                fileData.ErrorDescription = ex.Message;
            }
            return fileData;
        }

        ///// <summary>
        ///// Download file from File Storage
        ///// </summary>
        ///// <param name="fileData">upload file details</param>
        ///// <returns>downlaoded files with Operation Status</returns>
        //public async Task<FileUploadDTO> DownloadFilesAsync(FileUploadDTO fileData)
        //{
        //    try
        //    {
        //        await _fileStorage.DownloadFilesAsync(fileData).ConfigureAwait(true);
        //        LogCalls(fileData);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex.Message, ex.StackTrace);
        //        fileData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        //        fileData.ErrorDescription = ex.Message;
        //    }
        //    return fileData;
        //}

        ///// <summary>
        ///// Delete file from File Storage
        ///// </summary>
        ///// <param name="fileData">upload file details</param>
        ///// <returns>Operation Status</returns>
        //public async Task<FileUploadDTO> DeleteFilesAsync(FileUploadDTO fileData)
        //{
        //    try
        //    {
        //        await _fileStorage.DeleteFilesAsync(fileData).ConfigureAwait(true);
        //        LogCalls(fileData);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex.Message, ex.StackTrace);
        //        fileData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
        //        fileData.ErrorDescription = ex.Message;
        //    }
        //    return fileData;
        //}

        ///// <summary>
        ///// Delete file container from File Storage
        ///// </summary>
        ///// <param name="fileData">upload file details</param>
        ///// <returns>Operation Status</returns>
        //public async Task<FileUploadDTO> DeleteBlobContainersAsync(FileUploadDTO fileData)
        //{
        //    try
        //    {
        //        await _fileStorage.DeleteBlobContainersAsync(fileData).ConfigureAwait(true);
        //        LogCalls(fileData);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex.Message, ex.StackTrace);
        //        fileData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
        //        fileData.ErrorDescription = ex.Message;
        //    }
        //    return fileData;
        //}

        private void LogCalls(FileUploadDTO fileData)
        {
            if (_libraryServiceData.LibraryInfo.LogCalls)
            {
                _ = SaveServiceCallLogsAsync(_libraryServiceData.LibraryInfo.OrganisationServiceID, fileData.ErrCode.ToString(), fileData.ErrorDescription).ConfigureAwait(false);
            }
        }
    }
}