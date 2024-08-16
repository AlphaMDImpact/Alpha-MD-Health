using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class DepartmentPage : BasePage
{
    private readonly DepartmentDTO _departmentData = new DepartmentDTO { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;


    /// <summary>
    /// Department ID parameter
    /// </summary>
    [Parameter]
    public byte DepartmentId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _departmentData.Department = new DepartmentModel{DepartmentID = DepartmentId};
        await SendServiceRequestAsync(new DepartmentService(AppState.webEssentials).SyncDepartmentsFromServerAsync(_departmentData, CancellationToken.None), _departmentData).ConfigureAwait(true);
        if (_departmentData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_departmentData.FeaturePermissions, AppPermissions.DepartmentAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_departmentData.ErrCode.ToString());
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
       new TabDataStructureModel{ DataField=nameof(DepartmentModel.DepartmentName), ResourceKey= ResourceConstants.R_DEPARTMENT_NAME_TEXT_KEY}
    };

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE,ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO,ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopupClickedAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            Success = Error = string.Empty;
            _hideDeletedConfirmationPopup = true;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _departmentData.IsActive = false;
                await SaveDepartmentAsync(_departmentData).ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            DepartmentDTO departmentData = new DepartmentDTO
            {
                Department = new DepartmentModel { DepartmentID = DepartmentId },
                Departments = _departmentData.Departments
            };
            await SaveDepartmentAsync(departmentData).ConfigureAwait(true);
        }
    }

    private async Task SaveDepartmentAsync(DepartmentDTO departmentData)
    {
        await SendServiceRequestAsync(new DepartmentService(AppState.webEssentials).SyncDepartmentToServerAsync(departmentData, CancellationToken.None), departmentData).ConfigureAwait(true);
        if (departmentData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(departmentData.ErrCode.ToString());
        }
        else
        {
            Error = departmentData.ErrCode.ToString();
        }
    }
}