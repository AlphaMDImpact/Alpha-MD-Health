using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/BillingItemService")]
    [ApiAuthorize]
    public class BillingItemController : BaseController
    {
        /// <summary>
        /// Get BillingItems from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="billingItemID">Id of BillingItem</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of BillingItems and operation status</returns>
        [Route("GetBillingItemsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetBillingItemsAsync(byte languageID, long permissionAtLevelID, short billingItemID, long recordCount)
        {
            return HttpActionResult(await new BillingItemServiceBusinessLayer(HttpContext).GetBillingItemsAsync(languageID, permissionAtLevelID, billingItemID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save BillingItem to database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="billingItemData">Billing item data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SaveBillingItemAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveBillingItemAsync(byte languageID, long permissionAtLevelID, [FromBody] BillingItemDTO billingItemData)
        {
            return HttpActionResult(await new BillingItemServiceBusinessLayer(HttpContext).SaveBillingItemAsync(billingItemData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Return PaymentModes configured for system
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="paymentModeID">Id of PaymentMode </param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of PaymentModes and operation status</returns>
        [Route("GetPaymentModesAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetPaymentModesAsync(byte languageID, long permissionAtLevelID, byte paymentModeID, long recordCount)
        {
            return HttpActionResult(await new PaymentModeServiceBusinessLayer(HttpContext).GetPaymentModesAsync(languageID, permissionAtLevelID, paymentModeID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Payment mode
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="paymentModeData">Payment Mode data to be saved</param>
        /// <returns>Operation status</returns>
        [Route("SavePaymentModeAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SavePaymentModeAsync(byte languageID, long permissionAtLevelID, [FromBody] PaymentModeDTO paymentModeData)
        {
            return HttpActionResult(await new PaymentModeServiceBusinessLayer(HttpContext).SavePaymentModeAsync(paymentModeData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets Patient Bill(s) data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="patientBillID">Bill ID of patient</param>
        /// <param name="recordCount">Record Count </param>
        /// <param name="selectedUserID">Selected user ID</param>
        /// <param name="organisationID">organisation ID</param>
        /// <returns>Patient Bill(s) Data</returns>
        [Route("GetPatientBillsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetPatientBillsAsync(byte languageID, long permissionAtLevelID, Guid patientBillID, long recordCount, long selectedUserID, long organisationID)
        {
            return HttpActionResult(await new BillingItemServiceBusinessLayer(HttpContext).GetPatientBillsAsync(languageID, permissionAtLevelID, patientBillID, recordCount, selectedUserID, organisationID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Patient Bills Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="billingItemData">Obh=ject to be saved</param>
        /// <returns>Operation status </returns>
        [Route("SavePatientBillAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SavePatientBillAsync(byte languageID, long permissionAtLevelID, [FromBody] BillingItemDTO billingItemData)
        {
            return HttpActionResult(await new BillingItemServiceBusinessLayer(HttpContext).SavePatientBillAsync(billingItemData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }
    }
}