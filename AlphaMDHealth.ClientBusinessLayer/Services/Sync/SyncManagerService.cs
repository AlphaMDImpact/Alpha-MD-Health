using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Base page to use common pages logic
/// </summary>
public class SyncManagerService : BaseService
{
    public SyncManagerService(IEssentials serviceEssentials) : base(serviceEssentials)
    {
        
    }
    #region Sync Data Methods

    /// <summary>
    /// Sync Data from server / Sync Data to server for specified service directly without any configuration check
    /// </summary>
    /// <param name="action">Task action to invoke for service cal</param>
    /// <param name="syncGroup">Group name which decides data need to Sync from or Sync To</param>
    /// <param name="syncFrom">Page from where sync request is raised</param>
    /// <param name="syncDataFor">Table for which data need to be synced</param>
    /// <param name="tablesInBatch">Tables data which will fetched in this batch</param>
    /// <param name="patientID">PatientID for which data needs to fetch</param>
    /// <returns>Operation Status</returns>
    public async Task<BaseDTO> SyncDataAsync(Func<BaseDTO, Task> action, ServiceSyncGroups syncGroup, string syncFrom, string syncDataFor
        , string tablesInBatch, long patientID)
    {
        return MobileConstants.CheckInternet
            ? await InvokeCallBackAsync(action, syncFrom, syncGroup, syncDataFor, tablesInBatch, true, patientID).ConfigureAwait(false)
            : new BaseDTO { ErrCode = ErrorCode.NoInternetConnection };
    }

