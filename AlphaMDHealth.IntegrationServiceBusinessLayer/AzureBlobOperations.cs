using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Globalization;
using Azure.Storage;
using Azure.Storage.Sas;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    internal class AzureBlobOperations : BaseService, IFileStorage
    {
        private readonly LibraryServiceModel _libraryServiceData;

        public AzureBlobOperations(LibraryServiceModel libraryServiceData)
        {
            _libraryServiceData = libraryServiceData;
        }

        private string GetConnectionString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DefaultEndpointsProtocol=https;" + "AccountName={0};" + "AccountKey={1}" + ";EndpointSuffix=core.windows.net",
                _libraryServiceData.ServiceClientIdentifier, _libraryServiceData.ServiceClientSecrete);
        }

        #region Blob Operation Methods

        /// <summary>
        /// Uploads file to cloud
        /// </summary>
        /// <param name="fileData">file Details</param>
        /// <returns>cdn link with Operation Status</returns>
        public async Task UploadFilesAsync(FileUploadDTO fileData)
        {
            foreach (var fileFolder in fileData.FileContainers)
            {
                if (IsFullFolderData(fileData.FileTypeUploading))
                {
                    // Download base 64 files for new data, before deleting all files of current folder
                    await DownloadBase64FileFromLinksAsync(fileData.FileTypeUploading, fileFolder);
                    //Delete matching folder
                    await DeleteBlobContainerAsync(fileData.FileTypeUploading, fileFolder.ID).ConfigureAwait(true);
                }
                //Save files in this folder now
                foreach (FileDataModel file in fileFolder.FileData)
                {
                    if (!string.IsNullOrWhiteSpace(file.Base64File))
                    {
                        if (file.HasMultiple)
                        {
                            file.Base64File = await UploadRichTextDataImagesAsync(fileData.FileTypeUploading, fileFolder.ID, file.Base64File).ConfigureAwait(true);
                        }
                        else
                        {
                            file.Base64File = await UploadFileAsync(fileData.FileTypeUploading, fileFolder.ID, file.Base64File, file.HasMultiple).ConfigureAwait(true);
                        }
                        file.Base64File = RandomiseFilePath(file.Base64File);
                    }
                }
            }
            fileData.ErrCode = ErrorCode.OK;
        }

        /// <summary>
        /// Fetch dummy cdn link with actual cdn link
        /// </summary>
        /// <param name="fileData">Object to store links</param>
        /// <returns>Dummy cdn link with actual cdn link</returns>
        public Task GetReplacementCdnLinkAsync(FileUploadDTO fileData)
        {
            fileData.AddedBy = Constants.DUMMY_BLOB_URL_LINK;
            fileData.LastModifiedBy = GetLinkBasePath(); //GetCdnLinkBasePath();
            if (!string.IsNullOrWhiteSpace(fileData.FileName))
            {
                fileData.EmailID = GetSASToken(GenericMethods.ReplaceString(fileData.FileName, fileData.AddedBy, ""));
            }
            fileData.ErrCode = ErrorCode.OK;
            return Task.CompletedTask;
        }

        private string GetSASToken(string BlobName)
        {
            BlobSasBuilder blobSasBuilder = new()
            {
                BlobContainerName = _libraryServiceData.ContainerName,
                BlobName = BlobName,
                ExpiresOn = DateTime.UtcNow.AddMinutes(5)
            };

            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_libraryServiceData.ServiceClientIdentifier, _libraryServiceData.ServiceClientSecrete)).ToString();
        }

        ///// <summary>
        ///// Get file from file Storage cloud
        ///// </summary>
        ///// <param name="fileData">file Details</param>
        ///// <returns>base64 data with Operation Status</returns>
        //public async Task DownloadFilesAsync(FileUploadDTO fileData)
        //{
        //    foreach (var fileFolder in fileData.FileContainers)
        //    {
        //        //Download files of this folder now
        //        foreach (FileDataModel file in fileFolder.FileData)
        //        {
        //            file.Base64File = await DownloadFileAsync(fileData.FileTypeUploading, fileFolder.ID, file.Base64File, file.HasMultiple);
        //        }
        //    }
        //    fileData.ErrCode = ErrorCode.OK;
        //}

        ///// <summary>
        ///// Delete file from file storage
        ///// </summary>
        ///// <param name="fileData">file Details</param>
        ///// <returns>Operation Status</returns>
        //public async Task DeleteFilesAsync(FileUploadDTO fileData)
        //{
        //    string connectionString = GetConnectionString();
        //    if (!string.IsNullOrWhiteSpace(connectionString))
        //    {
        //        fileData.ErrCode = ErrorCode.OK;
        //        BlobContainerClient containerClient = GetBlobContainerClient(connectionString);
        //        foreach (var fileFolder in fileData.FileContainers)
        //        {
        //            var result = await DeleteContainerFiles(fileData.FileTypeUploading, containerClient, fileFolder).ConfigureAwait(false);
        //            if (result != ErrorCode.OK)
        //            {
        //                fileData.ErrCode = result;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        fileData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
        //    }
        //}

        ///// <summary>
        ///// Delete file container from  Storage cloud
        ///// </summary>
        ///// <param name="fileData">file Details</param>
        ///// <returns>Operation Status</returns>
        //public async Task DeleteBlobContainersAsync(FileUploadDTO fileData)
        //{
        //    foreach (var fileFolder in fileData.FileContainers)
        //    {
        //        await DeleteBlobContainerAsync(fileData.FileTypeUploading, fileFolder.ID).ConfigureAwait(true);
        //    }
        //    fileData.ErrCode = ErrorCode.OK;
        //}

        #endregion

        #region Private File Operation Methods

        private bool IsFullFolderData(FileTypeToUpload fileTypeUploading)
        {
            return !(fileTypeUploading == FileTypeToUpload.ChatImages
                || fileTypeUploading == FileTypeToUpload.FileAndDocuments
                || fileTypeUploading == FileTypeToUpload.ExternalCommunicationFiles);
        }

        private async Task<string> UploadFileAsync(FileTypeToUpload fileTypeUploading, string ID, string base64File, bool hasMultiple)
        {
            if (IsLink(base64File, hasMultiple))
            {
                return base64File;
            }
            string connectionString = GetConnectionString();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, _libraryServiceData.ContainerName);
                await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                string fileExtension = GenericMethods.GetFileExtension(base64File);
                //generate a unique image name (in case of add) else we will use the existing file name for updates
                var fileNameWithExtension = GenericMethods.GenerateGuidWithFileExt(fileExtension);
                string folderPath = GetFolderPath(fileTypeUploading, ID);
                BlobClient blobClient = containerClient.GetBlobClient($"{folderPath}{fileNameWithExtension}");
                byte[] imageBytes = Convert.FromBase64String(SanitiseBase64File(base64File));
                await blobClient.UploadAsync(new MemoryStream(imageBytes), new BlobHttpHeaders
                {
                    ContentType = GenericMethods.GetFileContentType(base64File)
                }).ConfigureAwait(false);
                return GetContentCdn(_libraryServiceData.ContainerName, fileNameWithExtension, folderPath, _libraryServiceData.ServiceTarget);
            }
            return string.Empty;
        }

        private bool IsLink(string base64File, bool hasMultiple)
        {
            return (base64File.StartsWith(Constants.DUMMY_BLOB_URL_LINK) || base64File.StartsWith(GetCdnLinkBasePath()))
                || (hasMultiple && base64File.Contains(Constants.DUMMY_BLOB_URL_LINK) || base64File.Contains(GetCdnLinkBasePath()))
                || GenericMethods.IsPathString(base64File);
        }

        private async Task<string> DownloadFileAsync(FileTypeToUpload fileTypeUploading, string ID, string imageName)
        {
            try
            {
                string connectionString = GetConnectionString();
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    BlobContainerClient containerClient = new BlobContainerClient(connectionString, _libraryServiceData.ContainerName);
                    //the location of main container
                    string filePath = GetFilePath(fileTypeUploading, ID, imageName);
                    BlobClient blobClient = containerClient.GetBlobClient(filePath);
                    BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync().ConfigureAwait(false);

                    //MemoryStream memStream = new MemoryStream();
                    //await downloadInfo.Content.CopyToAsync(memStream).ConfigureAwait(false);
                    //string fileBase64 = Convert.ToBase64String(memStream.ToArray());

                    return await GenericMethods.CreateFileBase64StringAsync(imageName, downloadInfo.Content).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
            }
            return string.Empty;
        }

        private string GetFilePath(FileTypeToUpload fileTypeUploading, string ID, string imageName)
        {
            string filePath;
            if (imageName.Contains(_libraryServiceData.ServiceTarget, StringComparison.InvariantCultureIgnoreCase))
            {
                filePath = new Uri(imageName).AbsolutePath.Replace(
                    Constants.BACK_SLASH + _libraryServiceData.ContainerName + Constants.BACK_SLASH,
                    string.Empty, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                filePath = $"{GetFolderPath(fileTypeUploading, ID)}{imageName}";
            }
            return filePath;
        }

        //private BlobContainerClient GetBlobContainerClient(string connectionString)
        //{
        //    return new BlobContainerClient(connectionString, _libraryServiceData.ContainerName);
        //}

        //private async Task<ErrorCode> DeleteContainerFiles(FileTypeToUpload fileTypeToUpload, BlobContainerClient containerClient, FileContainerModel fileFolder)
        //{
        //    ErrorCode result = ErrorCode.OK;
        //    //loop through files of folder
        //    foreach (FileDataModel file in fileFolder.FileData)
        //    {
        //        //Delete matching file
        //        if (!string.IsNullOrWhiteSpace(file.Base64File))
        //        {
        //            if (!await DeleteFileAsync(containerClient, fileTypeToUpload, fileFolder.ID, file).ConfigureAwait(false))
        //            {
        //                result = ErrorCode.ErrorWhileDeletingRecords;
        //            }
        //        }
        //    }
        //    return result;
        //}

        //private async Task<bool> DeleteFileAsync(BlobContainerClient containerClient, FileTypeToUpload fileTypeUploading, string ID, FileDataModel file)
        //{
        //    string filePath;
        //    file.Base64File = RandomiseFilePath(file.Base64File);
        //    if (file.Base64File.Contains(Constants.DUMMY_BLOB_URL_LINK, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        file.Base64File = CorrectFilePath(file.Base64File);
        //        filePath = new Uri(file.Base64File).AbsolutePath.Replace(
        //            Constants.BACK_SLASH + _libraryServiceData.ContainerName + Constants.BACK_SLASH,
        //            string.Empty, StringComparison.InvariantCultureIgnoreCase);
        //    }
        //    else
        //    {
        //        var path = GetFolderPath(fileTypeUploading, ID);
        //        filePath = file.Base64File.Contains(path)
        //            ? file.Base64File
        //            : $"{path}{file.Base64File}";
        //    }

        //    BlobClient blobClient = containerClient.GetBlobClient(filePath);
        //    var response = await blobClient.DeleteAsync().ConfigureAwait(false);
        //    return response.Status == 202;
        //}

        private async Task DeleteBlobContainerAsync(FileTypeToUpload fileTypeUploading, string ID)
        {
            string connectionString = GetConnectionString();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                string folderPath = GetFolderPath(fileTypeUploading, ID);
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, _libraryServiceData.ContainerName);
                //the location of main container
                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    if (blobItem.Name.Contains(folderPath, StringComparison.InvariantCulture))
                    {
                        await containerClient.DeleteBlobIfExistsAsync(blobItem.Name).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<string> UploadRichTextDataImagesAsync(FileTypeToUpload fileTypeUploading, string ID, string richTextData)
        {
            bool endOfString = false;
            int startPosition = 0;
            int endPosition;
            string imageValue;
            string imageName;
            while (!endOfString)
            {
                startPosition = richTextData.IndexOf(Constants.IMAGE_WITH_BASE64_START_TAG, startPosition, StringComparison.InvariantCultureIgnoreCase);
                if (startPosition == -1)
                {
                    endOfString = true;
                }
                else
                {
                    endPosition = richTextData.IndexOf(@"""", richTextData.IndexOf(@"""", startPosition, StringComparison.InvariantCultureIgnoreCase) + 1, StringComparison.InvariantCultureIgnoreCase);
                    startPosition += 10;
                    endPosition -= startPosition;
                    imageValue = richTextData.Substring(startPosition, endPosition);
                    if (!string.IsNullOrWhiteSpace(imageValue))
                    {
                        imageName = await UploadFileAsync(fileTypeUploading, ID, imageValue, true).ConfigureAwait(true);
                        richTextData = richTextData.Replace(imageValue, imageName, StringComparison.InvariantCulture);
                    }
                }
            }
            return richTextData;
        }

        private async Task DownloadBase64FileFromLinksAsync(FileTypeToUpload fileTypeToUpload, FileContainerModel fileFolder)
        {
            foreach (FileDataModel file in fileFolder.FileData)
            {
                if (!string.IsNullOrWhiteSpace(file.Base64File) && IsLink(file.Base64File, file.HasMultiple))
                {
                    file.Base64File = await DownloadFileAsync(fileTypeToUpload, fileFolder.ID, file.Base64File, file.HasMultiple);
                }
            }
        }

        private async Task<string> DownloadFileAsync(FileTypeToUpload fileTypeToUpload, string folderName, string base64File, bool hasMultiple)
        {
            if (hasMultiple)
            {
                base64File = await DownloadRichTextDataImagesAsync(fileTypeToUpload, folderName, base64File).ConfigureAwait(true);
            }
            else
            {
                base64File = await DownloadFileAsync(fileTypeToUpload, folderName, base64File).ConfigureAwait(true);
            }
            return RandomiseFilePath(base64File);
        }

        private async Task<string> DownloadRichTextDataImagesAsync(FileTypeToUpload fileTypeUploading, string folderName, string richTextData)
        {
            bool endOfString = false;
            int startPosition = 0;
            int endPosition;
            string imageValue;
            string imageName;
            while (!endOfString)
            {
                startPosition = richTextData.IndexOf(Constants.IMAGE_START_TAG_WITH_SRC, startPosition, StringComparison.InvariantCultureIgnoreCase);
                if (startPosition == -1)
                {
                    endOfString = true;
                }
                else
                {
                    endPosition = richTextData.IndexOf(@"""", richTextData.IndexOf(@"""", startPosition, StringComparison.InvariantCultureIgnoreCase) + 1, StringComparison.InvariantCultureIgnoreCase);
                    startPosition += 10;
                    endPosition -= startPosition;
                    imageName = richTextData.Substring(startPosition, endPosition);
                    if (!string.IsNullOrWhiteSpace(imageName) && imageName.Contains(_libraryServiceData.ServiceTarget, StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageValue = await DownloadFileAsync(fileTypeUploading, folderName, imageName).ConfigureAwait(false);
                        richTextData = richTextData.Replace(imageName, imageValue, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }
            return richTextData;
        }

        private string GetLinkBasePath()
        {
            var abc = string.Format(CultureInfo.InvariantCulture, "https://" + "{0}" + ".blob.core.windows.net/", _libraryServiceData.ServiceClientIdentifier);

            return $"{abc}{_libraryServiceData.ContainerName}{Constants.BACK_SLASH}";
        }

        private string GetCdnLinkBasePath()
        {
            return $"{_libraryServiceData.ServiceTarget}{_libraryServiceData.ContainerName}{Constants.BACK_SLASH}";
        }

        private string RandomiseFilePath(string inputFilePath)
        {
            return GenericMethods.ReplaceString(inputFilePath, GetCdnLinkBasePath(), Constants.DUMMY_BLOB_URL_LINK);
        }

        #endregion
    }
}
