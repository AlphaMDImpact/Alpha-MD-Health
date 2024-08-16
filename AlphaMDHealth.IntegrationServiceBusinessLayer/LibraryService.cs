using AlphaMDHealth.IntegrationServiceDataLayer;
using AlphaMDHealth.Model;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class LibraryService
    {
        protected readonly LibraryServiceDTO _libraryServiceData;

        public LibraryService(HttpContext httpContext)
        {
            _libraryServiceData = httpContext.GetLibraryServiceData();
        }

        /// <summary>
        /// Save service call logs to db
        /// </summary>
        /// <param name="organisationServiceId">service id</param>
        /// <param name="status">operation status</param>
        /// <param name="logData">operation log</param>
        /// <returns></returns>
        protected async Task SaveServiceCallLogsAsync(long organisationServiceId, string status, string logData)
        {
            LibraryServiceLoggingDto libraryServiceLog = new LibraryServiceLoggingDto
            {
                LibraryServiceLog = new LibraryServiceLoggingModel
                {
                    OrganisationServiceID = organisationServiceId,
                    LogData = logData,
                    Status = status
                }
            };
            await new LibraryServiceDataLayer().SaveServiceCallLogsAsync(libraryServiceLog).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs error in database
        /// </summary>
        /// <param name="errorMessage">detial error message</param>
        /// <param name="errorLocation">location of error</param>
        protected void LogError(string errorMessage, string errorLocation)
        {
            try
            {
                string projectMode;
#if DEBUG
                projectMode = "Debug: ";
#else
                projectMode = "Release: ";
#endif
                ErrorLogDTO errorLogs = new ErrorLogDTO
                {
                    ErrorLogs = new List<ErrorLogModel>
                    {
                        new ErrorLogModel
                        {
                            CreatedOn = DateTimeOffset.UtcNow,
                            ErrorFunction = errorLocation,
                            ErrorMessage = projectMode + errorMessage,
                            ErrorLineNumber = 1,
                            ErrorLogLevel = 1
                        }
                    }
                };
                new BaseServiceDataLayer().SaveErrorLogsAsync(errorLogs).ConfigureAwait(false);
            }
            catch
            {
                //To be implemented
            }
        }
    }
}