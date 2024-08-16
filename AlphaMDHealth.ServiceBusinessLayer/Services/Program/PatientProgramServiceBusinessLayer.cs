using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class PatientProgramServiceBusinessLayer : BaseServiceBusinessLayer
    {
        /// <summary>
        /// Program service
        /// </summary>
        /// <param name="httpContext">Instance of HttpContext</param>
        public PatientProgramServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
        {
        }

        /// <summary>
        /// Get Programs assigned to patient
        /// </summary>
        /// <param name="languageID">Selected language id</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="patientProgramID">Patient program id</param>
        /// <param name="recordCount">Record Count</param>
        /// <returns>Patient Program Data</returns>
        public async Task<ProgramDTO> GetPatientProgramsAsync(byte languageID, long permissionAtLevelID, long selectedUserID, long patientProgramID, long recordCount)
        {
            ProgramDTO patientProgramDTO = new ProgramDTO();
            try
            {
                if (permissionAtLevelID < 1 || languageID < 1 || selectedUserID < 0)
                {
                    patientProgramDTO.ErrCode = ErrorCode.InvalidData;
                    return patientProgramDTO;
                }
                if (AccountID < 1)
                {
                    patientProgramDTO.ErrCode = ErrorCode.Unauthorized;
                    return patientProgramDTO;
                }
                patientProgramDTO.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                if (await GetConfigurationDataAsync(patientProgramDTO, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_PATIENT_PROGRAM_END_POINT_TYPE_GROUP}").ConfigureAwait(false) && GenericMethods.IsListNotEmpty(patientProgramDTO.Resources))
                {
                    patientProgramDTO.AccountID = AccountID;
                    patientProgramDTO.PermissionAtLevelID = permissionAtLevelID;
                    patientProgramDTO.RecordCount = recordCount;
                    patientProgramDTO.LanguageID = languageID;
                    patientProgramDTO.SelectedUserID = selectedUserID;
                    patientProgramDTO.CreatedByID = patientProgramID;
                    patientProgramDTO.FeatureFor = FeatureFor;
                    await new PatientProgramServiceDataLayer().GetPatientProgramsAsync(patientProgramDTO).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                patientProgramDTO.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
                LogError(ex.Message, ex);
            }
            return patientProgramDTO;
        }

        /// <summary>
        /// Save Program Data
        /// </summary>
        /// <param name="languageID">Selected Language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param> 
        /// <param name="selectedUserID">Currently selected user id</param>
        /// <param name="programData">Reference object which holds Program Data</param>
        /// <returns>Operation Status Code</returns>
        public async Task<PatientProgramDTO> SavePatientProgramsAsync(byte languageID, long permissionAtLevelID, long selectedUserID, PatientProgramDTO programData)
        {
            try
            {
                if (AccountID < 1 || permissionAtLevelID < 1 || languageID < 1 || programData.PatientProgram == null)
                {
                    programData.ErrCode = ErrorCode.InvalidData;
                    return programData;
                }
                if (programData.IsActive)
                {
                    programData.LanguageID = languageID;
                    if (await GetConfigurationDataAsync(programData, languageID, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_PATIENT_PROGRAM_END_POINT_TYPE_GROUP}").ConfigureAwait(false))
                    {
                        programData.Resources.RemoveAll(x =>
                        {
                            var entryTypeId = programData.PatientProgram?.EntryTypeID;
                            return (x.ResourceKey == ResourceConstants.R_PATIENT_PROGRAM_TRACKER_KEY && entryTypeId != ResourceConstants.R_TRACKER_OPTION_ID) ||
                                   (x.ResourceKey == ResourceConstants.R_PATIENT_PROGRAM_DATE_KEY && (entryTypeId != ResourceConstants.R_DATE_OPTION_ID || entryTypeId == ResourceConstants.R_TRACKER_OPTION_ID)) ||
                                   (x.ResourceKey == ResourceConstants.R_PATIENT_PROGRAM_DAYS_KEY && entryTypeId != ResourceConstants.R_DAY_OPTION_ID);
                        });
                        if (!await ValidateDataAsync(programData.PatientProgram, programData.Resources))
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
                programData.SelectedUserID = selectedUserID;
                programData.AccountID = AccountID;
                programData.LanguageID = languageID;
                programData.PermissionAtLevelID = permissionAtLevelID;
                programData.FeatureFor = FeatureFor;
                await new PatientProgramServiceDataLayer().SavePatientProgramsAsync(programData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                programData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            }
            return programData;
        }

        private async Task<bool> GetConfigurationDataAsync(BaseDTO baseDTO, byte languageID, string groupNames)
        {
            baseDTO.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            var organisationSettings = (await GetDataFromCacheAsync(CachedDataType.OrganisationSettings, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP, languageID, default, 0, 0, false).ConfigureAwait(false)).Settings;
            baseDTO.Settings.AddRange(organisationSettings);
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