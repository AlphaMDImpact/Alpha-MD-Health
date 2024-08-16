using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get 3rd party library service details from the current Http context
        /// </summary>
        /// <param name="context">Http context</param>
        /// <returns>library service details</returns>
        public static LibraryServiceDTO GetLibraryServiceData(this HttpContext context)
        {
            LibraryServiceDTO libraryServiceData = new LibraryServiceDTO();
            if (context?.User != null && context.User is ServicePrincipal)
            {
                ServicePrincipal contextData = context.User as ServicePrincipal;
                libraryServiceData.LibraryDetails = contextData.LibraryDetails;
                libraryServiceData.LibraryInfo = contextData.LibraryDetails[0];
            }
            return libraryServiceData;
        }
    }
}