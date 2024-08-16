using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Text;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class HttpService
    {
        /// <summary>
        /// Send http request
        /// </summary>
        /// <param name="result">use this DTO to get service response</param>
        /// <param name="httpMethod">Methos type GET/POST</param>
        /// <param name="requestUri">Formatted url to call API</param>
        /// <param name="content">Method body in case of post call</param>
        /// <param name="headers">Send headers as dictionary</param>
        public async Task SendHttpRequestAsync(BaseDTO result, HttpMethod httpMethod, Uri requestUri, string content, Dictionary<string, string> headers)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                HttpRequestMessage request = new HttpRequestMessage(httpMethod, requestUri);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    request.Content = new StringContent(content, Encoding.UTF8, Constants.MEDIA_TYPE);
                }
                if (headers?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> item in headers)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    result.Response = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    result.ErrCode = ErrorCode.OK;
                }
                else
                {
                    result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                }
            }
        }

        /// <summary>
        /// Http get call
        /// </summary>
        /// <param name="httpMethod">Methos type GET/POST</param>
        /// <param name="requestUri">Formatted url to call API</param>
        /// <returns>Http Response</returns>
        public async Task<HttpResponseMessage> GetImageAsync(HttpMethod httpMethod, Uri requestUri)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                HttpRequestMessage request = new HttpRequestMessage(httpMethod, requestUri);
                return await client.SendAsync(request).ConfigureAwait(false);
            }
        }
    }
}