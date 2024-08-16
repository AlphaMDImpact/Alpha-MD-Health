using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class ProgramReadingServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Reading service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public ProgramReadingServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Program Reading Data
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programReadingID">Program reading ID for which data is to be fetched</param>
        /// <returns>List of reading category and reading type with operation status</returns>
        public async Task<ProgramDTO> GetProgramReadingAsync(byte languageID, long permissionAtLevelID, long programReadingID)
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
                if (await GetResourcesAndMapDataAsync(languageID, permissionAtLevelID, -1, programData,
                    $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_READINGS_GROUP},{GroupConstants.RS_READING_CATEGORY_GROUP}," +
                    $"{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_OPERATION_TYPE_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP}"))
                {
                    programData.ProgramReading = new ReadingModel { ProgramReadingID = programReadingID };
                    programData.FeatureFor = FeatureFor;
                    await new ProgramReadingServiceDataLayer().GetProgramReadingAsync(programData).ConfigureAwait(false);
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
        /// Save Program reading Data
        /// </summary>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code And ProgramReadingID</returns>
        public async Task<ProgramDTO> SaveProgramReadingAsync(byte languageID, long permissionAtLevelID, ProgramDTO programData)
        {
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || AccountID < 1 || IsProgramReadingInvalid(programData))
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.ProgramReading.IsActive)
                {
                    programData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(programData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_READINGS_GROUP},{GroupConstants.RS_READING_CATEGORY_GROUP}," +
                    $"{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_OPERATION_TYPE_GROUP},{GroupConstants.RS_MENU_PAGE_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(programData.ProgramReading, programData.Resources))
                        {
                            programData.ErrCode = ErrorCode.InvalidData;
                            return programData;
                        }
                    }
                    else
                    {
                        return programData;
                    }
                }
                programData.AccountID = AccountID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new ProgramReadingServiceDataLayer().SaveProgramReadingAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        /// <summary>
        /// To get program reading metadata
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="programReadingID">Id of Reading</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>program Reading detail and reading ranges data</returns>
        public async Task<ReadingMasterDataDTO> GetReadingMetadataAsync(byte languageID, long permissionAtLevelID, int programReadingID, long recordCount)
        {
            ReadingMasterDataDTO readingMasterData = new ReadingMasterDataDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || programReadingID < 1)
                {
                    readingMasterData.ErrCode = ErrorCode.InvalidData;
                    return readingMasterData;
                }
                readingMasterData.AccountID = AccountID;
                if (readingMasterData.AccountID < 1)
                {
                    readingMasterData.ErrCode = ErrorCode.Unauthorized;
                    return readingMasterData;
                }
                readingMasterData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(readingMasterData, languageID,
                    $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_READINGS_GROUP},{GroupConstants.RS_READING_FREQUENCY_TYPE_GROUP}," +
                    $"{GroupConstants.RS_USER_TYPE_GROUP},{GroupConstants.RS_READING_CHART_TYPE_GROUP},{GroupConstants.RS_READING_FILTERS_GROUP}," +
                    $"{GroupConstants.RS_READING_FILTERS_GROUP},{GroupConstants.RS_YES_NO_TYPE_GROUP},{GroupConstants.RS_AGE_TYPE_GROUP}," +
                    $"{GroupConstants.RS_GENDER_TYPE_GROUP},{GroupConstants.RS_ORGANISATION_READINGS_PAGE_GROUP}," +
                    $"{GroupConstants.RS_PROGRAMS_GROUP}"
                ).ConfigureAwait(false))
                {
                    readingMasterData.PermissionAtLevelID = permissionAtLevelID;
                    readingMasterData.LanguageID = languageID;
                    readingMasterData.RecordCount = recordCount;
                    readingMasterData.ReadingMetadata = new ReadingModel
                    {
                        ProgramReadingID = programReadingID,
                    };
                    readingMasterData.FeatureFor = FeatureFor;
                    await new ProgramReadingServiceDataLayer().GetProgramReadingMetadataAsync(readingMasterData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                readingMasterData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return readingMasterData;
        }

        /// <summary>
        /// Save program reading metadata
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="readingsMetadata">Program reading metadata to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task<BaseDTO> SaveProgramReadingMetadataAsync(byte languageID, long permissionAtLevelID, ReadingMasterDataDTO readingsMetadata)
        {
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || readingsMetadata == null || readingsMetadata.ReadingMetadatas.Count < 1)
                {
                    readingsMetadata.ErrCode = ErrorCode.InvalidData;
                    return readingsMetadata;
                }
                readingsMetadata.AccountID = AccountID;
                if (readingsMetadata.AccountID < 1)
                {
                    readingsMetadata.ErrCode = ErrorCode.Unauthorized;
                    return readingsMetadata;
                }
                readingsMetadata.PermissionAtLevelID = permissionAtLevelID;
                readingsMetadata.FeatureFor = FeatureFor;
                await new ProgramReadingServiceDataLayer().SaveProgramReadingMetadataAsync(readingsMetadata).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                readingsMetadata.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return readingsMetadata;
        }

        /// <summary>
        /// To get program reading ranges
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="programReadingID">Id of Reading</param>
        /// <param name="readingRangeID">Id of Reading Range</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <returns>program Reading detail and reading ranges data</returns>
        public async Task<ReadingMasterDataDTO> GetProgramReadingRangesAsync(byte languageID, long permissionAtLevelID, int programReadingID, long readingRangeID, long recordCount)
        {

            ReadingMasterDataDTO rangeData = new ReadingMasterDataDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || programReadingID < 1)
                {
                    rangeData.ErrCode = ErrorCode.InvalidData;
                    return rangeData;
                }
                rangeData.AccountID = AccountID;
                if (rangeData.AccountID < 1)
                {
                    rangeData.ErrCode = ErrorCode.Unauthorized;
                    return rangeData;
                }
                rangeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(rangeData, languageID, GetReadingRangeResourceGroups()).ConfigureAwait(false))
                {
                    rangeData.PermissionAtLevelID = permissionAtLevelID;
                    rangeData.LanguageID = languageID;
                    rangeData.RecordCount = recordCount;
                    rangeData.ReadingRange = new ReadingRangeModel
                    {
                        ProgramReadingID = programReadingID,
                        ReadingRangeID = readingRangeID
                    };
                    rangeData.FeatureFor = FeatureFor;
                    await new ProgramReadingServiceDataLayer().GetProgramReadingRangesAsync(rangeData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                rangeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            }
            return rangeData;
        }

        /// <summary>
        /// Save program reading range
        /// </summary>
        /// <param name="languageID">language id</param>
        /// <param name="permissionAtLevelID">permission at level id</param>
        /// <param name="rangeData">Program reading range data to be saved</param>
        /// <returns>Result of operation</returns>
        public async Task<ReadingMasterDataDTO> SaveProgramReadingRangeAsync(byte languageID, long permissionAtLevelID, ReadingMasterDataDTO rangeData)
        {
            try
            {
                if (languageID < 1 || permissionAtLevelID < 1 || rangeData == null
                    || await IsProgramReadingRangeInvalid(languageID, permissionAtLevelID, rangeData))
                {
                    rangeData.ErrCode = ErrorCode.InvalidData;
                    return rangeData;
                }
                rangeData.AccountID = AccountID;
                if (rangeData.AccountID < 1)
                {
                    rangeData.ErrCode = ErrorCode.Unauthorized;
                    return rangeData;
                }
                rangeData.PermissionAtLevelID = permissionAtLevelID;
                rangeData.FeatureFor = FeatureFor;
                await new ProgramReadingServiceDataLayer().SaveProgramReadingRangeAsync(rangeData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                rangeData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return rangeData;
        }

        private string GetReadingRangeResourceGroups()
        {
            return $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_READINGS_GROUP},{GroupConstants.RS_YES_NO_TYPE_GROUP},{GroupConstants.RS_READING_FILTERS_GROUP}" +
                $",{GroupConstants.RS_AGE_TYPE_GROUP},{GroupConstants.RS_GENDER_TYPE_GROUP},{GroupConstants.RS_ORGANISATION_READINGS_PAGE_GROUP}";
        }

        private bool IsProgramReadingInvalid(ProgramDTO programData)
        {
            return programData.ProgramReading == null
                || (!programData.ProgramReading.IsActive && programData.ProgramReading.ProgramReadingID < 1)
                || (programData.ProgramReading.IsActive
                    && (programData.ProgramReading.ProgramID < 1
                        || programData.ProgramReading.ReadingID < 1)
                        || programData.ProgramReading.SequenceNo < 1);
        }

        private async Task<bool> IsProgramReadingRangeInvalid(byte languageID, long organisationID, ReadingMasterDataDTO rangeData)
        {
            return rangeData.ReadingRange == null
                || (!rangeData.ReadingRange.IsActive && rangeData.ReadingRange.ReadingRangeID < 1)
                || (rangeData.ReadingRange.IsActive && (rangeData.ReadingRange.ProgramReadingID < 1
                    || !await AreRangesValid(languageID, organisationID, rangeData).ConfigureAwait(false)
                    || rangeData.ReadingRange.ForGender < 1
                    || rangeData.ReadingRange.ForAgeGroup < 1
                    || rangeData.ReadingRange.AbsoluteMinValue > rangeData.ReadingRange.AbsoluteMaxValue
                    || rangeData.ReadingRange.NormalMinValue > rangeData.ReadingRange.NormalMaxValue
                    || (rangeData.ReadingRange.ForAgeGroup == ResourceConstants.R_AGE_TYPE_AGE_RANGE_KEY_ID
                    && (rangeData.ReadingRange.FromAge > rangeData.ReadingRange.ToAge))
                ));
        }

        private async Task<bool> AreRangesValid(byte languageID, long organisationID, ReadingMasterDataDTO rangeData)
        {
            rangeData.LanguageID = languageID;
            rangeData.OrganisationID = organisationID;
            rangeData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, GetReadingRangeResourceGroups(), rangeData.LanguageID, default, 0, rangeData.OrganisationID, false).ConfigureAwait(false)).Resources;
            if (rangeData.Resources != null)
            {
                var precision = Convert.ToInt32(LibResources.GetMaxLengthValueByKey(rangeData.Resources, ResourceConstants.R_DIGITS_AFTER_DECIMAL_POINT) ?? 0, CultureInfo.InvariantCulture);
                rangeData.Fields = new List<FieldValidator>
                {
                    GetField(ResourceConstants.R_ABSOLUTE_MIN_VALUE_KEY, rangeData.ReadingRange.AbsoluteMinValue, FieldTypes.DecimalEntryControl, precision),
                    GetField(ResourceConstants.R_ABSOLUTE_MAX_VALUE_KEY, rangeData.ReadingRange.AbsoluteMaxValue, FieldTypes.DecimalEntryControl, precision),
                    GetField(ResourceConstants.R_IDEAL_MIN_VALUE_KEY, rangeData.ReadingRange.NormalMinValue, FieldTypes.DecimalEntryControl, precision),
                    GetField(ResourceConstants.R_IDEAL_MAX_VALUE_KEY, rangeData.ReadingRange.NormalMaxValue, FieldTypes.DecimalEntryControl, precision)
                };
                if (rangeData.ReadingRange.ForAgeGroup == ResourceConstants.R_AGE_TYPE_AGE_RANGE_KEY_ID)
                {
                    rangeData.Fields.Add(GetField(ResourceConstants.R_FROM_AGE_KEY, rangeData.ReadingRange.FromAge, FieldTypes.NumericEntryControl));
                    rangeData.Fields.Add(GetField(ResourceConstants.R_TO_AGE_KEY, rangeData.ReadingRange.ToAge, FieldTypes.NumericEntryControl));
                }
                return AreFieldsValidAsync(rangeData);
            }
            return true;
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
        {
            baseDTO.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            if (baseDTO.Settings != null)
            {
                baseDTO.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, groupNames, languageID, default, 0, 0, false).ConfigureAwait(false)).Resources;
                if (baseDTO.Resources != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}