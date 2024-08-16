using AlphaMDHealth.Model;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    interface IFileStorage
    {
        /// <summary>
        /// Upload file to file Storage
        /// </summary>
        /// <param name="fileData">upload file details</param>
        /// <returns>Uploaded data</returns>
        Task UploadFilesAsync(FileUploadDTO fileData);

        /// <summary>
        /// Fetch dummy cdn link with actual cdn link
        /// </summary>
        /// <param name="fileData">Object to store links</param>
        /// <returns>Dummy cdn link with actual cdn link</returns>
        Task GetReplacementCdnLinkAsync(FileUploadDTO fileData);

        ///// <summary>
        ///// Download file from File Storage
        ///// </summary>
        ///// <param name="uploadFile">upload file details</param>
        ///// <returns>downlaoded files with Operation Status</returns>
        //Task DownloadFilesAsync(FileUploadDTO fileData);

        ///// <summary>
        ///// Delete file from File Storage
        ///// </summary>
        ///// <param name="uploadFile">upload file details</param>
        ///// <returns>Operation Status</returns>
        //Task DeleteFilesAsync(FileUploadDTO fileData);
        
        ///// <summary>
        ///// Delete file container from File Storage
        ///// </summary>
        ///// <param name="uploadFile">upload file details</param>
        ///// <returns>Operation Status</returns>
        //Task DeleteBlobContainersAsync(FileUploadDTO fileData);

    }
}