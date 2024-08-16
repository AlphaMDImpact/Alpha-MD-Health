using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceDataLayer;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class TrackerServiceBusinessLayer : BaseServiceBusinessLayer
	{
		/// <summary>
		/// Tracker service
		/// </summary>
		/// <param name="httpContext">Instance of HttpContext</param>
		public TrackerServiceBusinessLayer(HttpContext httpContext) : base(httpContext)
		{
		}

		/// <summary>
		/// Get Trackers assigned to patient
		/// </summary>
		/// <param name="languageID">Selected language id</param>
		/// <param name="permissionAtLevelID">Level at which permission is required</param>
		/// <param name="selectedUserID">Currently selected user id</param>
		/// <param name="patientTrackerID">Patient tracker id</param>
		/// <param name="recordCount">Record Count</param>
		/// <param name="trackerName">Name of tracker</param>
		/// <param name="fromDate">Start date from where data needs to fetch</param>
		/// <param name="toDate">End date, till data needs to fetched</param>
		/// <returns>Patient Tracker Data</returns>
		public async Task<TrackerDTO> GetPatientTrackersAsync(byte languageID, long permissionAtLevelID, long organisationID, long selectedUserID, Guid patientTrackerID, long recordCount, string trackerName, string fromDate, string toDate)
		{
			TrackerDTO trackerData = new TrackerDTO();
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1 || selectedUserID < 0)
				{
					trackerData.ErrCode = ErrorCode.InvalidData;
					return trackerData;
				}
				if (AccountID < 1)
				{
					trackerData.ErrCode = ErrorCode.Unauthorized;
					return trackerData;
				}
				if (await GetTrackersResourceAndSettingsAsync(trackerData, organisationID, languageID))
				{
					trackerData.PermissionAtLevelID = permissionAtLevelID;
					trackerData.RecordCount = recordCount;
					trackerData.SelectedUserID = selectedUserID;
					trackerData.FromDate = fromDate;
					trackerData.ToDate = toDate;
					trackerData.PatientTracker = new PatientTrackersModel
					{
						PatientTrackerID = patientTrackerID,
						TrackerName = trackerName
					};
					trackerData.FeatureFor = FeatureFor;
					await new TrackerServiceDataLayer().GetPatientTrackersAsync(trackerData).ConfigureAwait(false);
					await MapTrackersDependancyAsync(trackerData);
				}
			}
			catch (Exception ex)
			{
				trackerData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
				LogError(ex.Message, ex);
			}
			return trackerData;
		}

		internal async Task MapTrackersDependancyAsync(TrackerDTO trackerData)
		{
			if (trackerData.ErrCode == ErrorCode.OK)
			{
				await ReplaceTrackerImageCdnLinkAsync(trackerData.PatientTrackers);
				await ReplaceTrackerImageCdnLinkAsync(trackerData.PatientTracker);
				await ReplaceTrackerImageCdnLinkAsync(trackerData.TrackerRanges);
				await ReplaceTrackerRangeDetailsImageCdnLinkAsync(trackerData.TrackerRangesI18N);
			}
		}

		internal async Task<bool> GetTrackersResourceAndSettingsAsync(TrackerDTO trackerData, long organisationID, byte languageID)
		{
			trackerData.AccountID = AccountID;
			trackerData.OrganisationID = organisationID;
			trackerData.LanguageID = languageID;
			if (await GetSettingsResourcesAsync(trackerData, true, GroupConstants.RS_COMMON_GROUP,
				$"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_TRACKERS_GROUP},{GroupConstants.RS_PROGRAMS_GROUP}"
			).ConfigureAwait(false))
			{
				trackerData.ErrCode = ErrorCode.OK;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Save Tracker Data
		/// </summary>
		/// <param name="languageID">Selected Language ID</param>
		/// <param name="permissionAtLevelID">Level at which permission is required</param> 
		/// <param name="selectedUserID">Currently selected user id</param>
		/// <param name="trackerData">Reference object which holds Patient Data</param>
		/// <returns>Operation Status Code</returns>
		public async Task<TrackerDTO> SavePatientTrackerAsync(byte languageID, long permissionAtLevelID, long selectedUserID, TrackerDTO trackerData)
		{
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1 || AccountID < 1 
                    || trackerData.PatientTracker == null
					|| (trackerData.PatientTracker.PatientTrackerID == Guid.Empty && trackerData.PatientTracker.TrackerID < 1))
				{
					trackerData.ErrCode = ErrorCode.InvalidData;
					return trackerData;
				}
				if (trackerData.IsActive)
				{
                    trackerData.LanguageID = 1;
					if (await GetSettingsResourcesAsync(trackerData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_TRACKERS_GROUP}").ConfigureAwait(false))
					{
						if (!await ValidateDataAsync(trackerData.PatientTracker, trackerData.Resources))
						{
							trackerData.ErrCode = ErrorCode.InvalidData;
							return trackerData;
						}
					}
					else
					{
						return trackerData;
					}
					trackerData.SelectedUserID = selectedUserID;
					trackerData.AccountID = AccountID;
					trackerData.LanguageID = languageID;
					trackerData.PermissionAtLevelID = permissionAtLevelID;
					trackerData.FeatureFor = FeatureFor;
					await new TrackerServiceDataLayer().SavePatientTrackerAsync(trackerData).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				trackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return trackerData;
		}

		/// <summary>
		/// Save Patient Tracker Value
		/// </summary>
		/// <param name="languageID">Selected Language ID</param>
		/// <param name="permissionAtLevelID">Level at which permission is required</param> 
		/// <param name="selectedUserID">Currently selected user id</param>
		/// <param name="trackerData">Reference object which holds Patient Data</param>
		/// <returns>Operation Status Code</returns>
		public async Task<TrackerDTO> SavePatientTrackerValueAsync(byte languageID, long permissionAtLevelID, long selectedUserID, TrackerDTO trackerData)
		{
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1
					|| trackerData.PatientTrackerValue == null
					|| (trackerData.PatientTrackerValue.PatientTrackerID == Guid.Empty && string.IsNullOrEmpty(trackerData.PatientTrackerValue.CurrentValue)))
				{
					trackerData.ErrCode = ErrorCode.InvalidData;
					return trackerData;
				}
				if (AccountID < 1)
				{
					trackerData.ErrCode = ErrorCode.Unauthorized;
					return trackerData;
				}
				trackerData.SelectedUserID = selectedUserID;
				trackerData.AccountID = AccountID;
				trackerData.LanguageID = languageID;
				trackerData.PermissionAtLevelID = permissionAtLevelID;
				trackerData.FeatureFor = FeatureFor;
				await new TrackerServiceDataLayer().SavePatientTrackerValueAsync(trackerData).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				trackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return trackerData;
		}


		/// <summary>
		/// Get Trackers
		/// </summary>
		/// <param name="languageID">Language Id</param>
		/// <param name="permissionAtLevelID">Level at which permission is required</param>
		/// <param name="recordCount">Record Count</param>
		/// <param name="trackerID">Tracker Id</param>
		/// <param name="trackerRangeID">Tracker Range ID</param>
		/// <returns>Tracker Data and operation status</returns>
		public async Task<TrackerDTO> GetTrackersAsync(byte languageID, long permissionAtLevelID, long recordCount, short trackerID, short trackerRangeID)
		{
			TrackerDTO trackerData = new TrackerDTO();
			try
			{
				if (permissionAtLevelID < 1 || languageID < 1)
				{
					trackerData.ErrCode = ErrorCode.InvalidData;
					return trackerData;
				}
				trackerData.AccountID = AccountID;
				if (trackerData.AccountID < 1)
				{
					trackerData.ErrCode = ErrorCode.Unauthorized;
					return trackerData;
				}
				trackerData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
				if (await GetPageDataAsync(languageID, trackerData).ConfigureAwait(false))
				{
					trackerData.AccountID = AccountID;
					trackerData.LanguageID = languageID;
					trackerData.PermissionAtLevelID = permissionAtLevelID;
					trackerData.RecordCount = recordCount;
					trackerData.Tracker = new TrackersModel { TrackerID = trackerID };
					trackerData.TrackerRange = new TrackerRangeModel { TrackerID = trackerID, TrackerRangeID = trackerRangeID };
					trackerData.FeatureFor = FeatureFor;
					await new TrackerServiceDataLayer().GetTrackersAsync(trackerData).ConfigureAwait(false);
					if (trackerData.ErrCode == ErrorCode.OK && recordCount == -2 && trackerRangeID > 0)
					{
						//if DB retrieve is successful and operation is ranges Edit then only do this operation
						await ReplaceTrackerRangeImageCdnLinkAsync(trackerData?.TrackerRange);
						await ReplaceTrackerRangeDetailsImageCdnLinkAsync(trackerData.TrackerRangesI18N);
					}
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				trackerData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
			}
			return trackerData;
		}

		/// <summary>
		/// Save Tracker Data
		/// </summary>
		/// <param name="permissionAtLevelID">Level at which permission is required</param>
		/// <param name="trackerData">Reference object which holds Data</param>
		/// <returns>Operation Status Code</returns>
		public async Task<TrackerDTO> SaveTrackerAsync(long permissionAtLevelID, TrackerDTO trackerData)
		{
			try
			{
				if (permissionAtLevelID < 1 || AccountID < 1 || trackerData.Tracker == null)
				{
					trackerData.ErrCode = ErrorCode.InvalidData;
					return trackerData;
				}
                if (trackerData.IsActive)
                {
                    if (!GenericMethods.IsListNotEmpty(trackerData.TrackersI18N))
                    {
                        trackerData.ErrCode = ErrorCode.InvalidData;
                        return trackerData;
                    }
                    trackerData.LanguageID = 1;
                    if (await GetSettingsResourcesAsync(trackerData, false, string.Empty, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_TRACKERS_GROUP}").ConfigureAwait(false))
                    {
                        if (!await ValidateDataAsync(trackerData.TrackersI18N, trackerData.Resources))
                        {
                            trackerData.ErrCode = ErrorCode.InvalidData;
                            return trackerData;
                        }
                    }
                    else
                    {
                        return trackerData;
                    }
                }
                trackerData.AccountID = AccountID;
				trackerData.PermissionAtLevelID = permissionAtLevelID;
				trackerData.FeatureFor = FeatureFor;
				await new TrackerServiceDataLayer().SaveTrackerAsync(trackerData).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				trackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return trackerData;
		}

		/// <summary>
		/// Save Tracker Ranges Data
		/// </summary>
		/// <param name="trackerData">Reference object which holds Data</param>
		/// <param name="permissionAtLevelID">Level at which permission is required</param>
		/// <returns>Operation Status Code</returns>
		public async Task<TrackerDTO> SaveTrackerRangesAsync(TrackerDTO trackerData, long permissionAtLevelID)
		{
			try
			{
				if (permissionAtLevelID < 1 || trackerData?.TrackerRange?.TrackerID < 1)
				{
					trackerData.ErrCode = ErrorCode.InvalidData;
					return trackerData;
				}
				if (AccountID < 1)
				{
					trackerData.ErrCode = ErrorCode.Unauthorized;
					return trackerData;
				}
				trackerData.AccountID = AccountID;
				trackerData.PermissionAtLevelID = permissionAtLevelID;
				trackerData.FeatureFor = FeatureFor;
				//To handle first insert and delete operation
				if (trackerData.TrackerRange.TrackerRangeID == 0 || !trackerData.TrackerRange.IsActive)
				{
					await new TrackerServiceDataLayer().SaveTrackerRangesAsync(trackerData).ConfigureAwait(false);
				}
				//To handle sucessfull insert or update operation
				if (trackerData.ErrCode == ErrorCode.OK && trackerData.TrackerRange.IsActive)
				{
					await UploadTrackerImagesAsync(trackerData).ConfigureAwait(false);
					if (trackerData.ErrCode == ErrorCode.OK)
					{
						await new TrackerServiceDataLayer().SaveTrackerRangesAsync(trackerData).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message, ex);
				trackerData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
			}
			return trackerData;
		}

		private async Task<bool> GetPageDataAsync(byte languageID, TrackerDTO trackerData)
		{
			trackerData.ErrCode = ErrorCode.ErrorWhileDeletingRecords;
			trackerData.Settings = (await GetDataFromCacheAsync(CachedDataType.Settings, GroupConstants.RS_COMMON_GROUP, languageID,
				default, 0, 0, false).ConfigureAwait(false)).Settings;
			trackerData.Resources = (await GetDataFromCacheAsync(CachedDataType.Resources, $"{GroupConstants.RS_COMMON_GROUP},{GroupConstants.RS_TRACKERS_GROUP}," +
				$"{GroupConstants.RS_QUESTIONNAIRE_QUESTION_TYPE_GROUP},{GroupConstants.RS_ORGANISATION_SETTINGS_GROUP},{GroupConstants.RS_QUESTIONNAIRE_PAGE_GROUP}," +
				$"{GroupConstants.RS_MENU_PAGE_GROUP}",
				languageID, default, 0, 0, false).ConfigureAwait(false))?.Resources;
			if (GenericMethods.IsListNotEmpty(trackerData.Resources))
			{
				return true;
			}
			return false;
		}

		internal async Task ReplaceTrackerRangeImageCdnLinkAsync(TrackerRangeModel trackerRange)
		{
			BaseDTO cdnCacheData = new BaseDTO();
			if (!string.IsNullOrWhiteSpace(trackerRange?.ImageName))
			{
				trackerRange.ImageName = await ReplaceCDNLinkAsync(trackerRange.ImageName, cdnCacheData);
			}
		}

		internal async Task ReplaceTrackerImageCdnLinkAsync(List<PatientTrackersModel> patientTrackers)
		{
			BaseDTO cdnCacheData = new BaseDTO();
			if (GenericMethods.IsListNotEmpty(patientTrackers))
			{
				foreach (var detail in patientTrackers)
				{
					if (!string.IsNullOrWhiteSpace(detail?.ImageName))
					{
						detail.ImageName = await ReplaceCDNLinkAsync(detail.ImageName, cdnCacheData);
					}
				}
			}
		}

		internal async Task ReplaceTrackerImageCdnLinkAsync(PatientTrackersModel detail)
		{
			BaseDTO cdnCacheData = new BaseDTO();
			if (!string.IsNullOrWhiteSpace(detail?.ImageName))
			{
				detail.ImageName = await ReplaceCDNLinkAsync(detail.ImageName, cdnCacheData);
			}
			if (!string.IsNullOrWhiteSpace(detail?.InstructionsText))
			{
				detail.InstructionsText = await ReplaceCDNLinkAsync(detail.InstructionsText, cdnCacheData);
			}
		}

		internal async Task ReplaceTrackerImageCdnLinkAsync(List<TrackerRangeModel> trackerRanges)
		{
			BaseDTO cdnCacheData = new BaseDTO();
			if (GenericMethods.IsListNotEmpty(trackerRanges))
			{
				foreach (var detail in trackerRanges)
				{
					if (!string.IsNullOrWhiteSpace(detail?.ImageName))
					{
						detail.ImageName = await ReplaceCDNLinkAsync(detail.ImageName, cdnCacheData);
					}
				}
			}
		}

		internal async Task ReplaceTrackerRangeDetailsImageCdnLinkAsync(List<TrackerRangesI18N> trackerRangeDetails)
		{
			BaseDTO cdnCacheData = new BaseDTO();
			if (GenericMethods.IsListNotEmpty(trackerRangeDetails))
			{
				foreach (var detail in trackerRangeDetails)
				{
					if (!string.IsNullOrWhiteSpace(detail.InstructionsText))
					{
						detail.InstructionsText = await ReplaceCDNLinkAsync(detail.InstructionsText, cdnCacheData);
					}
				}
			}
		}

		private async Task UploadTrackerImagesAsync(TrackerDTO trackerData)
		{
			FileUploadDTO files = CreateSingleFileDataObject(FileTypeToUpload.TrackerImages,
			   trackerData.TrackerRange.TrackerRangeID.ToString(CultureInfo.InvariantCulture),
			   trackerData.TrackerRange.ImageName);
			files.FileContainers[0].FileData.AddRange(from detail in trackerData.TrackerRangesI18N
													  select new FileDataModel
													  {
														  HasMultiple = true,
														  Base64File = detail.InstructionsText,
														  RecordID = $"{detail.LanguageID}_{nameof(detail.InstructionsText)}",
													  });
			files = await UploadDocumentsToBlobAsync(files).ConfigureAwait(false);
			trackerData.ErrCode = files.ErrCode;
			if (trackerData.ErrCode == ErrorCode.OK)
			{
				trackerData.TrackerRange.ImageName = GetBase64FileFromFirstContainer(files, trackerData.TrackerRange.TrackerRangeID.ToString(CultureInfo.InvariantCulture));
				foreach (var detail in trackerData.TrackerRangesI18N)
				{
					detail.InstructionsText = GetBase64FileFromFirstContainer(files, $"{detail.LanguageID}_{nameof(detail.InstructionsText)}");
				}
			}
		}
	}
}