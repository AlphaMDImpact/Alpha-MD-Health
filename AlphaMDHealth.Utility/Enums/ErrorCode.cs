namespace AlphaMDHealth.Utility;

/// <summary>
/// ErrorCode For Database Operation
/// </summary>
public enum ErrorCode
{
    #region Default API Error codes

    /// <summary>
    /// Equivalent to HTTP status 100. System.Net.HttpStatusCode.Continue indicates that
    /// the client can continue with its request.
    /// </summary>
    Continue = 100,

    /// <summary>
    /// Equivalent to HTTP status 101. System.Net.HttpStatusCode.SwitchingProtocols indicates
    /// that the protocol version or protocol is being changed.
    /// </summary>
    SwitchingProtocols = 101,

    /// <summary>
    /// Equivalent to HTTP status 102. System.Net.HttpStatusCode.Processing indicates
    /// that the server has accepted the complete request but hasn't completed it yet.
    /// </summary>
    Processing = 102,

    /// <summary>
    /// Equivalent to HTTP status 103. System.Net.HttpStatusCode.EarlyHints indicates
    /// to the client that the server is likely to send a final response with the header
    /// fields included in the informational response.
    /// </summary>
    EarlyHints = 103,

    /// <summary>
    /// Equivalent to HTTP status 200. System.Net.HttpStatusCode.OK indicates that the
    /// request succeeded and that the requested information is in the response. This
    /// is the most common status code to receive.
    /// </summary>
    OK = 200,

    /// <summary>
    /// Equivalent to HTTP status 201. System.Net.HttpStatusCode.Created indicates that
    /// the request resulted in a new resource created before the response was sent.
    /// </summary>
    Created = 201,

    /// <summary>
    /// Equivalent to HTTP status 202. System.Net.HttpStatusCode.Accepted indicates that
    /// the request has been accepted for further processing.
    /// </summary>
    Accepted = 202,

    /// <summary>
    /// Equivalent to HTTP status 203. System.Net.HttpStatusCode.NonAuthoritativeInformation
    /// indicates that the returned meta information is from a cached copy instead of
    /// the origin server and therefore may be incorrect.
    /// </summary>
    NonAuthoritativeInformation = 203,

    /// <summary>
    /// Equivalent to HTTP status 204. System.Net.HttpStatusCode.NoContent indicates
    /// that the request has been successfully processed and that the response is intentionally
    /// blank.
    /// </summary>
    NoContent = 204,

    /// <summary>
    /// Equivalent to HTTP status 205. System.Net.HttpStatusCode.ResetContent indicates
    /// that the client should reset (not reload) the current resource.
    /// </summary>
    ResetContent = 205,

    /// <summary>
    /// Equivalent to HTTP status 206. System.Net.HttpStatusCode.PartialContent indicates
    /// that the response is a partial response as requested by a GET request that includes
    /// a byte range.
    /// </summary>
    PartialContent = 206,

    /// <summary>
    /// Equivalent to HTTP status 207. System.Net.HttpStatusCode.MultiStatus indicates
    /// multiple status codes for a single response during a Web Distributed Authoring
    /// and Versioning (WebDAV) operation. The response body contains XML that describes
    /// the status codes.
    /// </summary>
    MultiStatus = 207,

    /// <summary>
    /// Equivalent to HTTP status 208. System.Net.HttpStatusCode.AlreadyReported indicates
    /// that the members of a WebDAV binding have already been enumerated in a preceding
    /// part of the multistatus response, and are not being included again.
    /// </summary>
    AlreadyReported = 208,

    /// <summary>
    /// Equivalent to HTTP status 226. System.Net.HttpStatusCode.IMUsed indicates that
    /// the server has fulfilled a request for the resource, and the response is a representation
    /// of the result of one or more instance-manipulations applied to the current instance.
    /// </summary>
    IMUsed = 226,

    /// <summary>
    /// Equivalent to HTTP status 300. System.Net.HttpStatusCode.Ambiguous indicates
    /// that the requested information has multiple representations. The default action
    /// is to treat this status as a redirect and follow the contents of the Location
    /// header associated with this response. Ambiguous is a synonym for MultipleChoices.
    /// </summary>
    Ambiguous = 300,

