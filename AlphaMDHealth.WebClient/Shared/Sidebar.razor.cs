using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;

namespace AlphaMDHealth.WebClient;

public partial class Sidebar
{
    private List<IGrouping<long, OrganisationCollapsibleModel>> _groupItems;
    private bool _isOrgTreeOpen;
    private NavigationComponent _navigator = new NavigationComponent();
    private List<MenuModel> _organisationMenus;
    private List<MenuModel> _branchMenus;
    private List<MenuModel> _deptMenus;
    private bool _isOrganisation;
    private bool _isBranch;
    private bool _isDepartment;
    private bool _orgAccordion = true;
    private long _selectedBranch;
    private bool _isOpen;
    private string _selectedMenuTitle;
    private bool isBaseMenu;
    private MenuModel _selectedMenu;

    /// <summary>
    /// Flag representing Sidebar expanded or not
    /// </summary>
    [Parameter]
    public bool SidebarExpanded { get; set; } = true;

    /// <summary>
    /// bindable property of SidebarExpanded
    /// </summary>
    [Parameter]
    public EventCallback<bool> SidebarExpandedChanged { get; set; }

    protected override void OnInitialized()
    {
        InitializeSideBarMenus();
        NavRefreshServ.RefreshRequested += NavRefreshServ_RefreshRequested;
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (AppState.MasterData?.Menus?.FirstOrDefault(x => x.IsActive)?.MenuLocation != MenuLocation.Left)
        {
            //_selectedMenu = null;
            _organisationMenus?.ForEach(x => x.IsActive = false);
            _branchMenus?.ForEach(x => x.IsActive = false);
            _deptMenus?.ForEach(x => x.IsActive = false);
        }
        StateHasChanged();
        base.OnParametersSet();
    }

    private void InitializeSideBarMenus()
    {
        if (AppState.MasterData != null)
        {
            if (AppState.MasterData.SelectedUserID == 0 && GenericMethods.IsListNotEmpty(AppState.MasterData.Users))
            {
                AppState.MasterData.SelectedUserID = AppState.MasterData.Users.FirstOrDefault().UserID;
            }
            UserModel selectedUser = GetSelectedUser(x => x.UserID == AppState.MasterData.SelectedUserID && x.IsActive);
            if (selectedUser == null)
            {
                // Will be null incase of organisation setup is not yet complete as user does not have any role in any organisation
            }
            else if (selectedUser.RoleAtLevelID == AppState.MasterData.OrganisationID)
            {
                _isOrganisation = true;
                _selectedMenuTitle = AppState.MasterData.OrganisationName;
                _groupItems = GetGroupItems();
                if (AppState.MasterData.Menus != null)
                {
                    _organisationMenus = Clone(new List<MenuModel>(GetAppropriateMenus(1)));
                    _branchMenus = Clone(new List<MenuModel>(GetAppropriateMenus(2)));
                    _deptMenus = Clone(new List<MenuModel>(GetAppropriateMenus(3)));
                }
            }
            else if (CheckForBranchDepartment(x => x.BranchID == selectedUser.RoleAtLevelID) != null)
            {
                BranchMenuClickAsync(AppState.MasterData.BranchDepartments.FirstOrDefault(x => x.BranchID == selectedUser.RoleAtLevelID).BranchID).ConfigureAwait(true);
            }
            else if (CheckForBranchDepartment(x => x.DepartmentID == selectedUser.RoleAtLevelID) != null)
            {
                DepartmentMenuClickAsync(AppState.MasterData.BranchDepartments.FirstOrDefault(x => x.DepartmentID == selectedUser.RoleAtLevelID).DepartmentID).ConfigureAwait(true);
            }
            isBaseMenu = true;
        }
    }

    private async Task OnOrganisationFeatureMenuClickAsync(MenuModel menuItem)
    {
        //_selectedBranch = 0;
        _organisationMenus.ForEach(x => x.IsActive = false);
        var menu = _organisationMenus.Find(x => x.TargetID == menuItem.TargetID && menuItem.MenuLocation == MenuLocation.Left);
        if (menu != null)
        {
            menu.IsActive = true;
        }
        var selectedMenu = AppState.RouterData.Routes.FirstOrDefault(x => x.FeatureId == menuItem.TargetID);
        _isOrgTreeOpen = false;
        if (selectedMenu != null)
        {
            AppState.MasterData.Menus.ForEach(x =>
            {
                x.IsActive = x.TargetID == menuItem.TargetID;
            });
            //AppState.MasterData.Menus.ForEach(x => x.IsActive = false);
            //_selectedMenu = AppState.MasterData.Menus.FirstOrDefault(x => x.TargetID == menuItem.TargetID && x.MenuLocation == menuItem.MenuLocation);
            //if (_selectedMenu != null)
            //{
            //    AppState.MasterData.Menus.FirstOrDefault(x => x.TargetID == menuItem.TargetID && x.MenuLocation == menuItem.MenuLocation).IsActive = true;
            //}
            AppState.UpdateOrgDetails(AppState.MasterData.OrganisationName, AppState.MasterData.OrganisationDomain, AppState.MasterData.AddedON ?? GenericMethods.GetDefaultDateTime);
            await _navigator.SetSelectedPageAsync(selectedMenu, 0, true).ConfigureAwait(false);
        }
        else
        {
            AppState.SelectedTabTitle = menuItem.PageHeading;
            StateHasChanged();
            await _navigator.NavigateToAsync(AppPermissions.StaticMessageView.ToString(), ErrorCode.NotImplemented.ToString()).ConfigureAwait(false);
        }
    }

