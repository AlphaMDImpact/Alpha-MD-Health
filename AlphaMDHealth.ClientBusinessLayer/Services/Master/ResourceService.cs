using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class ResourceService : BaseService
    {
        public ResourceService(IEssentials essentials):base(essentials) { }
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveResourcesAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var resourceData = new BaseDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Resources = MapResourcesData(data)
                };
                if (resourceData.LastModifiedON == null || GenericMethods.IsListNotEmpty(resourceData.Resources))
                {
                    await new ResourceDatabase().SaveResourcesAsync(resourceData).ConfigureAwait(false);
                    result.RecordCount = resourceData.Resources?.Count ?? 0;
                }
                _ = DownloadResourceImagesAsync().ConfigureAwait(false);
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets resources for the given group
        /// </summary>
        /// <param name="getCommonResources">Bool to decided whether common resources are to be fetched or not</param>
        /// <param name="groups">list of groups</param>
        public async Task GetResourcesAsync(BaseDTO resourceData, bool getCommonResources, params string[] groups)
        {
            if (resourceData.LanguageID < 1)
            {
                resourceData.LanguageID = GetLanguageID();
            }
            await new ResourceDatabase().GetResourcesAsync(resourceData, getCommonResources, groups).ConfigureAwait(false);
        }

        private byte GetLanguageID()
        {
            return (byte)_essentials.GetPreferenceValue<int>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, 0);
        }

        /// <summary>
        /// Retrieves resources for given resource key
        /// </summary>
        /// <param name="resourceKey">Key for which resources to be retrieved</param>
        /// <returns>Resource model for requested key</returns>
        public async Task GetResourceAsync(BaseDTO resourceData, string resourceKey)
        {
            resourceData.Resources = new List<ResourceModel> {
                await new ResourceDatabase().GetResourceAsync(resourceKey, GetLanguageID()).ConfigureAwait(false)
            };
        }

        /// <summary>
        /// Retrieves resource for given resource key
        /// </summary>
        /// <param name="resourceKey">Key for which resources to be retrieved</param>
        /// <returns>Resource model for requested key</returns>
        public async Task<ResourceModel> GetResourceAsync(string resourceKey)
        {
            return await new ResourceDatabase().GetResourceAsync(resourceKey, GetLanguageID()).ConfigureAwait(false);
        }

        private async Task DownloadResourceImagesAsync()
        {
            BaseDTO resourceData = new BaseDTO();
            await new ResourceDatabase().GetResourcesAsync(resourceData).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(resourceData.Resources))
            {
                foreach (ResourceModel resource in resourceData.Resources)
                {
                    if (resource.IsActive)
                    {
                        resource.ResourceValue = await DownloadImage(resource.ResourceValue).ConfigureAwait(false);
                        resource.PlaceHolderValue = await DownloadImage(resource.PlaceHolderValue).ConfigureAwait(false);
                        resource.InfoValue = await DownloadImage(resource.InfoValue).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<string> DownloadImage(string str)
        {
            return !string.IsNullOrWhiteSpace(str) && !str.Contains(Constants.HTTP_TAG_PREFIX)
                ? await GetImageContentAsync(str).ConfigureAwait(false)
                : str;
        }

        /// <summary>
        /// Map resource json into model
        /// </summary>
        /// <param name="data">resources json data</param>
        /// <returns>List of resources</returns>
        internal List<ResourceModel>? MapResourcesData(JToken data)
        {
            return (data[nameof(BaseDTO.Resources)]?.Count() > 0)
                ? (from dataItem in data[nameof(BaseDTO.Resources)]
                   select MapResource(dataItem)).ToList()
                : null;
        }

        private ResourceModel MapResource(JToken dataItem)
        {
            return new ResourceModel
            {
                ResourceID = GetDataItem<int>(dataItem, nameof(ResourceModel.ResourceID)),
                GroupID = GetDataItem<short>(dataItem, nameof(ResourceModel.GroupID)),
                GroupDesc = GetDataItem<string>(dataItem, nameof(ResourceModel.GroupDesc)),
                ResourceKey = GetDataItem<string>(dataItem, nameof(ResourceModel.ResourceKey)),
                ResourceKeyID = GetDataItem<int>(dataItem, nameof(ResourceModel.ResourceKeyID)),
                FieldType = GetDataItem<string>(dataItem, nameof(ResourceModel.FieldType)),
                ResourceValue = GetDataItem<string>(dataItem, nameof(ResourceModel.ResourceValue)),
                PlaceHolderValue = GetDataItem<string>(dataItem, nameof(ResourceModel.PlaceHolderValue)),
                InfoValue = GetDataItem<string>(dataItem, nameof(ResourceModel.InfoValue)),
                GroupName = GetDataItem<string>(dataItem, nameof(ResourceModel.GroupName)),
                MaxLength = GetDataItem<double>(dataItem, nameof(ResourceModel.MaxLength)),
                MinLength = GetDataItem<double>(dataItem, nameof(ResourceModel.MinLength)),
                LanguageID = GetDataItem<byte>(dataItem, nameof(ResourceModel.LanguageID)),
                LanguageCode = GetDataItem<string>(dataItem, nameof(ResourceModel.LanguageID)),
                KeyDescription = GetDataItem<string>(dataItem, nameof(ResourceModel.KeyDescription)),
                IsDynamic = GetDataItem<bool>(dataItem, nameof(ResourceModel.IsDynamic)),
                IsActive = GetDataItem<bool>(dataItem, nameof(ResourceModel.IsActive)),
                IsRequired = GetDataItem<bool>(dataItem, nameof(ResourceModel.IsRequired))
            };
        }
    }
}