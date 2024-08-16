using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web;

namespace AlphaMDHealth.CommonBusinessLayer
{
    /// <summary>
    /// Syncs data with External services
    /// </summary>
    public class HttpLibService : BaseLibService
    {
        private readonly IHttpService _consumerHttpService;

        /// <summary>
        /// Provides Api to construct HTTP requestMessage and recieve response based on the parameters provided
        /// </summary>
        /// <param name="httpService">Instance of http service</param>
        public HttpLibService(IHttpService httpService)
        {
            _consumerHttpService = httpService;
        }

        public HttpLibService(IHttpService httpService, IEssentials essentials) : base(essentials)
        {
            _consumerHttpService = httpService;
        }

        /// <summary>
        /// HTTP GET CALL
        /// </summary>
        /// <typeparam name="T">Input Type</typeparam>
        /// <param name="httpData">The requestMessage data for create http requestMessage object to get data from server</param>
        /// <returns>Json response or corresponding error codes in ServiceDTO object</returns>
        public async Task GetAsync<T>(HttpServiceModel<T> httpData)
        {
            httpData.HttpMethod = HttpMethods.GET;
            await GetAsync(httpData, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP POST CALL
        /// </summary>
        /// <typeparam name="T">Content data to send to server.</typeparam>
        /// <param name="httpData">Request body to send to server.</param>
        /// <returns>Service data with errorCode</returns>
        public async Task PostAsync<T>(HttpServiceModel<T> httpData)
        {
            httpData.HttpMethod = HttpMethods.POST;
            await SendAsync(httpData, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP POST CALL
        /// </summary>
        /// <typeparam name="T">Content data to send to server.</typeparam>
        /// <param name="httpData">Request body to send to server.</param>
        /// <returns>Service data with errorCode</returns>
        public async Task PatchAsync<T>(HttpServiceModel<T> httpData)
        {
            httpData.HttpMethod = HttpMethods.PATCH;
            await SendAsync(httpData, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP POST CALL
        /// </summary>
        /// <typeparam name="T">Content data to send to server.</typeparam>
        /// <param name="httpData">Request body to send to server.</param>
        /// <returns>Service data with errorCode</returns>
        public async Task DeleteAsync<T>(HttpServiceModel<T> httpData)
        {
            httpData.HttpMethod = HttpMethods.DELETE;
            await SendAsync(httpData, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP GET CALL
        /// </summary>
        /// <typeparam name="T">Input Type</typeparam>
        /// <param name="httpData">The requestMessage data for create http requestMessage object to get data from server</param>
        /// <param name="retryCount">Number of times service is call for limit service call looping</param>
        /// <returns>Json response or corresponding error codes in ServiceDTO object</returns>
        private async Task GetAsync<T>(HttpServiceModel<T> httpData, int retryCount)
        {
            try
            {
                //requestMessage type is basic or there is no refresh token requestMessage in process in another thread
                if (httpData.AuthType != AuthorizationType.Bearer
                    || !(_essentials.GetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, false)))
                {
                    //todo:certificate validation on mobile
                    using HttpClient httpClient = new HttpClientService().GetPlatfromSpecificHttpClient(CreateHttpClientHandler());

                    UriBuilder serviceUri = await CreateGetServiceRequestAsync(httpData, httpClient).ConfigureAwait(false);
                    int currentIteration = 1;
                    int totalIterations = (int)httpData.RecordCount;
                    JObject serviceResponse = default;
                    do
                    {
                        if (currentIteration > 1)
                        {
                            UpdateQueryParameters(serviceUri, httpData);
                        }
                        await _consumerHttpService.SetHeadersAsync(httpClient, serviceUri.Uri, httpData, null).ConfigureAwait(true);
                        GenericMethods.LogData($" ****GetAsync****:{serviceUri} | {serviceUri.Uri.PathAndQuery} ");
                        var response = await httpClient.GetAsync(serviceUri.Uri, httpData.CancellationToken).ConfigureAwait(false);
                        await MapHttpResponseAsync(httpData, response).ConfigureAwait(false);
                        GenericMethods.LogData($" ****GetAsync.Response****:{httpData.ErrCode} from {serviceUri}");
                        if (await HandleErrorCodeAsync(httpData, retryCount, true).ConfigureAwait(false))
                        {
                            // If RetryServiceCallAsync called than return from here and do not call service for rest pages, as response will already contain data of all pages
                            return;
                        }
                        // Calculate total number of pages based on total reocrd count and max page size number
                        if (httpData.ErrCode == ErrorCode.OK && !string.IsNullOrWhiteSpace(httpData.Response))
                        {
                            serviceResponse = _consumerHttpService.HandlePagination(httpData, totalIterations, serviceResponse, currentIteration);
                            totalIterations = (int)httpData.RecordCount;
                        }
                        currentIteration++;
                    }
                    while (totalIterations >= currentIteration);
                    HandleResponse(httpData, serviceResponse);
                }
                else
                {
                    await RetryServiceCallAsync(httpData, retryCount, true).ConfigureAwait(false);
                }
            }
            catch (InvalidOperationException)
            {
                await RetryServiceCallAsync(httpData, retryCount, true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HandelException(httpData, ex, ErrorCode.ErrorWhileRetrievingRecords);
            }
        }

        private async Task SendAsync<T>(HttpServiceModel<T> httpData, int retryCount)
        {
            try
            {
                //requestMessage type is basic or there is no refresh token requestMessage in process in another thread
                if (httpData.AuthType != AuthorizationType.Bearer
                    || !(_essentials.GetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, false)))
                {
                    await GetUrlAndAuthTokenAsync(httpData).ConfigureAwait(false);
                    UriBuilder serviceUri = new UriBuilder(new Uri(httpData.BaseUrl + httpData.PathWithoutBasePath));
                    HttpMethod method = new HttpMethod(httpData.HttpMethod.ToString());
                    ByteArrayContent requestBody = new ByteArrayContent(new byte[] { });
                    NameValueCollection query = HttpUtility.ParseQueryString(serviceUri.Query);
                    if (httpData.QueryParameters != null)
                    {
                        query.Add(httpData.QueryParameters);
                    }
                    _consumerHttpService.AddDefaultQueryParams(query);
                    //LibGenericMethods.LogData($" ****SendAsync****:{serviceUri} ");

                    //todo:certificate validation on mobile
                    using HttpClient httpClient = new HttpClientService().GetPlatfromSpecificHttpClient(CreateHttpClientHandler());
                    //during login
                    requestBody = await CreateSendRequestAsync(httpData, serviceUri, requestBody, query, httpClient).ConfigureAwait(false);
                    await _consumerHttpService.SetHeadersAsync(httpClient, serviceUri.Uri, httpData, await (requestBody?.ReadAsStreamAsync()).ConfigureAwait(false)).ConfigureAwait(false);
                    var response = await SendHttpCallAsync(httpClient, method, serviceUri.Uri, requestBody, httpData.CancellationToken).ConfigureAwait(false);
                    await MapHttpResponseAsync(httpData, response).ConfigureAwait(false);
                    GenericMethods.LogData($" ****SendAsync.Response****:{httpData.ErrCode} from {serviceUri}");
                    await HandleErrorCodeAsync(httpData, retryCount, false).ConfigureAwait(false);
                }
                else
                {
                    await RetryServiceCallAsync(httpData, retryCount, false).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                HandelException(httpData, ex, ErrorCode.ErrorWhileSavingRecords);
            }
        }

        private void HandelException<T>(HttpServiceModel<T> httpData, Exception ex, ErrorCode errorCode)
        {
            string errorMessage = ex.InnerException?.InnerException?.Message;
            httpData.ErrCode = (ex.InnerException?.ToString()?.Contains(Constants.HTTP_CERTIFICATE_ERROR_01) ?? false)
                || (!string.IsNullOrWhiteSpace(errorMessage)
                    && (errorMessage.Contains(Constants.HTTP_CERTIFICATE_ERROR)
                        || errorMessage.Contains(Constants.HTTP_CERTIFICATE_ERROR_02))) ? ErrorCode.UnknownCertificate : errorCode;
            LogError(ex.Message, ex);
        }

        private void HandleResponse<T>(HttpServiceModel<T> httpData, JObject serviceResponse)
        {
            if (!string.IsNullOrWhiteSpace(serviceResponse?.ToString()))
            {
                httpData.Response = serviceResponse.ToString();
            }
        }

        private HttpClientHandler CreateHttpClientHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = _consumerHttpService.OnValidateCertificate
            };
        }

        private async Task<UriBuilder> CreateGetServiceRequestAsync<T>(HttpServiceModel<T> httpData, HttpClient httpClient)
        {
            await GetUrlAndAuthTokenAsync(httpData).ConfigureAwait(false);
            UriBuilder serviceUri = new UriBuilder(new Uri(httpData.BaseUrl + httpData.PathWithoutBasePath));
            NameValueCollection query = HttpUtility.ParseQueryString(serviceUri.Query);
            if (httpData.QueryParameters != null)
            {
                query.Add(httpData.QueryParameters);
            }
            _consumerHttpService.AddDefaultQueryParams(query);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_CONTENT_TYPE_KEY, Constants.SE_ACCEPT_HEADER_JSON_KEY);
            await SetAuthTokenBasedOnTypeAsync(httpData, httpClient).ConfigureAwait(false);
            //httpClient.Timeout = TimeSpan.FromMilliseconds(-1); //new TimeSpan(0, LibConstants.HTTP_GET_TIMEOUT_MINUTE_INTERVAL, 0);
            serviceUri.Query = query.ToString();
            return serviceUri;
        }

        private void UpdateQueryParameters<T>(UriBuilder serviceUri, HttpServiceModel<T> httpData)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(serviceUri.Query);
            if (httpData.QueryParameters != null)
            {
                foreach (var queryParam in httpData.QueryParameters.AllKeys)
                {
                    query.Remove(queryParam);
                }
                query.Add(httpData.QueryParameters);
            }
            serviceUri.Query = query.ToString();
        }

        private async Task<bool> HandleErrorCodeAsync<T>(HttpServiceModel<T> httpData, int retryCount, bool isGetRequest)
        {
            if (httpData.ErrCode == ErrorCode.UseRefreshToken)
            {
                if (GetRefreshTokenInProgressKey())
                {
                    await RetryServiceCallAsync(httpData, retryCount, isGetRequest).ConfigureAwait(false);
                    return true;
                }
                else
                {
                    _essentials.SetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, true);
                    await _consumerHttpService.RefreshTokenAsync(httpData).ConfigureAwait(false);
                    _essentials.SetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, false);
                    if (httpData.ErrCode == ErrorCode.OK)
                    {
                        await RetryServiceCallAsync(httpData, retryCount, isGetRequest).ConfigureAwait(false);
                        return true;
                    }
                }
            }
            if (httpData.ErrCode == ErrorCode.Unauthorized)
            {
                await CheckIsTokenRefreshingAndWaitAsync().ConfigureAwait(false);
                string latestToken = await _consumerHttpService.GetAccessTokenAsync(httpData.AuthType).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(latestToken))
                {
                    return false;
                }
                else if (httpData.AuthToken == latestToken)
                {
                    if (retryCount < 2)
                    {
                        retryCount += 1;
                        await RetryServiceCallAsync(httpData, retryCount, isGetRequest).ConfigureAwait(false);
                        return true;
                    }
                }
                else
                {
                    await RetryServiceCallAsync(httpData, retryCount, isGetRequest).ConfigureAwait(false);
                    return true;
                }
            }
            return false;
        }

        private bool GetRefreshTokenInProgressKey()
        {
            return _essentials.GetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, false);
        }

