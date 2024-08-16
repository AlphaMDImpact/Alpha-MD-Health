using AlphaMDHealth.IntegrationServiceBusinessLayer;
using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.IntegrationServiceLayer.Controllers
{
    [Route("api/FileStorageService")]
    [ApiController]
    public class FileStorageServiceController : BaseController
    {
        /// <summary>
        /// Upload file to blob
        /// </summary>
        /// <param name="uploadFile">upload file details</param>
        /// <returns>uploaded cdn url</returns>
        [Route("UploadFilesAsync")]
        [HttpPost]
        public async Task<IActionResult> UploadFilesAsync([FromBody] FileUploadDTO uploadFile)
        {
            return HttpActionResult(await new FileStorageService(HttpContext).UploadFilesAsync(uploadFile), 1);
        }

        /// <summary>
        /// Fetch dummy cdn link with actual cdn link
        /// </summary>
        /// <param name="uploadFile">Object to store links</param>
        /// <returns>Dummy cdn link with actual cdn link</returns>
        [Route("GetReplacementCdnLinkAsync")]
        [HttpPost]
        public async Task<IActionResult> GetReplacementCdnLinkAsync([FromBody] FileUploadDTO uploadFile)
        {
            return HttpActionResult(await new FileStorageService(HttpContext).GetReplacementCdnLinkAsync(uploadFile), 1);
        }
    }
}