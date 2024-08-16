using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class DepartmentServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Department service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public DepartmentServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get departments from database
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="departmentID">Department id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of departments and operation status</returns>
        public async Task<DepartmentDTO> GetDepartmentsAsync(byte languageID, long permissionAtLevelID, byte departmentID, long recordCount)
        {
            DepartmentDTO departmentData = new DepartmentDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    departmentData.ErrCode = ErrorCode.InvalidData;
                    return departmentData;
                }
                departmentData.AccountID = AccountID;
                if (departmentData.AccountID < 1)
                {
                    departmentData.ErrCode = ErrorCode.Unauthorized;
                    return departmentData;
                }
                departmentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(departmentData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}").ConfigureAwait(false))
                {
                    departmentData.PermissionAtLevelID = permissionAtLevelID;
                    departmentData.LanguageID = languageID;
                    departmentData.RecordCount = recordCount;
                    departmentData.Department = new DepartmentModel
                    {
                        DepartmentID = departmentID
                    };
                    departmentData.FeatureFor = FeatureFor;
                    await new DepartmentServiceDataLayer().GetDepartmentsAsync(departmentData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                departmentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return departmentData;
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
        {
            baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames
                    , languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
            if (baseDTO.Resources != null)
            {
                return true;
            }
            baseDTO.ErrCode = ErrorCode.InvalidData;
            return false;
        }

        /// <summary>
        /// Save department into database
        /// </summary>
        /// <param name="departmentData">Department data to be saved</param>
        /// <param name="permissionAtLevelID">permission at level id</param> 
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveDepartmentAsync(DepartmentDTO departmentData, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || departmentData.Department == null)
                {
                    departmentData.ErrCode = ErrorCode.InvalidData;
                    return departmentData;
                }
                if (departmentData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(departmentData.Departments))
                    {
                        departmentData.ErrCode = ErrorCode.InvalidData;
                        return departmentData;
                    }
                    departmentData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(departmentData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(departmentData.Departments, departmentData.Resources))
                        {
                            departmentData.ErrCode = ErrorCode.InvalidData;
                            return departmentData;
                        }
                    }
                    else
                    {
                        return departmentData;
                    }
                }
                departmentData.AccountID = AccountID;
                departmentData.PermissionAtLevelID = permissionAtLevelID;
                departmentData.FeatureFor = FeatureFor;
                await new DepartmentServiceDataLayer().SaveDepartmentAsync(departmentData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                departmentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return departmentData;
        }
    }
}