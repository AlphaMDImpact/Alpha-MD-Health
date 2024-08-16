using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer;

[Route("api/AppIntroService")]
[ApiAuthorize]
public class AppIntroController : BaseController
{
    /// <summary>
    /// Get app intros from database
    /// </summary>
    /// <param name="languageID">user's selected language</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="organisationID">organisation id</param>
    /// <param name="introSlideID">intro slide id</param>
    /// <param name="recordCount">number of record count to fetch</param>
    /// <returns>Operation status and list of app intros/returns>
    [Route("GetAppIntrosAsync")]
    [HttpGet]
    public async Task<IActionResult> GetAppIntrosAsync(byte languageID, long permissionAtLevelID, long organisationID, long introSlideID, long recordCount)
    {
        return HttpActionResult(await new AppIntroServiceBusinessLayer(HttpContext).GetAppIntrosAsync(languageID, permissionAtLevelID, organisationID, introSlideID, recordCount).ConfigureAwait(false), languageID);
    }

    /// <summary>
    /// Save files to database
    /// </summary>
    /// <param name="languageID">language id</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <param name="appIntroData">appIntro  data to be saved</param>
    /// <returns>Operation status</returns>
    [Route("SaveAppIntroAsync")]
    [HttpPost]
    public async Task<IActionResult> SaveAppIntroAsync(byte languageID, long permissionAtLevelID, [FromBody] AppIntroDTO appIntroData)
    {
        return HttpActionResult(await new AppIntroServiceBusinessLayer(HttpContext).SaveAppIntroAsync(permissionAtLevelID, appIntroData).ConfigureAwait(false), languageID);
    }
}