using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class HttpService : IHttpService
    {
        public IEssentials _serviceEssentials;

        /// <summary>
        /// Provides Api to construct HTTP request and recieve response based on the parameters provided
        /// </summary>
        /// <param name="serviceEssentials">Instance of atom essentials</param>
        public HttpService(IEssentials serviceEssentials)
        {
            _serviceEssentials = serviceEssentials;
        }

        public HttpService()
        {
           
        }

        /// <summary>
        /// Adds default query parameters required for service calls
        /// </summary>
        /// <param name="query">Existing query paramters</param>
        public void AddDefaultQueryParams(NameValueCollection query)
        {

            try
            {
                if (query[Constants.SE_LANGUAGE_ID_QUERY_KEY] == null)
                {
                    query.Add(Constants.SE_LANGUAGE_ID_QUERY_KEY, _serviceEssentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0).ToString(CultureInfo.InvariantCulture));
                }
                if (query[Constants.SE_SELECTED_USER_ID_QUERY_KEY] == null)
                {
                    query.Add(Constants.SE_SELECTED_USER_ID_QUERY_KEY, _serviceEssentials.GetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, (long)0).ToString(CultureInfo.InvariantCulture));
                }
                if (query[Constants.SE_ORGANISATION_ID_QUERY_KEY] == null)
                {
                    query.Add(Constants.SE_ORGANISATION_ID_QUERY_KEY, _serviceEssentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, Constants.DEFAULT_ORGANISATION_ID).ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                //to delete
            }
        }

        /// <summary>
        /// Gets access token for the service call
        /// </summary>
        /// <param name="authorizationType">Authorization Type</param>
        /// <returns>Access token</returns>
        public async Task<string> GetAccessTokenAsync(AuthorizationType authorizationType)
        {
            return authorizationType == AuthorizationType.Bearer
                ? await _serviceEssentials.GetSecureStorageValueAsync(StorageConstants.SS_ACCESS_TOKEN_KEY).ConfigureAwait(false)
                : string.Empty;
        }

        /// <summary>
        /// Gets the selected base url for the service call
        /// </summary>
        /// <returns>Selected base url string</returns>
        public async Task<string> GetSelectedBaseUrlAsync()
        {
            return await new EnvironmentService(_serviceEssentials).GetSelectedBaseUrlAsync(UrlConstants.DEFAULT_ENVIRONMENT_KEY_VALUE).ConfigureAwait(false);
        }

        /// <summary>
        /// Http certicate validation callback
        /// </summary>
        /// <param name="sender">Sender object of http call</param>
        /// <param name="certificate">Received certificate for the given call</param>
        /// <param name="chain">chain-building engine for System.Security.Cryptography.X509Certificates.X509Certificate2</param>
        /// <param name="sslPolicyErrors">Ssl policy errors</param>
        /// <returns>true if valid else return false</returns>
        public bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return new DataSyncService(_serviceEssentials).ValidateCertificate(certificate?.GetPublicKeyString());
        }

        /// <summary>
        /// Refresh token async
        /// </summary>
        /// <param name="responseData">Response received from service</param>
        /// <returns>The task object representing the operation to refresh token</returns>
        public Task RefreshTokenAsync(BaseDTO responseData)
        {
            return new AuthService(_serviceEssentials).RefreshTokenAsync(responseData);
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
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_DEVICE_UNIQUE_ID_HEADER_KEY, await _serviceEssentials.GetDeviceIDAsync().ConfigureAwait(false));
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_DEVICE_TYPE_HEADER_KEY, _serviceEssentials.DeviceType);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_DEVICE_PLATFORM_HEADER_KEY, MobileConstants.IsMobilePlatform ? _serviceEssentials.DeviceOS : Constants.CLIENT_PLATFORM_WEB);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_DEVICE_INFORMATION_HEADER_KEY, $"{_serviceEssentials.DeviceManufacturer},{_serviceEssentials.DeviceModel},{_serviceEssentials.DeviceOS},{_serviceEssentials.DeviceOSVersionString}");
            TryGetClientItentity(out string clientIdentifier, out string clientSecret);

            string requestContentBase64String = string.Empty;
            if (content?.Length > 0)
            {
                requestContentBase64String = Convert.ToBase64String(GenericMethods.ComputeHash(content));
                // Needs to reset content stream as the controller needs to read the content data (i.e. body of POST/PUT calls)
                content.Seek(0, SeekOrigin.Begin);
            }
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(clientSecret);
            byte[] dataBytes = Encoding.UTF8.GetBytes($"{clientIdentifier}{httpRequestData.HttpMethod}{requestUri.AbsoluteUri}{(string.IsNullOrWhiteSpace(httpRequestData.AuthToken) ? string.Empty : Constants.SE_BEARER_TEXT_KEY + httpRequestData.AuthToken)}{requestContentBase64String}");
            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_HMAC_SIGNATURE_HEADER_KEY, Convert.ToBase64String(hmac.ComputeHash(dataBytes)));
            }
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_CLIENT_IDENTIFIER_HEADER_KEY, clientIdentifier);
        }

        private void TryGetClientItentity(out string clientIdentifier, out string clientSecret)
        {
            try
            {
                if (MobileConstants.IsDeviceTablet)
                {
                    clientIdentifier = GenericMethods.GetPlatformSpecificValue(Constants.CLIENT_ID_IOS_TABLET, Constants.CLIENT_ID_ANDROID_TABLET, Constants.CLIENT_ID_WEB);
                    clientSecret = GenericMethods.GetPlatformSpecificValue(Constants.CLIENT_SECRET_IOS_TABLET, Constants.CLIENT_SECRET_ANDROID_TABLET, Constants.CLIENT_SECRET_WEB);
                    return;
                }
            }
            catch (Exception ex)
            {
            }
            clientIdentifier = GenericMethods.GetPlatformSpecificValue(Constants.CLIENT_ID_IOS_PHONE, Constants.CLIENT_ID_ANDROID_PHONE, Constants.CLIENT_ID_WEB);
            clientSecret = GenericMethods.GetPlatformSpecificValue(Constants.CLIENT_SECRET_IOS_PHONE, Constants.CLIENT_SECRET_ANDROID_PHONE, Constants.CLIENT_SECRET_WEB);
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

        /// <summary>
        /// Get clients Geo location based on IP address
        /// </summary>
        /// <typeparam name="T">Request body type</typeparam>
        /// <param name="httpServiceModel">service request data</param>
        /// <returns>Operation status and service response</returns>
        public async Task GetDefaultCountryCodeAsync<T>(HttpServiceModel<T> httpServiceModel)
        {
            httpServiceModel.HttpMethod = HttpMethods.GET;
            UriBuilder serviceUri = new UriBuilder(httpServiceModel.BaseUrl);
            HttpMethod method = new HttpMethod(Convert.ToString(httpServiceModel.HttpMethod, CultureInfo.InvariantCulture));
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(-1); // new TimeSpan(0, LibConstants.HTTP_GET_TIMEOUT_MINUTE_INTERVAL, 0);
                var response = await httpClient.SendAsync(new HttpRequestMessage(method, serviceUri.Uri), httpServiceModel.CancellationToken).ConfigureAwait(false);
                if (response != null)
                {
                    httpServiceModel.ErrCode = (ErrorCode)response.StatusCode;
                    httpServiceModel.Response = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    httpServiceModel.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                }
            }
        }
    }
}