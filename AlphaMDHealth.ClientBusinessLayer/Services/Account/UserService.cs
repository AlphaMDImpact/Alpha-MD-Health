using AlphaMDHealth.ClientDataLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace AlphaMDHealth.ClientBusinessLayer;

public partial class UserService : BaseService
{
    public UserService(IEssentials serviceEssentials = null) : base(serviceEssentials)
    {
    }

    /// <summary>
    /// Get users data
    /// </summary>
    /// <param name="userData">Reference object for returning users</param>
    /// <returns>users in reference object</returns>
    public async Task GetUsersAsync(UserDTO userData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                userData.SelectedUserID = GetLoginUserID();
                await Task.WhenAll(
                    new UserDatabase().GetUsersAsync(userData),
                    GetResourcesAsync(GroupConstants.RS_GENDER_TYPE_GROUP)
                ).ConfigureAwait(false);
                userData.ErrCode = ErrorCode.OK;
            }
            else
            {
                await SyncUsersFromServerAsync(userData, CancellationToken.None).ConfigureAwait(false);
            }

            //Map UI Data
            if (userData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(userData.Users))
            {
                foreach (var user in userData.Users)
                {
                    user.Gender = LibResources.GetResourceValueByKey(PageData?.Resources, user.GenderID);
                    user.FullName = string.Join(Constants.CHAR_SPACE, user.FirstName, user.MiddleName, user.LastName);
                    if (string.IsNullOrWhiteSpace(user.ImageName))
                    {
                        user.ImageName = GetInitials(user.FullName);
                    }

                    if (userData.ViewFor != AppPermissions.UsersView)
                    {
                        CalculateUserAge(user);
                        user.BloodGroup = LibResources.GetResourceValueByKeyID(PageData?.Resources, user.BloodGroupID);
                    }
                    //if (MobileConstants.IsMobilePlatform)
                    //{
                    //    MapUserUIData(user);
                    //}
                }
            }
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sync users to server
    /// </summary>
    /// <param name="requestData">object reference to return output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>operation status</returns>
    public async Task SyncUserToServerAsync(UserDTO requestData, CancellationToken cancellationToken)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                await new UserDatabase().GetUsersForSyncToServerAsync(requestData).ConfigureAwait(true);
                if (!GenericMethods.IsListNotEmpty(requestData.Users))
                {
                    return;
                }
                foreach (var user in requestData.Users)
                {
                    requestData.User = user;
                    requestData.CreatedByID = user.UserTempID;
                    requestData.User.IsActive = requestData.User.IsLinkedUser ? user.IsActive : true;
                    //todo: requestData.IsProfile = requestData.IsActive = user.RoleID == (int)RoleName.CareTaker ? true : false; //is used for permission check 
                    await SyncToServerAsync(requestData, cancellationToken).ConfigureAwait(false);
                    if (requestData.ErrCode == ErrorCode.OK || requestData.ErrCode == ErrorCode.DuplicateUser)
                    {
                        JToken data = JToken.Parse(requestData.Response);
                        requestData.User.UserID = (long)data[nameof(UserDTO.User)][nameof(UserModel.UserID)];
                        if (data?.HasValues == true)
                        {
                            await new UserDatabase().UpdateUserIDAsync(requestData);
                        }
                    }
                }
            }
            else
            {
                await SyncToServerAsync(requestData, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            requestData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// save Shared Profile Data 
    /// </summary>
    /// <param name="userData"></param>
    /// <returns></returns>
    public async Task SaveShareProfileDataAsync(UserDTO userData, CancellationToken cancellationToken)
    {
        try
        {
            await SyncToServerAsync(userData, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Save Users from View to Database
    /// </summary>
    /// <param name="userData">Object containing User Data</param>
    /// <returns>Operation Status</returns>
    public async Task SaveUsersAsync(UserDTO userData)
    {
        try
        {
            //is useed for adding provider id in caregiver table
            userData.CreatedByID = GetLoginUserID();
            if (userData.User.IsLinkedUser)
            {
                userData.SelectedUserID = GetUserID();
                await new UserDatabase().SaveLinkedUserAsync(userData).ConfigureAwait(false);
            }
            else if (userData.User.IsMobile)
            {
                await new UserDatabase().InsertUsersAsync(userData, false).ConfigureAwait(false);
            }
            else
            {
                await new UserDatabase().SaveUsersAsync(userData, false).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            userData.ErrCode = ErrorCode.ErrorWhileSavingRecords;
        }
    }

    /// <summary>
    /// Get linked users from database
    /// </summary>
    /// <param name="userData">Reference object to return linked users</param>
    /// <returns>linked users in reference object</returns>
    public async Task GetRelatedUsersAsync(UserDTO userData)
    {
        var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
        if (roleID == (int)RoleName.CareTaker)
        {
            await new UserDatabase().GetCareTakersPatientsAsync(userData).ConfigureAwait(false);
        }
        else
        {
            await new UserDatabase().GetLinkedUsersAsync(userData).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Check if any program is selected 
    /// </summary>
    /// <returns>0= Single USer, 1= Multiple Users, 2= Restart Page</returns>
    public async Task<byte> CheckIfUserSelectionRequiredAsync()
    {
        try
        {
            var logedInUserID = GetLoginUserID();
            UserDTO userData = new UserDTO
            {
                AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0),
            };
            await GetRelatedUsersAsync(userData).ConfigureAwait(false);
            if (GenericMethods.IsListNotEmpty(userData.Users))
            {
                if (logedInUserID > 0 && userData.Users.Any(x => x.UserID == logedInUserID))
                {
                    return 0;
                }
                if (userData.Users.Count == 1)
                {
                    _essentials.SetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, userData.Users[0].UserID);
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
        }
        return 2;
    }

    /// <summary>
    /// Check if account have multiple usern or not
    /// </summary>
    /// <param name="userID">user id to check</param>
    /// <returns>returns true/false</returns>
    public async Task<bool> IsMultipleUserAsync(long userID)
    {
        var loginUserID = GetLoginUserID();
        if (userID == loginUserID)
        {
            UserDTO userData = new UserDTO
            {
                AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0),
            };
            await GetRelatedUsersAsync(userData).ConfigureAwait(false);
            return GenericMethods.IsListNotEmpty(userData.Users) && userData.Users.Count > 1;
        }
        return true;
    }

    /// <summary>
    /// Check if any program is selected 
    /// </summary>
    /// <returns>Is Program Selected Boolean Value</returns>
    public async Task<int> IsProfileCompletionRequiredAsync()
    {
        try
        {
            // Step 1: Check Logedin user is Patient
            if (_essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, 0) == (int)RoleName.Patient)
            {
                // Step 2: Check Logedin user have permission for ProfileAddEdit
                await GetFeaturesAsync(AppPermissions.ProfileAddEdit.ToString()).ConfigureAwait(true);
                if (CheckFeaturePermissionByCode(AppPermissions.ProfileAddEdit.ToString()))
                {
                    // Step 3: Check Logedin users does not have all the manadatory data of Complete profile page 
                    UserDTO userData = new UserDTO
                    {
                        User = new UserModel
                        {
                            UserID = GetLoginUserID()
                        }
                    };
                    await new UserDatabase().GetUserAsync(userData).ConfigureAwait(false);
                    if (userData.User != null)
                    {
                        return userData.User.Dob == default
                            || userData.User.GenderID == default
                            || userData.User.PrefferedLanguageID == default ? 1 : 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
            return 2;
        }
        return 0;
    }

    /// <summary>
    /// Get linked users from database
    /// </summary>
    /// <param name="userData">Reference object to return linked users</param>
    /// <returns>linked users in reference object</returns>
    public async Task GetLinkedUsersAsync(UserDTO userData)
    {
        try
        {
            userData.SelectedUserID = GetUserID();
            userData.ErrCode = ErrorCode.RestartApp;
            userData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
            await Task.WhenAll(
                GetResourcesAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_GENDER_TYPE_GROUP),
                GetFeaturesAsync(AppPermissions.LinkedUsersView.ToString(), AppPermissions.LinkedUserAddEdit.ToString(), AppPermissions.MultipleUsersView.ToString()),
                GetRelatedUsersAsync(userData)
            ).ConfigureAwait(false);
            SetCommonPageData(userData);

            if (GenericMethods.IsListNotEmpty(userData.Users))
            {
                string nextIcon = GetNextIcon();
                foreach (UserModel user in userData.Users)
                {
                    MapUserUIData(user);
                    if (!userData.User.IsLinkedUser)
                    {
                        user.LastName = nextIcon;
                    }
                }
                if (!userData.User.IsLinkedUser && userData.Users.Count == 1)
                {
                    _essentials.SetPreferenceValue(StorageConstants.PR_LOGIN_USER_ID_KEY, userData.Users[0].UserID);
                }
                userData.ErrCode = ErrorCode.OK;
            }
            else
            {
                if (userData.User?.IsLinkedUser ?? false)
                {
                    userData.ErrCode = ErrorCode.OK;
                }
            }

            SetMainLoggedInUser(userData);
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void SetMainLoggedInUser(UserDTO userData)
    {
        if (userData.ErrCode == ErrorCode.OK && userData.IsActive)
        {
            var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
            if (roleID != (int)RoleName.CareTaker)
            {
                var minAddedON = userData.Users?.Min(x => x.AddedON);
                if (minAddedON != null)
                {
                    var userID = userData.Users?.FirstOrDefault(p => p.AddedON == minAddedON)?.UserID;
                    _essentials.SetPreferenceValue(StorageConstants.PR_IS_MAIN_LOGGEDIN_USER_KEY, userID);
                }
            }
        }
    }

    /// <summary>
    /// Get Register user info 
    /// </summary>
    /// <param name="userData">userdata</param>
    /// <returns>operation status</returns>
    public async Task GetRegisterUserInfoAsync(UserDTO userData)
    {
        try
        {
            userData.User.UserID = GetLoginUserID();
            await Task.WhenAll(
                  GetResourcesAsync(GroupConstants.RS_GENDER_TYPE_GROUP, GroupConstants.RS_USER_PROFILE_PAGE_GROUP, GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP),
                  GetSettingsAsync(GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                  new UserDatabase().GetUserAsync(userData)
              ).ConfigureAwait(false);
            userData.Resources = PageData.Resources;
            userData.Settings = PageData.Settings;
            await MapProfileDataAsync(userData).ConfigureAwait(false);
            userData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get user based on selected userId
    /// </summary>
    /// <param name="userData">Reference object for returning user</param>
    /// <returns>users in reference object with operation status</returns>
    public async Task GetUserAsync(UserDTO userData)
    {
        try
        {
            if (MobileConstants.IsMobilePlatform)
            {
                //bool isLinkedUser = userData.User.IsLinkedUser;
                //List<Task> taskList = new List<Task>();
                //taskList.Add(new UserDatabase().GetUserAsync(userData));
                //taskList.Add(GetResourcesAsync(GroupConstants.RS_GENDER_TYPE_GROUP, GroupConstants.RS_BLOOD_TYPE_GROUP, GroupConstants.RS_USER_RELATION_TYPE_GROUP, GroupConstants.RS_USER_PROFILE_PAGE_GROUP
                //       , GroupConstants.RS_ORGANISATION_PROFILE_GROUP, GroupConstants.RS_MENU_ACTION_GROUP, GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_USER_DEGREES_GROUPS));
                //taskList.Add(GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP));
                //if (isLinkedUser)
                //{
                //    taskList.Add(GetFeaturesAsync(new[] { AppPermissions.LinkedUserAddEdit.ToString(), AppPermissions.LinkedUserDelete.ToString() }));
                //}
                //else
                //{
                //    taskList.Add(new UserDatabase().GetUserRelationsAsync(userData));
                //    taskList.Add(new CountryDatabase().GetCountriesAsync(userData));
                //    if (userData.User.UserID == 0)
                //    {
                //        taskList.Add(GetFeaturesAsync(new string[] { AppPermissions.PatientAddEdit.ToString(), AppPermissions.ShareProfileDelete.ToString() }));
                //    }
                //    else
                //    {
                //        taskList.Add(new FeatureDatabase().GetTabFeaturesAsync(userData, AppPermissions.PatientTabView.ToString()));
                //    }
                //}
                //await Task.WhenAll(taskList).ConfigureAwait(false);
                //if (userData.User != null && (userData.User.UserID != 0 || userData.User.UserTempID != 0))
                //{
                //    if (getUserImage)
                //    {
                //        MapUserUIData(userData.User);
                //        userData.User.LastName = LibResources.GetResourceByKeyID(PageData?.Resources, userData.User.BloodGroupID)?.ResourceValue ?? string.Empty;
                //    }
                //    if (userData.User.RoleID == (byte)RoleName.Patient && userData.UserRelations?.Count > 0)
                //    {
                //        foreach (var relation in userData.UserRelations)
                //        {
                //            relation.RoleName = LibResources.GetResourceByKeyID(PageData?.Resources, relation.RelationID)?.ResourceValue ?? string.Empty;
                //            relation.ShowRemoveButtonText = LibResources.GetResourceByKey(PageData?.Resources, ResourceConstants.R_REMOVE_TEXT_KEY)?.ResourceValue ?? string.Empty;
                //            MapUserRealtionUIDtaa(relation);
                //        }

                //    }
                //}
                //if (isForProfile)
                //{
                //    userData.User.IsLinkedUser = isLinkedUser;
                //    await MapProfileDataAsync(userData).ConfigureAwait(false);
                //}
                userData.ErrCode = ErrorCode.OK;
            }
            else
            {
                await SyncUsersFromServerAsync(userData, CancellationToken.None).ConfigureAwait(false);
            }
            if (userData.ErrCode == ErrorCode.OK && userData.User != null)
            {
                MapUserDates(userData);
            }
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Fetch logedIn user profile data
    /// </summary>
    /// <returns>logedin user profile</returns>
    internal async Task<UserDTO> GetLoggedInUserProfileAsync()
    {
        // Get user profile image
        UserDTO userData = new UserDTO() { User = new UserModel { RoleID = Convert.ToByte(_essentials.GetPreferenceValue(StorageConstants.PR_ROLE_ID_KEY, (int)0), CultureInfo.InvariantCulture) } };
        if (userData.User.RoleID == (int)RoleName.CareTaker)
        {
            userData.User.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, (long)0);
        }
        else
        {
            userData.User.UserID = GetLoginUserID();
        }
        await new UserDatabase().GetUserAsync(userData).ConfigureAwait(false);
        return userData;
    }

    /// <summary>
    /// Get user based on selected userId
    /// </summary>
    /// <param name="userData">Reference object for returning user</param>
    /// <returns>users in reference object with operation status</returns>
    public async Task GetShareProfileDataAsync(UserDTO userData)
    {
        try
        {
            await Task.WhenAll(
                GetFeaturesAsync(AppPermissions.ShareProfileAddEdit.ToString(), AppPermissions.ShareProfilesView.ToString()),
                new CountryDatabase().GetCountriesAsync(userData),
                new UserDatabase().GetShareProfileDataAsync(userData),
                GetResourcesAsync(GroupConstants.RS_USER_RELATION_TYPE_GROUP, GroupConstants.RS_USER_PROFILE_PAGE_GROUP, GroupConstants.RS_PROGRAMS_GROUP
                    , GroupConstants.RS_ORGANISATION_PROFILE_GROUP, GroupConstants.RS_LOGIN_FLOW_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP),
                GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP)
            ).ConfigureAwait(false);

            userData.UserRelationTypes = (from resource in PageData.Resources.Where(x => x.GroupName == GroupConstants.RS_USER_RELATION_TYPE_GROUP)
                                          select new OptionModel
                                          {
                                              GroupName = resource.ResourceKey,
                                              OptionID = resource.ResourceKeyID,
                                              OptionText = resource.ResourceValue,
                                              IsSelected = resource.ResourceKey == userData.User?.GenderID
                                          }).ToList();
            userData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            userData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    private void MapUsersData(JToken data, UserDTO userData)
    {
        SetPageResources(userData.Resources);
        userData.Users = MapUsers(data, nameof(UserDTO.Users));
        if (userData.RecordCount == -1)
        {
            if (GenericMethods.IsListNotEmpty(userData.Users))
            {
                userData.User = userData.Users.FirstOrDefault();
                userData.Users.Clear();
            }
            userData.Organisations = GetPickerSource(data, nameof(UserDTO.Organisations), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText)
                , userData.User?.SelectedOrganisationID > 0 ? userData.User.SelectedOrganisationID : userData.User?.OrganisationID ?? -1
                , false, null);
            userData.Departments = GetPickerSource(data, nameof(UserDTO.Departments), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), userData.User?.DepartmentID ?? -1, false, nameof(OptionModel.ParentOptionID));
            userData.Branches = GetPickerSource(data, nameof(UserDTO.Branches), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), userData.User?.BranchID ?? -1, false, null);
            userData.Roles = GetPickerSource(data, nameof(UserDTO.Roles), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), userData.User?.RoleID ?? -1, false, null);
            userData.Professions = GetPickerSource(data, nameof(UserDTO.Professions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), userData.User?.ProffessionID ?? -1, false, null);
            userData.Genders = MapResourcesIntoOptions(GroupConstants.RS_GENDER_TYPE_GROUP, userData.User?.GenderID ?? string.Empty, string.Empty, false);
            userData.BloodGroups = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_BLOOD_TYPE_GROUP, string.Empty, false, userData.User?.BloodGroupID ?? -1);
            userData.Languages = GetPickerSource(data, nameof(UserDTO.Languages), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), userData.User?.PrefferedLanguageID ?? -1, false, null);
            GetUserDegrees(userData);
            userData.OrganisationTags = GetPickerSource(data, nameof(UserDTO.OrganisationTags), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText), userData.User?.OrganisationID ?? -1, false, null);
            //GetUserOrganisationTags(userData);
            userData.CountryCodes = new CountryService(_essentials).MapCountryCodes(data);
        }
    }

    //private int GetAge(DateTime dateOfBirth)
    //{
    //    return (int)((LibGenericMethods.GetUtcDateTime.Date.Subtract(dateOfBirth).Days - Math.Ceiling((double)(LibGenericMethods.GetUtcDateTime.Year - dateOfBirth.Year) / 4)) / 365);
    //}
    private byte GetAge(DateTimeOffset dob)
    {
        DateTimeOffset today = GenericMethods.GetUtcDateTime;

        byte months = (byte)(today.Month - dob.Month);
        byte years = (byte)(today.Year - dob.Year);

        if (today.Day < dob.Day)
        {
            months--;
        }

        if (months < 0)
        {
            years--;
        }
        return years;
    }

    private void MapAttachment(JToken data, UserDTO userData)
    {
        userData.AttachmentBase64 = Convert.ToString(data[nameof(UserDTO.AddedBy)], CultureInfo.InvariantCulture);
        //: null;
    }

    private void MapUserDates(UserDTO userData)
    {
        if (userData.User.Dob.HasValue && userData.User.Dob != default)
        {
            LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            userData.User.DateOfBirth = GenericMethods.GetLocalDateTimeBasedOnCulture(userData.User.Dob.Value, DateTimeType.Date
                , dayFormat, monthFormat, yearFormat);
            userData.User.UserAge = GetAge(userData.User.Dob.Value);
            userData.User.Dob = _essentials.ConvertToLocalTime(userData.User.Dob.Value);
        }
        else
        {
            userData.User.UserAge = 0;
        }
        if (userData.User.Doj.HasValue)
        {
            userData.User.Doj = _essentials.ConvertToLocalTime(userData.User.Doj.Value);
        }
    }

    //private void MapUserRealtionUIDtaa(UserRelationModel userRelationModel)
    //{
    //    if (string.IsNullOrWhiteSpace(userRelationModel.FromDateString))
    //    {
    //        userRelationModel.FromDateString = string.IsNullOrEmpty(userRelationModel.CareGiverName) ? ImageConstants.I_DEFAULT_PROFILE_ICON : GetInitials(userRelationModel.CareGiverName);
    //    }
    //    else
    //    {
    //        //Todo:
    //        //userRelationModel.ImageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(userRelationModel.FromDateString));
    //    }
    //}

    private void MapUserUIData(UserModel user)
    {
        //user.FirstName = $"{user.FirstName}{Constants.STRING_SPACE}{user.LastName}";
        //if (string.IsNullOrWhiteSpace(user.ImageBase64))
        //{
        //    user.ImageName = string.IsNullOrEmpty(user.FirstName) ? ImageConstants.I_DEFAULT_PROFILE_ICON : GetInitials(user.FirstName);
        //}
        //else
        //{
        //    //todo:
        //    //user.ImageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(user.ImageBase64));
        //}
        //CalculateUserAge(user);
        user.LastName = $"{user.UserAge} {Constants.SYMBOL_PIPE_SEPERATOR} {LibResources.GetResourceByKey(PageData?.Resources, user.GenderID)?.ResourceValue}";
    }

    private void CalculateUserAge(UserModel user)
    {
        if (user.Dob.HasValue)
        {
            user.Dob = _essentials.ConvertToLocalTime(user.Dob.Value);
            user.UserAge = (byte)(DateTime.Now.Subtract(user.Dob.Value.Date).Days / 365);
        }
        else
        {
            user.UserAge = 0;
        }
    }

    /// <summary>
    /// Get caregiver details to patient login
    /// </summary>
    /// <param name="caregiverData">holds patient caregiver id</param>
    /// <returns>patients caregiver data</returns>
    public async Task GetCaregiverDetailsAsync(CaregiverDTO caregiverData)
    {
        try
        {
            caregiverData.AccountID = GetLoginUserID();
            await Task.WhenAll(
                GetResourcesAsync(GroupConstants.RS_CAREGIVER_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP),
                GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
                GetFeaturesAsync(caregiverData.RecordCount == 0
                ? new[] { AppPermissions.CaregiversView.ToString(), AppPermissions.CaregiverAddEdit.ToString() }
                : new[] { AppPermissions.CaregiverAddEdit.ToString(), AppPermissions.CaregiverDelete.ToString() }),
                new UserDatabase().GetCaregiverDetailsAsync(caregiverData)
            ).ConfigureAwait(false);
            LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
            caregiverData.Caregiver.ToDateValue = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(caregiverData.Caregiver.ToDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            caregiverData.Caregiver.FromDateValue = GenericMethods.GetDateTimeBasedOnCulture(_essentials.ConvertToLocalTime(caregiverData.Caregiver.FromDate.Value), DateTimeType.Date, dayFormat, monthFormat, yearFormat);
            caregiverData.ErrCode = ErrorCode.OK;
        }
        catch (Exception ex)
        {
            caregiverData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

    /// <summary>
    /// Get patient caregivers
    /// </summary>
    /// <param name="caregiverData">caregiverData reference to return output</param>
    /// <returns>caregivers received from server in caregiverData</returns>
    public async Task GetPatientCareGiversAsync(CaregiverDTO caregiverData)
    {
        try
        {

            if (MobileConstants.IsMobilePlatform)
            {
                caregiverData.SelectedUserID = GetUserID();
                await GetCaregiversAsync(caregiverData).ConfigureAwait(false);
            }
            else
            {
                await SyncPatientCaregiversFromServerAsync(caregiverData, CancellationToken.None).ConfigureAwait(false);
                SetPageSettings(caregiverData.Settings);
            }
            if (caregiverData.ErrCode == ErrorCode.OK
                && GenericMethods.IsListNotEmpty(caregiverData.Caregivers)
                && caregiverData.RecordCount != -1)
            {
                LibSettings.TryGetDateFormatSettings(PageData?.Settings, out string dayFormat, out string monthFormat, out string yearFormat);
                caregiverData.Caregiver = new CaregiverModel
                {
                    FromDateValue = GenericMethods.GetDateTimeFormat(DateTimeType.Date, dayFormat, monthFormat, yearFormat)
                };
                caregiverData.Caregivers.ForEach(caregiver =>
                {
                    caregiver.ToDateValue = GenericMethods.GetLocalDateTimeBasedOnCulture(caregiver.ToDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    caregiver.FromDateValue = GenericMethods.GetLocalDateTimeBasedOnCulture(caregiver.FromDate.Value, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
                    caregiver.DateStyle = GetDateTimeStyle(caregiver.FromDate, caregiver.ToDate);
                    if (MobileConstants.IsMobilePlatform)
                    {
                        caregiver.FromDateValue = $"{caregiver.FromDateValue} {Constants.SYMBOL_DASH} {caregiver.ToDateValue}";
                        caregiver.Initials = GetInitials(caregiver.FirstName);
                    }
                });
                var roleID = _essentials.GetPreferenceValue<int>(StorageConstants.PR_ROLE_ID_KEY, (int)0);
                if (roleID != (int)RoleName.Patient && roleID != (int)RoleName.CareTaker)
                {
                    //SortPatientCaregivers(caregiverData);
                    GenericMethods.SortByDate(caregiverData.Caregivers, x => x.FromDate, x => x.ToDate);
                }

            }
            if (caregiverData.RecordCount == -1 || caregiverData.RecordCount == -11)
            {
                if (caregiverData.Caregiver.ToDate != null && caregiverData.Caregiver.FromDate != null)
                {
                    caregiverData.Caregiver.ToDate = _essentials.ConvertToLocalTime(caregiverData.Caregiver.ToDate.Value);
                    caregiverData.Caregiver.FromDate = _essentials.ConvertToLocalTime(caregiverData.Caregiver.FromDate.Value);
                }
                ResetOptions(caregiverData);
            }
        }
        catch (Exception ex)
        {
            caregiverData.ErrCode = ErrorCode.ErrorWhileRetrievingRecords;
            LogError(ex.Message, ex);
        }
    }

   
    private async Task GetCaregiversAsync(CaregiverDTO caregiverData)
    {
        caregiverData.AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0);
        await Task.WhenAll(
            GetResourcesAsync(GroupConstants.RS_CAREGIVER_PAGE_GROUP, GroupConstants.RS_COMMON_GROUP),
            GetSettingsAsync(GroupConstants.RS_COMMON_GROUP, GroupConstants.RS_ORGANISATION_SETTINGS_GROUP),
            GetFeaturesAsync(caregiverData.RecordCount == 0
                ? new[] { AppPermissions.CaregiversView.ToString(), AppPermissions.CaregiverAddEdit.ToString() }
                : new[] { AppPermissions.CaregiverAddEdit.ToString(), AppPermissions.CaregiverDelete.ToString() }),
            caregiverData.RecordCount == -11
                ? SyncPatientCaregiversFromServerAsync(caregiverData, CancellationToken.None)
                : new UserDatabase().GetCaregiversAsync(caregiverData, false)
        ).ConfigureAwait(false);
        if (caregiverData.ErrCode == ErrorCode.OK)
        {
            if (caregiverData.RecordCount == -11)
            {
                await new UserDatabase().GetCaregiverAsync(caregiverData).ConfigureAwait(false);
                caregiverData.Branches = GetPickerSource(caregiverData.Branches, nameof(OptionModel.OptionID), nameof(OptionModel.OptionText)
                    , caregiverData.Caregiver.OrganisationID, false, Constants.CNT_PARENT_OPTIONID_TEXT);
                caregiverData.Departments = GetPickerSource(caregiverData.Departments, nameof(OptionModel.OptionID), nameof(OptionModel.OptionText)
                    , caregiverData.Caregiver.OrganisationID, false, Constants.CNT_PARENT_OPTIONID_TEXT);
                if (caregiverData.Departments.FirstOrDefault(x => x.IsSelected) != null)
                {
                    caregiverData.Branches.FirstOrDefault(x => x.OptionID == caregiverData.Departments.FirstOrDefault(y => y.IsSelected).ParentOptionID)
                        .IsSelected = true;
                }
                caregiverData.CaregiverOptions = GetPickerSource(caregiverData.CaregiverOptions, nameof(OptionModel.OptionID), nameof(OptionModel.OptionText)
                    , caregiverData.Caregiver.CareGiverID, false, Constants.CNT_PARENT_OPTIONID_TEXT);
            }
        }
        caregiverData.ErrCode = ErrorCode.OK;
    }

    private void MapCaregiverData(JToken data, CaregiverDTO caregiverData)
    {
        if (caregiverData.RecordCount == -11)
        {
            caregiverData.CaregiverOptions = GetPickerSource(data, nameof(CaregiverDTO.CaregiverOptions), nameof(OptionModel.OptionID)
                , nameof(OptionModel.OptionText), caregiverData.Caregiver.CareGiverID, false, Constants.CNT_PARENT_OPTIONID_TEXT);
        }
        else if (caregiverData.RecordCount == -1)
        {
            MapAddEditPatientCaregiverData(data, caregiverData);
        }
        else
        {
            caregiverData.Caregivers = (from dataItem in data[nameof(CaregiverDTO.Caregivers)]
                                        select MapCaregiver(dataItem, true)
                                        ).ToList();
        }
    }

    private void MapAddEditPatientCaregiverData(JToken data, CaregiverDTO caregiverData)
    {
        if (!MobileConstants.IsMobilePlatform)
        {
            SetPageResources(caregiverData.Resources);
        }
        caregiverData.Caregiver = MapCaregiver(data[nameof(CaregiverDTO.Caregiver)], false);
        caregiverData.Organisations = (from dataItem in data[nameof(CaregiverDTO.Organisations)]
                                       select new OptionModel
                                       {
                                           OptionID = (long)dataItem[nameof(OptionModel.OptionID)],
                                           OptionText = (string)dataItem[nameof(OptionModel.OptionText)],
                                           IsSelected = (bool)dataItem[nameof(OptionModel.IsSelected)],
                                       }).ToList();
        caregiverData.Branches = data[nameof(CaregiverDTO.Branches)].Any()
            ? GetPickerSource(data, nameof(CaregiverDTO.Branches), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText)
                , caregiverData.Caregiver.OrganisationID, false, null)
            : new List<OptionModel>();
        caregiverData.Departments = data[nameof(CaregiverDTO.Departments)].Any()
            ? GetPickerSource(data, nameof(CaregiverDTO.Departments), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText)
                , caregiverData.Caregiver.OrganisationID, false, Constants.CNT_PARENT_OPTIONID_TEXT)
            : new List<OptionModel>();
        if (caregiverData.Departments.FirstOrDefault(x => x.IsSelected) != null)
        {
            caregiverData.Branches.FirstOrDefault(x => x.OptionID == caregiverData.Departments.FirstOrDefault(y => y.IsSelected).ParentOptionID)
                .IsSelected = true;
        }
        caregiverData.CaregiverOptions = GetPickerSource(data, nameof(CaregiverDTO.CaregiverOptions), nameof(OptionModel.OptionID), nameof(OptionModel.OptionText)
            , caregiverData.Caregiver.CareGiverID, false, Constants.CNT_PARENT_OPTIONID_TEXT);
    }

    protected List<UserRelationModel> MapUserRelations(JToken data, string collactionName)
    {
        return (data[collactionName]?.Count() > 0)
            ? (from dataItem in data[collactionName]
               select MapUserRelation(dataItem)).ToList()
            : null;
    }

    /// <summary>
    /// To assign respective picker values when a parent picker is changed.
    /// </summary>
    /// <param name="caregiverData">caregiverData reference to return output</param>
    /// <param name="level">level of the selected picker based on parent</param>
    /// <param name="optionID"> option id of the selected value from picker</param>
    /// <param name="shouldResetList"> flag to decide data needs to be reset or not</param>
    public void AssignPickerValues(CaregiverDTO caregiverData, byte level, long optionID, bool shouldResetList)
    {
        if (optionID > 0)
        {
            if (shouldResetList)
            {
                caregiverData.DepartmentList.ForEach(x => { x.IsSelected = false; });
                caregiverData.CaregiverList.ForEach(x => { x.IsSelected = false; });
            }
            // level:- 1-> Branches, 2->Departments
            if (level == 1)
            {
                caregiverData.Branches.ForEach(x => { x.IsSelected = false; });
                caregiverData.Branches.FirstOrDefault(x => x.OptionID == optionID).IsSelected = true;
            }
            else
            {
                caregiverData.DepartmentList.FirstOrDefault(x => x.OptionID == optionID).IsSelected = true;
            }
            ResetOptions(caregiverData);
        }

        if (GenericMethods.IsListNotEmpty(caregiverData.CaregiverList) && caregiverData.Caregiver.CareGiverID > 0)
        {
            var selectedOption = caregiverData.CaregiverList.FirstOrDefault(x => x.OptionID == caregiverData.Caregiver.CareGiverID);
            if (selectedOption != null)
            {
                selectedOption.IsSelected = true;
            }
        }
    }

    private void ResetOptions(CaregiverDTO caregiverData)
    {
        //if department is selected, display providers based on selected department, otherwise display providers based on selectedBranch
        var selectedDept = caregiverData.Departments?.FirstOrDefault(x => x.IsSelected && x.OptionID != -1);
        if (selectedDept == null)
        {
            //if branch is selected, display departments and providers based on branch otherwise clear departments and display providers based on organization
            var selectedBranch = caregiverData.Branches?.FirstOrDefault(x => x.IsSelected && x.OptionID != -1);
            if (selectedBranch == null)
            {
                // Clear departments and display providers, branches, departments based on organization                    
                //SelectOption(caregiverData.Branches);
                ResetDepartments(caregiverData, 0);
                ResetCaregivers(caregiverData, caregiverData.Organisations.FirstOrDefault(x => x.IsSelected && x.OptionID != -1).OptionID);
            }
            else
            {
                //display departments and providers based on branch
                ResetDepartments(caregiverData, selectedBranch.OptionID);
                ResetCaregivers(caregiverData, selectedBranch.OptionID);
            }
        }
        else
        {
            //display departments and providers based on branch
            ResetDepartments(caregiverData, selectedDept.ParentOptionID);
            ResetCaregivers(caregiverData, selectedDept.OptionID);
        }
    }

    private void ResetDepartments(CaregiverDTO caregiverData, long parentOptionID)
    {
        caregiverData.DepartmentList = caregiverData.Departments.Where(x => x.ParentOptionID == parentOptionID || x.OptionID == -1)?.ToList();
    }

    private void ResetCaregivers(CaregiverDTO caregiverData, long parentOptionID)
    {
        caregiverData.CaregiverList = caregiverData.CaregiverOptions.Where(x => x.ParentOptionID == parentOptionID || x.OptionID == -1)?.ToList();
    }

    private async Task MapProfileDataAsync(UserDTO userData)
    {
        GetUserGenders(userData);
        GetUserBloodGroups(userData);
        if (!userData.User.IsLinkedUser)
        {
            await GetUserCulturesAsync(userData).ConfigureAwait(false);
            if (userData.User.UserID != 0 && userData.User.RoleID != (int)RoleName.Patient)
            {
                await Task.WhenAll(
                    GetUserProfessionAsync(userData),
                    GetUserRoleAsync(userData)
                ).ConfigureAwait(false);

                GetUserDegrees(userData);
            }
            // GetUserOrganisationTags(userData);
        }
        //if (!string.IsNullOrWhiteSpace(userData.User?.ImageBase64))
        //{
        //    //todo:
        //    //userData.User.ImageSource = ImageSource.FromStream(() => LibGenericMethods.GetMemoryStreamFromBase64(userData.User.ImageBase64));
        //}
        if (userData.User != null)
        {
            userData.User.UserInitials = string.IsNullOrEmpty(userData.User.FirstName)
                ? ImageConstants.I_DEFAULT_PROFILE_ICON
                : GetInitials($"{userData.User.FirstName} {userData.User.LastName}");
        }
    }

    private void GetUserBloodGroups(UserDTO userData)
    {
        if (userData.User?.UserID >= 0 || userData.User.IsLinkedUser)
        {
            userData.BloodGroups = (from resource in PageData.Resources.Where(x => x.GroupName == GroupConstants.RS_BLOOD_TYPE_GROUP)
                                    select new OptionModel
                                    {
                                        GroupName = resource.ResourceKey,
                                        OptionID = resource.ResourceKeyID,
                                        OptionText = resource.ResourceValue,
                                        IsSelected = resource.ResourceKey == userData.User?.BloodGroupID.ToString()
                                    }).ToList();
        }
    }

    private void GetUserGenders(UserDTO userData)
    {
        userData.Genders = (from resource in PageData.Resources.Where(x => x.GroupName == GroupConstants.RS_GENDER_TYPE_GROUP)
                            select new OptionModel
                            {
                                GroupName = resource.ResourceKey,
                                OptionID = resource.ResourceKeyID,
                                OptionText = resource.ResourceValue,
                                IsSelected = resource.ResourceKey == userData.User?.GenderID
                            }).ToList();
    }

    private void GetUserDegrees(UserDTO userData)
    {
        userData.UserDegrees = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_USER_DEGREES_GROUPS, string.Empty, false, 0);
    }

    //private void GetUserOrganisationTags(UserDTO userData)
    //{
    //    userData.OrganisationTags = MapResourcesIntoOptionsByKeyID(GroupConstants.RS_ORGANISATION_TAG_GROUP, string.Empty, false, 0);
    //}

    private async Task GetUserCulturesAsync(UserDTO userData)
    {
        using (LanguageDatabase languagesDB = new LanguageDatabase())
        {
            LanguageDTO languageData = new LanguageDTO { Languages = new List<LanguageModel>() };
            await languagesDB.GetLanguagesAsync(languageData).ConfigureAwait(false);
            userData.Languages = (from language in languageData.Languages
                                  select new OptionModel
                                  {
                                      OptionID = Convert.ToInt64(language.LanguageID, CultureInfo.InvariantCulture),
                                      OptionText = language.LanguageName,
                                      IsSelected = language.LanguageID == userData.User?.PrefferedLanguageID
                                  }).ToList();
        }
    }

    private async Task GetUserRoleAsync(UserDTO userData)
    {
        using (RoleDatabase roleDB = new RoleDatabase())
        {
            RoleDTO roleData = new RoleDTO { Roles = new List<UserRolesModel>() };
            await roleDB.GetRolesAsync(roleData).ConfigureAwait(false);
            userData.User.RoleName = roleData.Roles.FirstOrDefault(x => x.RoleID == userData.User.RoleID).RoleName;
        }
    }

    private async Task GetUserProfessionAsync(UserDTO userData)
    {
        if (userData.User.RoleID != (int)RoleName.CareTaker)
        {
            using (ProfessionDatabase professionDB = new ProfessionDatabase())
            {
                ProfessionDTO profession = new ProfessionDTO { UserProfessions = new List<UserProfessionModel>() };
                await professionDB.GetProfessionsAsync(profession).ConfigureAwait(false);
                if (userData.User.ProffessionID > 0)
                {
                    userData.User.Proffession = profession.UserProfessions.FirstOrDefault(x => x.ProfessionID == userData.User.ProffessionID).Profession;
                }
            }
        }
    }

    /// <summary>
    /// Register Device for push notification
    /// </summary>
    /// <returns>Task representing registring device for push notification</returns>
    public async Task RegisterDeviceAsync()
    {
        try
        {
            if (_essentials.GetPreferenceValue<bool>(StorageConstants.PR_NOTIFICATION_TOKEN_CHANGED_KEY, true))
            {
                UserDTO userData = new UserDTO
                {
                    AccountID = _essentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0),
                };
                await new UserDatabase().GetLinkedUsersAsync(userData).ConfigureAwait(false);
                if (userData.Users?.Count > 0)
                {
                    NotificationDTO notificationData = new NotificationDTO
                    {
                        NotificationData = new NotificationModel
                        {
                            DeviceHandle = _essentials.GetPreferenceValue<string>(StorageConstants.PR_NOTIFICATION_TOKEN_KEY, string.Empty)
                        }
                    };

                    notificationData.NotificationData.PNS = (MobileConstants.IsAndroidPlatform)
                        ? PushNotificationServerType.GOOGLE
                        : PushNotificationServerType.APPLE;
                    //todo:
                    //notificationData.NotificationData.DeviceUniqueId = (await Microsoft.AppCenter.AppCenter.GetInstallIdAsync()).ToString();
                    //todo:
                    //notificationData.NotificationData.DeviceModel = DeviceInfo.Model;
                    //notificationData.NotificationData.DeviceOS = DeviceInfo.Platform.ToString();
                    //notificationData.NotificationData.DeviceDetails = DeviceInfo.Manufacturer;
                    notificationData.NotificationData.Tags = GetNotificationTags(userData);
                    await new PushNotificationService(_essentials).RegisterDeviceAsync(notificationData, new CancellationTokenSource().Token).ConfigureAwait(false);
                    if (notificationData.ErrCode == ErrorCode.OK)
                    {
                        _essentials.GetPreferenceValue<string>(StorageConstants.PR_NOTIFICATION_TAGS_KEY, notificationData.NotificationData.Tags);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.Message, ex);
        }
    }

    private string GetNotificationTags(UserDTO userData)
    {
        string tags = string.Empty;
        foreach (var user in userData.Users)
        {
            if (!string.IsNullOrWhiteSpace(tags))
            {
                tags += Constants.COMMA_SEPARATOR;
            }
            tags += Constants.USER_TAG_PREFIX + user.UserID;
        }
        return tags;
    }

    private string GetDateTimeStyle(DateTimeOffset? fromDate, DateTimeOffset? toDate)
    {
        var currentDate = GenericMethods.GetUtcDateTime;
        string dateStyle;
        if (!fromDate.HasValue || !toDate.HasValue || (fromDate <= currentDate && toDate >= currentDate))
        {
            dateStyle = MobileConstants.IsMobilePlatform
                ? StyleConstants.SUCCESS_COLOR
                : "date-present ";
        }
        else if (fromDate > currentDate && toDate > currentDate)
        {
            dateStyle = MobileConstants.IsMobilePlatform
                ? StyleConstants.SECONDARY_APP_COLOR
                : "date-future ";
        }
        else
        {
            dateStyle = MobileConstants.IsMobilePlatform
                ? StyleConstants.ERROR_COLOR
                : "date-past ";
        }
        return dateStyle;
    }

}