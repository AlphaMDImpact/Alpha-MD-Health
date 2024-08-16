using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

/// <summary>
/// Reading ranges implementation of reading service
/// </summary>
public partial class ReadingService : BaseService
{
    /// <summary>
    /// Sync reading ranges and Reading details from service
    /// </summary>
    /// <param name="rangeData">reading range data reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reading detail and List of reading ranges data recieved from server</returns> 
    public async Task SyncProgramReadingRangesFromServerAsync(ReadingMasterDataDTO rangeData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_PROGRAM_READING_RANGES_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(ReadingRangeModel.ProgramReadingID), Convert.ToString(rangeData.ReadingRange.ProgramReadingID, CultureInfo.InvariantCulture) },
                    { nameof(ReadingRangeModel.ReadingRangeID), Convert.ToString(rangeData.ReadingRange.ReadingRangeID, CultureInfo.InvariantCulture) },
                    { nameof(BaseDTO.RecordCount), Convert.ToString(rangeData.RecordCount, CultureInfo.InvariantCulture) },
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            rangeData.ErrCode = httpData.ErrCode;
            if (rangeData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(rangeData, data);
                    SetPageResources(rangeData.Resources);
                    MapReadingRangesData(data, rangeData);
                }
            }
        }
        catch (Exception ex)
        {
            rangeData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync Organsisation Reading Range to server
    /// </summary>
    /// <param name="rangeData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status call</returns>
    public async Task SyncReadingRangeToServerAsync(ReadingMasterDataDTO rangeData, CancellationToken cancellationToken)
    {
        try
        {
            if (rangeData.ReadingRange != null)
            {
                var httpData = new HttpServiceModel<ReadingMasterDataDTO>
                {
                    CancellationToken = cancellationToken,
                    PathWithoutBasePath = UrlConstants.SAVE_PROGRAM_READING_RANGE_ASYNC_PATH,
                    QueryParameters = new NameValueCollection
                    {
                        { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() }
                    },
                    ContentToSend = rangeData,
                };
                await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
                rangeData.ErrCode = httpData.ErrCode;
                if (rangeData.ErrCode == ErrorCode.OK)
                {
                    JToken data = JToken.Parse(httpData.Response);
                    if (data != null && data.HasValues)
                    {
                        rangeData.ReadingRange.ReadingRangeID = (long)data[nameof(ReadingMasterDataDTO.ReadingRange)][nameof(ReadingRangeModel.ReadingRangeID)];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            rangeData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Maps gender and age string based on the values present in range data
    /// </summary>
    /// <param name="range">Range reference object to perform operation</param>
    public void MapRangeResourceData(ReadingRangeModel range)
    {
        range.Reading = LibResources.GetResourceValueByKeyID(PageData?.Resources, range.ReadingID);
        range.GenderString = LibResources.GetResourceValueByKeyID(PageData?.Resources, range.ForGender);
        range.AgeGroupString = range.ForAgeGroup == ResourceConstants.R_AGE_TYPE_AGE_RANGE_KEY_ID
            ? $"{range.FromAge}-{range.ToAge}"
            : LibResources.GetResourceValueByKeyID(PageData?.Resources, range.ForAgeGroup);
    }

    private void MapReadingRangesData(JToken data, ReadingMasterDataDTO rangeData)
    {
        rangeData.ReadingRanges = MapReadingRanges(data, nameof(ReadingMasterDataDTO.ReadingRanges));
        if (rangeData.RecordCount == -1)
        {
            if (GenericMethods.IsListNotEmpty(rangeData.ReadingRanges))
            {
                rangeData.ReadingRange = rangeData.ReadingRanges.FirstOrDefault();
                rangeData.ReadingRange.Reading = LibResources.GetResourceValueByKeyID(PageData?.Resources, rangeData.ReadingRange.ReadingID);
                rangeData.ReadingRanges.Clear();
            }
            rangeData.GenderOptions = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_GENDER_TYPE_GROUP, string.Empty, false, rangeData.ReadingRange?.ForGender ?? -1);
            rangeData.AgeOptions = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_AGE_TYPE_GROUP, string.Empty, false, rangeData.ReadingRange?.ForAgeGroup ?? -1);
        }
        else if (GenericMethods.IsListNotEmpty(rangeData.ReadingRanges))
        {
            rangeData.ReadingRanges.ForEach(range =>
            {
                MapRangeResourceData(range);
            });
        }
        rangeData.ErrCode = (ErrorCode)(int)data[nameof(ReadingMasterDataDTO.ErrCode)];
    }

    private List<ReadingRangeModel> MapReadingRanges(JToken data, string collectionName)
    {
        return data[collectionName]?.Count() > 0
            ? (from dataItem in data[collectionName]
               select new ReadingRangeModel
               {
                   ProgramID = GetDataItem<long>(dataItem, nameof(ReadingRangeModel.ProgramID)),
                   ProgramReadingID = GetDataItem<long>(dataItem, nameof(ReadingRangeModel.ProgramReadingID)),
                   ReadingRangeID = GetDataItem<long>(dataItem, nameof(ReadingRangeModel.ReadingRangeID)),
                   //PatientProgramID = GetDataItem<long>(dataItem, nameof(ReadingRangeModel.PatientProgramID)),
                   AbsoluteMinValue = GetDataItem<double>(dataItem, nameof(ReadingRangeModel.AbsoluteMinValue)),
                   AbsoluteMaxValue = GetDataItem<double>(dataItem, nameof(ReadingRangeModel.AbsoluteMaxValue)),
                   AbsoluteBandColor = GetDataItem<string>(dataItem, nameof(ReadingRangeModel.AbsoluteBandColor)),
                   NormalMinValue = GetDataItem<double>(dataItem, nameof(ReadingRangeModel.NormalMinValue)),
                   NormalMaxValue = GetDataItem<double>(dataItem, nameof(ReadingRangeModel.NormalMaxValue)),
                   NormalBandColor = GetDataItem<string>(dataItem, nameof(ReadingRangeModel.NormalBandColor)),
                   TargetBandColor = GetDataItem<string>(dataItem, nameof(ReadingRangeModel.TargetBandColor)),
                   ForGender = GetDataItem<short>(dataItem, nameof(ReadingRangeModel.ForGender)),
                   ForAgeGroup = GetDataItem<short>(dataItem, nameof(ReadingRangeModel.ForAgeGroup)),
                   FromAge = GetDataItem<short>(dataItem, nameof(ReadingRangeModel.FromAge)),
                   ToAge = GetDataItem<short>(dataItem, nameof(ReadingRangeModel.ToAge)),
                   GenderString = GetDataItem<string>(dataItem, nameof(ReadingRangeModel.GenderString)),
                   AgeGroupString = GetDataItem<string>(dataItem, nameof(ReadingRangeModel.AgeGroupString)),
                   Reading = GetDataItem<string>(dataItem, nameof(ReadingRangeModel.Reading)),
                   ReadingID = GetDataItem<short>(dataItem, nameof(ReadingRangeModel.ReadingID)),
                   UserID = GetDataItem<long>(dataItem, nameof(ReadingRangeModel.UserID)),
                   IsActive = GetDataItem<bool>(dataItem, nameof(ReadingRangeModel.IsActive))
               }).ToList()
            : new List<ReadingRangeModel>();
    }


    /// <summary>
    /// Gets metadata of all reading types if ReadingType is None else returns metadata of given ReadingType
    /// </summary>
    /// <param name="readingsData">Reference object to return metadata. If IsActive is true then only Dashboard enabled data will be fetched</param>
    /// <returns>List of metadata</returns>
    private async Task GetReadingRangesAsync(PatientReadingDTO readingsData, string readingIDs)
    {
        UserDTO userData = new UserDTO { User = new UserModel { UserID = readingsData.SelectedUserID, RoleID = (int)RoleName.Patient } };
        await Task.WhenAll(
            _readingDB.GetReadingRangesAsync(readingsData, readingIDs),
            new UserDatabase().GetUserAsync(userData)
        );
        if (GenericMethods.IsListNotEmpty(readingsData.ReadingRanges))
        {
            var ranges = readingsData.ReadingRanges;
            var currentUtcDateTime = GenericMethods.GetUtcDateTime;

            // checked if gender is not found use neutral gender
            // when found, fetch id of it from resource as users db is storing resource key for gender rathor than key id, thus need to fetch id to match
            short usersGenderID = string.IsNullOrWhiteSpace(userData.User?.GenderID)
                ? ResourceConstants.R_GENDER_TYPE_NEUTRAL_KEY_ID
                : Convert.ToInt16(LibResources.GetResourceKeyIDByKey(PageData?.Resources, userData.User.GenderID), CultureInfo.InvariantCulture);

            readingsData.ReadingRanges = new List<ReadingRangeModel>();
            foreach (var readingGroup in ranges.GroupBy(x => x.ReadingID))
            {
                // check if there is any ACTIVE range for users gender and age group
                if (!HasValidRange(readingsData, userData, currentUtcDateTime, usersGenderID, readingGroup.ToList().Where(x => x.IsActive)?.ToList())
                // in case of add edit, only active range should be allowed
                && readingsData.RecordCount != -1)
                {
                    // check if there is any INACTIVE range for users gender and age group to show as history data
                    HasValidRange(readingsData, userData, currentUtcDateTime, usersGenderID, readingGroup.ToList().Where(x => !x.IsActive)?.ToList());
                }
            }
        }
    }

    private bool HasValidRange(PatientReadingDTO readingsData, UserDTO userData, DateTimeOffset currentUtcDateTime, short usersGenderID, List<ReadingRangeModel> readingRanges)
    {
        if (GenericMethods.IsListNotEmpty(readingRanges))
        {
            List<ReadingRangeModel> filteredRanges = FilterReadingRangesBasedOnUsersGender(usersGenderID, readingRanges);
            if (GenericMethods.IsListNotEmpty(filteredRanges))
            {
                filteredRanges = FilterReadingRangesBasedOnUsersAge(currentUtcDateTime, userData.User?.Dob, filteredRanges);
                if (GenericMethods.IsListNotEmpty(filteredRanges))
                {
                    readingsData.ReadingRanges.Add(FetchRangeData(filteredRanges));
                    return true;
                }
            }
        }
        return false;
    }

    private ReadingRangeModel FetchRangeData(List<ReadingRangeModel> ranges)
    {
        ReadingRangeModel range = ranges.LastOrDefault();
        // fetch max and min value of ranges(Ex- Specially when user is part of multiple program for the same reading)
        range.AbsoluteMinValue = ranges.Min(x => x.AbsoluteMinValue);
        range.AbsoluteMaxValue = ranges.Max(x => x.AbsoluteMaxValue);
        range.NormalMinValue = ranges.Min(x => x.NormalMinValue);
        range.NormalMaxValue = ranges.Max(x => x.NormalMaxValue);
        return range;
    }

    private List<ReadingRangeModel> FilterReadingRangesBasedOnUsersGender(short usersGenderID, List<ReadingRangeModel> currentReadingRanges)
    {
        // filter ranges for other than neutral gender
        var filteredRanges = currentReadingRanges.Where(x =>
                x.ForGender != ResourceConstants.R_GENDER_TYPE_NEUTRAL_KEY_ID &&
                x.ForGender == usersGenderID
            )?.ToList();
        if (!GenericMethods.IsListNotEmpty(filteredRanges))
        {
            // filter ranges for neutral gender 
            filteredRanges = currentReadingRanges.Where(x => x.ForGender == ResourceConstants.R_GENDER_TYPE_NEUTRAL_KEY_ID)?.ToList();
        }
        return filteredRanges;
    }

    private List<ReadingRangeModel> FilterReadingRangesBasedOnUsersAge(DateTimeOffset utcDateTime, DateTimeOffset? dob, List<ReadingRangeModel> currentReadingRanges)
    {
        // filter ranges for other than neutral age
        var filteredRanges = dob.HasValue
            ? currentReadingRanges.Where(x =>
                    x.ForAgeGroup == ResourceConstants.R_AGE_TYPE_AGE_RANGE_KEY_ID &&
                    dob.Value <= utcDateTime.AddYears(-x.FromAge.Value) &&
                    dob.Value >= utcDateTime.AddYears(-x.ToAge.Value)
                )?.ToList()
            : null;
        if (!GenericMethods.IsListNotEmpty(filteredRanges))
        {
            // filter ranges for neutral age 
            filteredRanges = currentReadingRanges.Where(x => x.ForAgeGroup == ResourceConstants.R_AGE_TYPE_NEUTRAL_KEY_ID)?.ToList();
        }
        return filteredRanges;
    }
}