    /// <summary>
    /// Equivalent to HTTP status 300. System.Net.HttpStatusCode.MultipleChoices indicates
    /// that the requested information has multiple representations. The default action
    /// is to treat this status as a redirect and follow the contents of the Location
    /// header associated with this response. MultipleChoices is a synonym for Ambiguous.
    /// </summary>
    MultipleChoices = 300,

    /// <summary>
    /// Equivalent to HTTP status 301. System.Net.HttpStatusCode.Moved indicates that
    /// the requested information has been moved to the URI specified in the Location
    /// header. The default action when this status is received is to follow the Location
    /// header associated with the response. When the original request method was POST,
    /// the redirected request will use the GET method. Moved is a synonym for MovedPermanently.
    /// </summary>
    Moved = 301,

    /// <summary>
    /// Equivalent to HTTP status 301. System.Net.HttpStatusCode.MovedPermanently indicates
    /// that the requested information has been moved to the URI specified in the Location
    /// header. The default action when this status is received is to follow the Location
    /// header associated with the response. MovedPermanently is a synonym for Moved.
    /// </summary>
    MovedPermanently = 301,

    /// <summary>
    /// Equivalent to HTTP status 302. System.Net.HttpStatusCode.Found indicates that
    /// the requested information is located at the URI specified in the Location header.
    /// The default action when this status is received is to follow the Location header
    /// associated with the response. When the original request method was POST, the
    /// redirected request will use the GET method. Found is a synonym for Redirect.
    /// </summary>
    Found = 302,

    /// <summary>
    /// Equivalent to HTTP status 302. System.Net.HttpStatusCode.Redirect indicates that
    /// the requested information is located at the URI specified in the Location header.
    /// The default action when this status is received is to follow the Location header
    /// associated with the response. When the original request method was POST, the
    /// redirected request will use the GET method. Redirect is a synonym for Found.
    /// </summary>
    Redirect = 302,

    /// <summary>
    /// Equivalent to HTTP status 303. System.Net.HttpStatusCode.RedirectMethod automatically
    /// redirects the client to the URI specified in the Location header as the result
    /// of a POST. The request to the resource specified by the Location header will
    /// be made with a GET. RedirectMethod is a synonym for SeeOther.
    /// </summary>
    RedirectMethod = 303,

    /// <summary>
    /// Equivalent to HTTP status 303. System.Net.HttpStatusCode.SeeOther automatically
    /// redirects the client to the URI specified in the Location header as the result
    /// of a POST. The request to the resource specified by the Location header will
    /// be made with a GET. SeeOther is a synonym for RedirectMethod.
    /// </summary>
    SeeOther = 303,

    /// <summary>
    /// Equivalent to HTTP status 304. System.Net.HttpStatusCode.NotModified indicates
    /// that the client's cached copy is up to date. The contents of the resource are
    /// not transferred.
    /// </summary>
    NotModified = 304,

    /// <summary>
    /// Equivalent to HTTP status 305. System.Net.HttpStatusCode.UseProxy indicates that
    /// the request should use the proxy server at the URI specified in the Location
    /// header.
    /// </summary>
    UseProxy = 305,

    /// <summary>
    /// Equivalent to HTTP status 306. System.Net.HttpStatusCode.Unused is a proposed
    /// extension to the HTTP/1.1 specification that is not fully specified.
    /// </summary>
    Unused = 306,

    /// <summary>
    /// Equivalent to HTTP status 307. System.Net.HttpStatusCode.RedirectKeepVerb indicates
    /// that the request information is located at the URI specified in the Location
    /// header. The default action when this status is received is to follow the Location
    /// header associated with the response. When the original request method was POST,
    /// the redirected request will also use the POST method. RedirectKeepVerb is a synonym
    /// for TemporaryRedirect.
    /// </summary>
    RedirectKeepVerb = 307,

    /// <summary>
    /// Equivalent to HTTP status 307. System.Net.HttpStatusCode.TemporaryRedirect indicates
    /// that the request information is located at the URI specified in the Location
    /// header. The default action when this status is received is to follow the Location
    /// header associated with the response. When the original request method was POST,
    /// the redirected request will also use the POST method. TemporaryRedirect is a
    /// synonym for RedirectKeepVerb.
    /// </summary>
    TemporaryRedirect = 307,

