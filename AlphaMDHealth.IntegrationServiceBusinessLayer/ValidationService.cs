using AlphaMDHealth.IntegrationServiceDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceConfiguration;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class ValidationService
    {
        private const string _COLON_VALUE = ":";
        private const string _SERVICE_CLIENT_IDENTIFIER = "ServiceClientIdentifier";
        private const string _SERVICE_CLIENT_SECRETE = "ServiceClientSecrete";
        private const string _SERVICE_CONTAINER_NAME = "ServiceContainerName";

        /// <summary>
        /// Validate request
        /// </summary>
        /// <param name="sessionData">session details</param>
        /// <returns>operation status</returns>
        public async Task ValidateRequestAsync(LibraryServiceDTO sessionData)
        {
            try
            {
                sessionData.LibraryInfo = new LibraryServiceModel();
                MapHeadersInSession((sessionData.Request as HttpRequest).Headers, sessionData.LibraryInfo);
                (sessionData.Request as HttpRequest).Headers.TryGetValue(Constants.SE_HMAC_SIGNATURE_HEADER_KEY, out StringValues signature);
                sessionData.RequestSignature = signature.FirstOrDefault() ?? string.Empty;
                //if (!(string.IsNullOrWhiteSpace(sessionData.LibraryInfo.ClientIdentifier) || string.IsNullOrWhiteSpace(sessionData.RequestSignature)))
                //{
                //    await ValidateSignatureAsync(sessionData).ConfigureAwait(false);
                //}
                //else
                //{
                //    sessionData.ErrCode = ErrorCode.Unauthorized;
                //}
                if (string.IsNullOrWhiteSpace(sessionData.LibraryInfo.ForApplication) || string.IsNullOrWhiteSpace(sessionData.LibraryInfo.ClientIdentifier)
                    || string.IsNullOrWhiteSpace(sessionData.RequestSignature))
                {
                    sessionData.ErrCode = ErrorCode.Unauthorized;
                }
                else
                {
                    await ValidateSignatureAsync(sessionData).ConfigureAwait(false);
                }
            }
            catch
            {
                sessionData.ErrCode = ErrorCode.InternalServerError;
            }
        }

        /// <summary>
        /// Map http request headers in session model
        /// </summary>
        /// <param name="headers">http request header data</param>
        /// <param name="sessionData">session model ref variable to store resultant data</param>
        private void MapHeadersInSession(IHeaderDictionary headers, LibraryServiceModel sessionData)
        {
            if (headers != null)
            {
                headers.TryGetValue(Constants.SE_CLIENT_IDENTIFIER_HEADER_KEY, out StringValues headerValues);
                sessionData.ClientIdentifier = headerValues.FirstOrDefault() ?? string.Empty;
                headers.TryGetValue(Constants.SE_FOR_APPLICATION_HEADER_KEY, out StringValues forApp);
                sessionData.ForApplication = forApp.FirstOrDefault() ?? string.Empty;
            }
        }

        private async Task ValidateSignatureAsync(LibraryServiceDTO sessionData)
        {
            await GetClientDataAsync(sessionData).ConfigureAwait(false);
            if (sessionData.LibraryDetails == null || sessionData.LibraryDetails.Count < 1 ||
                string.IsNullOrWhiteSpace(sessionData.LibraryDetails[0].ClientSecrete)
                || sessionData.LibraryDetails[0].OrganisationServiceID < 1)
            {
                sessionData.ErrCode = ErrorCode.Unauthorized;
                return;
            }
            (sessionData.Request as HttpRequest).EnableBuffering();
            Stream content = (sessionData.Request as HttpRequest).Body;
            using (var reader = new StreamReader(content, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true))
            {
                await reader.ReadToEndAsync().ConfigureAwait(false);
            }
            content.Seek(0, SeekOrigin.Begin);
            byte[] hash = GenericMethods.ComputeHash(content);
            // Needs to reset content stream as the controller needs to read the content data
            // (i.e. body of POST/PUT calls)
            content.Seek(0, SeekOrigin.Begin);
            string requestContentBase64String = (hash == null) ? string.Empty : Convert.ToBase64String(hash);
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(sessionData.LibraryDetails[0].ClientSecrete);
            (sessionData.Request as HttpRequest).Headers.TryGetValue(Constants.SE_AUTHORIZATION_HEADER_KEY, out StringValues authorization);
            byte[] dataBytes = Encoding.UTF8.GetBytes($"{sessionData.LibraryInfo.ClientIdentifier}{(sessionData.Request as HttpRequest).Method}{(sessionData.Request as HttpRequest).GetDisplayUrl()}{authorization.FirstOrDefault() ?? string.Empty}{requestContentBase64String}");
            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
            {
                sessionData.ErrCode = sessionData.RequestSignature.Equals(Convert.ToBase64String(hmac.ComputeHash(dataBytes)), StringComparison.Ordinal) ? ErrorCode.OK : ErrorCode.Unauthorized;
            }
        }

        private async Task<LibraryServiceDTO> GetClientDataAsync(LibraryServiceDTO sessionData)
        {
            try
            {
                await new LibraryServiceDataLayer().GetClientDataAsync(sessionData).ConfigureAwait(false);
                var myConfig = MyConfiguration.GetInstance;
                foreach (LibraryServiceModel data in sessionData.LibraryDetails)
                {
                    data.ServiceClientIdentifier = myConfig.GetConfigurationValue(data.ServiceCategory.Trim() + _COLON_VALUE + data.ForApplication.Trim() + data.ServiceType.ToString() + _SERVICE_CLIENT_IDENTIFIER);
                    data.ServiceClientSecrete = myConfig.GetConfigurationValue(data.ServiceCategory.Trim() + _COLON_VALUE + data.ForApplication.Trim() + data.ServiceType.ToString() + _SERVICE_CLIENT_SECRETE);
                    //this is in case of BLOB but let keep it as it is if not breaking
                    data.ContainerName = myConfig.GetConfigurationValue(data.ServiceCategory.Trim() + _COLON_VALUE + data.ForApplication.Trim() + data.ServiceType.ToString() + _SERVICE_CONTAINER_NAME);
                }
            }
            catch (Exception ex)
            {
                sessionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return sessionData;
        }
    }
}