using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Wangkanai.Extensions;

namespace AlphaMDHealth.WebClient;

public partial class UsersPage : BasePage
{
    private UserDTO _userData = new UserDTO();
    private List<ButtonActionModel> _actions = new List<ButtonActionModel>();
    private string _currentRoute;
    private bool _isDashboardView;
    private bool _isBulkUpload;
    private long _userID;

    // property used for Hiding Organization Profile page which is called from ManageOrganization
    [Parameter]
    public Action HideProfilePage { get; set; }

    /// <summary>
    /// UserID parameter
    /// </summary>
    [Parameter]
    public long UserID
    {
        get { return _userID; }
        set
        {
            if (_userID != value)
            {
                if (_userData.RecordCount > 0 || _userID == 0)
                {
                    _userData.RecordCount = default;
                    if (CurrentView == AppPermissions.LinkedUsersView)
                    {
                        ShowDetailPage = true;
                    }
                }
                _userID = value;
            }
        }
    }

    /// <summary>
    /// CurrentView parameter
    /// </summary>
    [Parameter]
    public AppPermissions CurrentView { get; set; }

    /// <summary>
    /// Flag represents is it manage organization flow
    /// </summary>
    [Parameter]
    public bool IsManageOrganization { get; set; }

    /// <summary>
    /// Organization ID
    /// </summary>
    [Parameter]
    public long OrganizationID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AppState.webEssentials.SetPreferenceValue(StorageConstants.PR_IS_NAVIGATE_TO_EDIT_KEY, false);
        await GetUserDataAsync().ConfigureAwait(true);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_currentRoute != AppState.RouterData.SelectedRoute.Page)
        {
            CurrentView = default;
            await GetUserDataAsync().ConfigureAwait(true);
        }
        else if(IsManageOrganization)
        {
            await GetUserDataAsync().ConfigureAwait(true);
        }
        await base.OnParametersSetAsync();
    }

    private async Task GetUserDataAsync()
    {
        _currentRoute = AppState.RouterData.SelectedRoute.Page;
        if (CurrentView == default)
        {
            CurrentView = _currentRoute.ToEnum<AppPermissions>();
        }
        _userData = new UserDTO()
        {
            ViewFor = CurrentView,
            User = new UserModel()
            {
                SelectedOrganisationID = IsManageOrganization ? OrganizationID : 0
            }
        };
        if (Parameters?.Count > 0)
        {
            _userData.RecordCount = GenericMethods.MapValueType<long>(GetParameterValue(nameof(UserDTO.RecordCount)));
        }
        await SendServiceRequestAsync(new UserService(AppState.webEssentials).GetUsersAsync(_userData), _userData).ConfigureAwait(true);
        _isDashboardView = _userData.RecordCount > 0;
        GetBulkUploadAction();
        _isDataFetched = true;
    }

    private async Task OnViewAllClickedAsync()
    {
        await NavigateToAsync(CurrentView.ToString()).ConfigureAwait(false);
    }

    private void OnAddBulkUserClick()
    {
        _isBulkUpload = true;
        ShowDetailPage = true;
    }

    private async Task OnAddEditClicked(UserModel user)
    {
        Success = Error = string.Empty;
        switch (CurrentView)
        {
            case AppPermissions.UsersView:
                await HandelDetailPageNavigationAsync(user, AppPermissions.UserTabView, OrganizationID).ConfigureAwait(false);
                break;
            case AppPermissions.PatientsView:
                await HandelDetailPageNavigationAsync(user, AppPermissions.PatientTabView, user?.UserID ?? 0).ConfigureAwait(false);
                break;
            case AppPermissions.LinkedUsersView:
                if (_userData.RecordCount > 0)
                {
                    await NavigateToAsync(CurrentView.ToString(), (user?.UserID ?? 0).ToString()).ConfigureAwait(false);
                }
                else
                {
                    _userID = user?.UserID ?? 0;
                    ShowDetailPage = true;
                }
                break;
            default:
                break;
        }
        HideProfilePage?.Invoke();
    }

    private async Task HandelDetailPageNavigationAsync(UserModel user, AppPermissions view, long parameterVal)
    {
        if (SetDataInAppState(user))
        {
            if (user == null)
            {
                _userID = user?.UserID ?? 0;
                ShowDetailPage = true;
            }
            else
            {
                if (_userData.RecordCount > 0)
                {
                    await NavigateToAsync(CurrentView.ToString(), (user?.UserID ?? 0).ToString()).ConfigureAwait(true);
                }
                await NavigateToAsync(view.ToString(), parameterVal.ToString()).ConfigureAwait(false);
            }
        }
    }

    private async Task OnAddEditClosedAsync(string actionResult)
    {
        ShowDetailPage = false;
        _isBulkUpload = false;
        _userID = 0;
        Success = Error = string.Empty;
        if (actionResult == ErrorCode.OK.ToString() || actionResult == ErrorCode.AllExcelDataSuccess.ToString())
        {
            _isDataFetched = false;
            Success = actionResult;
            await GetUserDataAsync();
        }
        else
        {
            Error = actionResult;
        }
    }

    private bool SetDataInAppState(UserModel user)
    {
        if (user == null)
        {
            AppState.webEssentials.SetPreferenceValue(StorageConstants.PR_SELECTED_USER_ID_KEY, 0);
            AppState.UserDetails.User = user;
        }
        else
        {
            if (AppState.webEssentials.GetPreferenceValue(StorageConstants.PR_IS_NAVIGATE_TO_EDIT_KEY, false))
            {
                return false;
            }
            AppState.webEssentials.SetPreferenceValue(StorageConstants.PR_IS_NAVIGATE_TO_EDIT_KEY, true);
            AppState.webEssentials.SetPreferenceValue(StorageConstants.PR_SELECTED_USER_ID_KEY, user.UserID);
            AppState.UserDetails.User = user;
            if (IsManageOrganization)
            {
                AppState.UserDetails.User.SelectedOrganisationID = OrganizationID;
                AppState.UserDetails.User.IsViewedFromOther = true;
            }
            AppState.UserDetails.User.IsUser = CurrentView == AppPermissions.UsersView;
            AppState.UserDetails.Resources = _userData.Resources;
        }
        return true;
    }

    private string GetPageHeader()
    {
        if (IsManageOrganization)
        {
            return RoleName.Admin.ToString();
        }
        else
        {
            return LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, CurrentView.ToString());
        }
    }

    private AppPermissions GetAddEditPage()
    {
        return CurrentView switch
        {
            AppPermissions.UsersView => AppPermissions.UserView,
            AppPermissions.LinkedUsersView => AppPermissions.LinkedUsersView,
            AppPermissions.PatientsView => AppPermissions.PatientView,
            _ => default,
        };
    }

    private bool ShowAddButton()
    {
        AppPermissions addPermission = default;
        switch (CurrentView)
        {
            case AppPermissions.UsersView:
                addPermission = AppPermissions.UserAddEdit;
                break;
            case AppPermissions.LinkedUsersView:
                addPermission = AppPermissions.LinkedUserAddEdit;
                break;
            case AppPermissions.PatientsView:
                addPermission = AppPermissions.PatientAddEdit;
                break;
        }
        if (addPermission != default)
        {
            return LibPermissions.HasPermission(_userData.FeaturePermissions, addPermission.ToString());
        }
        else
        {
            return false;
        }
    }

    private void GetBulkUploadAction()
    {
        bool hasBulkUploadPermission = false;
        if (!IsManageOrganization && !_isDashboardView)
        {
            hasBulkUploadPermission = CurrentView switch
            {
                AppPermissions.UsersView => LibPermissions.HasPermission(_userData.FeaturePermissions, AppPermissions.AddBulkUploadUserView.ToString()),
                AppPermissions.PatientsView => LibPermissions.HasPermission(_userData.FeaturePermissions, AppPermissions.AddBulkUploadPatients.ToString()),
                _ => false,
            };
        }
        _actions = hasBulkUploadPermission
            ? new List<ButtonActionModel> { new ButtonActionModel() { ButtonResourceKey = ResourceConstants.R_ADD_BULK_TEXT_KEY, ButtonAction = OnAddBulkUserClick } }
            : new List<ButtonActionModel>();
    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        List<TableDataStructureModel> tableColumns = new()
        {
            new TableDataStructureModel { DataField = nameof(UserModel.UserID), IsKey = true, IsSearchable = false, IsHidden = true, IsSortable = false },
            new TableDataStructureModel { HasImage = true, ImageSrc = nameof(UserModel.ImageName), ImageHeight = AppImageSize.ImageSizeM, ImageWidth = AppImageSize.ImageSizeM, ImageFieldType = FieldTypes.SquareWithBackgroundImageControl },
            new TableDataStructureModel { DataField =nameof(UserModel.FullName), DataHeader = ResourceConstants.R_FULL_NAME_KEY },
            new TableDataStructureModel { DataField = nameof(UserModel.Gender), DataHeader = ResourceConstants.R_GENDER_KEY }
        };
        if (!_isDashboardView)
        {
            if (CurrentView == AppPermissions.UsersView) // Employee View
            {
                tableColumns.Add(new TableDataStructureModel { DataField = nameof(UserModel.Proffession), DataHeader = ResourceConstants.R_PROFESSION_KEY });
                tableColumns.Add(new TableDataStructureModel { DataField = nameof(UserModel.RoleName), DataHeader = ResourceConstants.R_ROLES_KEY });
            }
            else
            {
                tableColumns.Add(new TableDataStructureModel { DataField = nameof(UserModel.UserAge), DataHeader = ResourceConstants.R_AGE_KEY });
                tableColumns.Add(new TableDataStructureModel { DataField = nameof(UserModel.BloodGroup), DataHeader = ResourceConstants.R_BLOOD_GROUP_KEY });
            }
            if (CurrentView != AppPermissions.LinkedUsersView)
            {
                tableColumns.Add(new TableDataStructureModel { DataField = nameof(UserModel.PhoneNo), DataHeader = ResourceConstants.R_MOBILE_NUMBER_KEY });
                tableColumns.Add(new TableDataStructureModel { DataField = nameof(UserModel.EmailId), DataHeader = ResourceConstants.R_EMAIL_ADDRESS_KEY });
            }
        }
        return tableColumns;
    }
}