using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/ContactsService")]
    public class ContactsController : BaseController
    {
        /// <summary>
        /// Get Contacts
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="selectedUserID">Selected user's ID for whoe data needs to be retrived</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="recordCount">Record count to decide how much data to retrive</param>
        /// <param name="contactID">Contact ID to retrive specific contact</param>
        /// <param name="lastModifiedOn">Last Modified on DateTime</param>
        /// <param name="contactType">Requested contact Type</param>
        /// <returns>Contact List</returns>
        [Route("GetContactsAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetContactsAsync(byte languageID, long selectedUserID, long permissionAtLevelID, long recordCount, Guid contactID, DateTimeOffset lastModifiedOn, ContactType contactType)
        {
            return HttpActionResult(await new ContactsServiceBusinessLayer(HttpContext).GetContactsAsync(languageID, selectedUserID, permissionAtLevelID, recordCount, contactID, lastModifiedOn, contactType).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Save Contacts
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">User's permission level</param>
        /// <param name="contactData">Contact data to be saved</param>
        /// <returns>Result of operation</returns>
        [Route("SaveContactsAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> SaveContactsAsync(byte languageID, long permissionAtLevelID, [FromBody] ContactDTO contactData)
        {
            return HttpActionResult(await new ContactsServiceBusinessLayer(HttpContext).SaveContactsAsync(contactData, permissionAtLevelID).ConfigureAwait(false), languageID);
        }
    }
}