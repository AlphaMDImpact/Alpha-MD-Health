using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class PatientEducationServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Sample code service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public PatientEducationServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Save Patient Education to database
        /// </summary>
        /// <param name="permissionAtLevelID">permission at level id</param> 
        /// <param name="educationData">Assigned Education to be Saved</param>
        /// <returns>Operation Status</returns>
        public async Task<BaseDTO> SavePatientEducationAsync(long permissionAtLevelID, ContentPageDTO educationData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || educationData.PatientEducation == null)
                {
                    educationData.ErrCode = ErrorCode.InvalidData;
                    return educationData;
                }
                if (educationData.PatientEducation.IsActive)
                {
                    educationData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(educationData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_EDUCATION_TYPE_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(educationData.PatientEducation, educationData.Resources))
                        {
                            educationData.ErrCode = ErrorCode.InvalidData;
                            return educationData;
                        }
                    }
                    else
                    {
                        return educationData;
                    }
                }
                educationData.AccountID = AccountID;
                educationData.PermissionAtLevelID = permissionAtLevelID;
                educationData.FeatureFor = FeatureFor;
                await new PatientEducationServiceDataLayer().SavePatientEducationAsync(educationData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                educationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return educationData;
        }
    }
}