    /// <summary>
    /// Equivalent to HTTP status 308. System.Net.HttpStatusCode.PermanentRedirect indicates
    /// that the request information is located at the URI specified in the Location
    /// header. The default action when this status is received is to follow the Location
    /// header associated with the response. When the original request method was POST,
    /// the redirected request will also use the POST method.
    /// </summary>
    PermanentRedirect = 308,

    /// <summary>
    /// Equivalent to HTTP status 400. System.Net.HttpStatusCode.BadRequest indicates
    /// that the request could not be understood by the server. System.Net.HttpStatusCode.BadRequest
    /// is sent when no other error is applicable, or if the exact error is unknown or
    /// does not have its own error code.
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// Equivalent to HTTP status 401. System.Net.HttpStatusCode.Unauthorized indicates
    /// that the requested resource requires authentication. The WWW-Authenticate header
    /// contains the details of how to perform the authentication.
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Equivalent to HTTP status 402. System.Net.HttpStatusCode.PaymentRequired is reserved
    /// for future use.
    /// </summary>
    PaymentRequired = 402,

    /// <summary>
    /// Equivalent to HTTP status 403. System.Net.HttpStatusCode.Forbidden indicates
    /// that the server refuses to fulfill the request.
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// Equivalent to HTTP status 404. System.Net.HttpStatusCode.NotFound indicates that
    /// the requested resource does not exist on the server.
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// Equivalent to HTTP status 405. System.Net.HttpStatusCode.MethodNotAllowed indicates
    /// that the request method (POST or GET) is not allowed on the requested resource.
    /// </summary>
    MethodNotAllowed = 405,

    /// <summary>
    /// Equivalent to HTTP status 406. System.Net.HttpStatusCode.NotAcceptable indicates
    /// that the client has indicated with Accept headers that it will not accept any
    /// of the available representations of the resource.
    /// </summary>
    NotAcceptable = 406,

    /// <summary>
    /// Equivalent to HTTP status 407. System.Net.HttpStatusCode.ProxyAuthenticationRequired
    /// indicates that the requested proxy requires authentication. The Proxy-authenticate
    /// header contains the details of how to perform the authentication.
    /// </summary>
    ProxyAuthenticationRequired = 407,

    /// <summary>
    /// Equivalent to HTTP status 408. System.Net.HttpStatusCode.RequestTimeout indicates
    /// that the client did not send a request within the time the server was expecting
    /// the request.
    /// </summary>
    RequestTimeout = 408,

    /// <summary>
    /// Equivalent to HTTP status 409. System.Net.HttpStatusCode.Conflict indicates that
    /// the request could not be carried out because of a conflict on the server.
    /// </summary>
    Conflict = 409,

    /// <summary>
    /// Equivalent to HTTP status 410. System.Net.HttpStatusCode.Gone indicates that
    /// the requested resource is no longer available.
    /// </summary>
    Gone = 410,

    /// <summary>
    /// Equivalent to HTTP status 411. System.Net.HttpStatusCode.LengthRequired indicates
    /// that the required Content-length header is missing.
    /// </summary>
    LengthRequired = 411,

    /// <summary>
    /// Equivalent to HTTP status 412. System.Net.HttpStatusCode.PreconditionFailed indicates
    /// that a condition set for this request failed, and the request cannot be carried
    /// out. Conditions are set with conditional request headers like If-Match, If-None-Match,
    /// or If-Unmodified-Since.
    /// </summary>
    PreconditionFailed = 412,

    /// <summary>
    /// Equivalent to HTTP status 413. System.Net.HttpStatusCode.RequestEntityTooLarge
    /// indicates that the request is too large for the server to process.
    /// </summary>
    RequestEntityTooLarge = 413,

    /// <summary>
    /// Equivalent to HTTP status 414. System.Net.HttpStatusCode.RequestUriTooLong indicates
    /// that the URI is too long.
    /// </summary>
    RequestUriTooLong = 414,

    /// <summary>
    /// Equivalent to HTTP status 415. System.Net.HttpStatusCode.UnsupportedMediaType
    /// indicates that the request is an unsupported type.
    /// </summary>
    UnsupportedMediaType = 415,

    /// <summary>
    /// Equivalent to HTTP status 416. System.Net.HttpStatusCode.RequestedRangeNotSatisfiable
    /// indicates that the range of data requested from the resource cannot be returned,
    /// either because the beginning of the range is before the beginning of the resource,
    /// or the end of the range is after the end of the resource.
    /// </summary>
    RequestedRangeNotSatisfiable = 416,

