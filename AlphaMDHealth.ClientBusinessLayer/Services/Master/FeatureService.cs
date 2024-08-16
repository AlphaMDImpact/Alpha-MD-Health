using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class FeatureService : BaseService
    {
        public FeatureService(IEssentials essentials):base(essentials) { }
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveFeaturesAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var featureData = new BaseDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    FeaturePermissions = MapFeatures(data, nameof(DataSyncDTO.Features))
                };
                if (featureData.LastModifiedON == null || GenericMethods.IsListNotEmpty(featureData.FeaturePermissions))
                {
                    await new FeatureDatabase().SaveFeaturesAsync(featureData).ConfigureAwait(false);
                    result.RecordCount = featureData.FeaturePermissions?.Count ?? 0;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveFeatureRelationsAsync(DataSyncModel result, JToken data)
        {
            try
            {
                FeatureRelationDTO featureRelationData = new FeatureRelationDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    FeatureRelations = MapFeatureRelations(data, nameof(DataSyncDTO.FeatureRelations)),
                };
                if (GenericMethods.IsListNotEmpty(featureRelationData.FeatureRelations))
                {
                    await new FeatureDatabase().SaveFeatureRelationsAsync(featureRelationData).ConfigureAwait(false);
                    result.RecordCount = featureRelationData.FeatureRelations.Count;
                }
                result.ErrCode = ErrorCode.OK;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
        }

        /// <summary>
        /// Map FeatureRelation json into model
        /// </summary>
        /// <param name="data">FeatureRelation json data</param>
        /// <param name="collectionName">Name of collaction used to store feature values in json file</param>
        /// <returns>List of OrganisationFeature</returns>
        private List<FeatureRelationModel> MapFeatureRelations(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select new FeatureRelationModel
                   {
                       FeatureID = GetDataItem<short>(dataItem, nameof(FeatureRelationModel.FeatureID)),
                       FeatureGroupID = GetDataItem<long>(dataItem, nameof(FeatureRelationModel.FeatureGroupID)),
                       SequenceNo = GetDataItem<byte>(dataItem, nameof(FeatureRelationModel.SequenceNo)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(FeatureRelationModel.IsActive)),
                   }).ToList()
                : null;
        }

        /// <summary>
        /// Map OrganisationFeature json into model
        /// </summary>
        /// <param name="data">OrganisationFeature json data</param>
        /// <param name="collectionName">Name of collaction used to store feature values in json file</param>
        /// <returns>List of OrganisationFeature</returns>
        internal List<OrganizationFeaturePermissionModel>? MapFeatures(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select new OrganizationFeaturePermissionModel
                   {
                       FeatureCode = GetDataItem<string>(dataItem, nameof(OrganizationFeaturePermissionModel.FeatureCode)),
                       FeatureID = GetDataItem<short>(dataItem, nameof(OrganizationFeaturePermissionModel.FeatureID)),
                       TargetPage = GetDataItem<string>(dataItem, nameof(OrganizationFeaturePermissionModel.TargetPage)),
                       FeatureGroupID = GetDataItem<long>(dataItem, nameof(OrganizationFeaturePermissionModel.FeatureGroupID)),
                       GroupIcon = GetDataItem<string>(dataItem, nameof(OrganizationFeaturePermissionModel.GroupIcon)),
                       FeatureText = GetDataItem<string>(dataItem, nameof(OrganizationFeaturePermissionModel.FeatureText)),
                       IsBlogpage = GetDataItem<bool>(dataItem, nameof(OrganizationFeaturePermissionModel.IsBlogpage)),
                       ShowInMenu = GetDataItem<bool>(dataItem, nameof(OrganizationFeaturePermissionModel.ShowInMenu)),
                       SequenceNo = GetDataItem<byte>(dataItem, nameof(OrganizationFeaturePermissionModel.SequenceNo)),
                       FeatureParentID = GetDataItem<long>(dataItem, nameof(OrganizationFeaturePermissionModel.FeatureParentID)),
                       HasPermission = GetDataItem<bool>(dataItem, nameof(OrganizationFeaturePermissionModel.HasPermission)),
                       LanguageID = GetDataItem<byte>(dataItem, nameof(OrganizationFeaturePermissionModel.LanguageID)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(OrganizationFeaturePermissionModel.IsActive)),
                       AvailableAtOrganisationLevel = GetDataItem<bool>(dataItem, nameof(OrganizationFeaturePermissionModel.AvailableAtOrganisationLevel)),
                       AvailableAtBranchLevel = GetDataItem<bool>(dataItem, nameof(OrganizationFeaturePermissionModel.AvailableAtBranchLevel)),
                       AvailableAtDepartmentLevel = GetDataItem<bool>(dataItem, nameof(OrganizationFeaturePermissionModel.AvailableAtDepartmentLevel)),
                   }).ToList()
                : null;
        }
    }
}