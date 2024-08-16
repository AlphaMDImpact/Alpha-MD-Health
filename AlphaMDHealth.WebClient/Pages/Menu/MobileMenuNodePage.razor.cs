using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class MobileMenuNodePage : BasePage
{
    private readonly MobileMenuNodeDTO _mobileMenuNodeData = new() { RecordCount = -1 };
    private string _selectedLeftMenuAction;
    private string _selectedRightMenuAction;
    private string _selectedLeftMenuNode;
    private string _selectedRightMenuNode;
    private string _leftIconOrLabel;
    private string _rightIconOrLabel;
    private List<OptionModel> _renderTypes = new();
    private List<ButtonActionModel> _actionButtons;
    private bool _hideConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Menu Node ID parameter
    /// </summary>
    [Parameter]
    public long MenuNodeID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _mobileMenuNodeData.MobileMenuNode = new MobileMenuNodeModel { MobileMenuNodeID = MenuNodeID };
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenuNodesFromServerAsync(_mobileMenuNodeData, CancellationToken.None), _mobileMenuNodeData).ConfigureAwait(true);
        if (_mobileMenuNodeData.ErrCode == ErrorCode.OK)
        {
            if (_mobileMenuNodeData.MobileMenuNode.MobileMenuNodeID == 0)
            {
                _mobileMenuNodeData.MobileMenuNode.IsActive = true;
            }
            _selectedLeftMenuAction = LibResources.GetResourceKeyIDByKey(_mobileMenuNodeData.Resources, _mobileMenuNodeData.MobileMenuNode.LeftMenuActionID.ToString()).ToString();
            _selectedRightMenuAction = LibResources.GetResourceKeyIDByKey(_mobileMenuNodeData.Resources, _mobileMenuNodeData.MobileMenuNode.RightMenuActionID.ToString()).ToString();
            _selectedLeftMenuNode = _mobileMenuNodeData.MobileMenuNode.LeftMenuNodeID.ToString();
            _selectedRightMenuNode = _mobileMenuNodeData.MobileMenuNode.RightMenuNodeID.ToString();
            _renderTypes = (from dataItem in _mobileMenuNodeData.Resources
                            where dataItem.ResourceKey == ResourceConstants.R_SHOW_ICON_KEY || dataItem.ResourceKey == ResourceConstants.R_SHOW_LABEL_KEY
                            select new OptionModel
                            {
                                OptionID = dataItem.ResourceID,
                                OptionText = dataItem.ResourceValue,
                            }).ToList();
            _leftIconOrLabel = IsRadioButtonSelected(_mobileMenuNodeData.MobileMenuNode.ShowIconInLeftMenu);
            _rightIconOrLabel = IsRadioButtonSelected(_mobileMenuNodeData.MobileMenuNode.ShowIconInRightMenu);
            _isEditable = LibPermissions.HasPermission(_mobileMenuNodeData.FeaturePermissions, AppPermissions.MobileMenuNodeAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_mobileMenuNodeData.ErrCode.ToString());
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _mobileMenuNodeData.MobileMenuNode.NodeName = _mobileMenuNodeData.MobileMenuNode.NodeName.Trim();
            _mobileMenuNodeData.MobileMenuNode.TargetID = _mobileMenuNodeData.MenuFeatures.FirstOrDefault(x => x.IsSelected).OptionID;
            _mobileMenuNodeData.MobileMenuNode.NodeType = (MenuType)(byte)_mobileMenuNodeData.MenuFeatures.Find(x => x.IsSelected)?.ParentOptionID;
            _mobileMenuNodeData.MobileMenuNode.LeftMenuActionID = GetHeaderMenuActionID(_selectedLeftMenuAction);
            _mobileMenuNodeData.MobileMenuNode.LeftMenuNodeID = string.IsNullOrWhiteSpace(_selectedLeftMenuNode) || _selectedLeftMenuNode == Constants.CONSTANT_NEG_ONE ? null : Convert.ToInt64(_selectedLeftMenuNode);
            _mobileMenuNodeData.MobileMenuNode.RightMenuActionID = GetHeaderMenuActionID(_selectedRightMenuAction);
            _mobileMenuNodeData.MobileMenuNode.RightMenuNodeID = string.IsNullOrWhiteSpace(_selectedRightMenuNode) || _selectedRightMenuNode == Constants.CONSTANT_NEG_ONE ? null : Convert.ToInt64(_selectedRightMenuNode);
            _mobileMenuNodeData.MobileMenuNode.ShowIconInLeftMenu = GetShowIconValue(true, _mobileMenuNodeData.MobileMenuNode.LeftMenuActionID);
            _mobileMenuNodeData.MobileMenuNode.ShowIconInRightMenu = GetShowIconValue(false, _mobileMenuNodeData.MobileMenuNode.RightMenuActionID);
            await SaveNodeDataAsync();
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _actionButtons = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object value)
    {
        if (value != null)
        {
            _hideConfirmationPopup = true;
            if (Convert.ToInt64(value) == 1)
            {
                _isDataFetched = false;
                _mobileMenuNodeData.MobileMenuNode.IsActive = false;
                await SaveNodeDataAsync();
            }
        }
    }

    private async Task SaveNodeDataAsync()
    {
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenuNodeToServerAsync(_mobileMenuNodeData, CancellationToken.None), _mobileMenuNodeData).ConfigureAwait(true);
        if (_mobileMenuNodeData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_mobileMenuNodeData.ErrCode.ToString());
        }
        else
        {
            Error = _mobileMenuNodeData.ErrCode.ToString();
        }
    }

    private string IsRadioButtonSelected(bool? isIconShowing)
    {
        if (isIconShowing == null)
        {
            return string.Empty;
        }
        else if (isIconShowing.Value)
        {
            return LibResources.GetResourceKeyIDByKey(_mobileMenuNodeData.Resources, ResourceConstants.R_SHOW_ICON_KEY).ToString();
        }
        else
        {
            return LibResources.GetResourceKeyIDByKey(_mobileMenuNodeData.Resources, ResourceConstants.R_SHOW_LABEL_KEY).ToString();
        }
    }

    private bool? GetShowIconValue(bool isLeftHeaderValue, MenuAction value)
    {
        var data = isLeftHeaderValue ? _leftIconOrLabel : _rightIconOrLabel;
        if (data == null || value == MenuAction.MenuActionDefaultKey)
        {
            return null;
        }
        else
        {
            return LibResources.GetResourceByKeyID(_mobileMenuNodeData.Resources, Convert.ToInt32(data, CultureInfo.InvariantCulture)).ResourceKey == ResourceConstants.R_SHOW_ICON_KEY;
        }
    }

    private MenuAction GetHeaderMenuActionID(string menuActionString)
    {
        long menuActionId = string.IsNullOrWhiteSpace(menuActionString) ? 0 : Convert.ToInt64(menuActionString);
        if (menuActionId == 0 || menuActionId == -1)
        {
            return MenuAction.MenuActionDefaultKey;
        }
        else
        {
            return LibResources.GetResourceByKeyID(_mobileMenuNodeData.Resources, menuActionId)?.ResourceKey.ToEnum<MenuAction>() ?? MenuAction.MenuActionDefaultKey;
        }
    }
}