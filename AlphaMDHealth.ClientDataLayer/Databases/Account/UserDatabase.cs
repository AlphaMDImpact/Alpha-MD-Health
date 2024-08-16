using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.ClientDataLayer;

public class UserDatabase : BaseDatabase
{
    /// <summary>
    /// Save user data in database
    /// </summary>
    /// <param name="userData">object to save user data</param>
    /// <param name="isSyncFlow">true if flow is from sync</param>
    /// <returns>Operation status</returns>
    public async Task SaveUsersAsync(UserDTO userData, bool isSyncFlow)
    {
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            if (GenericMethods.IsListNotEmpty(userData.Users))
            {
                foreach (UserModel user in userData.Users)
                {
                    transaction.InsertOrReplace(user);
                }
            }
            SaveUserRelations(userData, isSyncFlow, transaction);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Save user data in database
    /// </summary>
    /// <param name="userData">object to save user data</param>
    /// <param name="isSyncFlow">true if flow is from sync</param>
    /// <returns>Operation status</returns>
    public async Task InsertUsersAsync(UserDTO userData, bool isSyncFlow)
    {
        long lastUser = 0;
        long lastPatientCaregiverID = 0;
        if (!isSyncFlow)
        {
            // if user is offline
            lastUser = await getLastUserID(lastUser);
            lastPatientCaregiverID = await getLastPatientCaregiverID(lastPatientCaregiverID);
        }
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            if (GenericMethods.IsListNotEmpty(userData.Users))
            {
                foreach (UserModel user in userData.Users)
                {
                    var userID = lastUser < 0 ? lastUser - 1 : -1;
                    user.UserTempID = userID;
                    user.IsSynced = false;
                    user.IsActive = true;
                    if (transaction.FindWithQuery<UserModel>("SELECT 1 FROM UserModel WHERE EmailId=? OR PhoneNo=? ", user.EmailId, user.PhoneNo) == null)
                    {
                        transaction.InsertOrReplace(user);
                        AddUserRelationAsync(userData, transaction, lastPatientCaregiverID, userID);
                    }
                    else
                    {
                        userData.ErrCode = ErrorCode.DuplicateUser;
                    }
                }
            }
        }).ConfigureAwait(false);
    }

    private void AddUserRelationAsync(UserDTO userData, SQLiteConnection transaction, long lastPatientCaregiverID, long userID)
    {
        var patientCaregiverID = lastPatientCaregiverID < 0 ? lastPatientCaregiverID - 1 : -1;
        if (transaction.FindWithQuery<UserRelationModel>("SELECT * FROM UserRelationModel WHERE PatientCareGiverID = ?", patientCaregiverID) == null)
        {
            transaction.Execute("INSERT INTO UserRelationModel (PatientCareGiverID, PatientID, CareGiverID, FromDate, ToDate, IsActive, IsSynced) VALUES (?, ?, ?, ?, ?, ?,?)",
                                        patientCaregiverID, userID, userData.CreatedByID, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), true, false);

        }
        else
        {
            transaction.Execute("UPDATE UserRelationModel SET PatientID = ?, CareGiverID = ?, FromDate = ?, ToDate = ?, IsActive=?, IsSynced = ? WHERE PatientCareGiverID = ? ",
                                                                        userID, userData.CreatedByID, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), true, false, patientCaregiverID);

        }
    }

    private async Task<long> getLastUserID(long lastUser)
    {
        var users = await SqlConnection.QueryAsync<UserModel>(
        "SELECT * FROM UserModel WHERE IsSynced = 0 ORDER BY UserID ASC");
        if (users?.Count > 0)
        {
            lastUser = Convert.ToInt64(users.FirstOrDefault().UserTempID);
        }
        return lastUser;
    }

    private async Task<long> getLastPatientCaregiverID(long lastPatientCaregiverID)
    {
        var userRelations = await SqlConnection.FindWithQueryAsync<UserRelationModel>(
        "SELECT * FROM UserRelationModel WHERE IsSynced = 0 ORDER BY PatientCareGiverID ASC LIMIT 1");
        if (userRelations != null)
        {
            lastPatientCaregiverID = Convert.ToInt64(userRelations.PatientCareGiverID);
        }
        return lastPatientCaregiverID;
    }

    /// <summary>
    /// Save linked user data in database
    /// </summary>
    /// <param name="userData">Linked user data to save</param>
    /// <returns>operation status</returns>
    public async Task SaveLinkedUserAsync(UserDTO userData)
    {
        long lastUser = 0;
        // if user is offline
        lastUser = await getLastUserID(lastUser);
        long lastPatientCaregiverID = 0;
        lastPatientCaregiverID = await getLastPatientCaregiverID(lastPatientCaregiverID);
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            if (GenericMethods.IsListNotEmpty(userData.Users))
            {
                foreach (UserModel user in userData.Users)
                {
                    if (user.IsActive)
                    {
                        var userID = user.UserID == 0 ? (lastUser < 0 ? lastUser - 1 : -1) : user.UserID;
                        user.UserTempID = userID;
                        user.IsSynced = false;
                        user.LastModifiedON = DateTime.UtcNow;
                        if (user.AccountID == 0)
                        {
                            UserModel currentUser = transaction.FindWithQuery<UserModel>(
                                "SELECT AccountID, PrefferedLanguageID  FROM UserModel WHERE UserID = ?", userData.SelectedUserID);
                            user.PrefferedLanguageID = currentUser.PrefferedLanguageID;
                            user.AccountID = currentUser.AccountID;
                        }
                        transaction.InsertOrReplace(user);

                        if (user.UserTempID < 0 && transaction.FindWithQuery<UserRelationModel>("SELECT 1 FROM UserRelationModel WHERE PatientID = ? AND CareGiverID = ?", user.UserTempID, userData.CreatedByID) == null)
                        {
                            var patientCaregiverID = lastPatientCaregiverID < 0 ? lastPatientCaregiverID - 1 : -1;

                            AddUserRelationAsync(userData, transaction, lastPatientCaregiverID, userID);
                            /*transaction.Execute("INSERT INTO UserRelationModel (PatientCareGiverID, PatientID, CareGiverID, FromDate, ToDate, IsActive, IsSynced) VALUES (?, ?, ?, ?, ?, ?, ?)",
															patientCaregiverID, user.UserTempID, userData.CreatedByID, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), true, false);*/
                        }
                        userData.ErrCode = ErrorCode.OK;
                    }
                    else
                    {
                        transaction.Execute("UPDATE UserModel SET IsSynced = ?, IsActive = ?, IsLinkedUser = ? WHERE UserID = ?"
                            , user.IsSynced, user.IsActive, user.IsLinkedUser, user.UserID);
                    }
                }
            }
        }).ConfigureAwait(false);
    }

    private void SaveUserRelations(UserDTO userData, bool isSyncFlow, SQLiteConnection transaction)
    {
        if (isSyncFlow && GenericMethods.IsListNotEmpty(userData.UserRelations))
        {
            foreach (UserRelationModel relation in userData.UserRelations)
            {
                transaction.InsertOrReplace(relation);
            }
        }
    }

    /// <summary>
    /// update user id after sync to server
    /// </summary>
    /// <param name="user">user data to update</param>
    /// <returns>operation status</returns>
    public async Task UpdateUserIDAsync(UserDTO user)
    {
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            if (user.CreatedByID > 0)
            {
                transaction.Execute("UPDATE UserModel SET IsSynced = 1 WHERE UserID = ?", user.User.UserID);
            }
            else
            {
                if (user.ErrCode == ErrorCode.OK)
                {
                    if (user.User.IsActive && user.User.UserTempID < 0)
                    {
                        transaction.Execute("UPDATE UserModel SET IsSynced = 1, UserID = ?, UserTempID =? WHERE UserTempID = ?"
                            , user.User.UserID, user.User.UserID, user.CreatedByID);

                        transaction.Execute("UPDATE UserRelationModel SET IsActive = 0, IsSynced = 1 WHERE UserID = ?", user.CreatedByID);
                    }
                    else
                    {
                        transaction.Execute("UPDATE UserModel SET IsSynced = 1 WHERE UserID = ?", user.User.UserID);
                    }
                }
                else
                {
                    transaction.Execute("UPDATE UserModel SET IsSynced = 0, ErrorCode = ? WHERE UserTempID = ?"
                        , (int)user.ErrCode, user.CreatedByID);
                }
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Get linked users from database
    /// </summary>
    /// <param name="userData">Reference object to return linked users</param>
    /// <returns>linked users in reference object</returns>
    public async Task GetLinkedUsersAsync(UserDTO userData)
    {
        long selectedAccountID;
        string query = string.Empty;
        if (userData.User != null && userData.User.IsLinkedUser)
        {
            selectedAccountID = await SqlConnection.ExecuteScalarAsync<long>(
                "SELECT AccountID FROM UserModel WHERE UserID = ?", userData.SelectedUserID);
            query = $"AND UserID != {userData.SelectedUserID}";
            if (userData.RecordCount > 0)
            {
                query += $" LIMIT {userData.RecordCount}";
            }
            query += " ORDER BY LastModifiedON DESC";
        }
        else
        {
            selectedAccountID = userData.AccountID;
        }
        userData.Users = await SqlConnection.QueryAsync<UserModel>(
             $"SELECT UserID, UserTempID, AccountID, FirstName, LastName, ImageName, GenderID, Dob, AddedON FROM UserModel WHERE IsActive = 1 AND AccountID = ?  {query} "
             , selectedAccountID
         ).ConfigureAwait(false);
    }

    /// <summary>
    /// Get linked users from database
    /// </summary>
    /// <param name="userData">Reference object to return linked users</param>
    /// <returns>linked users in reference object</returns>
    public async Task GetCareTakersPatientsAsync(UserDTO userData)
    {
        var logedInUser = await SqlConnection.FindWithQueryAsync<UserModel>("SELECT * FROM UserModel WHERE AccountID = ?", userData.AccountID);
        if (logedInUser != null)
        {
            //var UR = await SqlConnection.QueryAsync<UserRelationModel>(
            //    "SELECT * " +
            //    "FROM UserRelationModel UR " +
            //    "WHERE UR.RelationID > 0 AND UR.CareGiverID = ? AND UR.IsActive = 1 " +
            //    "ORDER BY UR.ToDate DESC", logedInUser.UserID
            // ).ConfigureAwait(false);

            //var patientIds = string.Join(",", UR?.Select(x => x.PatientID).Distinct());
            //var U = await SqlConnection.QueryAsync<UserModel>(
            //   "SELECT * " +
            //   "FROM UserModel U " +
            //   $"WHERE U.UserID IN ({patientIds}) AND U.IsActive = 1 AND U.RoleID = ? "
            //   , RoleName.Patient
            //).ConfigureAwait(false);

            //var PatientCareGiverIDs = string.Join(",", UR?.Select(x => x.PatientCareGiverID).Distinct());
            //var PSP = await SqlConnection.QueryAsync<PatientsSharedProgramsModel>(
            //   "SELECT * " +
            //   "FROM PatientsSharedProgramsModel PSP " +
            //   $"WHERE PSP.PatientCareGiverID IN ({PatientCareGiverIDs}) AND PSP.IsActive = 1 "
            //).ConfigureAwait(false);

            //var PatientProgramIDs = string.Join(",", PSP?.Select(x => x.PatientProgramID).Distinct());
            //var PP = await SqlConnection.QueryAsync<PatientProgramModel>(
            //   "SELECT * " +
            //   "FROM PatientProgramModel PP " +
            //   $"WHERE PP.PatientProgramID IN ({PatientProgramIDs}) AND PP.IsActive = 1 "
            //).ConfigureAwait(false);

            //var ProgramIDs = string.Join(",", PP?.Select(x => x.ProgramID).Distinct());
            //var P = await SqlConnection.QueryAsync<ProgramModel>(
            //   "SELECT * " +
            //   "FROM ProgramModel P " +
            //   $"WHERE P.ProgramID IN ({ProgramIDs}) AND P.IsActive = 1 "
            //).ConfigureAwait(false);

            userData.Users = await SqlConnection.QueryAsync<UserModel>(
               "SELECT DISTINCT U.UserID, U.AccountID, U.FirstName, U.LastName, U.ImageName, UR.FromDate, UR.ToDate " +
               "FROM UserRelationModel UR " +
               "JOIN UserModel U ON U.UserID = UR.PatientID AND U.IsActive = 1 AND U.RoleID = ? " +
               "JOIN PatientProgramModel PP ON PP.PatientID = UR.PatientID AND PP.IsActive = 1 " +
               "JOIN ProgramModel P ON P.ProgramID = PP.ProgramID AND P.IsActive = 1 " +
               "JOIN PatientsSharedProgramsModel PSP ON PSP.PatientProgramID = PP.PatientProgramID AND PSP.IsActive = 1 " +
               "WHERE UR.RelationID > 0 AND UR.CareGiverID = ? AND UR.IsActive = 1 " +
               "ORDER BY UR.FromDate DESC",
               RoleName.Patient, logedInUser.UserID
            ).ConfigureAwait(false);

            userData.Users = userData.Users?.Where(c =>
                c.FromDate.Value.Ticks <= GenericMethods.GetUtcDateTime.Ticks &&
                c.ToDate.Value.Ticks >= GenericMethods.GetUtcDateTime.Ticks
            )?.ToList();
        }
    }

    /// <summary>
    /// Get user data from database
    /// </summary>
    /// <param name="userData">object to get user data</param>
    /// <returns>User data with operation status</returns>
    public async Task GetUsersAsync(UserDTO userData)
    {
        userData.Users = await SqlConnection.QueryAsync<UserModel>(
            "SELECT DISTINCT A.PatientID AS UserID, B.ErrorCode, A.PatientCaregiverID, B.GenderID, A.FromDate, A.ToDate, B.FirstName, B.MiddleName, B.LastName, B.ImageName, B.Dob " +
            "FROM UserRelationModel A " +
            "JOIN UserModel B ON A.PatientID = B.UserID OR A.PatientID = B.UserTempID " +
            $"WHERE A.CareGiverID = ? AND A.IsActive = 1 AND B.IsActive = 1 AND B.RoleID = ? " +
            $"ORDER BY A.FromDate DESC", userData.SelectedUserID, RoleName.Patient
        ).ConfigureAwait(false);
        if (userData.IsChatsView)
        {
            userData.Users = userData.Users.Where(c => c.ToDate.Value.Date >= GenericMethods.GetUtcDateTime.Date).ToList();
        }
        else
        {
            userData.Users = userData.Users.Where(c => c.FromDate.Value.Date <= GenericMethods.GetUtcDateTime.Date && c.ToDate.Value.Date >= GenericMethods.GetUtcDateTime.Date).ToList();
        }
        if (userData.RecordCount > 0)
        {
            userData.Users = userData.Users.Take(Convert.ToInt32(userData.RecordCount)).ToList();
        }
        userData.Users = userData.Users?.GroupBy(elem => elem.UserID)?.Select(group => group.First()).ToList();
    }

    /// <summary>
    /// Get user data from database based on user id
    /// </summary>
    /// <param name="userData">object to get user data</param>
    /// <returns>User data with operation status</returns>
    public async Task GetUserAsync(UserDTO userData)
    {
        if (userData.User.RoleID == 0)
        {
            userData.User.RoleID = (byte)Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
        }
        if (userData.User.UserID == 0 && userData.User.AccountID == 0)
        {
            return;
        }
        var idCondition = userData.User.UserID > 0 
            ? $"UserID = {userData.User.UserID}" 
            : (userData.User.IsLinkedUser ? $"UserTempID = {userData.User.UserID}" : $"AccountID = {userData.User.AccountID}");
        userData.User = await SqlConnection.FindWithQueryAsync<UserModel>(
            $"SELECT * FROM UserModel WHERE {idCondition} AND IsActive= 1 AND RoleID = ?", userData.User.RoleID
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Get user data from database based on user id
    /// </summary>
    /// <param name="userData">object to get user data</param>
    /// <returns>User data with operation status</returns>
    public async Task GetUserRelationsAsync(UserDTO userData)
    {
        if (userData.User.RoleID == (byte)RoleName.Patient)
        {
            userData.UserRelations = await SqlConnection.QueryAsync<UserRelationModel>(
               "SELECT DISTINCT A.CareGiverID, A.RelationID, A.PatientCareGiverID , A.FromDate,A.ToDate, B.RoleID As UserID, " +
               "B.FirstName || ' ' || B.LastName AS CareGiverName, " +
               "B.ImageName As FromDateString, A.IsActive as ShowRemoveButton, E.ProgramGroupIdentifier As ProgramColor " +
               "FROM UserRelationModel A " +
               "JOIN UserModel B ON A.CareGiverID = B.UserID " +
               "JOIN PatientProgramModel E ON E.PatientProgramID = C.PatientProgramID " +
               "JOIN PatientsSharedProgramsModel C ON A.PatientCareGiverID = C.PatientCareGiverID AND A.IsActive = C.IsActive " +
               "AND C.PatientProgramID = (SELECT D.PatientProgramID From PatientsSharedProgramsModel D WHERE D.PatientCareGiverID = A.PatientCareGiverID AND D.IsActive = 1 LIMIT 1) " +
               "WHERE A.PatientID = ? AND A.IsActive = 1  AND B.RoleID = ? AND A.RelationID > 0"
               , userData.User.UserID, (byte)RoleName.CareTaker
              ).ConfigureAwait(false);
            userData.UserRelations = userData.UserRelations?.Where(c => c.ToDate.DateTime >= GenericMethods.GetUtcDateTime.DateTime)?.ToList();
        }
    }

    /// <summary>
    /// Save caregiver data in database
    /// </summary>
    /// <param name="caregiverData">object to save caregiversS data</param>
    /// <returns>Operation status</returns>
    public async Task SaveCaregiversAsync(UserDTO caregiverData)
    {
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            transaction.InsertOrReplace(caregiverData.UserRelation);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Get caregiver data from database
    /// </summary>
    /// <param name="caregiverData">object to get caregiver data</param>
    /// <returns>caregiver data with operation status</returns>
    public async Task GetCaregiversAsync(CaregiverDTO caregiverData, bool isChatCaregiver)
    {
        var roleID = Preferences.Get(StorageConstants.PR_ROLE_ID_KEY, 0);
        string conditionalColumns = isChatCaregiver ? string.Empty : ", D.Name AS ProgramName, A.PatientCareGiverID ";
        conditionalColumns += (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
            ? ", E.ProgramGroupIdentifier AS ProgramColor, A.ProgramID "
            : ", D.ProgramGroupIdentifier AS ProgramColor ";
        caregiverData.Caregivers = await SqlConnection.QueryAsync<CaregiverModel>(
            "SELECT DISTINCT A.CareGiverID, A.FromDate, A.ToDate, B.FirstName || ' ' || B.LastName AS FirstName, B.ImageName, PRO.Profession AS Department " +
                $"{conditionalColumns}" +
            "FROM UserRelationModel A " +
            "JOIN UserModel B ON A.CareGiverID = B.UserID AND B.IsActive = 1 " +
            "LEFT JOIN ProgramModel D ON D.ProgramID = A.ProgramID AND D.IsActive = 1 " +
            "LEFT JOIN PatientProgramModel E ON E.ProgramID = D.ProgramID AND E.PatientID = A.PatientID " +
            "LEFT JOIN OrganisationModel C ON B.RoleAtLevelID = C.OrganisationID AND C.IsActive = 1 " +
            "LEFT JOIN UserProfessionModel PRO ON B.ProffessionID = PRO.ProfessionID " +
            "WHERE B.RoleID <> ? AND A.PatientID = ? AND A.IsActive = 1 "
            , RoleName.CareTaker, caregiverData.SelectedUserID
        ).ConfigureAwait(false);
        if (roleID == (int)RoleName.CareTaker && caregiverData.Caregivers?.Count > 0)
        {
            List<PatientProgramModel> sharedPrograms = await GetActiveSharedProgramsAsync(caregiverData).ConfigureAwait(false);
            caregiverData.Caregivers = sharedPrograms?.Count > 0
                 ? caregiverData.Caregivers.Where(x => sharedPrograms.Any(y => y.ProgramID == x.ProgramID))?.ToList()
                 : null;
        }
        if (roleID == (int)RoleName.Patient || roleID == (int)RoleName.CareTaker)
        {
            caregiverData.Caregivers = caregiverData.Caregivers?.Where(c =>
                     c.FromDate.Value.Date <= GenericMethods.GetUtcDateTime.Date &&
                     c.ToDate.Value.Date >= GenericMethods.GetUtcDateTime.Date
                 )?.ToList();
        }
        caregiverData.Caregivers = caregiverData.Caregivers?.OrderByDescending(x => x.FromDate).ThenBy(x => x.FirstName)?.ToList();
        if (caregiverData.RecordCount > 0)
        {
            caregiverData.Caregivers = caregiverData.Caregivers?.Take((int)caregiverData.RecordCount)?.ToList();
        }
    }

    /// <summary>
    /// Get patient's caregiver data from database
    /// </summary>
    /// <param name="caregiverData">object to get patient's caregivers data</param>
    /// <returns>caregiverData with operation status</returns>
    public async Task GetCaregiverAsync(CaregiverDTO caregiverData)
    {
        caregiverData.Organisations = await SqlConnection.QueryAsync<OptionModel>(
            "SELECT OrganisationID AS OptionID, OrganisationName AS OptionText, 1 AS IsSelected FROM OrganisationModel " +
            "WHERE ParentID = 0 AND IsActive = 1 "
        ).ConfigureAwait(false);

        caregiverData.Branches = await SqlConnection.QueryAsync<OptionModel>(
            "SELECT OrganisationID AS OptionID, ParentID AS ParentOptionID, OrganisationName AS OptionText FROM OrganisationModel " +
            "WHERE ParentID <> 0 AND DepartmentID = 0 AND IsActive = 1"
        ).ConfigureAwait(false);

        caregiverData.Departments = await SqlConnection.QueryAsync<OptionModel>(
            "SELECT OrganisationID AS OptionID, ParentID AS ParentOptionID, OrganisationName AS OptionText FROM OrganisationModel " +
            "WHERE DepartmentID <> 0 AND IsActive = 1"
        ).ConfigureAwait(false);

        if (caregiverData.Caregiver.PatientCareGiverID > 0)
        {
            caregiverData.Caregiver = await SqlConnection.FindWithQueryAsync<CaregiverModel>(
                "SELECT A.PatientCareGiverID, A.CareGiverID, A.FromDate, A.ToDate, B.RoleAtLevelID AS OrganisationID" +
                    ", A.ProgramID, P.Name AS ProgramName " +
                "FROM UserRelationModel A " +
                "JOIN UserModel B ON A.CareGiverID = B.UserID " +
                "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
                "WHERE A.PatientCareGiverID = ? ", caregiverData.Caregiver.PatientCareGiverID
            ).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Get caregiver details to patient login
    /// </summary>
    /// <param name="caregiverData">holds patient caregiver id</param>
    /// <returns>patients caregiver data</returns>
    public async Task GetCaregiverDetailsAsync(CaregiverDTO caregiverData)
    {
        caregiverData.Caregiver = await SqlConnection.FindWithQueryAsync<CaregiverModel>(
            "SELECT A.PatientCareGiverID, A.CareGiverID, B.FirstName || ' ' || B.LastName AS FirstName, B.ImageName, A.FromDate" +
                ", A.ToDate, D.Profession AS RoleName, C.OrganisationName AS Department, B.RoleAtLevelID AS OrganisationID" +
                ", A.ProgramID, P.Name AS ProgramName " +
            "FROM UserRelationModel A " +
            "JOIN UserModel B ON A.CareGiverID = B.UserID " +
            "LEFT JOIN OrganisationModel C ON B.RoleAtLevelID = C.OrganisationID AND C.IsActive = 1 " +
            "LEFT JOIN UserProfessionModel D ON B.ProffessionID = D.ProfessionID " +
            "LEFT JOIN ProgramModel P ON P.ProgramID = A.ProgramID " +
            "WHERE A.PatientCareGiverID = ? ", caregiverData.Caregiver.PatientCareGiverID
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Get user data from database for sending it to server
    /// </summary>
    /// <param name="userData">object to get user data</param>
    /// <returns>User data with operation status</returns>
    public async Task GetUsersForSyncToServerAsync(UserDTO userData)
    {
        userData.Users = await SqlConnection.QueryAsync<UserModel>(
            "SELECT * FROM UserModel WHERE IsSynced = 0 ");
    }

    /// <summary>
    /// Update user sync status
    /// </summary>
    /// <param name="requestData">data to update sync status</param>
    public async Task UpdateUserSyncStatusAsync(long userID)
    {
        await SqlConnection.RunInTransactionAsync(transaction =>
        {
            transaction.Execute("UPDATE UserModel SET IsSynced = 1,  WHERE UserID = ?", userID);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Return Shared profiles based on patient Giver id
    /// </summary>
    /// <param name="userData">Data for filter</param>
    /// <returns>Returns caregivers/ users/ and programs</returns>
    public async Task GetShareProfileDataAsync(UserDTO userData)
    {
        if (userData.UserRelation?.PatientCareGiverID > 0)
        {
            userData.User = await SqlConnection.FindWithQueryAsync<UserModel>(
                "SELECT A.FirstName, A.LastName, A.MiddleName, A.PhoneNo,B.CareGiverID AS UserID,A.IsTempPassword, A.ImageName,A.GenderID, A.EmailId, B.RelationID " +
                "FROM UserModel A " +
                "JOIN UserRelationModel B ON A.UserID = B.CareGiverID " +
                "JOIN PatientsSharedProgramsModel C ON C.PatientCareGiverID = B.PatientCareGiverID " +
                "WHERE B.PatientCareGiverID = ? AND B.IsActive = 1  AND B.PatientID = ?"
                , userData.UserRelation.PatientCareGiverID, userData.SelectedUserID
            ).ConfigureAwait(false);

            userData.Programs = await SqlConnection.QueryAsync<PatientProgramModel>(
                "SELECT B.PatientProgramID, A.ProgramID, A.Name As Name, B.PatientID, IFNULL(B.ProgramGroupIdentifier, A.ProgramGroupIdentifier) AS ProgramGroupIdentifier," +
                    " CASE WHEN (SELECT D.PatientProgramID FROM PatientsSharedProgramsModel D WHERE D.PatientProgramID = B.PatientProgramID AND D.IsActive = 1 AND D.PatientCareGiverID = ?) THEN '1' ELSE '0' END AS IsActive " +
                    ", B.ProgramEntryPoint AS ProgramEntryPoint, A.AllowSelfRegistration " +
                "FROM ProgramModel A " +
                "JOIN PatientProgramModel B ON A.ProgramID = B.ProgramID AND B.IsActive = 1 AND B.PatientID = ? " +
                "WHERE A.IsPublished = 1 ORDER BY A.Name "
                , userData.UserRelation?.PatientCareGiverID, userData.SelectedUserID
             ).ConfigureAwait(false);
        }
        else
        {
            userData.Programs = await SqlConnection.QueryAsync<PatientProgramModel>(
                "SELECT B.PatientProgramID, A.ProgramID, A.Name As Name, B.PatientID, " +
                    "IFNULL(B.ProgramGroupIdentifier, A.ProgramGroupIdentifier) AS ProgramGroupIdentifier, " +
                    "'0' AS IsActive, B.ProgramEntryPoint AS ProgramEntryPoint, A.AllowSelfRegistration " +
                "FROM ProgramModel A " +
                "JOIN PatientProgramModel B ON A.ProgramID = B.ProgramID AND B.IsActive = 1 AND B.PatientID = ? " +
                "WHERE A.IsPublished = 1 ORDER BY A.Name "
                , userData.SelectedUserID
            ).ConfigureAwait(false);
        }
    }
}