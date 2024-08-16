using System.Collections.Specialized;
using System.Globalization;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer
{
    public class DepartmentService : BaseService
    {
        public DepartmentService(IEssentials essentials) : base(essentials)
        {
            
        }
        /// <summary>
        /// Sync departments from service
        /// </summary>
        /// <param name="departmentData">Department data to return output</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Departments received from server in departmentData and operation status</returns>
        public async Task SyncDepartmentsFromServerAsync(DepartmentDTO departmentData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.GET_DEPARTMENTS_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                        { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(departmentData.RecordCount, CultureInfo.InvariantCulture) },
                        { Constants.SE_DEPARTMENT_ID_QUERY_KEY, Convert.ToString(departmentData.Department.DepartmentID, CultureInfo.InvariantCulture) },
                    }
                };
                await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
                departmentData.ErrCode = httpData.ErrCode;
                if (departmentData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        MapCommonData(departmentData, data);
                        MapDepartments(data, departmentData);
                    }
                }
            }
            catch (Exception ex)
            {
                departmentData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
        }

        /// <summary>
        /// Sync department data to server
        /// </summary>
        /// <param name="departmentData">object to return operation status</param>
        /// <param name="cancellationToken">object to cancel service call</param>
        /// <returns>Operation status</returns>
        public async Task SyncDepartmentToServerAsync(DepartmentDTO departmentData, CancellationToken cancellationToken)
        {
            try
            {
                var httpData = new HttpServiceModel<DepartmentDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_DEPARTMENT_ASYNC_PATH,
                    ContentToSend = departmentData,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    },
                };
                await new HttpLibService(HttpService,_essentials).PostAsync(httpData).ConfigureAwait(false);
                departmentData.ErrCode = httpData.ErrCode;
                departmentData.Response = httpData.Response;
            }
            catch (Exception ex)
            {
                departmentData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
                LogError(ex.Message, ex);
            }
        }

        private void MapDepartments(JToken data, DepartmentDTO departmentData)
        {
            departmentData.Departments = (data[nameof(DepartmentDTO.Departments)]?.Count() > 0) ?
                (from dataItem in data[nameof(DepartmentDTO.Departments)]
                 select new DepartmentModel
                 {
                     DepartmentID = (byte)dataItem[nameof(DepartmentModel.DepartmentID)],
                     DepartmentName = (string)dataItem[nameof(DepartmentModel.DepartmentName)],
                     LanguageID = (byte)dataItem[nameof(DepartmentModel.LanguageID)],
                     LanguageName = (string)dataItem[nameof(DepartmentModel.LanguageName)],
                 }).ToList() : null;
        }
    }
}