    /// <summary>
    /// Equivalent to HTTP status 417. System.Net.HttpStatusCode.ExpectationFailed indicates
    /// that an expectation given in an Expect header could not be met by the server.
    /// </summary>
    ExpectationFailed = 417,

    /// <summary>
    /// Equivalent to HTTP status 421. System.Net.HttpStatusCode.MisdirectedRequest indicates
    /// that the request was directed at a server that is not able to produce a response.
    /// </summary>
    MisdirectedRequest = 421,

    /// <summary>
    /// Equivalent to HTTP status 422. System.Net.HttpStatusCode.UnprocessableEntity
    /// indicates that the request was well-formed but was unable to be followed due
    /// to semantic errors.
    /// </summary>
    UnprocessableEntity = 422,

    /// <summary>
    /// Equivalent to HTTP status 423. System.Net.HttpStatusCode.Locked indicates that
    /// the source or destination resource is locked.
    /// </summary>
    Locked = 423,

    /// <summary>
    /// Equivalent to HTTP status 424. System.Net.HttpStatusCode.FailedDependency indicates
    /// that the method couldn't be performed on the resource because the requested action
    /// depended on another action and that action failed.
    /// </summary>
    FailedDependency = 424,

    /// <summary>
    /// Equivalent to HTTP status 426. System.Net.HttpStatusCode.UpgradeRequired indicates
    /// that the client should switch to a different protocol such as TLS/1.0.
    /// </summary>
    UpgradeRequired = 426,

    /// <summary>
    /// Equivalent to HTTP status 428. System.Net.HttpStatusCode.PreconditionRequired
    /// indicates that the server requires the request to be conditional.
    /// </summary>
    PreconditionRequired = 428,

    /// <summary>
    /// Equivalent to HTTP status 429. System.Net.HttpStatusCode.TooManyRequests indicates
    /// that the user has sent too many requests in a given amount of time.
    /// </summary>
    TooManyRequests = 429,

    /// <summary>
    /// Equivalent to HTTP status 431. System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge
    /// indicates that the server is unwilling to process the request because its header
    /// fields (either an individual header field or all the header fields collectively)
    /// are too large.
    /// </summary>
    RequestHeaderFieldsTooLarge = 431,

    /// <summary>
    /// Equivalent to HTTP status 451. System.Net.HttpStatusCode.UnavailableForLegalReasons
    /// indicates that the server is denying access to the resource as a consequence
    /// of a legal demand.
    /// </summary>
    UnavailableForLegalReasons = 451,

    /// <summary>
    /// Equivalent to HTTP status 500. System.Net.HttpStatusCode.InternalServerError
    /// indicates that a generic error has occurred on the server.
    /// </summary>
    InternalServerError = 500,

    /// <summary>
    /// Equivalent to HTTP status 501. System.Net.HttpStatusCode.NotImplemented indicates
    /// that the server does not support the requested function.
    /// </summary>
    NotImplemented = 501,

    /// <summary>
    /// Equivalent to HTTP status 502. System.Net.HttpStatusCode.BadGateway indicates
    /// that an intermediate proxy server received a bad response from another proxy
    /// or the origin server.
    /// </summary>
    BadGateway = 502,

    /// <summary>
    /// Equivalent to HTTP status 503. System.Net.HttpStatusCode.ServiceUnavailable indicates
    /// that the server is temporarily unavailable, usually due to high load or maintenance.
    /// </summary>
    ServiceUnavailable = 503,

    /// <summary>
    /// Equivalent to HTTP status 504. System.Net.HttpStatusCode.GatewayTimeout indicates
    /// that an intermediate proxy server timed out while waiting for a response from
    /// another proxy or the origin server.
    /// </summary>
    GatewayTimeout = 504,

    /// <summary>
    /// Equivalent to HTTP status 505. System.Net.HttpStatusCode.HttpVersionNotSupported
    /// indicates that the requested HTTP version is not supported by the server.
    /// </summary>
    HttpVersionNotSupported = 505,

    /// <summary>
    /// Equivalent to HTTP status 506. System.Net.HttpStatusCode.VariantAlsoNegotiates
    /// indicates that the chosen variant resource is configured to engage in transparent
    /// content negotiation itself and, therefore, isn't a proper endpoint in the negotiation
    /// process.
    /// </summary>
    VariantAlsoNegotiates = 506,

