using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer;

[Route("api/SubscriptionService")]
[ApiAuthorize]
public class SubscriptionController : BaseController
{
    /// <summary>
    /// Get SubscriptionPlans
    /// </summary>
    /// <param name="languageID">Id of current language</param>
    /// <param name="permissionAtLevelID">permission at level id</param>
    /// <returns>Subscription Plans Data with operation status</returns>
    [Route("GetSubscriptionPlansAsync")]
    [HttpGet]
    public async Task<IActionResult> GetSubscriptionPlansAsync(byte languageID, long permissionAtLevelID)
    {
        return HttpActionResult(await new SubscriptionServiceBusinessLayer(HttpContext).GetSubscriptionPlansAsync(languageID, permissionAtLevelID).ConfigureAwait(false), languageID);
    }

    /// <summary>
    /// Save User Subscription Plan
    /// </summary>
    /// <param name="languageID">Language ID</param>
    /// <param name="permissionAtLevelID">level at which permission is required</param>
    /// <param name="subscriptionData">reference object which holds subscription data</param>
    /// <returns>Operation status</returns>
    [Route("SaveUserSubscriptionPlanAsync")]
    [HttpPost]
    public async Task<IActionResult> SaveUserSubscriptionPlanAsync(byte languageID, long permissionAtLevelID, [FromBody] SubscriptionDTO subscriptionData)
    {
        return HttpActionResult(await new SubscriptionServiceBusinessLayer(HttpContext).SaveUserSubscriptionPlanAsync(languageID, permissionAtLevelID, subscriptionData).ConfigureAwait(false), languageID);
    }
}