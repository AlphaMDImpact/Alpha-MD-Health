using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Dapper;
using System.Data;
using System.Globalization;

namespace AlphaMDHealth.ServiceDataLayer;

public class UserServiceDataLayer : BaseServiceDataLayer
{
    /// <summary>
    /// Get Users Temp Token
    /// </summary>
    /// <param name="userData">object to hold user data</param>
    /// <returns>Temp Token based on ID</returns>
    public async Task GetPatientTempTokenByIDAsync(AuthDTO userData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        MapDeviceDataInSpInputParams(userData.Session, parameters);
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), userData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(UserModel.AccountID)), userData.AccountID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(BaseDTO.ErrCode)), userData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        userData.TempSession = await connection.QueryFirstOrDefaultAsync<TempSessionModel>(SPNameConstants.USP_CREATE_PATIENT_TEMP_TOKEN, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        userData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    /// <summary>
    /// Save admin user data in database
    /// </summary>
    /// <param name="userData">object to save admin data</param>
    /// <returns>User token data and operation status</returns>
    public async Task RegisterUserAsync(UserDTO userData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserModel.FirstName), userData.User.FirstName, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserModel.LastName), userData.User.LastName, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserModel.IsUser), userData.User.IsUser, DbType.Boolean, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserAccountModel.PhoneNo), userData.User.PhoneNo, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserAccountModel.EmailId), userData.User.EmailId.ToLowerInvariant(), DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserAccountModel.AccountPassword), userData.User.AccountPassword, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserAccountModel.Otp), userData.User.Otp, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserAccountModel.RememberMe), userData.User.RememberMe, DbType.Boolean, ParameterDirection.Input);
        MapDeviceDataInSpInputParams(userData.Session, parameter);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), userData.OrganisationID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID), userData.AccountID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode), userData.ErrCode, DbType.Int16, direction: ParameterDirection.Output);
        userData.Session = await connection.QueryFirstOrDefaultAsync<SessionModel>(SPNameConstants.USP_REGISTER_USER, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        userData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        userData.User.AccountID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.AccountID));
    }

    /// <summary>
    /// Get users from database
    /// </summary>
    /// <param name="userData">object to hold user data</param>
    /// <returns>Users data and operation status</returns>
    public async Task GetUsersAsync(UserDTO userData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameters = new DynamicParameters();
        parameters.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), userData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(UserModel.UserID)), userData.User.UserID, DbType.Int64, ParameterDirection.Input);
        parameters.Add(ConcateAt(nameof(UserModel.SelectedOrganisationID)), userData.User.SelectedOrganisationID, DbType.Int64, ParameterDirection.Input);
        MapCommonSPParameters(userData, parameters, userData.ViewFor.ToString(), ReturnUserPermission(userData));
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_USERS, parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            switch (userData.ViewFor)
            {
                case AppPermissions.ProfileView:
                case AppPermissions.UsersView:
                case AppPermissions.UserView:
                    if (userData.RecordCount == -1)
                    {
                        userData.Languages = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
                        userData.Professions = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
                        userData.OrganisationTags = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);//
                        await MapOrganizationBranchesDepartmentsAsync(userData, result).ConfigureAwait(false);
                        userData.Roles = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
                    }
                    break;
                case AppPermissions.PatientsView:
                case AppPermissions.PatientView:
                    if (userData.RecordCount == -1)
                    {
                        userData.Languages = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
                        userData.OrganisationTags = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
                    }
                    break;
            }
            userData.Users = await MapTableDataAsync<UserModel>(result).ConfigureAwait(false);
            await MapReturnPermissionsAsync(userData, result).ConfigureAwait(false);
        }
        userData.ErrCode = GenericMethods.MapDBErrorCodes(parameters.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Retirieve);
    }

    /// <summary>
    /// Save user data in database
    /// </summary>
    /// <param name="userData">object to save data</param>
    /// <returns>Operation status</returns>
    public async Task SaveUserAsync(UserDTO userData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        if (userData.RecordCount == -13)
        {
            parameter.Add(ConcateAt(SPFieldConstants.FIELD_DETAIL_RECORDS), MapPatientSharedProgramsToTable(userData.PatientsSharedPrograms).AsTableValuedParameter());
        }
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), userData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.RelationID)), userData.User.RelationID, DbType.Int16, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.FirstName)), userData.User.FirstName, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.MiddleName)), userData.User.MiddleName, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.LastName)), userData.User.LastName, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.Doj)), userData.User.Doj, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.Dob)), userData.User.Dob, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.GenderID)), userData.User.GenderID, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.UserDegrees)), userData.User.UserDegrees, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.BloodGroupID)), userData.User.BloodGroupID, DbType.Int16, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.PrefferedLanguageID)), userData.User.PrefferedLanguageID, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.SocialSecurityNo)), userData.User.SocialSecurityNo, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.RoleID)), userData.User.RoleID, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.ProffessionID)), userData.User.ProffessionID, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.PhoneNo)), userData.User.PhoneNo, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.EmailId)), userData.User.EmailId?.ToLowerInvariant(), DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.AccountPassword)), userData.User.AccountPassword, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.IsTempPassword)), userData.User.IsTempPassword, DbType.Boolean, ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(nameof(UserModel.ImageName)), userData.User.ImageName, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.HospitalIdenfier)), userData.User.HospitalIdenfier, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.GeneralMedicalIdenfier)), userData.User.GeneralMedicalIdenfier, DbType.String, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.RoleAtLevelID)), userData.User.RoleAtLevelID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.IsLinkedUser)), userData.User.IsLinkedUser, DbType.Boolean, ParameterDirection.Input);//todo:toremove
        parameter.Add(ConcateAt(nameof(UserDTO.SelectedUserID)), userData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.OrganisationID)), userData.User.OrganisationID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(nameof(UserModel.OrganisationDomain)), string.Empty, DbType.String, ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(nameof(UserModel.UserID)), userData.User.UserID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_LINKED_ACCOUNT_ID), userData.User.AccountID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(SPFieldConstants.FIELD_RETURN_PERMISSIONS_FOR), string.Empty, DbType.String, ParameterDirection.Input);
        MapCommonSPParameters(userData, parameter, CheckAddEditPermission(userData.ViewFor));
        await connection.ExecuteAsync(SPNameConstants.USP_SAVE_USER, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        userData.User.UserID = parameter.Get<long>(ConcateAt(nameof(userData.User.UserID)));
        userData.User.OrganisationID = parameter.Get<long>(ConcateAt(nameof(userData.User.OrganisationID)));
        userData.User.OrganisationDomain = parameter.Get<string>(ConcateAt(nameof(userData.User.OrganisationDomain)));
        userData.User.IsTempPassword = parameter.Get<bool>(ConcateAt(nameof(userData.User.IsTempPassword)));
        userData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Save);
    }

    private DataTable MapPatientSharedProgramsToTable(List<PatientsSharedProgramsModel> userRelations)
    {
        DataTable dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture,
            Columns =
            {
                new DataColumn(nameof(PatientsSharedProgramsModel.PatientCareGiverID), typeof(long)),
                new DataColumn(nameof(PatientsSharedProgramsModel.PatientProgramID), typeof(long)),
                new DataColumn(nameof(PatientsSharedProgramsModel.IsActive), typeof(bool)),
            }
        };
        foreach (PatientsSharedProgramsModel record in userRelations)
        {
            dataTable.Rows.Add(record.PatientCareGiverID, record.PatientProgramID, record.IsActive);
        }
        return dataTable;
    }

    /// <summary>
    /// Delete user data from database
    /// </summary>
    /// <param name="userData">object to delete user data</param>
    /// <returns>Operation status</returns>
    public async Task DeleteUserAsync(UserDTO userData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), userData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.UserID)), userData.User.UserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(UserModel.IsActive)), userData.User.IsActive, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(userData, parameter, CheckDeletePermission(userData.ViewFor));
        await connection.ExecuteAsync(SPNameConstants.USP_DELETE_USER, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        userData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(ConcateAt(nameof(BaseDTO.ErrCode))), OperationType.Delete);
    }

    private string CheckAddEditPermission(AppPermissions viewFor)
    {
        return viewFor switch
        {
            AppPermissions.UserView => AppPermissions.UserAddEdit.ToString(),
            AppPermissions.LinkedUsersView => AppPermissions.LinkedUserAddEdit.ToString(),
            AppPermissions.PatientView => AppPermissions.PatientAddEdit.ToString(),
            _ => AppPermissions.ProfileAddEdit.ToString(),
        };
    }

    private string CheckDeletePermission(AppPermissions viewFor)
    {
        return viewFor switch
        {
            AppPermissions.UserView => AppPermissions.UserDelete.ToString(),
            AppPermissions.LinkedUsersView => AppPermissions.LinkedUserDelete.ToString(),
            AppPermissions.PatientView => AppPermissions.PatientDelete.ToString(),
            _ => AppPermissions.PatientDelete.ToString(),

        };
    }

    /// <summary>
    /// Get user data to send activation link
    /// </summary>
    /// <param name="userData">object to hold user data</param>
    /// <returns>Operation status with user data</returns>
    public async Task ResendActivationAsync(UserDTO userData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), userData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserModel.UserID), userData.User.UserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserAccountModel.AccountPassword), userData.User.AccountPassword, DbType.String, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(UserAccountModel.IsTempPassword), userData.User.IsTempPassword, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(userData, parameter, userData.User.IsUser ? AppPermissions.UserAddEdit.ToString() : AppPermissions.PatientAddEdit.ToString());
        UserModel user = await connection.QueryFirstOrDefaultAsync<UserModel>(SPNameConstants.USP_UPDATE_PASSWORD, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        userData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
        if (userData.ErrCode == ErrorCode.OK)
        {
            userData.User.PhoneNo = user.PhoneNo;
            userData.User.FirstName = user.FirstName;
            userData.User.EmailId = user.EmailId;
            userData.User.OrganisationDomain = user.OrganisationDomain;
            userData.OrganisationID = userData.User.OrganisationID = user.OrganisationID;
        }
    }

    /// <summary>
    /// Get patient caregiver(s) from database
    /// </summary>
    /// <param name="caregiverData">object to hold caregiver data</param>
    /// <returns>Caregivers data and operation status</returns> 
    public async Task GetPatientCaregiversAsync(CaregiverDTO caregiverData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), caregiverData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.PatientCareGiverID), caregiverData.Caregiver.PatientCareGiverID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.SelectedUserID), caregiverData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        MapCommonSPParameters(caregiverData, parameter, AppPermissions.CaregiversView.ToString(), $"{AppPermissions.CaregiverAddEdit},{AppPermissions.CaregiverDelete}");
        SqlMapper.GridReader result = await connection.QueryMultipleAsync(SPNameConstants.USP_GET_PATIENT_CAREGIVERS, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        if (result.HasRows())
        {
            if (caregiverData.RecordCount == -11)
            {
                caregiverData.CaregiverOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
            }
            else if (caregiverData.RecordCount == -1)
            {
                await MapOrganisationDataAsync(caregiverData, result).ConfigureAwait(false);
            }
            else
            {
                caregiverData.Caregivers = (await result.ReadAsync<CaregiverModel>().ConfigureAwait(false))?.ToList();
            }
            await MapReturnPermissionsAsync(caregiverData, result).ConfigureAwait(false);
        }
        caregiverData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Retirieve);
    }

    /// <summary>
    /// Save patient caregiver in database
    /// </summary>
    /// <param name="caregiverData">object to save data</param>
    /// <returns>Operation status</returns>
    public async Task SavePatientCaregiverAsync(CaregiverDTO caregiverData)
    {
        using var connection = ConnectDatabase();
        connection.Open();
        DynamicParameters parameter = new DynamicParameters();
        parameter.Add(ConcateAt(nameof(BaseDTO.FeatureFor)), caregiverData.FeatureFor, DbType.Byte, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.PatientCareGiverID), caregiverData.Caregiver.PatientCareGiverID, DbType.Int64, ParameterDirection.InputOutput);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.FromDate), caregiverData.Caregiver.FromDate.Value.Date, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.ToDate), caregiverData.Caregiver.ToDate.Value.Date, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.OrganisationID), caregiverData.Caregiver.OrganisationID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.SelectedUserID), caregiverData.SelectedUserID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.CareGiverID), caregiverData.Caregiver.CareGiverID, DbType.Int64, ParameterDirection.Input);
        parameter.Add(ConcateAt(nameof(BaseDTO.LastModifiedON)), caregiverData.Caregiver.LastModifiedON, DbType.DateTimeOffset, ParameterDirection.Input);
        parameter.Add(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.IsActive), caregiverData.IsActive, DbType.Boolean, ParameterDirection.Input);
        MapCommonSPParameters(caregiverData, parameter, caregiverData.IsActive ? AppPermissions.PatientAddEdit.ToString() : AppPermissions.CaregiverDelete.ToString());
        await connection.ExecuteAsync(SPNameConstants.USP_SAVE_PATIENT_CAREGIVER, parameter, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        caregiverData.ErrCode = GenericMethods.MapDBErrorCodes(parameter.Get<short>(Constants.SYMBOL_AT_THE_RATE + nameof(BaseDTO.ErrCode)), OperationType.Save);
        if (caregiverData.ErrCode == ErrorCode.OK)
        {
            caregiverData.Caregiver.PatientCareGiverID = parameter.Get<long>(Constants.SYMBOL_AT_THE_RATE + nameof(CaregiverModel.PatientCareGiverID));
        }
    }

    private string ReturnUserPermission(UserDTO userData)
    {
        return userData.ViewFor switch
        {
            AppPermissions.UsersView or AppPermissions.UserView => $"{AppPermissions.UserAddEdit},{AppPermissions.AddBulkUploadUserView},{AppPermissions.UserDelete},{AppPermissions.UserResendActivation}",
            AppPermissions.PatientsView or AppPermissions.PatientView => $"{AppPermissions.PatientAddEdit},{AppPermissions.AddBulkUploadPatients},{AppPermissions.PatientDelete},{AppPermissions.UserResendActivation}",
            AppPermissions.LinkedUsersView => $"{AppPermissions.LinkedUserAddEdit},{AppPermissions.LinkedUserDelete}",
            _ => $"{AppPermissions.ProfileAddEdit},{AppPermissions.ChangePasswordView}",
        };
    }

    private async Task MapOrganisationDataAsync(CaregiverDTO caregiverData, SqlMapper.GridReader result)
    {
        await MapOrganizationBranchesDepartmentsAsync(caregiverData, result).ConfigureAwait(false);
        if (!result.IsConsumed)
        {
            caregiverData.CaregiverOptions = (await result.ReadAsync<OptionModel>().ConfigureAwait(false))?.ToList();
        }
        if (caregiverData.Caregiver.PatientCareGiverID > 0)
        {
            caregiverData.Caregiver = (await result.ReadAsync<CaregiverModel>().ConfigureAwait(false))?.FirstOrDefault();
        }
    }

    private async Task MapOrganizationBranchesDepartmentsAsync(UsersDTO caregiverData, SqlMapper.GridReader result)
    {
        caregiverData.Organisations = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
        caregiverData.Branches = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
        caregiverData.Departments = await MapTableDataAsync<OptionModel>(result).ConfigureAwait(false);
    }
}