    /// <summary>
    /// Equivalent to HTTP status 507. System.Net.HttpStatusCode.InsufficientStorage
    /// indicates that the server is unable to store the representation needed to complete
    /// the request.
    /// </summary>
    InsufficientStorage = 507,

    /// <summary>
    /// Equivalent to HTTP status 508. System.Net.HttpStatusCode.LoopDetected indicates
    /// that the server terminated an operation because it encountered an infinite loop
    /// while processing a WebDAV request with "Depth: infinity". This status code is
    /// meant for backward compatibility with clients not aware of the 208 status code
    /// System.Net.HttpStatusCode.AlreadyReported appearing in multistatus response bodies.
    /// </summary>
    LoopDetected = 508,

    /// <summary>
    /// Equivalent to HTTP status 510. System.Net.HttpStatusCode.NotExtended indicates
    /// that further extensions to the request are required for the server to fulfill
    /// it.
    /// </summary>
    NotExtended = 510,

    /// <summary>
    /// Equivalent to HTTP status 511. System.Net.HttpStatusCode.NetworkAuthenticationRequired
    /// indicates that the client needs to authenticate to gain network access; it's
    /// intended for use by intercepting proxies used to control access to the network.
    /// </summary>
    NetworkAuthenticationRequired = 511,

    #endregion

    #region Custom Error codes

    /// <summary>
    /// Error during data fetch operation
    /// </summary>
    ErrorWhileRetrievingRecords = 600,

    /// <summary>
    /// Error during save operation
    /// </summary>
    ErrorWhileSavingRecords = 601,

    /// <summary>
    /// Error during delete operation
    /// </summary>
    ErrorWhileDeletingRecords = 602,

    /// <summary>
    /// User already exists with given email/Phone number
    /// </summary>
    DuplicateUser = 603,

    /// <summary>
    /// Error due to multiple users for selected account, needs to select one
    /// </summary>
    MultipleUsers = 604,

    /// <summary>
    /// Error to indicate user is not active
    /// </summary>
    InActiveUser = 605,

    /// <summary>
    /// Error to indicate given OTP is not valid
    /// </summary>
    InValidOTP = 606,

    /// <summary>
    /// Error to indicate user needs to perform SMS authentication 
    /// </summary>
    SMSAuthentication = 607,

    /// <summary>
    /// Error to indicate Password needs to reset now
    /// </summary>
    ResetPassword = 608,

    /// <summary>
    /// Error to indicate Given password is same as existing one
    /// </summary>
    SameOldPassword = 609,

    /// <summary>
    /// Error to indicate Existing Password is expired
    /// </summary>
    PasswordExpired = 610,

    /// <summary>
    /// Error to indicate user needs to setup new password
    /// </summary>
    SetNewPassword = 611,

    /// <summary>
    /// Error to indicate users account is locked
    /// </summary>
    AccountLockout = 612,

    /// <summary>
    /// Error to indicate pincode login is required to proceed
    /// </summary>
    PinCodeLogin = 613,

    /// <summary>
    /// Error to indicate pincode setup is necessary to proceed
    /// </summary>
    SetPinCode = 614,

    /// <summary>
    /// Error to indicate needs to create new access token with the help of refresh token
    /// </summary>
    UseRefreshToken = 615,

    /// <summary>
    /// Error to indicate Token is expired or not found or invalid 
    /// </summary>
    TokenExpired = 616,

    /// <summary>
    /// Error to indicate selected Plan is not valid
    /// </summary>
    InvalidPlan = 617,

    /// <summary>
    /// Error to indicate selected plan is expired
    /// </summary>
    PlanExpired = 618,

    /// <summary>
    /// Error to indicate plan needs to renew
    /// </summary>
    RenewPlan = 619,

    /// <summary>
    /// Error to indicate Need to complete payment for selected plan
    /// </summary>
    PlanPayment = 620,

    /// <summary>
    /// Error to indicate given data is duplicate
    /// </summary>
    DuplicateData = 621,

    /// <summary>
    /// Error to indicate given data is not valid 
    /// </summary>
    InvalidData = 622,

    /// <summary>
    /// Error to indicate Needs to complete organization setup
    /// </summary>
    OrganisationSetup = 623,