        private async Task MapHttpResponseAsync<T>(HttpServiceModel<T> httpData, HttpResponseMessage httpResponseData)
        {
            try
            {
                if (!await _consumerHttpService.MapHttpResponseAsync(httpData, httpResponseData).ConfigureAwait(false))
                {
                    if (httpResponseData != null)
                    {
                        httpData.ErrCode = (ErrorCode)httpResponseData.StatusCode;
                        httpData.Response = await httpResponseData.Content.ReadAsStringAsync().ConfigureAwait(true);
                        if (httpData.ErrCode == ErrorCode.MultiStatus)
                        {
                            var responseStream = await httpResponseData.Content.ReadAsStreamAsync().ConfigureAwait(true);
                            await DeserializeAsync<BaseDTO>(responseStream, httpData).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        httpData.ErrCode = httpData.HttpMethod == HttpMethods.GET
                            ? ErrorCode.ErrorWhileRetrievingRecords
                            : ErrorCode.ErrorWhileSavingRecords;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<HttpResponseMessage> SendHttpRequestAsync<T>(HttpServiceModel<T> httpData)
        {
            //requestMessage type is basic or there is no refresh token requestMessage in process in another thread
            if (httpData.AuthType != AuthorizationType.Bearer
                || !(_essentials.GetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, false)))
            {
                await GetUrlAndAuthTokenAsync(httpData).ConfigureAwait(false);
                UriBuilder serviceUri = new UriBuilder(new Uri(httpData.BaseUrl + httpData.PathWithoutBasePath));
                HttpMethod method = new HttpMethod(httpData.HttpMethod.ToString());
                ByteArrayContent requestBody = new ByteArrayContent(new byte[] { });
                NameValueCollection query = HttpUtility.ParseQueryString(serviceUri.Query);
                if (httpData.QueryParameters != null)
                {
                    query.Add(httpData.QueryParameters);
                }
                _consumerHttpService.AddDefaultQueryParams(query);
                //HttpClientHandler handler = new HttpClientHandler
                //{
                //    ServerCertificateCustomValidationCallback = _consumerHttpService.OnValidateCertificate
                //};

                //todo:certificate validation on mobile
                using HttpClient httpClient = new HttpClientService().GetPlatfromSpecificHttpClient(CreateHttpClientHandler());
                requestBody = await CreateSendRequestAsync(httpData, serviceUri, requestBody, query, httpClient).ConfigureAwait(false);
                await _consumerHttpService.SetHeadersAsync(httpClient, serviceUri.Uri, httpData, await (requestBody?.ReadAsStreamAsync()).ConfigureAwait(false)).ConfigureAwait(false);
                return await SendHttpCallAsync(httpClient, method, serviceUri.Uri, requestBody, httpData.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                await CheckIsTokenRefreshingAndWaitAsync().ConfigureAwait(false);
                httpData.AuthToken = null;
                return await SendHttpRequestAsync(httpData).ConfigureAwait(false);
            }
        }

        private async Task<ByteArrayContent> CreateSendRequestAsync<T>(HttpServiceModel<T> httpData, UriBuilder serviceUri, ByteArrayContent requestBody, NameValueCollection query, HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_CONTENT_TYPE_KEY,
                httpData.ContentType == HttpContentType.FormEncoded ? Constants.SE_ACCEPT_HEADER_FORM_ENCODED_KEY : Constants.SE_ACCEPT_HEADER_JSON_KEY);
            await SetAuthTokenBasedOnTypeAsync(httpData, httpClient).ConfigureAwait(false);
            serviceUri.Query = query.ToString();
            if (!Equals(httpData.ContentToSend, default))
            {
                if (httpData.ContentType == HttpContentType.FormEncoded)
                {
                    return new FormUrlEncodedContent(httpData.ContentToSend as IEnumerable<KeyValuePair<string, string>>);
                }
                else
                {
                    return (httpData.ContentToSend is string)
                                   ? new StringContent(httpData.ContentToSend.ToString(), Encoding.UTF8, Constants.SE_ACCEPT_HEADER_JSON_KEY)
                                   : new StringContent(JsonConvert.SerializeObject(httpData.ContentToSend, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, Constants.SE_ACCEPT_HEADER_JSON_KEY);
                }
            }
            return requestBody;
        }

        private async Task SetAuthTokenBasedOnTypeAsync<T>(HttpServiceModel<T> httpData, HttpClient httpClient)
        {
            if (httpData.AuthType != AuthorizationType.NoAuth)
            {
                if (string.IsNullOrWhiteSpace(httpData.AuthToken))
                {
                    httpData.AuthToken = await _consumerHttpService.GetAccessTokenAsync(httpData.AuthType).ConfigureAwait(false);
                }
                if (!string.IsNullOrWhiteSpace(httpData.AuthToken))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(Constants.SE_AUTHORIZATION_HEADER_KEY, $"{httpData.AuthType} {httpData.AuthToken}");
                }
            }
        }

        private async Task<HttpResponseMessage> SendHttpCallAsync(HttpClient httpClient, HttpMethod httpMethod, Uri requestUri, HttpContent requestBody, CancellationToken cancellationToken)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(300);  //TimeSpan.FromMilliseconds(10); //new TimeSpan(0, LibConstants.HTTP_GET_TIMEOUT_MINUTE_INTERVAL, 0);
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, requestUri) { Content = requestBody };
            // return await httpClient.PostAsync(requestUri, requestBody).ConfigureAwait(false);

            return await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            
        }

        private async Task CheckIsTokenRefreshingAndWaitAsync()
        {
            await Task.Run(() =>
            {
                //check if there is any refresh toekn requestMessage in progress
                if (_essentials.GetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, false))
                {
                    //if yes wait untill the refresh token is completed
                    while (_essentials.GetPreferenceValue(StorageConstants.PR_TOKEN_REFRESH_IN_PROGRESS_KEY, false))
                    {
                        //wait untill the refresh token is completed
                    }
                }
            }).ConfigureAwait(false);
        }

        private async Task RetryServiceCallAsync<T>(HttpServiceModel<T> httpData, int retryCount, bool isGetRequest)
        {
            await CheckIsTokenRefreshingAndWaitAsync().ConfigureAwait(false);
            httpData.AuthToken = null;
            if (isGetRequest)
            {
                await GetAsync(httpData, retryCount).ConfigureAwait(false);
            }
            else
            {
                await SendAsync(httpData, retryCount).ConfigureAwait(false);
            }
        }

        private async Task GetUrlAndAuthTokenAsync<T>(HttpServiceModel<T> httpData)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(httpData.BaseUrl, CultureInfo.InvariantCulture)))
            {
                httpData.BaseUrl = new Uri(await _consumerHttpService.GetSelectedBaseUrlAsync().ConfigureAwait(false));
            }
            if (string.IsNullOrWhiteSpace(httpData.AuthToken))
            {
                httpData.AuthToken = await _consumerHttpService.GetAccessTokenAsync(httpData.AuthType).ConfigureAwait(false);
            }
        }
    }
}