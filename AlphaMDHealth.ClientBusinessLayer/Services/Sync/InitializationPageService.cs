using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public partial class DataSyncService : BaseService
    {
        /// <summary>
        /// validates the certificate public key
        /// </summary>
        /// <param name="certPublicString">public key of the request server</param>
        /// <returns>true, if certificate is valid else false</returns>
        public bool ValidateCertificate(string certPublicString)
        {
            if (_essentials.GetPreferenceValue(StorageConstants.PR_IS_CERTIFICATE_PINING_ENABLE_KEY, false)
                && _essentials.GetPreferenceValue(StorageConstants.PR_APPLY_CERTIFICATE_KEY, true))
            {
                var cert = _essentials.GetPreferenceValue(StorageConstants.PR_PUBLIC_KEY, string.Empty);
                if (!string.IsNullOrWhiteSpace(cert))
                {
                    return cert == certPublicString;
                }
            }
            return true;
        }

        /// <summary>
        /// Initialises DataSyncFor table with defaults
        /// </summary>
        /// <param name="isMasterInitialization">Flag which will decide master tables sync DateTime needs to initialize or user tables sync DateTime</param>
        /// <param name="patientID">ID for which data needs to clean</param>
        /// <returns>DataSync result as reference</returns>
        public async Task InitializeDataSyncForDateTimeAsync(bool isMasterInitialization, long patientID)
        {
            List<DataSyncFor> dataSyncForEnums = GetEnumsForSyncData(isMasterInitialization);
            DateTimeOffset dateTimeOffset = GenericMethods.GetUtcDateTime;
            using (DataSyncDatabase dataSyncForDB = new DataSyncDatabase())
            {
                await dataSyncForDB.SaveDataSyncInfoAsync(new DataSyncDTO
                {
                    PatientID = patientID,
                    DataSyncForRecords = (from syncFor in dataSyncForEnums
                                          select CreateDataSyncModel(dateTimeOffset, syncFor.ToString())
                                          ).ToList()
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Initializes DataSync date for the given batch
        /// </summary>
        /// <param name="syncFor">Data Sync batch for which times are to be reset</param>
        /// <returns>DataSync result as reference</returns>
        public async Task InitializeDataSyncForDateTimeAsync(DataSyncFor syncFor)
        {
            DateTimeOffset dateTimeOffset = GenericMethods.GetUtcDateTime;
            DataSyncDTO syncData = new DataSyncDTO { SyncBatch = syncFor.ToString(), DataSyncFor = new[] { syncFor.ToString() } };
            using (DataSyncDatabase dataSyncForDB = new DataSyncDatabase())
            {
                await dataSyncForDB.SaveDataSyncInfoAsync(new DataSyncDTO
                {
                    DataSyncForRecords = (from dataSyncFor in syncData.DataSyncFor select CreateDataSyncModel(dateTimeOffset, dataSyncFor)).ToList()
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes initialization page decision
        /// </summary>
        /// <param name="initializationResult">Object which holds data and operation status</param>
        /// <param name="isAfterLanguageSelection">Object which holds data and operation status</param>
        /// <param name="action">Sync call action</param>
        /// <returns>Operation status with target page</returns>
        public async Task InitializeApplicationAsync(BaseDTO initializationResult, Pages flow, Func<BaseDTO, Task> action)
        {
            try
            {
                // Initialize DB
                await InitializeDatabaseAsync().ConfigureAwait(false);
                // Set user permission level in case not set
                if (_essentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, -1) == -1)
                {
                    _essentials.SetPreferenceValue(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, Constants.DEFAULT_ORGANISATION_ID);
                }
                if (MobileConstants.CheckInternet)
                {
                    await InitializeAndSyncDataAsync(initializationResult, flow, action).ConfigureAwait(false);
                }
                else
                {
                    // Check offline support available
                    await CheckOfflineSupportAsync(initializationResult).ConfigureAwait(false);
                }
                SyncErrorLogsInBackground();
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                initializationResult.ErrCode = ErrorCode.RestartApp;
            }
        }

        internal async Task<ErrorCode> HandelMasterDataChangeAsync(Func<BaseDTO, Task> action)
        {
            if (_essentials.GetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_DEFAULT_BASE_PATH_KEY, string.Empty) == string.Empty)
            {
                _essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_DEFAULT_BASE_PATH_KEY,
                    await new EnvironmentService(_essentials).GetSelectedBaseUrlAsync(UrlConstants.DEFAULT_ENVIRONMENT_KEY_VALUE).ConfigureAwait(true));
            }
            await new CommonDatabase().CleanMasterDataAsync().ConfigureAwait(false);
            _essentials.SetPreferenceValue(StorageConstants.PR_APPLY_CERTIFICATE_KEY, false);
            _essentials.SetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
            var results = await InvokeServiceCallAsync(action, Pages.LoginPage, true, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.MasterData).ConfigureAwait(false);
            await SyncLanguageSpecificDataAsync(results, Pages.InitializationPage, action).ConfigureAwait(false);
            return results.ErrCode;
        }

        /// <summary>
        /// Invokes service call based on given parameters
        /// </summary>
        /// <param name="action">Action to be invoked</param>
        /// <param name="pages">Page for which service call is to be started</param>
        /// <param name="isFirstTime">true if first time service call</param>
        /// <param name="serviceSyncGroups">SyncFrom or SyncTo</param>
        /// <param name="syncFor">Data Sync batch</param>
        /// <param name="count">Record count</param>
        /// <returns>Result of operation</returns>
        protected async Task<BaseDTO> InvokeServiceCallAsync(Func<BaseDTO, Task> action, Pages pages, bool isFirstTime, ServiceSyncGroups serviceSyncGroups = ServiceSyncGroups.Default, DataSyncFor syncFor = DataSyncFor.Defaults, long count = default, string tables = null)
        {
            BaseDTO result = new BaseDTO
            {
                ErrorDescription = serviceSyncGroups.ToString(),
                AddedBy = pages.ToString(),
                LastModifiedBy = syncFor.ToString(),
                IsActive = isFirstTime,
                RecordCount = count,
                Response = tables,
            };
            await action(result).ConfigureAwait(false);
            if (result.ErrCode != ErrorCode.HandledRedirection && result.ErrCode != ErrorCode.NoInternetConnection && result.ErrCode != ErrorCode.OK && result.ErrCode != ErrorCode.LanguageNotAvailable)
            {
                result.ErrCode = ErrorCode.RestartApp;
            }
            return result;
        }

        private async Task InitializeAndSyncDataAsync(BaseDTO initializationResult, Pages flow, Func<BaseDTO, Task> action)
        {
            initializationResult.ErrCode = ErrorCode.OK;
            Pages? targetPage = null;
            if (flow == Pages.InitializationPage)
            {
                // Sync master data when user is not logged in and also initialize defaults when language is not yet selected
                targetPage = await SyncMasterDataAsync(initializationResult, action).ConfigureAwait(false);
            }
            await SyncLanguageSpecificDataAsync(initializationResult, flow, action).ConfigureAwait(false);
            if (initializationResult.ErrCode == ErrorCode.OK)
            {
                targetPage = targetPage ?? await GetNavigationPageNameAsync(flow).ConfigureAwait(false);
                await SetMainPageAsync(initializationResult, targetPage.Value).ConfigureAwait(false);
            }
            else
            {
                // Navigate to Login page after style load and other initialization in case of TokenExpired or InActiveUser
                if (initializationResult.ErrCode == ErrorCode.TokenExpired || initializationResult.ErrCode == ErrorCode.InActiveUser)
                {
                    await SetMainPageAsync(initializationResult, Pages.LoginPage).ConfigureAwait(false);
                }
            }
        }

        private async Task<Pages> GetNavigationPageNameAsync(Pages flow)
        {
            if (!string.IsNullOrWhiteSpace(await new BaseService(_essentials).GetSecuredValueAsync(StorageConstants.SS_PIN_CODE_KEY).ConfigureAwait(false)))
            {
                return flow == Pages.LanguageSelectionPage
                    ? Pages.ShellMasterPage
                    : Pages.PincodeLoginPage;
            }
            else if (!_essentials.GetPreferenceValue<bool>(StorageConstants.PR_IS_APP_INTRO_SHOWN_KEY, false)
                && await new AppIntroDatabase().CheckIfAppIntroRequiredAsync())
            {
                return Pages.AppIntroPage;
            }
            else
            {
                return Pages.LoginPage;
            }
        }

        private async Task CheckOfflineSupportAsync(BaseDTO initializationResult)
        {
            if (!string.IsNullOrWhiteSpace(await new BaseService(_essentials).GetSecuredValueAsync(StorageConstants.SS_PIN_CODE_KEY).ConfigureAwait(false))
                && Convert.ToBoolean(await new SettingService(_essentials).GetSettingsValueByKeyAsync(SettingsConstants.S_IS_OFFLINE_SUPPORT_AVAILABLE_KEY).ConfigureAwait(false), CultureInfo.InvariantCulture))
            {
                // When pincode setup is done, navigate user to pincode page
                await SetMainPageAsync(initializationResult, Pages.PincodeLoginPage).ConfigureAwait(false);
                return;
            }
            // User cannot continue without internet if not logged in previously
            initializationResult.ErrCode = ErrorCode.NoInternetConnection;
        }

        internal async Task SyncLanguageSpecificDataAsync(BaseDTO initializationResult, Pages flow, Func<BaseDTO, Task> action)
        {
            if (initializationResult.ErrCode == ErrorCode.OK
                && flow != Pages.WelcomeScreensPage
                && _essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0) != 0)
            {
                var pincode = await new BaseService(_essentials).GetSecuredValueAsync(StorageConstants.SS_PIN_CODE_KEY).ConfigureAwait(false);
                // Sync Language dependent data 
                initializationResult.ErrCode = flow == Pages.LanguageSelectionPage && !string.IsNullOrWhiteSpace(pincode)
                        ? (await InvokeServiceCallAsync(action, Pages.LanguageSelectionPage, true).ConfigureAwait(false)).ErrCode
                        : (await InvokeServiceCallAsync(action, Pages.InitializationPage, false).ConfigureAwait(false)).ErrCode;
            }
        }

        private async Task<Pages?> SyncMasterDataAsync(BaseDTO initializationResult, Func<BaseDTO, Task> action)
        {
            bool isLanguageNotSelected = _essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0) == 0;
            if (isLanguageNotSelected)
            {
                // Clean old data and Initialize Default data in DB
                await InitializeDefaultsInDbAsync().ConfigureAwait(false);
            }
            // Sync master data
            initializationResult.ErrCode = (await InvokeServiceCallAsync(action, Pages.InitializationPage, true, ServiceSyncGroups.RSSyncFromServerGroup, DataSyncFor.MasterData).ConfigureAwait(false)).ErrCode;
            if (initializationResult.ErrCode == ErrorCode.LanguageNotAvailable)
            {
                //reset sync from datetime of language specific tables
                initializationResult.ErrCode = (await InvokeServiceCallAsync(action, Pages.LanguageSelectionPage, _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0) < 1, count: -1).ConfigureAwait(false)).ErrCode;
                isLanguageNotSelected = true;
            }
            // When language is not selected yet, sync master data and navigate to next page
            if (isLanguageNotSelected && initializationResult.ErrCode == ErrorCode.OK)
            {
                // Decide target page based on organization's supported languages
                return await CheckSelectedLanguagesAsync(initializationResult).ConfigureAwait(false);
            }
            return null;
        }

        private async Task<Pages?> CheckSelectedLanguagesAsync(BaseDTO initializationResult)
        {
            // Sync languages
            LanguageDTO languageData = new LanguageDTO();
            await new LanguageService(_essentials).GetSupportedLanguagesAsync(languageData).ConfigureAwait(false);
            initializationResult.ErrCode = languageData.ErrCode;
            if (initializationResult.ErrCode == ErrorCode.OK)
            {
                if (languageData.Languages.Count == 1)
                {
                    // Sync resources for the only language fetched
                    if (_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0) != languageData.Languages[0].LanguageID)
                    {
                        _essentials.SetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, languageData.Languages[0].LanguageID);
                        _essentials.SetPreferenceValue(StorageConstants.PR_IS_RIGHT_ALIGNED_KEY, languageData.Languages[0].IsRightToLeft);
                    }
                    return null;
                }
                else
                {
                    return Pages.LanguageSelectionPage;
                }
            }
            return Pages.LoginPage;
        }

        private async Task SetMainPageAsync(BaseDTO initializationResult, Pages targetPage)
        {
            await new SettingService(_essentials).CheckAppUpdateAsync(Constants.APP_VERSON_NO, initializationResult).ConfigureAwait(false);
            if (initializationResult.ErrCode == ErrorCode.OK)
            {
                initializationResult.ErrorDescription = targetPage.ToString();
            }
        }

        private void SyncErrorLogsInBackground()
        {
            Task.Run(async () =>
            {
                // Start Syncing error log service in parallel thread
                await SyncDataAsync(new BaseDTO { Response = DataSyncFor.ErrorLogs.ToString() }, ServiceSyncGroups.RSSyncToServerGroup, DataSyncFor.ErrorLogs, CancellationToken.None).ConfigureAwait(false);
            });
        }

        private async Task InitializeDefaultsInDbAsync()
        {
            await Task.WhenAll(
                // Initialize Master data
                InitializeDataSyncForDateTimeAsync(true, 0),
                // Initialize user data
                InitializeDataSyncForDateTimeAsync(false, 0)
            ).ConfigureAwait(false);
            _essentials.SetPreferenceValue(StorageConstants.PR_SELECTED_ENVIRONMENT_KEY, Constants.DEFAULT_ENVIRONMENT_KEY);
        }

        private DataSyncModel CreateDataSyncModel(DateTimeOffset dateTimeOffset, string syncFor)
        {
            return new DataSyncModel
            {
                SyncFor = syncFor,
                SyncFromServerDateTime = null,
                SyncToServerDateTime = dateTimeOffset,
                SyncToStatus = SyncStatus.Pending,
                SyncToStartedDateTime = GenericMethods.GetDefaultDateTime
            };
        }

        private async Task WaitForTaskResultAsync(BaseDTO returnResult, int waitTimeInSec, CancellationToken cancellationToken)
        {
            DateTimeOffset waitStartedDateTime = DateTimeOffset.Now;
            bool isTaskWaiting = true;
            await Task.Run(() =>
            {
                while (isTaskWaiting && !cancellationToken.IsCancellationRequested)
                {
                    if ((DateTimeOffset.Now - waitStartedDateTime).TotalSeconds > waitTimeInSec)
                    {
                        returnResult.ErrCode = ErrorCode.RequestTimeout;
                        returnResult.RecordCount = 0;
                        isTaskWaiting = false;
                    }
                }
            });
        }

        /// <summary>
        /// Initializes database
        /// </summary>
        /// <returns>returns after initializing connection and tables</returns>
        private async Task InitializeDatabaseAsync()
        {
            if (string.IsNullOrWhiteSpace(await GetSecuredValueAsync(StorageConstants.SS_DB_ENCRYPTION_KEY).ConfigureAwait(false)))
            {
                string randomString = GenericMethods.RandomString(32);
                await SaveSecuredValueAsync(StorageConstants.SS_DB_ENCRYPTION_KEY, randomString).ConfigureAwait(false);
            }
            await new CommonDatabase().InitializeDatabaseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isMasterData"></param>
        /// <returns></returns>
        private List<DataSyncFor> GetEnumsForSyncData(bool isMasterData)
        {
            var masterSyncs = new List<DataSyncFor> {
                DataSyncFor.Settings,
                DataSyncFor.Languages,
                DataSyncFor.Countries,
                DataSyncFor.Resources,
                DataSyncFor.Features,
                DataSyncFor.Medicines
            };
            if (isMasterData)
            {
                return masterSyncs;
            }
            List<DataSyncFor> userSyncs = (from syncFor in Enum.GetNames(typeof(DataSyncFor))
                                           where !masterSyncs.Contains(syncFor.ToEnum<DataSyncFor>())
                                           select syncFor.ToEnum<DataSyncFor>()).ToList();
            return userSyncs;
        }
    }
}