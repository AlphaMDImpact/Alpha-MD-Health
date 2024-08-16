using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Text.Json;

namespace AlphaMDHealth.CommonBusinessLayer
{
    /// <summary>
    /// Represnts Base Service module
    /// </summary>
    public class BaseLibService
    {
        public IEssentials _essentials;

        /// <summary>
        /// Base contet page used
        /// </summary>
        public BaseDTO PageData { get; set; }

        /// <summary>
        /// Base for all service classes
        /// </summary>
        public BaseLibService(IEssentials essentials = null)
        {
            _essentials = essentials;
            PageData = new BaseDTO();
        }

        protected static readonly JsonSerializerOptions Options = new JsonSerializerOptions();

        protected static async Task<T?> DeserializeAsync<T>(Stream response, BaseDTO data)
        {
            T? result = response != null ? await JsonSerializer.DeserializeAsync<T>(response, Options) : default(T);
            if (data.ErrCode == ErrorCode.MultiStatus && result != null)
            {
                data.ErrCode = (result as BaseDTO).ErrCode;
            }
            return result;
        }

        /// <summary>
        /// To get list of Error log
        /// </summary>
        /// <param name="errorLogData"> object reference to return list of error log</param>
        /// <returns>List of Error logs</returns>
        public async Task GetErrorLogsAsync(ErrorLogDTO errorLogData)
        {
            try
            {
                //todo:
                //using (BaseLibDatabase errorDB = new BaseLibDatabase())
                //{
                //    await errorDB.GetErrorLogsToSyncAsync(errorLogData).ConfigureAwait(false);
                //}
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
            }

        }

        /// <summary>
        /// Logs error in database
        /// </summary>
        /// <param name="errorMessage">detail error message</param>
        /// <param name="exception">location of error</param>
        public void LogError(string errorMessage, Exception exception)
        {
            Task.Run(async () =>
            {
                try
                {
                    //todo:
                    //if (MobileConstants.IsMobilePlatform)
                    //{
                    //    _essentials.SetPreferenceValue(LibStorageConstants.PR_SYNC_ERROR_TO_APP_CENTER_KEY, true);
                        GenericMethods.LogData($"++++++++++LogError() errorMessage: {errorMessage},  errorLocation: {exception.StackTrace}");
                    //    if (_essentials.GetPreferenceValue(LibStorageConstants.PR_SYNC_ERROR_TO_APP_CENTER_KEY, false))
                    //    {
                    //        string userName = await GetSecuredValueAsync(LibStorageConstants.SS_USER_NAME_KEY).ConfigureAwait(false);
                    //        Crashes.TrackError(exception, new Dictionary<string, string>
                    //        {
                    //            { "User Name", userName },
                    //            { "Error Location", errorMessage },
                    //            { "Log Time: " , DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) },
                    //            { "ConnectionType", GetConnectionType() },
                    //            { "DeviceModel", $"{_essentials.DeviceManufacturer} {_essentials.DeviceModel}" }
                    //        });
                    //    }
                    //    else
                    //    {
                    //        ErrorLogModel errorLog = new ErrorLogModel
                    //        {
                    //            ErrorNumber = 2,
                    //            ErrorMessage = errorMessage + " @@This error occured @@ " + _essentials.DeviceOS + " With Model " + _essentials.DeviceModel + "manufactured by " + _essentials.DeviceManufacturer,
                    //            ErrorFunction = exception.StackTrace,
                    //            ErrorLineNumber = 2,
                    //            ErrorLogLevel = 1,
                    //            AccountID = _essentials.GetPreferenceValue<long>(LibStorageConstants.PR_ACCOUNT_ID_KEY, 0)
                    //        };
                    //        await new BaseLibDatabase().LogErrorsAsync(errorLog).ConfigureAwait(false);
                    //    }
                    //}
                    //else
                    //{
                    //    //// ToDo: call error log service to post data to server
                    //}
                }
                catch (Exception ex)
                {
                    LogError(ex.Message, ex);
                }
            });
        }

