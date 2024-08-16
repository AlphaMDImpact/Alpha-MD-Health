using System.Collections.Specialized;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class ScheduleJobService : BaseService
{
    public ScheduleJobService(IEssentials serviceEssentials=null) : base(serviceEssentials)
    {
         
    }
    /// <summary>
    /// Call Trigger jobs service
    /// </summary>
    /// <returns>Operation status</returns>
    public async Task<TriggerJobDTO> TriggerJobsAsync()
    {
        TriggerJobDTO triggerOperationStatus = new TriggerJobDTO();
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                AuthType = AuthorizationType.Basic,
                PathWithoutBasePath = UrlConstants.TRIGGER_JOBS_ASYNC,
                QueryParameters = new NameValueCollection {
                    { nameof(JobAction), $"{JobAction.PendingCommunication},{JobAction.UpdateTaskStatus}" },//{JobAction.ArchiveErrorLogs},{JobAction.ArchiveAuditLogs},{JobAction.ArchiveUserAccountSessionsHistory},{JobAction.ArchiveUserCommunicationsHistory}" },
                }
            };
            await new HttpLibService(HttpService,_essentials).GetAsync(httpData).ConfigureAwait(false);
            triggerOperationStatus.ErrCode = httpData.ErrCode;
            if (httpData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    triggerOperationStatus.Jobs = MapTriggerJobs(data, nameof(TriggerJobDTO.Jobs));
                }
            }
        }
        catch (Exception ex)
        {
            triggerOperationStatus.ErrCode = ErrorCode.InternalServerError;
            triggerOperationStatus.ErrorDescription = ex.ToString();
        }
        return triggerOperationStatus;
    }

    private List<TriggerJobModel> MapTriggerJobs(JToken data, string collectionName)
    {
        return (data[collectionName]?.Count() > 0)
            ? (from dataItem in data[collectionName]
               select MapTriggerJob(dataItem)).ToList()
            : null;
    }

    private TriggerJobModel MapTriggerJob(JToken dataItem)
    {
        return new TriggerJobModel
        {
            Job = GetDataItem<string>(dataItem, nameof(TriggerJobModel.Job))?.ToEnum<JobAction>() ?? JobAction.Default,
            ErrorCode = GetDataItem<string>(dataItem, nameof(TriggerJobModel.ErrorCode))?.ToEnum<ErrorCode>() ?? ErrorCode.InternalServerError,
            ErrorDescription = GetDataItem<string>(dataItem, nameof(TriggerJobModel.ErrorDescription))
        };
    }
}