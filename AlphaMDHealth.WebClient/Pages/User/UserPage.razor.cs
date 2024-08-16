using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AlphaMDHealth.WebClient;

public partial class UserPage : BasePage
{
    private UserDTO _userData;
    private List<ButtonActionModel> _deleteActionData;
    private List<OptionModel> _departmentList = new();
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;
    private bool _updating;
    private AmhDropdownControl _multiSelectValue = new AmhDropdownControl();
    private IList<ButtonActionModel> _actionButtons = null;
    /// <summary>
    /// UserID parameter
    /// </summary>
    [Parameter]
    public long UserID { get; set; }

    /// <summary>
    /// Organization ID parameter
    /// </summary>
    [Parameter]
    public long OrganizationID { get; set; }

    /// <summary>
    /// CurrentView parameter
    /// </summary>
    [Parameter]
    public AppPermissions CurrentView { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (CurrentView == default)
        {
            CurrentView = AppState.RouterData.SelectedRoute.Page.ToEnum<AppPermissions>();
        }
        _userData = new UserDTO()
        {
            RecordCount = -1,
            ViewFor = CurrentView,
            User = new UserModel
            {
                UserID = SetUserIdBasedOnPage(),
                SelectedOrganisationID = OrganizationID
            }
        };
        await SendServiceRequestAsync(new UserService(AppState.webEssentials).GetUserAsync(_userData), _userData).ConfigureAwait(true);
        if (_userData.ErrCode == ErrorCode.OK)
        {
            _departmentList = _userData.Departments;
            _userData.User.Dob = _userData.User.Dob == DateTimeOffset.MinValue ? null : _userData.User.Dob;
            _userData.User.UserAge = _userData.User.Dob == null ? (byte)0 : _userData.User.UserAge;
            _userData.User.Doj = _userData.User.Doj == DateTimeOffset.MinValue ? null : _userData.User.Doj;
            CheckAddEditPermission();
        }
        _isDataFetched = true;
        if (IsPatientMobileView)
        {
            _actionButtons ??= new List<ButtonActionModel>();
            if (_isEditable)
            {
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_NEXT_ACTION_KEY,
                    ButtonAction = () => { OnSaveButtonClickedAsync(true); },
                    ButtonClass = "mobile-view-button",
                });
            }
            if (CurrentView == AppPermissions.LinkedUsersView || AppState?.Tabs?.Count < 1 || OrganizationID > 0)
            {
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY,
                    ButtonAction = () => { onBackButtonCLick(); },
                    Icon = ImageConstants.PATIENT_MOBILE_BACK_SVG
                });
            }
        }

    }

    private async Task OnCancelClickedAsync()
    {
        await NavigateBackToMainPageAsync(string.Empty);
    }

    private async Task NavigateBackToMainPageAsync(string message)
    {
        switch (CurrentView)
        {
            case AppPermissions.LinkedUsersView:
                await OnClose.InvokeAsync(message);
                break;
            case AppPermissions.PatientView:
                if (AppState?.Tabs?.Count > 0)
                {
                    await NavigateToAsync(AppPermissions.PatientsView.ToString()).ConfigureAwait(false);
                }
                else
                {
                    await OnClose.InvokeAsync(message);
                }
                break;
            case AppPermissions.UserView:
                if (OrganizationID > 0)
                {
                    await NavigateToAsync(AppPermissions.ManageOrganisationView.ToString(), OrganizationID.ToString()).ConfigureAwait(false);
                }
                else if (AppState?.Tabs?.Count > 0)
                {
                    await NavigateToAsync(AppPermissions.UsersView.ToString()).ConfigureAwait(false);
                }
                else
                {
                    await OnClose.InvokeAsync(message);
                }
                break;
            default:
                if (!AppState.MasterData.IsProfileCompleted)
                {
                    AppState.MasterData.IsProfileCompleted = true;
                }
                await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true).ConfigureAwait(false);
                break;
        }
    }

    private void OnSelectedValueChanged(object optionID)
    {
        // Exits if optionID is null, empty, or not a valid long.
        if (!long.TryParse(optionID?.ToString(), out long parsedOptionId))
        {
            return;
        }
        _departmentList = _userData.Departments?.Where(x => x.ParentOptionID == parsedOptionId || x.OptionID == -1)?.ToList();
    }

    private async Task OnSaveButtonClickedAsync(bool isActive)
    {
        if (IsValid())
        {
            UserDTO users = new UserDTO()
            {
                ViewFor = CurrentView,
                SelectedUserID = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
                User = _userData.User,
                IsCompleteProfileFlow = !AppState.MasterData.IsProfileCompleted && CurrentView == AppPermissions.ProfileView
            };
            users.User.OrganisationID = IsUserAddEdit()
                    ? GetSelectedID(_userData.Organisations)
                : _userData.User.OrganisationID == 0
                    ? AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_SELECTED_ORGANISATION_ID_KEY, 0)
                : _userData.User.OrganisationID;
            users.User.RoleAtLevelID = GetRoleAtLevelID();
            users.User.OrganisationDomain = AppState.MasterData.OrganisationDomain;
            users.User.GenderID = _userData.Genders?.FirstOrDefault(x => x.IsSelected)?.GroupName ?? string.Empty;
            users.User.RoleID = IsUserAddEdit()
                ? (byte)GetSelectedID(_userData.Roles)
                : (byte)RoleName.Patient;
            users.User.ProffessionID = (byte)GetSelectedID(_userData.Professions);
            users.User.BloodGroupID = (short)GetSelectedID(_userData.BloodGroups);
            users.User.PrefferedLanguageID = (byte)(users.IsCompleteProfileFlow ? _userData.Languages?.FirstOrDefault()?.OptionID ?? 0 : GetSelectedID(_userData.Languages));
            users.User.IsActive = isActive;
            users.User.IsLinkedUser = CurrentView == AppPermissions.LinkedUsersView;  //todo: to remove 

            await SaveUserAsync(users).ConfigureAwait(false);
        }
    }

    private bool IsUserAddEdit()
    {
        return CurrentView == AppPermissions.UserView || (CurrentView == AppPermissions.ProfileView && _userData.User.RoleID != (byte)RoleName.Patient);
    }

    private async Task SaveUserAsync(UserDTO userDTO)
    {
        await SendServiceRequestAsync(new UserService(AppState.webEssentials).SyncUserToServerAsync(userDTO, CancellationToken.None), userDTO).ConfigureAwait(true);
        if (userDTO.ErrCode == ErrorCode.OK)
        {
            Success = userDTO.ErrCode.ToString();
            await NavigateBackToMainPageAsync(userDTO.ErrCode.ToString());
        }
        else
        {
            Error = userDTO.ErrCode.ToString();
            StateHasChanged();
        }
    }

    private long GetRoleAtLevelID()
    {
        long orgID = 0;
        if (CurrentView == AppPermissions.UserView)
        {
            orgID = GetSelectedID(_userData.Organisations);
            if (orgID > 0)
            {
                long branchID = GetSelectedID(_userData.Branches);
                if (branchID > 0)
                {
                    long deptID = GetSelectedID(_departmentList);
                    return deptID > 0 ? deptID : branchID;
                }
            }
        }
        return orgID;
    }

    private void OnRemoveClick()
    {
        _deleteActionData = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task PopUpCallbackAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 2)
            {
                _userData.IsActive = _userData.User.IsActive = false;
                await SaveUserAsync(_userData).ConfigureAwait(true); ;
            }
        }
    }

    private async Task OnChangePasswordLinkClickAsync()
    {
        await NavigateToAsync(AppPermissions.ChangePasswordView.ToString()).ConfigureAwait(false);
    }

    private long SetUserIdBasedOnPage()
    {
        return CurrentView switch
        {
            AppPermissions.UserView or AppPermissions.PatientView => AppState.webEssentials.GetPreferenceValue(StorageConstants.PR_SELECTED_USER_ID_KEY, 0),
            AppPermissions.ProfileView => AppState.MasterData?.Users?.FirstOrDefault()?.UserID ?? UserID,
            _ => UserID,
        };
    }

    private void CheckAddEditPermission()
    {
        AppPermissions addEditPermission = GetAddEditPermission();
        _isEditable = (addEditPermission != default) && LibPermissions.HasPermission(_userData.FeaturePermissions, addEditPermission.ToString());
    }

    private AppPermissions GetAddEditPermission()
    {
        return CurrentView switch
        {
            AppPermissions.UserView => AppPermissions.UserAddEdit,
            AppPermissions.PatientView => AppPermissions.PatientAddEdit,
            AppPermissions.LinkedUsersView => AppPermissions.LinkedUserAddEdit,
            AppPermissions.ProfileView => AppPermissions.ProfileAddEdit,
            _ => default
        };
    }

    private string GetPageTitle()
    {
        var permission = GetAddEditPermission();
        string pageTitle = string.Empty;
        if (permission != default)
        {
            if (AppState.MasterData.IsProfileCompleted == false && permission == AppPermissions.ProfileAddEdit)
            {
                pageTitle = LibResources.GetResourceValueByKey(_userData.Resources, ResourceConstants.R_ADDITIONAL_PROFILE_DATA_KEY);
            }
            else
            {
                pageTitle = LibPermissions.GetFeatureText(AppState.MasterData.OrganisationFeatures, permission.ToString());
            }
        }
        return pageTitle;
    }

    private void OnDobAgeChanged(object value)
    {
        if (!_updating)
        {
            if (value is DateTimeOffset newDob)
            {
                _updating = true;
                _userData.User.Dob = newDob;
                UpdateAge();
            }
            else if (value is byte newAge)
            {
                _userData.User.UserAge = newAge;
                UpdateDob();
            }
        }
        else
        {
            _updating = !_updating;
        }
    }
    private void UpdateAge()
    {
        _userData.User.UserAge = GetAgeFromDateOfBirth(_userData.User.Dob);
    }
    private void UpdateDob()
    {
        _userData.User.Dob = GetDobFromAge(_userData.User.UserAge);
    }
    private byte GetAgeFromDateOfBirth(DateTimeOffset? dob)
    {
        byte age = (byte)(DateTime.Now.Year - dob.Value.Year);
        if (DateTime.Now < dob.Value.AddYears(age)) age--;
        return age;
    }
    private DateTimeOffset GetDobFromAge(int age)
    {
        DateTimeOffset dob = DateTime.Now.AddYears(-age);
        return dob;
    }
    private async void onBackButtonCLick()
    {
        await JsRuntime.InvokeVoidAsync("invokeWebviewMethod", "backactionclicked", "200");
    }

}