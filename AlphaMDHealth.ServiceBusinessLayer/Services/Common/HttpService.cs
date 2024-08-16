using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class HttpService : IHttpService
    {
        /// <summary>
        /// Add default query parameter to request
        /// </summary>
        /// <param name="query">parameters to add</param>
        public void AddDefaultQueryParams(NameValueCollection query)
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Gets access token for the service call
        /// </summary>
        /// <param name="authorizationType">Authorization Type</param>
        /// <returns>Access token</returns>
        public Task<string> GetAccessTokenAsync(AuthorizationType authorizationType)
        {
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// Get selected base url
        /// </summary>
        /// <returns>base url</returns>
        public async Task<string> GetSelectedBaseUrlAsync()
        {
            return await Task.FromResult(UrlConstants.MICRO_SERVICE_PATH).ConfigureAwait(true);
        }

        /// <summary>
        /// Validate certificate
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="certificate">Provides methods that help you use X.509 v.3 certificates.</param>
        /// <param name="chain">chain-building engine for certificate</param>
        /// <param name="sslPolicyErrors">Secure Socket Layer (SSL) policy errors</param>
        /// <returns>true if valid certificate else false</returns>
        public bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Get refresh token
        /// </summary>
        /// <param name="responseData">object to get refresh token</param>
        /// <returns>refresh token</returns>
        public Task RefreshTokenAsync(BaseDTO responseData)
        {
            return default;
        }

        /// <summary>
        /// Sets required header parameters
        /// </summary>
        /// <typeparam name="T">Request object type</typeparam>
        /// <param name="httpClient">Http client instance</param>
        /// <param name="requestUri">Request Uri</param>
        /// <param name="httpRequestData">Request data</param>
        /// <param name="content">Request stream</param>
        /// <returns>The task object representing the operation to add required header parameters</returns>
        public async Task SetHeadersAsync<T>(HttpClient httpClient, Uri requestUri, HttpServiceModel<T> httpRequestData, Stream content)
        {
            await Task.Run(() =>
            {
                string requestContentBase64String = string.Empty;
                if (content?.Length > 0)
                {
                    requestContentBase64String = Convert.ToBase64String(GenericMethods.ComputeHash(content));
                    // Needs to reset content stream as the controller needs to read the content data (i.e. body of POST/PUT calls)
                    content.Seek(0, SeekOrigin.Begin);
                }
                byte[] secretKeyBytes = Encoding.UTF8.GetBytes(httpRequestData.ClientSecret);
                byte[] dataBytes = Encoding.UTF8.GetBytes($"{httpRequestData.ClientIdentifier}{httpRequestData.HttpMethod}{requestUri.AbsoluteUri}{(string.IsNullOrWhiteSpace(httpRequestData.AuthToken) ? string.Empty : Constants.SE_BEARER_TEXT_KEY + httpRequestData.AuthToken)}{requestContentBase64String}");
                using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_HMAC_SIGNATURE_HEADER_KEY, Convert.ToBase64String(hmac.ComputeHash(dataBytes)));
                }
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_CLIENT_IDENTIFIER_HEADER_KEY, httpRequestData.ClientIdentifier);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_FOR_APPLICATION_HEADER_KEY, httpRequestData.ForApplication);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Map http response error codes
        /// </summary>
        /// <typeparam name="T">Request object type</typeparam>
        /// <param name="responseData">service response</param>
        /// <param name="httpResponseData">Http response</param>
        /// <returns>true if mapping is done else returns false if mapping does not require custom handling</returns>
        public Task<bool> MapHttpResponseAsync<T>(HttpServiceModel<T> responseData, HttpResponseMessage httpResponseData)
        {
            // Return false as application does not require any custom handling
            return Task.FromResult(false);
        }

        /// <summary>
        /// Maps page data to previous data and provides total page size in RecordCount
        /// </summary>
        /// <typeparam name="T">Request object type</typeparam>
        /// <param name="responseData">service response</param>
        /// <param name="totalIterations">Current value of total iterations</param>
        /// <param name="serviceResponse">Current page response</param>
        /// <param name="currentIteration">Current iteration</param>
        /// <returns>Current page data mapped with all previous page data</returns>
        public JObject HandlePagination<T>(HttpServiceModel<T> responseData, int totalIterations, JObject serviceResponse, int currentIteration)
        {
            // Setting totalIterations to 1 as Atom does not have pagination
            responseData.RecordCount = 1;
            return JObject.Parse(responseData.Response);
        }
    }
}
