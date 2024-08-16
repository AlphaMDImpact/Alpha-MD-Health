using AlphaMDHealth.Utility;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Model which is used to http service calls Operations
    /// </summary>
    /// <typeparam name="T">Generic Parameter T</typeparam>
    public class HttpServiceModel<T> : BaseDTO
    {
        /// <summary>
        /// Auth token, if authorization header/basic authentication is required
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Type of Authorization header. Bearer or basic
        /// </summary>
        public AuthorizationType AuthType { get; set; } = AuthorizationType.Bearer;

        /// <summary>
        /// The base url of the request
        /// </summary>
        public Uri BaseUrl { get; set; }

        /// <summary>
        /// Cancellation token instance for cancel service call at any time
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = default;

        /// <summary>
        /// ClientId for HMAC signature
        /// </summary>
        public string ClientIdentifier { get; set; }

        /// <summary>
        /// Secret key for HMAC signature
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The object that is to be sent in the request
        /// </summary>
        public T ContentToSend { get; set; }

        /// <summary>
        /// value for Content-Type header. Json for application/json or else FormEncoded for application/x-www-form-urlencoded
        /// </summary>
        public HttpContentType ContentType { get; set; } = HttpContentType.Json;

        /// <summary>
        /// A helper field for retrieving and comparing standard HTTP methods and for creating new HTTP methods.
        /// </summary>
        public HttpMethods HttpMethod { get; set; } = HttpMethods.GET;

        /// <summary>
        /// Query parameters for pass with HTTP request
        /// </summary>
        [DataMember]
        public NameValueCollection QueryParameters { get; set; }

        /// <summary>
        /// Headers to pass custom headers
        /// </summary>
        [DataMember]
        public Dictionary<string, string> CustomHeaders { get; set; }

        /// <summary>
        /// URL to which request is to be made. This will be appended to baseUrl
        /// </summary>
        public string PathWithoutBasePath { get; set; }

        /// <summary>
        /// For Application Identifier
        /// </summary>
        public string ForApplication { get; set; }
    }
}