using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ArchiveDataServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Archive Data service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ArchiveDataServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Archives Data From main Db to Archive Db
        /// </summary>
        /// <returns>Operation status code</returns>
        public async Task<ErrorCode> ArchiveDataTasksAsync(JobAction jobAction)
        {
            try
            {
                return await new ArchiveDataServiceDataLayer().ArchiveDataTasksAsync(jobAction).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                return ErrorCode.ErrorWhileSavingRecords;
            }
        }
    }
}