        /// <summary>
        /// Stores provided value in the secure storage, for the provided key.
        /// </summary>
        /// <param name="key">key by which to store the value</param>
        /// <param name="value">the value to store in the secure storage</param>
        /// <returns>status of whether the value is stored. Duplicate if already value is stored for key and overwrite is false. Overwritten if already value is stored for key and overwrite is true. Success otherwise</returns>
        public async Task<ErrorCode> SaveSecuredValueAsync(string key, string value)
        {
            try
            {
                await _essentials.SetSecureStorageValueAsync(key, string.IsNullOrWhiteSpace(value) ? string.Empty : value).ConfigureAwait(false);
                return ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return await SaveSecuredValueAsync(key, value).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the value stored for the key in the secure storage.
        /// </summary>
        /// <param name="key">key by which value is stored</param>
        /// <returns>Found if value is found else returns NotFound</returns>
        public async Task<string> GetSecuredValueAsync(string key)
        {
            string value = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    value = await _essentials.GetSecureStorageValueAsync(key).ConfigureAwait(false);
                    value = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                await Task.Delay(50).ConfigureAwait(false);
                return await GetSecuredValueAsync(key).ConfigureAwait(false);
            }
            return value;
        }

        /// <summary>
        /// Deletes the entry for the key in the secure storage.
        /// </summary>
        /// <param name="keys">key by which value is stored</param>
        /// <returns>Success, if the value is deleted successfully</returns>
        public async Task DeleteSecuredValuesAsync(params string[] keys)
        {
            foreach (string key in keys)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        if (!string.IsNullOrWhiteSpace(await _essentials.GetSecureStorageValueAsync(key).ConfigureAwait(false)))
                        {
                            _essentials.DeleteSecureStorageValue(key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Clears Account related data from secure storage
        /// </summary>
        /// <returns>Task representing the clear storage</returns>
        public async Task ClearAccountTokensAsync()
        {
            await DeleteSecuredValuesAsync(
                StorageConstants.SS_ACCESS_TOKEN_KEY,
                StorageConstants.SS_REFRESH_TOKEN_KEY,
                StorageConstants.SS_PIN_CODE_KEY
            ).ConfigureAwait(false);
            _essentials.DeletePreferenceValue(StorageConstants.PR_IS_BIOMETRIC_AUTH_PREFERRED_KEY);
        }

        /// <summary>
        /// Validate pincode strength
        /// </summary>
        /// <param name="pincode">pincode</param>
        /// <param name="sequencePattern">regex pattern for sequence</param>
        /// <param name="pincodePattern">regex pattern for pincode</param>
        /// <returns>Operation status</returns>
        public ErrorCode ValidatePincodeStrength(string pincode, string sequencePattern, string pincodePattern)
        {
            return ((string.IsNullOrWhiteSpace(sequencePattern) || (!sequencePattern.Contains(pincode) && !new string(sequencePattern.Reverse().ToArray()).Contains(pincode)))
                && (string.IsNullOrWhiteSpace(pincodePattern) || !GenericMethods.IsRegexMatched(pincode, pincodePattern)))
                    ? ErrorCode.OK : ErrorCode.WeakPincode;
        }

        //todo:
        //private string GetConnectionType()
        //{
        //    var profiles = Connectivity.ConnectionProfiles;
        //    if (profiles.Contains(ConnectionProfile.WiFi))
        //    {
        //        return ConnectionProfile.WiFi.ToString();
        //    }
        //    if (profiles.Contains(ConnectionProfile.Cellular))
        //    {
        //        return ConnectionProfile.Cellular.ToString();
        //    }
        //    if (profiles.Contains(ConnectionProfile.Ethernet))
        //    {
        //        return ConnectionProfile.Ethernet.ToString();
        //    }
        //    if (profiles.Contains(ConnectionProfile.Bluetooth))
        //    {
        //        return ConnectionProfile.Bluetooth.ToString();
        //    }
        //    return ConnectionProfile.Unknown.ToString();
        //}
    }
}