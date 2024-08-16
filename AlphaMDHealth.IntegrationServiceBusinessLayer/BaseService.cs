using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class BaseService
    {
        protected BaseService() { }


        /// <summary>
        /// clean a base64 files from metadata
        /// </summary>
        /// <param name="base64File">base 64 files</param>
        /// <returns>pure base64 string</returns>
        protected static string SanitiseBase64File(string base64File)
        {
            string sanitizedBase64File = base64File;
            if (base64File.Contains(",", StringComparison.InvariantCulture))
            {
                //sanitize the image data
                string[] photo = base64File.Split(',');
                var selectedPhoto = photo.Count() > 2 ? photo[2] : photo[1];
                sanitizedBase64File = selectedPhoto.Trim().Replace(" ", "+", StringComparison.InvariantCulture);
            }
            if (sanitizedBase64File.Length % 4 > 0)
            {
                sanitizedBase64File = sanitizedBase64File.PadRight(sanitizedBase64File.Length + 4 - sanitizedBase64File.Length % 4, '=');
            }
            return sanitizedBase64File;
        }

        /// <summary>
        /// get content cdn
        /// </summary>
        /// <param name="fileNameWithExtension">filename with extention</param>
        /// <param name="folderPath">Container Folder Name</param>
        /// <param name="contentCdnLink">base url of cdn</param>
        /// <returns>cdn string</returns>
        protected string GetContentCdn(string containerName, string fileNameWithExtension, string folderPath, string contentCdnLink)
        {
            return $"{contentCdnLink}{containerName}{Constants.BACK_SLASH}{folderPath}{fileNameWithExtension}{Constants.BLOB_CDN_TOKEN}";
        }

        /// <summary>
        /// Get container based upon inputs
        /// </summary>
        /// <param name="fileTypeUploading">type of file to upload</param>
        /// <param name="uniqueID">Unique iD of the Page</param>
        /// <returns>container string</returns>
        protected string GetFolderPath(FileTypeToUpload fileTypeUploading, string uniqueID)
        {
            var path = $"{fileTypeUploading.ToString().ToLowerInvariant()}{Constants.BACK_SLASH}";
            if (!string.IsNullOrWhiteSpace(uniqueID))
            {
                path += $"{uniqueID}{Constants.BACK_SLASH}";
            }
            return path;
        }

        /// <summary>
        /// Update image base 64
        /// </summary>
        /// <param name="foodItemData">send image url in image feild to update base64 </param>
        /// <remarks>Update Image feild as base 64 from same image url</remarks>
        protected async Task UpdateImageBase64(FoodItemDTO foodItemData)
        {
            List<Task<HttpResponseMessage>> imageDownloadTasks = new List<Task<HttpResponseMessage>>();
            foreach (FoodItemModel item in foodItemData.FoodItems)
            {
                imageDownloadTasks.Add(new HttpService().GetImageAsync(HttpMethod.Get, new Uri(item.Image)));
            }
            HttpResponseMessage[] responses = await Task.WhenAll(imageDownloadTasks).ConfigureAwait(false);
            int index = 0;
            foreach (var response in responses)
            {
                if (response.IsSuccessStatusCode)
                {
                    foodItemData.FoodItems[index].Image = Convert.ToBase64String(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
                }
                index++;
            }
        }
    }
}