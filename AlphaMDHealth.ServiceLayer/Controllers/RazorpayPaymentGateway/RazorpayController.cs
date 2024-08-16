using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/RazorpayService")]
    [ApiAuthorize]
    public class RazorpayController : BaseController
    {
        /// <summary>
        /// Save Razorpay payment & order detail
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's organisation ID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="healthScanData">healthScan data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveRazorpayPaymentDetailAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveRazorpayPaymentDetailAsync(byte languageID, long permissionAtLevelID, [FromBody] RazorpayDTO razorpayData)
        {
            return HttpActionResult(await new RazorpayServiceBussinessLayer(HttpContext).SaveRazorpayPaymentDetailAsync(languageID,permissionAtLevelID, razorpayData).ConfigureAwait(false), languageID);
        }
    }
}