using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer
{
    public class AccountServiceDataLayer : BaseServiceDataLayer
    {
        /// <summary>
        /// Get Token
        /// </summary>
        /// <param name="sessionData">parameter passed to get refresh token and it's status details</param>
        /// <returns>object that holds refresh token details</returns>
        public async Task GetTokenAsync(SessionDTO sessionData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            MapDeviceDataInSpInputParams(sessionData.Session, parameters);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SessionModel.RefreshToken), sessionData.Session.RefreshToken, DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), sessionData.AccountID, DbType.Int64, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), sessionData.ErrCode, DbType.Int16, ParameterDirection.Output);
            sessionData.Session = await connection.QueryFirstOrDefaultAsync<SessionModel>(SPNameConstants.USP_GET_TOKEN, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            sessionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (sessionData.Session == null)
            {
                sessionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
        }

        /// <summary>
        /// Calls SP to validate the access token in the current session
        /// </summary>
        /// <param name="sessionData">Instance of current session</param>
        /// <returns>Operation Status</returns>
        public async Task ValidateAccessTokenAsync(SessionDTO sessionData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            MapDeviceDataInSpInputParams(sessionData.Session, parameters);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SessionModel.AccessToken), sessionData.Session.AccessToken, DbType.String, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SessionModel.IgnoreErrorCode), Convert.ToInt16(sessionData.Session.IgnoreErrorCode, CultureInfo.InvariantCulture), DbType.Int16, ParameterDirection.Input);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), sessionData.AccountID, DbType.Int64, ParameterDirection.Output);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), sessionData.ErrCode, DbType.Int16, ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_VALIDATE_ACCESS_TOKEN, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            sessionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Other);
            if (sessionData.ErrCode == ErrorCode.OK)
            {
                sessionData.AccountID = parameters.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID));
            }
        }

        /// <summary>
        /// Validate entered user credentails from database AND Creates user session in case of success
        /// </summary>
        /// <param name="authData">User input data to validate and create session</param>
        /// <returns>Operation status and token in case of success</returns>
        public async Task LoginAsync(AuthDTO authData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(AuthModel.UserName)), authData.AuthenticationData.UserName.ToLowerInvariant(), DbType.String);
            parameters.Add(ConcateAt(nameof(AuthModel.AccountPassword)), authData.AuthenticationData.AccountPassword, DbType.String);
            parameters.Add(ConcateAt(nameof(AuthModel.RememberMe)), authData.AuthenticationData.RememberMe, DbType.Boolean);
            parameters.Add(ConcateAt(nameof(AuthModel.Otp)), authData.AuthenticationData.Otp, DbType.String);
            MapDeviceDataInSpInputParams(authData.Session, parameters);
            parameters.Add(ConcateAt(nameof(BaseDTO.OrganisationID)), authData.OrganisationID, DbType.Int64);
            parameters.Add(ConcateAt(nameof(AuthModel.LockoutDuration)), 1, DbType.Int32, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(AuthModel.RoleID)), 0, DbType.Byte, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(AuthModel.UserOrganisationID)), 1, DbType.Int64, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(BaseDTO.PermissionAtLevelID)), 1, DbType.Int64, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(BaseDTO.AccountID)), 1, DbType.Int64, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(BaseDTO.ErrCode)), authData.ErrCode, DbType.Int16, ParameterDirection.Output);
            authData.Session = await connection.QueryFirstOrDefaultAsync<SessionModel>(SPNameConstants.USP_LOGIN, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            HandelLoginError(authData, parameters);
        }

        /// <summary>
        /// Validate entered user credentails from database AND Creates user session in case of success
        /// </summary>
        /// <param name="authData">User input data to validate and create session</param>
        /// <returns>Operation status and token in case of success</returns>
        public async Task LoginWithTempTokenAsync(AuthDTO authData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ConcateAt(nameof(TempSessionModel.TempToken)), authData.TempSession.TempToken.ToLowerInvariant(), DbType.String);
            parameters.Add(ConcateAt(nameof(TempSessionModel.TokenIdentifier)), authData.TempSession.TokenIdentifier, DbType.String);
            MapDeviceDataInSpInputParams(authData.Session, parameters);
            parameters.Add(ConcateAt(nameof(BaseDTO.OrganisationID)), authData.OrganisationID, DbType.Int64);
            parameters.Add(ConcateAt(nameof(AuthModel.LockoutDuration)), 1, DbType.Int32, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(AuthModel.RoleID)), 0, DbType.Byte, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(AuthModel.UserOrganisationID)), 1, DbType.Int64, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(BaseDTO.PermissionAtLevelID)), 1, DbType.Int64, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(BaseDTO.AccountID)), 1, DbType.Int64, ParameterDirection.Output);
            parameters.Add(ConcateAt(nameof(BaseDTO.ErrCode)), authData.ErrCode, DbType.Int16, ParameterDirection.Output);
            authData.Session = await connection.QueryFirstOrDefaultAsync<SessionModel>(SPNameConstants.USP_LOGIN_WITH_TEMP_TOKEN, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            HandelLoginError(authData, parameters);
        }

        private void HandelLoginError(AuthDTO authData, DynamicParameters parameters)
        {
            authData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Other);
            switch (authData.ErrCode)
            {
                case ErrorCode.OK:
                case ErrorCode.SetPinCode:
                case ErrorCode.MultipleUsers:
                case ErrorCode.SMSAuthentication:
                case ErrorCode.SetNewPassword:
                    // Update the organisation id with the one returned by the SP
                    authData.OrganisationID = parameters.Get<long?>(ConcateAt(nameof(AuthModel.UserOrganisationID))) ?? Constants.DEFAULT_ORGANISATION_ID;
                    authData.PermissionAtLevelID = parameters.Get<long?>(ConcateAt(nameof(BaseDTO.PermissionAtLevelID))) ?? Constants.DEFAULT_ORGANISATION_ID;
                    authData.AuthenticationData ??= new AuthModel();
                    authData.AuthenticationData.UserAccountID = authData.AccountID = parameters.Get<long>(ConcateAt(nameof(BaseDTO.AccountID)));
                    authData.AuthenticationData.RoleID = parameters.Get<byte>(ConcateAt(nameof(AuthModel.RoleID)));
                    break;
                case ErrorCode.AccountLockout:
                    authData.RecordCount = parameters.Get<int>(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.LockoutDuration));
                    break;
            }
        }

        /// <summary>
        /// Validates the user details sent for Forgot Password and on success sends relevant error
        /// code in response
        /// </summary>
        /// <param name="authData">Users input data to be validated on server</param>
        /// <returns>Object with error details</returns>
        public async Task ForgotPasswordAsync(AuthDTO authData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.EmailID), authData.AuthenticationData.EmailID.ToLowerInvariant(), DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.PhoneNo), authData.AuthenticationData.PhoneNo, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), authData.OrganisationID, DbType.Int64, ParameterDirection.InputOutput);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthDTO.AccountID), 0, DbType.Int64, ParameterDirection.Output);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), authData.ErrCode, DbType.Int16, ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_FORGOT_PASSWORD, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            authData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
            if (authData.ErrCode == ErrorCode.SMSAuthentication || authData.ErrCode == ErrorCode.ResetPassword)
            {
                authData.AuthenticationData.UserAccountID = parameters.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(AuthDTO.AccountID));
                // Update the organisation id with the one returned by the SP
                authData.OrganisationID = parameters.Get<long?>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID)) ?? Constants.DEFAULT_ORGANISATION_ID;
            }
        }

        /// <summary>
        /// SET/RESET new Password for given username
        /// </summary>
        /// <param name="authData">user input data</param>
        /// <returns>Operation status</returns>
        public async Task SetPasswordAsync(AuthDTO authData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), authData.OrganisationID, DbType.Int64);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.EmailID), authData.AuthenticationData.EmailID?.ToLowerInvariant(), DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.PhoneNo), authData.AuthenticationData.PhoneNo, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.Otp), authData.AuthenticationData.Otp, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.OldPassword), authData.AuthenticationData.OldPassword, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.AccountPassword), authData.AuthenticationData.AccountPassword, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.PageType), authData.AuthenticationData.PageType, DbType.Int16);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), authData.AccountID, DbType.Int64);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.LockoutDuration), authData.AuthenticationData.LockoutDuration, DbType.Int32, ParameterDirection.Output);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), authData.ErrCode, DbType.Int16, ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_SET_PASSWORD, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            authData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
            if (authData.ErrCode == ErrorCode.SMSAuthentication)
            {
                authData.AuthenticationData.LockoutDuration = parameters.Get<int>(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.LockoutDuration));
            }
        }

        /// <summary>
        /// Resend OTP code
        /// </summary>
        /// <param name="authData">data to generate OTP</param>
        /// <returns>Operation status</returns>
        public async Task ResendOtpAsync(AuthDTO authData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.PhoneNo), authData.AuthenticationData.PhoneNo, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(AuthModel.UserName), authData.AuthenticationData.UserName.ToLowerInvariant(), DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), authData.OrganisationID, DbType.Int64);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), authData.ErrCode, DbType.Int16, ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_CREATE_OTP, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            authData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }

        /// <summary>
        /// Set User pincode / verify user pincode
        /// </summary>
        /// <param name="sessionData">data to validate pincode</param>
        /// <returns>Operation status</returns>
        public async Task PincodeAsync(SessionDTO sessionData)
        {
            using var connection = ConnectDatabase();
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SessionModel.DeviceID), sessionData.Session.DeviceID, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(SessionModel.PinCode), sessionData.Session.PinCode, DbType.String);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), sessionData.AccountID, DbType.Int64);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), sessionData.IsActive, DbType.Boolean);
            parameters.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), sessionData.ErrCode, DbType.Int16, ParameterDirection.Output);
            await connection.ExecuteAsync(SPNameConstants.USP_PINCODE, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            sessionData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        }
    }
}