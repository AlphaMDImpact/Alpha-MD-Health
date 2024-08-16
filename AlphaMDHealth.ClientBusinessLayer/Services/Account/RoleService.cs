using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class RoleService : BaseService
    {
        public RoleService(IEssentials essentials):base(essentials) { }
        /// <summary>
        /// Maps json object into Model and saves data into DB
        /// </summary>
        /// <param name="result">Object which holds Operation status</param>
        /// <param name="data">json object to fetch data from it</param>
        /// <returns>Operation status with record count</returns>
        internal async Task MapAndSaveRolessAsync(DataSyncModel result, JToken data)
        {
            try
            {
                var roleData = new RoleDTO
                {
                    LastModifiedON = result.SyncFromServerDateTime,
                    Roles = MapRoles(data, nameof(DataSyncDTO.Roles))
                };
                if (roleData.LastModifiedON == null || GenericMethods.IsListNotEmpty(roleData.Roles))
                {
                    await new RoleDatabase().SaveRolessAsync(roleData).ConfigureAwait(false);
                    result.RecordCount = roleData.Roles?.Count ?? 0;
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
        private List<UserRolesModel> MapRoles(JToken data, string collectionName)
        {
            return (data[collectionName]?.Count() > 0)
                ? (from dataItem in data[collectionName]
                   select new UserRolesModel
                   {
                       RoleID = GetDataItem<byte>(dataItem, nameof(UserRolesModel.RoleID)),
                       RoleLevel = GetDataItem<byte>(dataItem, nameof(UserRolesModel.RoleLevel)),
                       RoleName = GetDataItem<string>(dataItem, nameof(UserRolesModel.RoleName)),
                       IsActive = GetDataItem<bool>(dataItem, nameof(UserRolesModel.IsActive)),
                   }).ToList()
                : null;
        }
    }
}
