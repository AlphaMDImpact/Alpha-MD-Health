using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/AuthService")]
    public class AccountController : BaseController
    {
        /// <summary>
        /// This will return all required resources, settings validations and other information for the loging flow related flow
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <returns>return all required resources, settings validations and other information for the loging flow related flow</returns>
        [Route("GetAccountDataAsync")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAccountDataAsync(byte languageID, long organisationID)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).GetAccountDataAsync(languageID, organisationID), languageID);
        }

        /// <summary>
        /// Get page data of Set Password Page
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <returns>List of resources and settings along with operation status</returns>
        [Route("GetChangePasswordDataAsync")]
        [HttpGet]
        [ApiAuthorize]
        public async Task<IActionResult> GetChangePasswordDataAsync(byte languageID, long organisationID)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).GetAccountDataAsync(languageID, organisationID), languageID);
        }

        /// <summary>
        /// Create user session based on user input data
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="authData">User input data to validate and create session</param>
        /// <returns>Operation status and token in case of success</returns>
        [Route("LoginAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(byte languageID, long organisationID, [FromBody] AuthDTO authData)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).LoginAsync(languageID, organisationID, authData, Request.Headers), languageID);
        }

        /// <summary>
        /// Create user session based on user input data
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="authData">User input data to validate and create session</param>
        /// <returns>Operation status and token in case of success</returns>
        [Route("LoginWithTempTokenAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithTempTokenAsync(byte languageID, long organisationID, [FromBody] AuthDTO authData)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).LoginWithTempTokenAsync(languageID, organisationID, authData, Request.Headers), languageID);
        }

        /// <summary>
        /// Resend otp code
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="authData">User input data</param>
        /// <returns>Operation status</returns>
        [Route("ResendOtpAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtpAsync(byte languageID, long organisationID, [FromBody] AuthDTO authData)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).ResendOtpAsync(languageID, organisationID, authData), languageID);
        }

        /// <summary>
        /// Get page data for set pincode page
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <returns>Page data for set pincode page with operation status</returns>
        [Route("GetPincodeDataAsync")]
        [HttpGet]
        [ApiAuthorize(ErrorCode.SetPinCode)]
        public async Task<IActionResult> GetPincodeDataAsync(byte languageID, long organisationID)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).GetAccountDataAsync(languageID, organisationID), languageID);
        }

        /// <summary>
        /// Save pincode in database
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="sessionData">Session data</param>
        /// <returns>Operation Status</returns>
        [Route("SetPincodeAsync")]
        [HttpPost]
        [ApiAuthorize(ErrorCode.SetPinCode)]
        public async Task<IActionResult> SetPincodeAsync(byte languageID, long organisationID, [FromBody] SessionDTO sessionData)
        {
            Request.Headers.TryGetValue(Constants.SE_DEVICE_UNIQUE_ID_HEADER_KEY, out StringValues headerValues);
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).PincodeAsync(organisationID, headerValues.FirstOrDefault(), sessionData, true), languageID);
        }

        /// <summary>
        /// Get Pincode page data
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="organisationID">User's Organisation ID</param>
        /// <returns>List of resources and settings along with operation status</returns>
        [Route("GetPincodeLoginDataAsync")]
        [HttpGet]
        [ApiAuthorize(ErrorCode.PinCodeLogin)]
        public async Task<IActionResult> GetPincodeLoginDataAsync(byte languageID, long organisationID)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).GetAccountDataAsync(languageID, organisationID), languageID);
        }

        /// <summary>
        /// Verify pincode in database
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="sessionData">Session data</param>
        /// <returns>Operation Status</returns>
        [Route("VerifyPincodeAsync")]
        [HttpPost]
        [ApiAuthorize(ErrorCode.PinCodeLogin)]
        public async Task<IActionResult> VerifyPincodeAsync(byte languageID, long organisationID, [FromBody] SessionDTO sessionData)
        {
            Request.Headers.TryGetValue(Constants.SE_DEVICE_UNIQUE_ID_HEADER_KEY, out StringValues headerValues);
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).PincodeAsync(organisationID, headerValues.FirstOrDefault(), sessionData, false), languageID);
        }

        /// <summary>
        /// Get new AccessToken, RefreshToken based on the given RefreshToken
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="refreshTokenData">RefreshToken data</param>
        /// <returns>New refresh token and access token if refresh token is valid</returns>
        [Route("GetTokenAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetTokenAsync(byte languageID, [FromBody] SessionDTO refreshTokenData)
        {
            refreshTokenData.Request = HttpContext.Request;
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).GetTokenAsync(refreshTokenData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// This will return all required resources, settings validations and other information for the loging flow related flow
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <returns>Returns all required resources, settings validations and other information for the loging flow related flow</returns>
        [Route("GetForgotPasswordDataAsync")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetForgotPasswordDataAsync(byte languageID, long organisationID)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).GetAccountDataAsync(languageID, organisationID), languageID);
        }

        /// <summary>
        /// Validates the user details sent for Forgot Password and on success sends OTP to resgitered Email address and Mobile number
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="authData">>Users input data to be validated on server</param>
        /// <returns>Status confirming the authenticity of data provided by user</returns>
        [Route("ForgotPasswordAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordAsync(byte languageID, long organisationID, [FromBody] AuthDTO authData)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).ForgotPasswordAsync(languageID, organisationID, authData), languageID);
        }

        /// <summary>
        /// Sets or Resets user password if the Data is valid
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="authData">User input data</param>
        /// <returns>Operation status</returns>
        [Route("ResetPasswordAsync")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordAsync(byte languageID, long organisationID, [FromBody] AuthDTO authData)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).ResetPasswordAsync(languageID, organisationID, authData), languageID);
        }

        /// <summary>
        /// Change password of logged in user
        /// </summary>
        /// <param name="languageID">User's Language Id</param>
        /// <param name="organisationID">User's Organisation Id</param>
        /// <param name="authData">User input data</param>
        /// <returns>Operation status</returns>
        [Route("ChangePasswordAsync")]
        [HttpPost]
        [ApiAuthorize]
        public async Task<IActionResult> ChangePasswordAsync(byte languageID, long organisationID, [FromBody] AuthDTO authData)
        {
            return HttpActionResult(await new AccountServiceBusinessLayer(HttpContext).ResetPasswordAsync(languageID, organisationID, authData), languageID);
        }
    }
}