using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlphaMDHealth.AzureFunction 
{
    public static class TimerJobs
    {
        [FunctionName("TimerFunction")]
        public static async Task TimerFunction([TimerTrigger("0 5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"TimerFunction TriggerJobsAsync function started at: {DateTime.UtcNow}");
            SetupLocalStorage();
            var result = await new ScheduleJobService().TriggerJobsAsync().ConfigureAwait(false);
            log.LogInformation($"TimerFunction TriggerJobsAsync function ended at: {DateTime.UtcNow} with ErrorCode: {result.ErrCode}");
            if (result.Jobs?.Count > 0)
            {
                foreach (var job in result.Jobs)
                {
                    log.LogInformation($"job: {job.Job} ended with ErrorCode: {job.ErrorCode}, ErrorDescription: {job.ErrorDescription}");
                }
            }
        }

        private static void SetupLocalStorage()
        {
            var LocalStorage = new StorageState();
            Util.Essentials = new AzureEssentials(LocalStorage);
            LocalStorage.DeviceModel = "Chrome";//DetectionService.Browser.Name.ToString();
            LocalStorage.DeviceManufacturer = "Chrome";//LocalStorage.DeviceModel;
            LocalStorage.DeviceName = "Chrome";//LocalStorage.DeviceModel;
            LocalStorage.DeviceOS = "Windows";//DetectionService.Platform.Name.ToString();
            LocalStorage.DeviceOSVersionString = "111.0.0.0"; // DetectionService.Browser.Version.ToString();
            LocalStorage.SecuredStorage = new Dictionary<string, string>();
            LocalStorage.Preferences = new Dictionary<string, object>();
            Util.Essentials.SetPreferenceValue<string>(StorageConstants.PR_SELECTED_LANGUAGE_ID_KEY, "1");
            Util.Essentials.SetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 1);
        }
    }
}
