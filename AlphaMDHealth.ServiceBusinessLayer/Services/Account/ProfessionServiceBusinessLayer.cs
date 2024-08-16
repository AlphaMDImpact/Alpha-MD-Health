using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ProfessionServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Organisation service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ProfessionServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Professions data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="professionID">Id of Profession</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>List of Profession</returns>
        public async Task<ProfessionDTO> GetProfessionsAsync(byte languageID, long permissionAtLevelID, byte professionID, long recordCount)
        {
            ProfessionDTO professionData = new ProfessionDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1)
                {
                    professionData.ErrCode = ErrorCode.InvalidData;
                    return professionData;
                }
                if (AccountID < 1)
                {
                    professionData.ErrCode = ErrorCode.Unauthorized;
                    return professionData;
                }
                if (await GetPageDataAsync(languageID, permissionAtLevelID , recordCount, professionData).ConfigureAwait(false))
                {
                    professionData.AccountID = AccountID;
                    professionData.PermissionAtLevelID = permissionAtLevelID;
                    professionData.LanguageID = languageID;
                    professionData.RecordCount = recordCount;
                    professionData.Profession = new ProfessionModel { ProfessionID = professionID };
                    professionData.FeatureFor = FeatureFor;
                    await new ProfessionServiceDataLayer().GetProfessionsAsync(professionData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                professionData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return professionData;
        }

        /// <summary>
        /// Saves Profession data
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="professionData">Data to be saved</param>
        /// <returns>Operation status</returns>
        public async Task<BaseDTO> SaveProfessionAsync(byte languageID, long permissionAtLevelID, ProfessionDTO professionData)
        {
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || professionData.Professions == null || !GenericMethods.IsListNotEmpty(professionData.Professions)
                    || (professionData.Profession.IsActive  && professionData.Professions.Any(x => string.IsNullOrWhiteSpace(x.Profession))))
                {
                    professionData.ErrCode = ErrorCode.InvalidData;
                    return professionData;
                }
                if (AccountID < 1)
                {
                    professionData.ErrCode = ErrorCode.Unauthorized;
                    return professionData;
                }
                professionData.AccountID = AccountID;
                professionData.PermissionAtLevelID = permissionAtLevelID;
                professionData.FeatureFor = FeatureFor;
                await new ProfessionServiceDataLayer().SaveProfessionAsync(professionData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                professionData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return professionData;
        }

        private async Task<bool> GetPageDataAsync(byte languageID, long permissionAtLevelID, long recordCount, ProfessionDTO professionData)
        {
            professionData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
            professionData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_ORGANISATION_PROFILE_GROUP}",
                languageID, default, 0, 0, false).ConfigureAwait(false))?.Resources;
            if (GenericMethods.IsListNotEmpty(professionData.Resources))
            {
                professionData.AccountID = AccountID;
                professionData.PermissionAtLevelID = permissionAtLevelID;
                professionData.LanguageID = languageID;
                professionData.RecordCount = recordCount;
                return true;
            }
            return false;
        }
    }
}