    /// <summary>
    /// Error to indicate given domain is not found in server
    /// </summary>
    NoDomainFound = 625,

    /// <summary>
    /// Error to indicate given domain is not active anymore 
    /// </summary>
    DomainInactive = 626,

    /// <summary>
    /// Error to indicate internet connection not exists to perform operation
    /// </summary>
    NoInternetConnection = 627,

    /// <summary>
    /// Error to indicate Error while storing data in secure storage
    /// </summary>
    SecureStorageError = 628,

    /// <summary>
    /// Error to indicate configuration mismatch
    /// </summary>
    RecordCountMismatch = 629,

    /// <summary>
    /// Error to indicate Sync is already in progress
    /// </summary>
    SyncInProgress = 630,

    /// <summary>
    /// Error to identify redirection is already handelled or need to handel in specific page
    /// </summary>
    HandledRedirection = 631,

    /// <summary>
    /// Error to indicate something went wrong, needs to restart application
    /// </summary>
    RestartApp = 632,

    /// <summary>
    /// Error to indicate trust fail due to unknown/wrong cretificate 
    /// </summary>
    UnknownCertificate = 633,

    /// <summary>
    /// Error to indicate local Guid is already present in server and has to be regenerated
    /// </summary>
    DuplicateGuid = 634,

    /// <summary>
    /// Error to indicate local Guid is already present in server and has to be regenerated
    /// </summary>
    UpdateApp = 635,

    /// <summary>
    /// Error to indicate given data is not valid 
    /// </summary>
    InvalidPincode = 636,

    /// <summary>
    /// Error to indicate given data is not valid 
    /// </summary>
    WeakPincode = 637,

    /// <summary>
    /// Error to indicate Current device is Jailbroken
    /// </summary>
    JailBrokenDevice = 638,

    /// <summary>
    /// Error to indicate call is for refresh master page
    /// </summary>
    RefreshMasterPage = 639,

    /// <summary>
    /// Error to indicate that server already has another guid for the same record
    /// </summary>
    GuidChanged = 640,

    /// <summary>
    /// Error code to indicate selected language is not available
    /// </summary>
    LanguageNotAvailable = 641,

    /// <summary>
    /// Error to indicate given domain is not found in server
    /// </summary>
    InvalidEnvironment = 642,

    /// <summary>
    /// Error to indicate that bluetooth is off
    /// </summary>
    BluetoothOff = 643,

    /// <summary>
    /// Error to indicate that device is disconnected
    /// </summary>
    DeviceDisconnected = 644,

    /// <summary>
    /// Error to indicate that security question needs to be set
    /// </summary>
    SetSecurity = 645,

    /// <summary>
    /// Error to indicate Error that questionnaire is not supported
    /// </summary>
    NotSupportedQuestionnaire = 646,

    /// <summary>
    /// Error to indicate Error that questionnaire is started on other device
    /// </summary>
    StartedInAnotherDevice = 647,

    /// <summary>
    /// epresents multiple devices found during scan
    /// </summary>
    MultipleDevices = 648,

    /// <summary>
    /// represents no device found during scan
    /// </summary>
    DeviceNotFound = 649,

    /// <summary>
    /// Error to indicate that bluetooth permission is denied by user
    /// </summary>
    BluetoothUnauthorized = 650,

    /// <summary>
    /// Error to indicate that User needs to Accept consents
    /// </summary>
    ConsentRequired = 651,

    ///// <summary>
    ///// Error to indicate that Language data sync is required
    ///// </summary>
    //LanguageSyncRequired = 652,

    ////Add Other error codes here whenever required

    /// <summary>
    /// Error to indicate Error while doing operations in SP 
    /// </summary>
    DatabaseError = 999,

    /// <summary>
    /// Wrong Excel  Data Format
    /// </summary>
    WrongExcelDataFormat = 652,

    /// <summary>
    /// success Excel  Data Format
    /// </summary>
    AllExcelDataSuccess = 654,

    /// <summary>
    /// Bulk Upload Data Entry
    /// </summary>
    BulkUploadDataEntryStatus = 653,

    /// <summary>
    /// Invalid User Type
    /// </summary>
    InvalidUserType = 655,

    /// <summary>
    /// data not found
    /// </summary>
    NoDataFoundKey = 40,

     
    #endregion
}