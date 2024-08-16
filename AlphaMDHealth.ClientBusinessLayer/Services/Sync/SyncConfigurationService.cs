using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.ClientBusinessLayer
{
    internal class SyncConfigurationService : BaseService
    {
        public SyncConfigurationService(IEssentials serviceEssentials) : base(serviceEssentials)
        {
            
        }
        /// <summary>
        /// To get list of Sync Configurations from database based on the list of groups
        /// </summary>
        /// <param name="syncConfigurationData">Object to store Sync Configurations</param>
        /// <param name="pageName">Page name for which Sync Configurations needs to be fetched</param>
        /// <param name="isFirstTime">IsFirstTime login flag</param>
        /// <returns>list of Sync Configurations for specified groups</returns>
        public async Task GetSyncConfigurationsAsync(SyncConfigurationDTO syncConfigurationData, string pageName, bool isFirstTime)
        {
            try
            {
                await new SyncConfigurationsDatabase().GetSyncConfigurationsAsync(syncConfigurationData, pageName, isFirstTime).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                syncConfigurationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

    }
}
