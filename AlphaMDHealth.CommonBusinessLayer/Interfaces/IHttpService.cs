using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AlphaMDHealth.CommonBusinessLayer
{
    /// <summary>
    /// Represents Http module
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Sets required header parameters
        /// </summary>
        /// <typeparam name="T">Request object type</typeparam>
        /// <param name="httpClient">Http client instance</param>
        /// <param name="requestUri">Request Uri</param>
        /// <param name="httpRequestData">Request data</param>
        /// <param name="content">Request stream</param>
        /// <returns>The task object representing the operation to add required header parameters</returns>
        Task SetHeadersAsync<T>(HttpClient httpClient, Uri requestUri, HttpServiceModel<T> httpRequestData, Stream content);

        /// <summary>
        /// Adds default query parameters required for service calls
        /// </summary>
        /// <param name="query">Existing query paramters</param>
        void AddDefaultQueryParams(NameValueCollection query);

        /// <summary>
        /// Gets the selected base url for the service call
        /// </summary>
        /// <returns>Selected base url string</returns>
        Task<string> GetSelectedBaseUrlAsync();

        /// <summary>
        /// Gets access token for the service call
        /// </summary>
        /// <param name="authorizationType">Authorization Type</param>
        /// <returns>Access token</returns>
        Task<string> GetAccessTokenAsync(AuthorizationType authorizationType);

        /// <summary>
        /// Http certicate validation callback
        /// </summary>
        /// <param name="sender">Sender object of http call</param>
        /// <param name="certificate">Received certificate for the given call</param>
        /// <param name="chain">chain-building engine for System.Security.Cryptography.X509Certificates.X509Certificate2</param>
        /// <param name="sslPolicyErrors">Ssl policy errors</param>
        /// <returns>true if valid else return false</returns>
        bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);

        /// <summary>
        /// Refresh token async
        /// </summary>
        /// <param name="responseData">Response received from service</param>
        /// <returns>The task object representing the operation to refresh token</returns>
        Task RefreshTokenAsync(BaseDTO responseData);

        /// <summary>
        /// Map http response error codes
        /// </summary>
        /// <typeparam name="T">Request object type</typeparam>
        /// <param name="responseData">service response</param>
        /// <param name="httpResponseData">Http response</param>
        /// <returns>true if mapping is done else returns false if mapping does not require custom handling</returns>
        Task<bool> MapHttpResponseAsync<T>(HttpServiceModel<T> responseData, HttpResponseMessage httpResponseData);

        /// <summary>
        /// Maps page data to previous data and provides total page size in RecordCount
        /// </summary>
        /// <typeparam name="T">Request object type</typeparam>
        /// <param name="responseData">service response</param>
        /// <param name="totalIterations">Current value of total iterations</param>
        /// <param name="serviceResponse">Current page response</param>
        /// <param name="currentIteration">Current iteration</param>
        /// <returns>Current page data mapped with all previous page data</returns>
        JObject HandlePagination<T>(HttpServiceModel<T> responseData, int totalIterations, JObject serviceResponse, int currentIteration);
    }
}