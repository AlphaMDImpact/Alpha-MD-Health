using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AlphaMDHealth.WebClient;

public partial class MobileMenuPage : BasePage
{
    private readonly MenuDTO _menuData = new MenuDTO { RecordCount = -1 };
    private bool _showConfirmationPopup = true;
    private double? _sequenceNo;
    private List<ButtonActionModel> _actionData;
    private bool _isEditable;

    /// <summary>
    /// Menu ID parameter
    /// </summary>
    [Parameter]
    public long MenuID { get; set; }

    /// <summary>
    /// Menu ID parameter
    /// </summary>
    [Parameter]
    public bool IsPatientMenu { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await GetDataAsync();
        await base.OnParametersSetAsync();
    }

    private async Task GetDataAsync()
    {
        _menuData.Menu = new MenuModel { MenuID = MenuID, IsPatientMenu = IsPatientMenu };
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenusFromServerAsync(_menuData, CancellationToken.None), _menuData).ConfigureAwait(true);
        if (_menuData.ErrCode == ErrorCode.OK)
        {
            _sequenceNo = _menuData.Menu.MenuID > 0 ? _menuData.Menu.SequenceNo : null;
            _isEditable = LibPermissions.HasPermission(_menuData.FeaturePermissions, IsPatientMenu ? AppPermissions.PatientMobileMenuAddEdit.ToString() : AppPermissions.MobileMenuAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_menuData.ErrCode.ToString());
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionData = new List<ButtonActionModel> {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY }
        };
        _showConfirmationPopup = false;
    }

    private async Task PopUpCallbackAsync(object e)
    {
        _showConfirmationPopup = true;
        Success = Error = string.Empty;
        if (Convert.ToInt64(e) == 1)
        {
            _menuData.Menu.IsActive = false;
            await SaveMobileMenuDataAsync();
        }
    }

    private async Task OnSaveButtonClickedAsync(MouseEventArgs e)
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _menuData.Menu.SequenceNo = (byte)_sequenceNo;
            _menuData.Menu.TargetID = (long)(_menuData.MenuNodesGroups.FirstOrDefault(x => x.IsSelected)?.OptionID);
            _menuData.Menu.PageTypeID = (MenuType)(long)(_menuData.MenuNodesGroups.FirstOrDefault(x => x.IsSelected)?.ParentOptionID);
            _menuData.Menu.RenderType = RenderTypeSelected(Convert.ToInt32(_menuData.MenuLocations.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0));
            _menuData.Menu.IsPatientMenu = IsPatientMenu;
            _menuData.Menu.IsActive = true;
            await SaveMobileMenuDataAsync();
        }
    }

    private async Task SaveMobileMenuDataAsync()
    {
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenuToServerAsync(_menuData, CancellationToken.None), _menuData).ConfigureAwait(true);
        if (_menuData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_menuData.ErrCode.ToString());
        }
        else
        {
            Error = _menuData.ErrCode.ToString();
        }
    }

    private MenuRenderType RenderTypeSelected(int data)
    {
        var key = LibResources.GetResourceKeyByKeyID(_menuData.Resources, data);
        if (key == ResourceConstants.R_SHOW_LABEL_KEY)
        {
            return MenuRenderType.OnlyTitle;
        }
        else if (key == ResourceConstants.R_SHOW_BOTH_KEY)
        {
            return MenuRenderType.Both;
        }
        else
        {
            return MenuRenderType.OnlyIcon;
        }
    }
}
