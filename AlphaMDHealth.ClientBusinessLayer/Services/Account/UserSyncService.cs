using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.CommonBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class UserService : BaseService
{
    /// <summary>
    /// Maps json object into Model and saves data into DB
    /// </summary>
    /// <param name="result">Object which holds Operation status</param>
    /// <param name="data">json object to fetch data from it</param>
    /// <returns>Operation status with record count</returns>
    internal async Task MapAndSaveUsersAsync(DataSyncModel result, JToken data)
    {
        try
        {
            UserDTO userData = new UserDTO
            {
                Users = MapUsers(data, nameof(DataSyncDTO.Users)),
                UserRelations = MapUserRelations(data, nameof(DataSyncDTO.UserRelations))
            };
            if (GenericMethods.IsListNotEmpty(userData.Users) || GenericMethods.IsListNotEmpty(userData.UserRelations))
            {
                await LoadDependancyAsync(userData).ConfigureAwait(false);
                await new UserDatabase().SaveUsersAsync(userData, true).ConfigureAwait(false);
                result.RecordCount = userData.Users?.Count ?? 0 + userData.UserRelations?.Count ?? 0;
            }
            if (MobileConstants.IsAndroidPlatform
                || _essentials.GetPreferenceValue<bool>(StorageConstants.PR_IS_NOTIFICATIONS_ALLOWED_KEY, true))
            {
                await RegisterDeviceAsync().ConfigureAwait(false);
            }
            result.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            result.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
        }
    }

    private async Task SyncUsersFromServerAsync(UserDTO userData, CancellationToken cancellationToken)
    {
        try
        {
            var userID = userData.ViewFor == AppPermissions.LinkedUsersView && userData.RecordCount == 0
                ? _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0)
                : userData.User.UserID;
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_USERS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { nameof(UserModel.UserID), userID.ToString(CultureInfo.InvariantCulture)},
                    { nameof(UserModel.SelectedOrganisationID), userData.User.SelectedOrganisationID.ToString(CultureInfo.InvariantCulture)},
                    { nameof(BaseDTO.RecordCount), userData.RecordCount.ToString(CultureInfo.InvariantCulture)},
                    { nameof(UserDTO.ViewFor), userData.ViewFor.ToString()}
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            userData.ErrCode = httpData.ErrCode;
            if (userData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(userData, data);
                    MapUsersData(data, userData);
                    if (userData.RecordCount == -1)
                    {
                        userData.SampleFilePath = GetDataItem<string>(data, nameof(UserDTO.SampleFilePath));
                    }
                    if (MobileConstants.IsMobilePlatform)
                    {
                        await MapAndSaveUsersImagesAsync(userData).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private async Task SyncToServerAsync(UserDTO requestData, CancellationToken cancellationToken)
    {
        var httpData = new HttpServiceModel<UserDTO>
        {
            CancellationToken = cancellationToken,
            PathWithoutBasePath = requestData.User.IsActive
                ? UrlConstants.SAVE_USER_ASYNC_PATH
                : UrlConstants.DELETE_USER_ASYNC_PATH,
            ContentToSend = requestData,
            QueryParameters = new NameValueCollection
            {
                { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
            }
        };
        await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
        requestData.ErrCode = httpData.ErrCode;
        requestData.Response = httpData.Response;
    }

    /// <summary>
    /// Sync users to server
    /// </summary>
    /// <param name="requestData">object reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>operation status</returns>
    public async Task SyncBulkUserToServerAsync(UserDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<UserDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_USERS_FROM_EXCEL_ASYNC_PATH,
                ContentToSend = requestData,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                }
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
            requestData.Response = httpData.Response;
            JToken data = JToken.Parse(httpData.Response);
            if (data != null && data.HasValues)
            {
                MapAttachment(data, requestData);
            }
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Resend activation link data to server
    /// </summary>
    /// <param name="requestData">object reference user data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>operation status</returns>
    public async Task ResendActivationAsync(UserDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<UserDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.RESND_ACTIVATION_ASYNC_PATH,
                ContentToSend = requestData,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                }
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            requestData.ErrCode = httpData.ErrCode;
            requestData.Response = httpData.Response;
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Register user 
    /// </summary>
    /// <param name="userData">object having user data to save</param>
    /// <returns>Operation status code and token data</returns>
    public async Task RegisterUserAsync(UserDTO userData)
    {
        try
        {
            var httpData = new HttpServiceModel<UserDTO>
            {
                AuthType = AuthorizationType.Basic,
                PathWithoutBasePath = UrlConstants.REGISTER_USER_ASYNC_PATH,
                ContentToSend = userData,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            userData.ErrCode = httpData.ErrCode;
            userData.Response = httpData.Response;
            if (userData.ErrCode == ErrorCode.SetPinCode || userData.ErrCode == ErrorCode.OrganisationSetup)
            {
                var sessionData = MapSessionData(userData);
                await SaveTokensAsync(sessionData).ConfigureAwait(false);
                _essentials.SetPreferenceValue(StorageConstants.PR_ACCOUNT_ID_KEY, sessionData.AccountID);
            }
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync patient caregiver(s) from service
    /// </summary>
    /// <param name="caregiverData">caregivers reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Caregiver data recieved from server</returns>
    private async Task SyncPatientCaregiversFromServerAsync(CaregiverDTO caregiverData, CancellationToken cancellationToken)
    {
        try
        {
            var httpData = new HttpServiceModel<List<KeyValuePair<string, string>>>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.GET_CAREGIVERS_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                    { Constants.SE_SELECTED_USER_ID_QUERY_KEY, Convert.ToString(_essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0), CultureInfo.InvariantCulture) },
                    { Constants.SE_PATIENT_CAREGIVER_ID_QUERY_KEY, Convert.ToString(caregiverData.Caregiver?.PatientCareGiverID, CultureInfo.InvariantCulture) },
                    { Constants.SE_RECORD_COUNT_QUERY_KEY, Convert.ToString(caregiverData.RecordCount, CultureInfo.InvariantCulture) }
                }
            };
            await new HttpLibService(HttpService, _essentials).GetAsync(httpData).ConfigureAwait(false);
            caregiverData.ErrCode = httpData.ErrCode;
            if (caregiverData.ErrCode == ErrorCode.OK)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data != null && data.HasValues)
                {
                    MapCommonData(caregiverData, data);
                    MapCaregiverData(data, caregiverData);
                }
            }
        }
        catch (Exception ex)
        {
            caregiverData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync patient caregiver data to server
    /// </summary>
    /// <param name="caregiverData">object to return operation status</param>
    /// <param name="cancellationToken">object to cancel service call</param>
    /// <returns>Operation status</returns>
    public async Task SyncPatientCaregiverToServerAsync(CaregiverDTO caregiverData, CancellationToken cancellationToken)
    {
        try
        {
            if (caregiverData.Caregiver != null)
            {
                caregiverData.Caregiver.LastModifiedON = GenericMethods.GetUtcDateTime;
            }
            caregiverData.SelectedUserID = caregiverData.SelectedUserID == 0 ? _essentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0) : caregiverData.SelectedUserID;
            var httpData = new HttpServiceModel<CaregiverDTO>
            {
                CancellationToken = cancellationToken,
                PathWithoutBasePath = UrlConstants.SAVE_CAREGIVER_ASYNC_PATH,
                QueryParameters = new NameValueCollection
                {
                    { Constants.SE_PERMISSION_AT_LEVEL_ID_QUERY_KEY, GetPermissionAtLevelID() },
                },
                ContentToSend = caregiverData,
            };
            await new HttpLibService(HttpService, _essentials).PostAsync(httpData).ConfigureAwait(false);
            caregiverData.ErrCode = httpData.ErrCode;
            caregiverData.Response = httpData.Response;
            if (httpData.ErrCode == ErrorCode.OK && MobileConstants.IsMobilePlatform)
            {
                JToken data = JToken.Parse(httpData.Response);
                if (data?.HasValues == true)
                {
                    caregiverData.Caregiver.PatientCareGiverID = (long)data[nameof(CaregiverDTO.Caregiver)][nameof(CaregiverModel.PatientCareGiverID)];
                }
                UserDTO usersDTO = new UserDTO
                {
                    UserRelation = new UserRelationModel
                    {
                        CareGiverID = caregiverData.Caregiver.CareGiverID,
                        PatientCareGiverID = caregiverData.Caregiver.PatientCareGiverID,
                        PatientID = caregiverData.SelectedUserID,
                        FromDate = caregiverData.Caregiver.FromDate.Value,
                        ToDate = caregiverData.Caregiver.ToDate.Value,
                        IsActive = caregiverData.IsActive,
                    }
                };
                await new UserDatabase().SaveCaregiversAsync(usersDTO).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            caregiverData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Map program caregiver data
    /// </summary>
    /// <param name="dataItem">program caregiver json object</param>
    /// <returns>Program caregiver data</returns>
    internal CaregiverModel MapProgramCaregiver(JToken dataItem)
    {
        return new CaregiverModel
        {
            ProgramCareGiverID = (long)dataItem[nameof(CaregiverModel.ProgramCareGiverID)],
            CareGiverID = (long)dataItem[nameof(CaregiverModel.CareGiverID)],
            ProgramID = GetDataItem<long>(dataItem, nameof(CaregiverModel.ProgramID)),
            OrganisationID = GetDataItem<long>(dataItem, nameof(CaregiverModel.OrganisationID)),
            AssignAfterDays = GetDataItem<short>(dataItem, nameof(CaregiverModel.AssignAfterDays)),
            AssignForDays = GetDataItem<short>(dataItem, nameof(CaregiverModel.AssignForDays)),
            FullName = GetDataItem<string>(dataItem, nameof(CaregiverModel.FullName)),
            FirstName = (string)dataItem[nameof(CaregiverModel.FirstName)],
            LastName = GetDataItem<string>(dataItem, nameof(CaregiverModel.LastName)),
            RoleName = GetDataItem<string>(dataItem, nameof(CaregiverModel.RoleName)),
            FromDate = GetDataItem<DateTimeOffset>(dataItem, nameof(CaregiverModel.FromDate)),
            ToDate = GetDataItem<DateTimeOffset>(dataItem, nameof(CaregiverModel.ToDate)),
            IsActive = GetDataItem<bool>(dataItem, nameof(CaregiverModel.IsActive)),
        };
    }

    private UserRelationModel MapUserRelation(JToken dataItem)
    {
        return new UserRelationModel
        {
            UserID = GetDataItem<long>(dataItem, nameof(UserRelationModel.UserID)),
            CareGiverID = GetDataItem<long>(dataItem, nameof(UserRelationModel.CareGiverID)),
            PatientID = GetDataItem<long>(dataItem, nameof(UserRelationModel.PatientID)),
            PatientCareGiverID = GetDataItem<long>(dataItem, nameof(UserRelationModel.PatientCareGiverID)),
            ProgramID = GetDataItem<long>(dataItem, nameof(UserRelationModel.ProgramID)),
            FromDate = GetDataItem<DateTimeOffset>(dataItem, nameof(UserRelationModel.FromDate)),
            ToDate = GetDataItem<DateTimeOffset>(dataItem, nameof(UserRelationModel.ToDate)),
            RelationID = GetDataItem<short>(dataItem, nameof(UserRelationModel.RelationID)),
            LastModifiedON = GetDataItem<DateTimeOffset>(dataItem, nameof(UserRelationModel.LastModifiedON)),
            IsActive = GetDataItem<bool>(dataItem, nameof(UserRelationModel.IsActive)),
            IsSynced = true,
        };
    }

    private CaregiverModel MapCaregiver(JToken dataItem, bool isList)
    {
        var fullName = $"{GetDataItem<string>(dataItem, nameof(CaregiverModel.FirstName))} {GetDataItem<string>(dataItem, nameof(CaregiverModel.LastName))}";
        return new CaregiverModel
        {
            PatientCareGiverID = GetDataItem<long>(dataItem, nameof(CaregiverModel.PatientCareGiverID)),
            OrganisationID = GetDataItem<long>(dataItem, nameof(CaregiverModel.OrganisationID)),
            ProgramCareGiverID = GetDataItem<long>(dataItem, nameof(CaregiverModel.ProgramCareGiverID)),
            CareGiverID = GetDataItem<long>(dataItem, nameof(CaregiverModel.CareGiverID)),
            FirstName = isList
                ? fullName
                : GetDataItem<string>(dataItem, nameof(CaregiverModel.FirstName)),
            LastName = isList
                ? string.Empty
                : GetDataItem<string>(dataItem, nameof(CaregiverModel.LastName)),
            Initials = isList
                ? GetInitials(fullName)
                : string.Empty,
            ImageName = GetDataItem<string>(dataItem, nameof(CaregiverModel.ImageName)),
            Department = GetDataItem<string>(dataItem, nameof(CaregiverModel.Department)),
            FromDate = GetDataItem<DateTimeOffset?>(dataItem, nameof(CaregiverModel.FromDate)),
            ToDate = GetDataItem<DateTimeOffset?>(dataItem, nameof(CaregiverModel.ToDate)),
            ProgramName = GetDataItem<string>(dataItem, nameof(CaregiverModel.ProgramName)),
            ProgramColor = GetDataItem<string>(dataItem, nameof(CaregiverModel.ProgramColor)),
        };
    }

    internal List<UserModel> MapUsers(JToken data, string collactionName)
    {
        return (data[collactionName]?.Count() > 0)
            ? (from dataItem in data[collactionName]
               select new UserModel
               {
                   UserID = GetDataItem<long>(dataItem, nameof(UserModel.UserID)),
                   AccountID = GetDataItem<long>(dataItem, nameof(UserModel.AccountID)),
                   FirstName = (string)dataItem[nameof(UserModel.FirstName)],
                   MiddleName = (string)dataItem[nameof(UserModel.MiddleName)],
                   LastName = (string)dataItem[nameof(UserModel.LastName)],
                   IsActive = GetDataItem<bool>(dataItem, nameof(UserModel.IsActive)),
                   ImageName = Convert.ToString(dataItem[nameof(UserModel.ImageName)], CultureInfo.InvariantCulture),
                   GenderID = (string)dataItem[nameof(UserModel.GenderID)],
                   UserDegrees = (string)dataItem[nameof(UserModel.UserDegrees)],
                   EmailId = (string)dataItem[nameof(UserModel.EmailId)],
                   PhoneNo = (string)dataItem[nameof(UserModel.PhoneNo)],
                   IsTempPassword = GetDataItem<bool>(dataItem, nameof(UserModel.IsTempPassword)),
                   OrganisationID = GetDataItem<long>(dataItem, nameof(UserModel.OrganisationID)),
                   RoleAtLevelID = GetDataItem<long>(dataItem, nameof(UserModel.RoleAtLevelID)),
                   RoleID = GetDataItem<byte>(dataItem, nameof(UserModel.RoleID)),
                   DepartmentID = GetDataItem<long>(dataItem, nameof(UserModel.DepartmentID)),
                   BranchID = GetDataItem<long>(dataItem, nameof(UserModel.BranchID)),
                   HospitalIdenfier = (string)dataItem[nameof(UserModel.HospitalIdenfier)],
                   GeneralMedicalIdenfier = (string)dataItem[nameof(UserModel.GeneralMedicalIdenfier)],
                   SocialSecurityNo = (string)dataItem[nameof(UserModel.SocialSecurityNo)],
                   PrefferedLanguageID = GetDataItem<byte>(dataItem, nameof(UserModel.PrefferedLanguageID)),
                   ProffessionID = GetDataItem<byte>(dataItem, nameof(UserModel.ProffessionID)),
                   Proffession = GetDataItem<string>(dataItem, nameof(UserModel.Proffession)),
                   BloodGroupID = GetDataItem<short>(dataItem, nameof(UserModel.BloodGroupID)),
                   RoleName = GetDataItem<string>(dataItem, nameof(UserModel.RoleName)),
                   Dob = GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.Dob)),
                   Doj = GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.Doj)),
                   FromDate = GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.FromDate)),
                   ToDate = GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.ToDate)),
                   PatientID = GetDataItem<long>(dataItem, nameof(UserModel.PatientID)),
                   UserAge = GetAge(GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.Dob))),
                   Height = GetDataItem<short>(dataItem, nameof(UserModel.Height)),
                   Weight = GetDataItem<double>(dataItem, nameof(UserModel.Weight)),
                   IsUser = GetDataItem<bool>(dataItem, nameof(UserModel.IsUser)),//todo: to remove
                   LastModifiedON = GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.LastModifiedON)),
                   AddedON = GetDataItem<DateTimeOffset>(dataItem, nameof(UserModel.AddedON)),
                   IsSynced = true
               }).ToList()
            : null;
    }

    private async Task MapAndSaveUsersImagesAsync(UserDTO userData)
    {
        await ConvertCdnLinksToBase64Async(userData);
        using (UserDatabase userDatabase = new UserDatabase())
        {
            await userDatabase.SaveUsersAsync(userData, false).ConfigureAwait(false);
        }
    }

    private async Task ConvertCdnLinksToBase64Async(UserDTO userData)
    {
        foreach (UserModel user in userData.Users)
        {
            user.ImageName = await GetImageAsBase64Async(user.ImageName).ConfigureAwait(true);
        }
    }

    private async Task LoadDependancyAsync(UserDTO userData)
    {
        if (GenericMethods.IsListNotEmpty(userData.Users))
        {
            long accountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
            var accountUser = userData.Users.FirstOrDefault(x => x.AccountID == accountID);
            if (accountUser != null)
            {
                _essentials.SetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, accountUser.RoleID);
            }
            await ConvertCdnLinksToBase64Async(userData);
            //foreach (UserModel user in userData.Users)
            //{
            //    user.ImageBytes = await GetImageAsByteArrayAsync(user.ImageName).ConfigureAwait(true);
            //    user.ImageName = ResetCdnLink(user.ImageBytes, user.ImageName);
            //    //user.ImageBase64 = await GetImageAsBase64Async(user.ImageBase64).ConfigureAwait(false);
            //}
        }
    }
}
