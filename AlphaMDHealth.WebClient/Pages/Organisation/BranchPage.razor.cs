using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class BranchPage : BasePage
{
    private readonly BranchDTO _branchData = new BranchDTO { RecordCount = -1 };
    private List<OptionModel> _departments;
    private List<ButtonActionModel> _actionData;
    private bool _hideDeletedConfirmationPopup = true;
    private long _permissionAtLevel;
    private bool _isEditable;

    /// <summary>
    /// Branch ID parameter
    /// </summary>
    [Parameter]
    public long BranchId { get; set; } = -1;

    protected override async Task OnInitializedAsync()
    {
        _permissionAtLevel = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0);
        //if (AppState.RouterData.SelectedRoute.Page == AppPermissions.BranchView.ToString())
        //{
        //    _branchData.RecordCount = -2;
        //}
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        if (BranchId == -1)
        {
            BranchId = _permissionAtLevel;
        }
        _branchData.Branch = new BranchModel { BranchID = BranchId };
        await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationBranchesFromServerAsync(_branchData, CancellationToken.None), _branchData).ConfigureAwait(true);
        if (_branchData.ErrCode == ErrorCode.OK)
        {
            _departments = _branchData.Departments?.Select(department =>
                new OptionModel
                {
                    OptionID = department.DepartmentID,
                    OptionText = department.DepartmentName,
                    IsSelected = department.IsActive
                }).ToList() ?? new List<OptionModel>();

            _isEditable = LibPermissions.HasPermission(_branchData.FeaturePermissions, AppPermissions.BranchAddEdit.ToString());
        }
        else
        {
            if (ShowDetailPage)
            {
                await OnClose.InvokeAsync(_branchData.ErrCode.ToString());
                return;
            }
            else
            {
                Error = _branchData.ErrCode.ToString();
            }
        }
        _isDataFetched = true;
    }

    private readonly List<TabDataStructureModel> _dataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel{DataField=nameof(BranchModel.BranchName), ResourceKey=ResourceConstants.R_BRANCH_NAME_KEY },
    };

    protected override async Task OnParametersSetAsync()
    {
        long currentPermissionAtLevel = AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_PERMISSION_AT_LEVEL_ID_KEY, 0);
        if (_permissionAtLevel != currentPermissionAtLevel)
        {
            _permissionAtLevel = currentPermissionAtLevel;
            await GetDataAsync().ConfigureAwait(true);
        }
    }
    private async Task OnSaveButtonClickedAsync()
    {
        if (IsValid())
        {
            if (GenericMethods.IsListNotEmpty(_branchData.Departments))
            {
                var selectedIds = _departments.Where(x => x.IsSelected).Select(x => x.OptionID);
                _branchData.Departments.ForEach(x =>
                {
                    x.IsActive = selectedIds.Contains(x.DepartmentID);
                });
                if (_branchData.Departments.Where(x => x.IsActive).ToList().Count == 0)
                {
                    Error = ResourceConstants.R_DEPARTMENT_SELECTION_ERROR_KEY;
                    return;
                }
            }
            await SaveAndDeleteDataAsync(true);
        }
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _hideDeletedConfirmationPopup = true;
            if (Convert.ToInt32(sequenceNo) == 1)
            {
                await SaveAndDeleteDataAsync(false);
            }
        }
    }

    private async Task SaveAndDeleteDataAsync(bool isSaveRequest)
    {
        BranchDTO branchData = new BranchDTO
        {
            RecordCount = _branchData.RecordCount,
            Branch = _branchData.Branch
        };
        branchData.IsActive = isSaveRequest;
        branchData.Branches = _branchData.Branches;
        branchData.Departments = _branchData.Departments.Where(x => x.IsActive).ToList();
        await SendServiceRequestAsync(new OrganisationService(AppState.webEssentials).SyncOrganisationBranchToServerAsync(branchData, CancellationToken.None), branchData).ConfigureAwait(true);
        if (branchData.ErrCode == ErrorCode.OK)
        {
            Success = branchData.ErrCode.ToString();
            await NavigateBackAsync(branchData.ErrCode.ToString());
        }
        else
        {
            Error = branchData.ErrCode.ToString();
        }
    }

    private async Task NavigateBackAsync(string message)
    {
        if (ShowDetailPage)
        {
            await OnClose.InvokeAsync(message);
        }
        await NavigateToAsync(AppPermissions.BranchesView.ToString(), true).ConfigureAwait(true);
    }

    private async Task OnCanceledClickAsync()
    {
        await OnClose.InvokeAsync(null);
    }
}