    private async Task OrganisationMenuClick()
    {
        if (_isOrganisation)
        {
            if (_isOpen)
            {
                await OnBackbuttonClickedAsync();
            }
            else
            {
                _isOrganisation = true;
                _isOpen = true;
                _isBranch = false;
                _isDepartment = false;
                isBaseMenu = false;
                AppState.webEssentials.SetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, AppState.MasterData.OrganisationID);
                _selectedMenuTitle = AppState.MasterData.OrganisationName;
                _groupItems = GetGroupItems();
                var selectedMenu = AppState.RouterData.Routes.FirstOrDefault(x => x.Page == AppPermissions.OrganisationTabView.ToString());
                if (selectedMenu != null)
                {

                    AppState.UpdateOrgDetails(AppState.MasterData.OrganisationName, AppState.MasterData.OrganisationDomain, AppState.MasterData.AddedON ?? GenericMethods.GetDefaultDateTime);
                    ///await _navigator.SetSelectedPageAsync(selectedMenu, 0).ConfigureAwait(false);
                }
                else
                {
                    StateHasChanged();
                }
            }
        }
    }

    private async Task BranchMenuClickAsync(long e)
    {
        if (_isBranch && _isOpen)
        {
            await OnBackbuttonClickedAsync();
        }
        else
        {
            _isOrganisation = false;
            _isBranch = true;
            _isDepartment = false;
            _isOpen = true;
            UserModel selectedUser = GetSelectedUser(x => x.UserID == AppState.MasterData.SelectedUserID);
            isBaseMenu = CheckForBranchDepartment(x => x.BranchID == selectedUser.RoleAtLevelID) != null;
            _organisationMenus = Clone(new List<MenuModel>(GetAppropriateMenus(2)));
            var selected = AppState.MasterData.BranchDepartments.FirstOrDefault(x => x.BranchID == e);
            _groupItems = AppState.MasterData.BranchDepartments?.Where(x => x.BranchID == selected?.BranchID && !string.IsNullOrWhiteSpace(x.DepartmentName))?.GroupBy(x => x.DepartmentID)?.ToList();
            if (selected != null)
            {
                _selectedBranch = e;
                AppState.webEssentials.SetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, e);
                selected.IsOpen = true;
                _selectedMenuTitle = selected.BranchName;
            }
            var selectedMenu = AppState.RouterData.Routes.FirstOrDefault();
            //var selectedMenu = AppState.RouterData.Routes.FirstOrDefault(x => x.Page == AppPermissions.DashboardView.ToString()) ?? AppState.RouterData.Routes.FirstOrDefault(); // x => x.Page == AppPermissions.DashboardView.ToString());
            if (selectedMenu != null)
            {
                AppState.UpdateOrgDetails(selected.BranchName, AppState.MasterData.OrganisationDomain, selected.AddedON);
                await _navigator.SetSelectedPageAsync(selectedMenu, 0).ConfigureAwait(false);
                await _navigator.NavigateToAsync(selectedMenu.Page).ConfigureAwait(false);
            }
            else
            {
                StateHasChanged();
            }
        }
    }

    private async Task DepartmentMenuClickAsync(long e)
    {
        if (_isDepartment && _isOpen)
        {
            await OnBackbuttonClickedAsync();
        }
        else
        {
            _organisationMenus = Clone(new List<MenuModel>(GetAppropriateMenus(3)));
            _isOrganisation = false;
            _isBranch = false;
            _isDepartment = true;
            _isOpen = true;
            UserModel selectedUser = GetSelectedUser(x => x.UserID == AppState.MasterData.SelectedUserID);
            isBaseMenu = CheckForBranchDepartment(x => x.DepartmentID == selectedUser.RoleAtLevelID) != null;

            var selected = AppState.MasterData.BranchDepartments.FirstOrDefault(x => x.DepartmentID == e);
            if (selected != null)
            {
                AppState.webEssentials.SetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, e);
                selected.IsOpen = !selected.IsOpen;
                _selectedMenuTitle = selected.DepartmentName;
            }
            _groupItems?.Clear();
            var selectedMenu = AppState.RouterData.Routes.FirstOrDefault(x => x.Page == AppPermissions.DashboardView.ToString()); // x => x.Page == AppPermissions.DashboardView.ToString());
            if (selectedMenu != null)
            {
                _organisationMenus.FirstOrDefault(x => x.TargetID == selectedMenu.FeatureId).IsActive = true;
                await _navigator.SetSelectedPageAsync(selectedMenu, 0).ConfigureAwait(false);
                await _navigator.NavigateToAsync(selectedMenu.Page).ConfigureAwait(false);
            }
            else
            {
                StateHasChanged();
            }
        }
    }

    private void BranchMenuExpandedChanged(bool isExpanded, long id)
    {
    }

    private void DepartmentMenuExpandedChanged(bool isExpanded, long id)
    {
    }


    private async Task OnBackbuttonClickedAsync()
    {
        if (_isOpen && _isOrganisation)
        {
            _isOpen = false;
        }
        else if (_isBranch)
        {
            _isBranch = false;
            _isOrganisation = true;
            _isOpen = false;
            AppState.webEssentials.SetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, AppState.MasterData.OrganisationID);
            _selectedMenuTitle = AppState.MasterData.OrganisationName;
            _groupItems = GetGroupItems();
            _organisationMenus = Clone(new List<MenuModel>(GetAppropriateMenus(1)));

            var selectedMenu = AppState.RouterData.Routes.FirstOrDefault(x => x.FeatureId == _organisationMenus.FirstOrDefault().TargetID);
            if (selectedMenu != null)
            {
                _organisationMenus.FirstOrDefault(x => x.TargetID == selectedMenu.FeatureId).IsActive = true;
                AppState.UpdateOrgDetails(AppState.MasterData.OrganisationName, AppState.MasterData.OrganisationDomain, AppState.MasterData.AddedON ?? GenericMethods.GetDefaultDateTime);
                await _navigator.SetSelectedPageAsync(selectedMenu, 0).ConfigureAwait(false);
            }
        }
        else if (_isDepartment)
        {
            _isBranch = true;
            _isDepartment = false;
            await BranchMenuClickAsync(_selectedBranch);
        }
        else
        {
            //future implmemtation
        }
    }

    private OrganisationCollapsibleModel? CheckForBranchDepartment(Func<OrganisationCollapsibleModel, bool> predicate)
    {
        return AppState.MasterData?.BranchDepartments?.FirstOrDefault(predicate);
    }

    private IEnumerable<MenuModel> GetAppropriateMenus(int permissionLevel)
    {
        return AppState?.MasterData?.Menus?.Where(x => x.MenuLocation == MenuLocation.Left && (
            (permissionLevel == 1 && x.AvailableAtOrganisationLevel) ||
            (permissionLevel == 2 && x.AvailableAtBranchLevel) ||
            (permissionLevel == 3 && x.AvailableAtDepartmentLevel)));
    }

    private void NavRefreshServ_RefreshRequested()
    {
        UserModel selectedUser = GetSelectedUser(x => x.UserID == AppState.MasterData.SelectedUserID);
        if (selectedUser?.RoleAtLevelID == AppState.MasterData.OrganisationID && _isOrganisation)
        {
            _organisationMenus = Clone(new List<MenuModel>(GetAppropriateMenus(1)));
        }
        else if (CheckForBranchDepartment(x => x.BranchID == selectedUser?.RoleAtLevelID) != null || _isBranch)
        {
            _organisationMenus = Clone(new List<MenuModel>(GetAppropriateMenus(2)));
        }
        else if (CheckForBranchDepartment(x => x.DepartmentID == selectedUser?.RoleAtLevelID) != null || _isDepartment)
        {
            _organisationMenus = Clone(new List<MenuModel>(GetAppropriateMenus(3)));
        }
        else
        {
            //future implmemtation
        }
        InvokeAsync(StateHasChanged);
    }

    private List<IGrouping<long, OrganisationCollapsibleModel>> GetGroupItems()
    {
        return AppState.MasterData.BranchDepartments?.Where(x => !string.IsNullOrWhiteSpace(x.BranchName))?.GroupBy(x => x.BranchID)?.ToList();
    }

    private UserModel GetSelectedUser(Func<UserModel, bool> predicate)
    {
        return AppState.MasterData.Users?.FirstOrDefault(predicate);
    }

    private List<MenuModel> Clone(List<MenuModel> listToCopy)
    {
        return JsonConvert.DeserializeObject<List<MenuModel>>(JsonConvert.SerializeObject(listToCopy));
    }

    private string GetInitials(string name)
    {
        if (!_isOrganisation)
        {
            return AppState.GetImageInitials(name);
        }
        return string.Empty;
    }

    private string RandomColorString(int index)
    {
        string[] colors = { StyleConstants.PRIMARY_APP_COLOR, StyleConstants.SECONDARY_APP_COLOR, "#02A5C3" };
        index = index >= colors.Length ? Math.Abs(index / colors.Length) : index;
        // Generate a random index less than the size of the array.  
        return colors[index];
    }
}