    /// <summary>
    /// Sync Data To/From server for specified Settings of given page with configuration check
    /// </summary>
    /// <param name="action">Task action to invoke for service cal</param>
    /// <param name="syncFrom">Page from where sync from request is raised</param>
    /// <param name="isFirstTime">Bool value which represent call is first time call or not</param>
    /// <param name="patientID">PatientID for which data needs to fetch</param>
    /// <returns>Operation status</returns>
    public async Task<BaseDTO> SyncDataAsync(Func<BaseDTO, Task> action, string syncFrom, bool isFirstTime, long patientID
        , Func<string, int> refreshAction)
    {
        BaseDTO operationResult = new BaseDTO();
        if (MobileConstants.CheckInternet)
        {
            // Get list of settings for given sync group.
            SyncConfigurationDTO syncConfiguration = new SyncConfigurationDTO();
            await new SyncConfigurationService(_essentials).GetSyncConfigurationsAsync(syncConfiguration, syncFrom, isFirstTime).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(syncConfiguration.Configurations))
            {
                //1 : this task will sync all the services which are awaited
                operationResult = await StartParallelCallsBasedOnSettingsAsync(syncConfiguration, syncFrom, isFirstTime, 1, action, patientID, refreshAction).ConfigureAwait(false);
                //2 : this task will sync all the services which are not awaited
                RunNonAwaitedCall(syncFrom, isFirstTime, action, syncConfiguration, patientID, refreshAction);
            }
        }
        else
        {
            operationResult.ErrCode = ErrorCode.NoInternetConnection;
        }
        return operationResult;
    }

    private void RunNonAwaitedCall(string syncFrom, bool isFirstTime, Func<BaseDTO, Task> action, SyncConfigurationDTO syncConfiguration
        , long patientID, Func<string, int> refreshAction)
    {
        Task.Run(async () =>
        {
            _ = await StartParallelCallsBasedOnSettingsAsync(syncConfiguration, syncFrom, isFirstTime, 2, action, patientID, refreshAction).ConfigureAwait(false);
        });
    }

    private async Task<BaseDTO> StartParallelCallsBasedOnSettingsAsync(SyncConfigurationDTO syncConfiguration, string syncFrom, bool isFirstTime, int shouldAwaitCall
        , Func<BaseDTO, Task> action, long patientID, Func<string, int> refreshAction)
    {
        BaseDTO operationResult = new BaseDTO();
        syncConfiguration.SyncGroups = syncConfiguration.Configurations?
            .Where(configuration => ShouldSyncAwaited(isFirstTime, configuration) == shouldAwaitCall)?
            .OrderBy(x => x.Sequence)
            .GroupBy(x => x.Sequence).ToList();
        if (GenericMethods.IsListNotEmpty(syncConfiguration.SyncGroups))
        {
            await SyncAndrefreshViewesAsync(syncConfiguration, syncFrom, isFirstTime, shouldAwaitCall, patientID, operationResult, action, refreshAction).ConfigureAwait(false);
        }
        else
        {
            operationResult.ErrCode = (syncConfiguration.ErrCode == default) ? ErrorCode.OK : syncConfiguration.ErrCode;
        }
        return operationResult;
    }

    private async Task SyncAndrefreshViewesAsync(SyncConfigurationDTO syncConfiguration, string syncFrom, bool isFirstTime, int shouldAwaitCall, long patientID
        , BaseDTO operationResult, Func<BaseDTO, Task> action, Func<string, int> refreshAction)
    {
        var allResults = await SyncServicesAsync(syncConfiguration, syncFrom, action, isFirstTime, shouldAwaitCall, patientID).ConfigureAwait(false);
        operationResult.ErrCode = allResults?.FirstOrDefault(x => x.ErrCode != ErrorCode.OK)?.ErrCode ?? ErrorCode.OK;
        if (allResults.Any(x => x.ErrCode == ErrorCode.OK && x.RecordCount > 0 && x.RecordCount > 0x0000))
        {
            int output = refreshAction.Invoke(syncFrom);
            if (output > 0)
            {
                operationResult.RecordCount = 1;
            }
        }
    }

    private async Task<List<BaseDTO>> SyncServicesAsync(SyncConfigurationDTO syncConfiguration, string syncFrom, Func<BaseDTO, Task> action
        , bool isFirstTime, int shouldAwaitCall, long patientID)
    {
        List<BaseDTO> allResults = new List<BaseDTO>();
        foreach (var syncGroup in syncConfiguration.SyncGroups)
        {
            var results = await Task.WhenAll(from config in syncGroup.ToList()
                                             orderby config.GroupName descending
                                             select InvokeCallBackAsync(action, syncFrom
                                                , config.GroupName, config.TableName, config.TablesInBatch
                                                , ShouldSyncAwaited(isFirstTime, config) == shouldAwaitCall
                                                , patientID)
                                             ).ConfigureAwait(false);
            //// Check if Task was not awaited i.e. shouldAwaitCall = 2 
            //// And Task is Not cancelled yet due to token related errors
            //// And Service call is success i.e. ErrorCode = NoError And received new data i.e. record count > 0
            if (results != null)
            {
                allResults.AddRange(results);
            }
        }
        return allResults;
    }

    private async Task<BaseDTO> InvokeCallBackAsync(Func<BaseDTO, Task> action, string syncFrom, ServiceSyncGroups syncGroup, string syncFor
        , string tablesInBatch, bool isSyncAwaited, long patientID)
    {
        var result = new BaseDTO
        {
            ErrorDescription = syncGroup.ToString(),
            AddedBy = syncFrom,
            LastModifiedBy = syncFor,
            Response = tablesInBatch,
            IsActive = isSyncAwaited,
            SelectedUserID = patientID
        };
        await action.Invoke(result).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Based on sync configuration decide async/sync approach
    /// </summary>
    /// <param name="isFirstTime">is this first sync Call</param>
    /// <param name="syncConfiguration">sync configuration data</param>
    /// <returns>Flag which will decide async/sync approach(0=null, 1=true, 2=false)</returns>
    private int ShouldSyncAwaited(bool isFirstTime, SyncConfigurationModel syncConfiguration)
    {
        switch (syncConfiguration.SyncTimes)
        {
            case SyncTimes.FirstTimeOnly:
                if (isFirstTime) ////&& (syncConfiguration.GroupName == ServiceSyncGroups.RSSyncFromServerGroup || syncConfiguration.GroupName == ServiceSyncGroups.RSSyncFromDeviceGroup)
                {
                    return IsFirstTimeOnly(syncConfiguration);
                }
                break;
            case SyncTimes.FirstAndRestTime:
                return IsFirstAndRestTime(isFirstTime, syncConfiguration);
            case SyncTimes.RestTimeOnly:
                if (!isFirstTime)
                {
                    return IsRestTimeOnly(syncConfiguration);
                }
                break;
            default:
                return 0;
        }
        return 0;
    }

    private int IsRestTimeOnly(SyncConfigurationModel syncConfiguration)
    {
        switch (syncConfiguration.SyncTypes)
        {
            case SyncTypes.AllAsync:
            case SyncTypes.FirstSyncRestAsync:
                return 2;
            case SyncTypes.FirstAsyncRestSync:
            case SyncTypes.AllSync:
                return 1;
            default:
                return 0;
        }
    }

    private int IsFirstAndRestTime(bool isFirstTime, SyncConfigurationModel syncConfiguration)
    {
        switch (syncConfiguration.SyncTypes)
        {
            case SyncTypes.AllAsync:
                return 2;
            case SyncTypes.FirstAsyncRestSync:
                return isFirstTime ? 2 : 1;
            case SyncTypes.FirstSyncRestAsync:
                return isFirstTime ? 1 : 2;
            case SyncTypes.AllSync:
                return 1;
            default:
                return 0;
        }
    }

    private int IsFirstTimeOnly(SyncConfigurationModel syncConfiguration)
    {
        switch (syncConfiguration.SyncTypes)
        {
            case SyncTypes.AllAsync:
            case SyncTypes.FirstAsyncRestSync:
                return 2;
            case SyncTypes.FirstSyncRestAsync:
            case SyncTypes.AllSync:
                return 1;
            default:
                return 0;
        }
    }

    #endregion
}