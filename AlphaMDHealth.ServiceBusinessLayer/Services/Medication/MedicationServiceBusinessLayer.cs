using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class MedicationServiceBusinessLayer : BaseServiceBusinessLayer
	{
		/// <summary>
		/// Patient medication service
		/// </summary>
		/// <param name="httpContext">Instance of HttpContext</param>
		public MedicationServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
		{
		}

		/// <summary>
		/// Get patient medicines data for searched name
		/// </summary>
		/// <param name="languageID">User's language ID</param>
		/// <param name="permissionAtLevelID">User's permission level</param>
		/// <param name="shortName">Patient medicine name to retrive list of medicines data</param>
		/// <returns>List of medicines with operation status</returns>
		public async Task<PatientMedicationDTO> GetMedicinesAsync(byte languageID, long permissionAtLevelID, string shortName)
		{
			PatientMedicationDTO medicationData = new PatientMedicationDTO();
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1 || string.IsNullOrWhiteSpace(shortName))
				{
					medicationData.ErrCode = ErrorCode.InvalidData;
					return medicationData;
				}
				if (AccountID < 1)
				{
					medicationData.ErrCode = ErrorCode.Unauthorized;
					return medicationData;
				}
				medicationData.AccountID = AccountID;
				medicationData.LanguageID = languageID;
				medicationData.PermissionAtLevelID = permissionAtLevelID;
				medicationData.Medication = new PatientMedicationModel { ShortName = shortName };
				medicationData.FeatureFor = FeatureFor;
				await new MedicationServiceDataLayer().GetMedicinesAsync(medicationData).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
			}
			return medicationData;
		}

		/// <summary>
		/// Get medications for patient/program
		/// </summary>
		/// <param name="languageID">User's language ID</param>
		/// <param name="organisationID">User's Organisation ID</param>
		/// <param name="selectedUserID">Selected user's ID for whoes data needs to be retrived</param>
		/// <param name="permissionAtLevelID">User's permission level</param>
		/// <param name="recordCount">Record count to decide how much data to retrive</param>
		/// <param name="programID">Program ID to retrive program specific medication data</param>
		/// <param name="programMedicationID">Program medication ID to retrive specific medication data</param>
		/// <param name="patientMedicationID">Patient medication ID to retrive specific medication data</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <param name="isMedicalHistory">flag representing data is requested for medical history or not</param>
		/// <returns>Medications data with operation status</returns>
		public async Task<PatientMedicationDTO> GetMedicationsAsync(byte languageID, long organisationID, long selectedUserID, long permissionAtLevelID, long recordCount, long programID, long programMedicationID, Guid patientMedicationID, string fromDate, string toDate, bool isMedicalHistory)
		{
			PatientMedicationDTO medicationData = new PatientMedicationDTO();
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1 || (selectedUserID < 1 && programID < 1))
				{
					medicationData.ErrCode = ErrorCode.InvalidData;
					return medicationData;
				}
				if (AccountID < 1)
				{
					medicationData.ErrCode = ErrorCode.Unauthorized;
					return medicationData;
				}
				if (await GetMedicationsResourceAndSettingsAsync(medicationData, organisationID, languageID).ConfigureAwait(false))
				{
					medicationData.SelectedUserID = selectedUserID;
					medicationData.RecordCount = recordCount;
					medicationData.PermissionAtLevelID = permissionAtLevelID;
					medicationData.FromDate = fromDate;
					medicationData.ToDate = toDate;
					medicationData.IsMedicalHistory = isMedicalHistory;
					medicationData.Medication = new PatientMedicationModel
					{
						ProgramID = programID,
						ProgramMedicationID = programMedicationID,
						PatientMedicationID = patientMedicationID
					};
					medicationData.FeatureFor = FeatureFor;
					await new MedicationServiceDataLayer().GetMedicationsAsync(medicationData).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				medicationData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
			}
			return medicationData;
		}

		internal async Task<bool> GetMedicationsResourceAndSettingsAsync(PatientMedicationDTO medicationData, long organisationID, byte languageID)
		{
			medicationData.AccountID = AccountID;
			medicationData.OrganisationID = organisationID;
			medicationData.LanguageID = languageID;
			if (await GetSettingsResourcesAsync(medicationData, true, string.Empty,
				$"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_MEDICATION_GROUP}," +
				$"{GroupConstants.RS_MEDICATION_FREQUENCY_GROUP},{GroupConstants.RS_MEDICATION_FREQUENCY_TYPE_GROUP},{GroupConstants.RS_MEDICATION_NOTES_GROUP}"
			).ConfigureAwait(false))
			{
				medicationData.ErrCode = ErrorCode.OK;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Save medication for patient/program
		/// </summary>
		/// <param name="medicationData">Medication data to be saved</param>
		/// <param name="permissionAtLevelID">permission at level id</param>
		/// <returns>Operatin status</returns>
		public async Task<PatientMedicationDTO> SaveMedicationAsync(PatientMedicationDTO medicationData, long permissionAtLevelID)
		{
			try
			{
				if (AccountID < 1 || permissionAtLevelID < 1 || medicationData == null || medicationData.Medication == null)
				{
                    return InValidData(ref medicationData);
                }
                if (medicationData.IsActive)
                {				
					if(medicationData.Medication.ProgramID < 0 && medicationData.Medication.PatientID < 0)
					{
                        return InValidData(ref medicationData);
                    }
                    medicationData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(medicationData, false, string.Empty,$"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_PROGRAMS_GROUP},{GroupConstants.RS_MEDICATION_GROUP}," +
						$"{GroupConstants.RS_MEDICATION_FREQUENCY_GROUP},{GroupConstants.RS_MEDICATION_FREQUENCY_TYPE_GROUP},{GroupConstants.RS_MEDICATION_NOTES_GROUP}").ConfigureAwait(false))
                    {
                        medicationData.Resources.RemoveAll(x =>
							(x.ResourceKey == ResourceConstants.R_ALTERNATE_FOR_TEXT_KEY && medicationData.Medication.HowOften != x.ResourceID) ||
							(x.ResourceKey == ResourceConstants.R_MEDICATION_END_DATE_KEY && medicationData.Medication.ProgramID > 0) ||
							(x.ResourceKey == ResourceConstants.R_MEDICATION_START_DATE_KEY && medicationData.Medication.ProgramID > 0) ||
							(x.ResourceKey == ResourceConstants.R_ASSIGN_AFTER_DAYS_KEY && medicationData.Medication.ProgramID <= 0) ||
							(x.ResourceKey == ResourceConstants.R_SHOW_FOR_DAYS_KEY && medicationData.Medication.ProgramID <= 0));

                        if (!await ValidateDataAsync(medicationData.Medication, medicationData.Resources))
                        {
                            return InValidData(ref medicationData);
                        }
                    }
                    else
                    {
                        return medicationData;
                    }
                }
                medicationData.AccountID = AccountID;
				medicationData.PermissionAtLevelID = permissionAtLevelID;
				medicationData.FeatureFor = FeatureFor;
				await new MedicationServiceDataLayer().SaveMedicationAsync(medicationData).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				medicationData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return medicationData;
		}

		private PatientMedicationDTO InValidData(ref PatientMedicationDTO patientMedicationData)
		{
			patientMedicationData.ErrCode = ErrorCode.InvalidData;
			return patientMedicationData;
        }


    }
}
