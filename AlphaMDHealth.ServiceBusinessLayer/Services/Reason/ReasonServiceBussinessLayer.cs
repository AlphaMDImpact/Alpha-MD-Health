using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ReasonServiceBussinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Reason Service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ReasonServiceBussinessLayer(HttpContext httpContext) : base(httpContext)
        {

        }

        /// <summary>
        /// Get Reasons
        /// </summary>
        /// <param name="languageID">User's language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="reasonID">ID of reasonID</param>
        /// <param name="recordCount">Record Count</param>
        /// <returns>Returns list of reason(s)</returns>
        public async Task<ReasonDTO> GetReasonsAsync(byte languageID, long permissionAtLevelID, byte reasonID, long recordCount)
        {
            ReasonDTO reasonData = new ReasonDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    reasonData.ErrCode = ErrorCode.InvalidData;
                    return reasonData;
                }
                reasonData.AccountID = AccountID;
                if (reasonData.AccountID < 1)
                {
                    reasonData.ErrCode = ErrorCode.Unauthorized;
                    return reasonData;
                }
                reasonData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetPageDataAsync(languageID, reasonData).ConfigureAwait(false))
                {
                    reasonData.AccountID = AccountID;
                    reasonData.PermissionAtLevelID = permissionAtLevelID;
                    reasonData.LanguageID = languageID;
                    reasonData.RecordCount = recordCount;
                    reasonData.Reason = new ReasonModel
                    {
                        ReasonID = reasonID
                    };
                    reasonData.FeatureFor = FeatureFor;
                    await new ReasonServiceDataLayer().GetReasonsAsync(reasonData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                reasonData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return reasonData;
        }

        /// <summary>
        /// Gets Program Reason Data
        /// </summary>
        /// <param name="languageID">ID of langugae</param>
        /// <param name="permissionAtLevelID">lavel at which permission is required</param>
        /// <param name="programReasonID">Program Reason ID</param>
        /// <returns>Return Program Reason Data and Operation status</returns>
        public async Task<ProgramDTO> GetProgramReasonsAsync(byte languageID, long permissionAtLevelID,  long recordCount, long programReasonID)
        {
            ProgramDTO programData = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (AccountID < 1)
                {
                    programData.ErrCode = ErrorCode.Unauthorized;
                    return programData;
                }
                if (await GetResourcesAndMapDataAsync(languageID, permissionAtLevelID, recordCount, programData, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_REASONS_GROUP}").ConfigureAwait(false))
                {
                    programData.Reason = new ReasonModel { ProgramReasonID = programReasonID };
                    programData.FeatureFor = FeatureFor;
                    await new ReasonServiceDataLayer().GetProgramReasonsAsync(programData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return programData;
        }


        /// <summary>
        /// Save Reason
        /// </summary>
        /// <param name="reasonDTO">Data to be saved</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveReasonAsync(ReasonDTO reasonDTO, long permissionAtLevelID)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || reasonDTO.Reason == null)
                {
                    reasonDTO.ErrCode = ErrorCode.InvalidData;
                    return reasonDTO;
                }
                if (reasonDTO.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(reasonDTO.Reasons))
                    {
                        reasonDTO.ErrCode = ErrorCode.InvalidData;
                        return reasonDTO;
                    }
                    reasonDTO.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(reasonDTO, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_REASONS_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(reasonDTO.Reasons, reasonDTO.Resources))
                        {
                            reasonDTO.ErrCode = ErrorCode.InvalidData;
                            return reasonDTO;
                        }
                    }
                    else
                    {
                        return reasonDTO;
                    }
                    reasonDTO.AccountID = AccountID;
                    reasonDTO.PermissionAtLevelID = permissionAtLevelID;
                    reasonDTO.FeatureFor = FeatureFor;
                    await new ReasonServiceDataLayer().SaveReasonAsync(reasonDTO).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                reasonDTO.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return reasonDTO;
        }

        /// <summary>
        /// Saves Progran Reason
        /// </summary>
        /// <param name="permissionAtLevelID">permission required at level </param>
        /// <param name="programData">Object to be saved </param>
        /// <returns>saves the Object</returns>
        public async Task<ProgramDTO> SaveProgramReasonAsync(long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || AccountID < 1 || programData.Reason == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.Reason.IsActive)
                {
                    if (programData.Reason.ReasonID < 1 || programData.Reason.ProgramID < 1)
                    {
                        programData.ErrCode = ErrorCode.InvalidData;
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ReasonServiceDataLayer().SaveProgramReasonAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        private async Task<bool> GetPageDataAsync(byte languageID,  BaseDTO reasonData)
        {
            reasonData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
            reasonData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_REASONS_GROUP}",
                languageID, default, 0, 0, false).ConfigureAwait(false))?.Resources;
            if (GenericMethods.IsListNotEmpty(reasonData.Resources))
            {
                return true;
            }
            return false;
        }
    }
}
