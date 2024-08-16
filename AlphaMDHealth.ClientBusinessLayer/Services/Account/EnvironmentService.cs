using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer
{
    /// <summary>
    /// Base account service
    /// </summary>
    public class EnvironmentService : BaseService
    {
        public EnvironmentService(IEssentials essentials):base(essentials)
        { }
        /// <summary>
        /// Get Selected Url
        /// </summary>
        /// <param name="defaultServerPath">default environment url</param>
        /// <returns>Returns selected url</returns>
        public async Task<string> GetSelectedBaseUrlAsync(string defaultServerPath)
        {
            //todo: 
            //enable this line for local debuging 
            return defaultServerPath;
            string selectedEnvironment = _essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_KEY, string.Empty);
            if (!string.IsNullOrWhiteSpace(selectedEnvironment))
            {
                if (MobileConstants.IsMobilePlatform)
                {
                    string baseurl = await GetSettingsValueByKeyAsync(selectedEnvironment).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(baseurl))
                    {
                        //// Set the public key for certificate pinning
                        var cert = await GetSettingsValueByKeyAsync(selectedEnvironment + Constants.ENVIRONMENT_PUBLIC_KEY_SUFFIX).ConfigureAwait(false);
                        _essentials.SetPreferenceValue(StorageConstants.PR_PUBLIC_KEY, string.IsNullOrWhiteSpace(cert) ? string.Empty : cert);
                        return baseurl;
                    }
                }
                else
                {
                    return _essentials.GetPreferenceValue(selectedEnvironment, string.Empty);
                }
            }
            _essentials.SetPreferenceValue(StorageConstants.PR_PUBLIC_KEY, string.Empty);
            string defaultSelectedEnvironmentUrl = _essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_DEFAULT_BASE_PATH_KEY, string.Empty);
            if (!string.IsNullOrWhiteSpace(defaultSelectedEnvironmentUrl))
            {
                return defaultSelectedEnvironmentUrl;
            }
            return defaultServerPath;
        }

        /// <summary>
        /// Select environment URL as per the username prefix and updates Username without prefix.
        /// </summary>
        /// <param name="accountData">Operation status variable instance</param>
        /// <param name="isEnvironmentCheckRequired">do we need to determine env or not?</param>
        /// <param name="defaultEnvironmentPrefix">User name with environment name</param>
        public void CheckEnvironment(BaseDTO accountData, bool isEnvironmentCheckRequired, string defaultEnvironmentPrefix)
        {
            if (isEnvironmentCheckRequired)
            {
                if (accountData.AddedBy.Contains(Constants.ENVIRONMENT_SEPERATOR))
                {
                    var userInput = accountData.AddedBy.Split(new[] { Constants.ENVIRONMENT_SEPERATOR }, StringSplitOptions.None);
                    CheckEnvironment(accountData, (userInput.Length > 0) ? userInput[0].ToLowerInvariant() : string.Empty, defaultEnvironmentPrefix);
                    if (accountData.ErrCode == ErrorCode.OK)
                    {
                        accountData.AddedBy = string.Join(Constants.ENVIRONMENT_SEPERATOR, userInput.Skip(1).Take(userInput.Length - 1).ToArray());
                    }
                }
                else
                {
                    CheckEnvironment(accountData, null, defaultEnvironmentPrefix);
                }
                if (accountData.ErrCode != ErrorCode.OK)
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(accountData.AddedBy))
                {
                    accountData.ErrCode = ErrorCode.InvalidData;
                }
            }
        }

        /// <summary>
        /// Determine which server to connect based on the username
        /// </summary>
        /// <param name="result">Operation status variable instance</param>
        /// <param name="environment">User name with environment name</param>
        /// <param name="defaultEnvironmentPrefix">User name with environment name</param>
        private void CheckEnvironment(BaseDTO result, string environment, string defaultEnvironmentPrefix)
        {
            environment = string.IsNullOrWhiteSpace(environment) ? defaultEnvironmentPrefix : environment;
            bool isEnvironmentFound = MobileConstants.IsMobilePlatform
                ? result.Settings?.Any(x => x.GroupName == GroupConstants.RS_ENVIRONMENT_GROUP && x.SettingKey == environment) ?? false
                : _essentials.ContainsPreferenceKey(environment);
            if (isEnvironmentFound)
            {
                //Check Current Environemt Is changed or not?
                if (_essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_KEY, string.Empty) == environment)
                {
                    _essentials.SetPreferenceValue(StorageConstants.PR_IS_ENVIRONMENT_CHANGED_KEY, false);
                }
                else
                {
                    //Save new Environment in DB and clear all tables excluding base tables and update modified since date to 1970 of all tables
                    _essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_KEY, environment);
                    _essentials.SetPreferenceValue(StorageConstants.PR_IS_ENVIRONMENT_CHANGED_KEY, true);
                }
                result.ErrCode = ErrorCode.OK;
            }
            else
            {
                result.ErrCode = ErrorCode.InvalidEnvironment;
            }
        }
    }
}