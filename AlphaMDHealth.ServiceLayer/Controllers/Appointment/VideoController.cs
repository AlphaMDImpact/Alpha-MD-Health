using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/VideoService")]
    [ApiAuthorize]
    public class VideoController : BaseController
    {
       /// <summary>
       /// Get video session token
       /// </summary>
       /// <param name="languageID">language id</param>
       /// <param name="organisationID">organisation id</param>
       /// <returns></returns>
        [Route("GetVideoSessionAsync")]
        [HttpGet]
        public async Task<IActionResult> GetVideoSessionAsync(byte languageID, long organisationID,string roomID,string userId,string userName )
        {
            return HttpActionResult(await new VideoServiceBusinessLayer(HttpContext).GetVideoSessionAsync(languageID, organisationID,roomID, userId, userName).ConfigureAwait(false), languageID);
        }
    }
}