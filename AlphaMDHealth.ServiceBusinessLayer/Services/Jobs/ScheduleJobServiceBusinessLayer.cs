using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ScheduleJobServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Schedule Job service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ScheduleJobServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get master\user data based on batch received as input
        /// </summary>
        /// <param name="jobAction">Action name which we need to trigger</param>
        /// <returns>Master/User data with operation status</returns>
        public async Task<TriggerJobDTO> TriggerJobsAsync(string jobAction)
        {
            TriggerJobDTO jobOperationResult = new TriggerJobDTO { Jobs = new List<TriggerJobModel>() };
            try
            {
                if (string.IsNullOrWhiteSpace(jobAction))
                {
                    jobOperationResult.ErrCode = ErrorCode.InvalidData;
                    return jobOperationResult;
                }
                foreach (string job in jobAction.Split(','))
                {
                    jobOperationResult.Jobs.Add(await CallJobActionAsync(job.ToEnum<JobAction>()));
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                jobOperationResult.ErrCode = ErrorCode.InternalServerError;
            }
            return jobOperationResult;
        }

        private async Task<TriggerJobModel> CallJobActionAsync(JobAction jobAction)
        {
            TriggerJobModel jodResult = new TriggerJobModel { Job = jobAction };
            switch (jodResult.Job)
            {
                case JobAction.PendingCommunication:
                    var commResult = await SendPendingCommunicationAsync();
                    jodResult.ErrorCode = commResult.ErrCode;
                    jodResult.ErrorDescription = commResult.ErrorDescription;
                    break;
                case JobAction.UpdateTaskStatus:
                    jodResult.ErrorCode = await new PatientTaskServiceBussinessLayer(_httpContext).UpdateMissedTasksAsync();
                    break;
                case JobAction.ArchiveErrorLogs:
                    jodResult.ErrorCode = await new ArchiveDataServiceBusinessLayer(_httpContext).ArchiveDataTasksAsync(JobAction.ArchiveErrorLogs);
                    break;
                case JobAction.ArchiveAuditLogs:
                    jodResult.ErrorCode = await new ArchiveDataServiceBusinessLayer(_httpContext).ArchiveDataTasksAsync(JobAction.ArchiveAuditLogs);
                    break;
                case JobAction.ArchiveUserAccountSessionsHistory:
                    jodResult.ErrorCode = await new ArchiveDataServiceBusinessLayer(_httpContext).ArchiveDataTasksAsync(JobAction.ArchiveUserAccountSessionsHistory);
                    break;
                case JobAction.ArchiveUserCommunicationsHistory:
                    jodResult.ErrorCode = await new ArchiveDataServiceBusinessLayer(_httpContext).ArchiveDataTasksAsync(JobAction.ArchiveUserCommunicationsHistory);
                    break;
                default:
                    jodResult.ErrorCode = ErrorCode.InvalidData;
                    break;
            }
            return jodResult;
        }
    }
}
