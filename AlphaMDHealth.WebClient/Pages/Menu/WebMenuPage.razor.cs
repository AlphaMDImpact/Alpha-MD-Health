using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class WebMenuPage : BasePage
{
    private readonly MenuDTO _menuData = new MenuDTO { RecordCount = -1 };
    private List<ButtonActionModel> _actionField;
    private bool _hideConfirmationPopup = true;
    private double? _sequenceNo;
    private bool _isEditable;

    /// <summary>
    /// Menu ID parameter
    /// </summary>
    [Parameter]
    public long MenuID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _menuData.Menu = new MenuModel { MenuID = MenuID };
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncWebMenusFromServerAsync(_menuData, CancellationToken.None), _menuData).ConfigureAwait(true);
        if (_menuData.ErrCode == ErrorCode.OK)
        {
            _sequenceNo = _menuData.Menu.MenuID > 0 ? _menuData.Menu.SequenceNo : null;
            _isEditable = LibPermissions.HasPermission(_menuData.FeaturePermissions, AppPermissions.WebMenuAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_menuData.ErrCode.ToString());
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(null);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            var selectedNodeGroup = _menuData.MenuNodesGroups.FirstOrDefault(x => x.IsSelected);
            _menuData.Menu.SequenceNo = Convert.ToByte(_sequenceNo, CultureInfo.InvariantCulture);
            _menuData.Menu.TargetID = selectedNodeGroup?.OptionID ?? 0;
            _menuData.Menu.PageTypeID = (MenuType)(byte)(selectedNodeGroup?.ParentOptionID ?? 0);
            _menuData.Menu.MenuLocation = RenderTypeSelected();
            _menuData.Menu.IsActive = true;
            await SaveMenuDataAsync().ConfigureAwait(true);
        }
    }

    private MenuLocation RenderTypeSelected()
    {
        OptionModel selectedOption = _menuData.MenuLocations?.FirstOrDefault(x => x.IsSelected);
        return selectedOption?.GroupName == ResourceConstants.R_HEADER_KEY
            ? MenuLocation.Header
            : MenuLocation.Footer;
    }

    private void GetScrollToPageDownClicked(string val)
    {
        _menuData.Menu.ScrollToPage = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private List<OptionModel> GetScrollToPageOption()
    {
        return new List<OptionModel> {
            new OptionModel {
                OptionID = 1,
                OptionText = LibResources.GetResourceValueByKey(_menuData.Resources, ResourceConstants.R_SCROLL_TO_PAGE_KEY)}
        };
    }

    private async Task SaveMenuDataAsync()
    {
        _menuData.Menus = new List<MenuModel> { _menuData.Menu };
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncWebMenuToServerAsync(_menuData, CancellationToken.None), _menuData).ConfigureAwait(true);
        if (_menuData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_menuData.ErrCode.ToString());
        }
        else
        {
            Error = _menuData.ErrCode.ToString();
        }
    }

    private void OnRemoveClick()
    {
        _actionField = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY }
        };
        _hideConfirmationPopup = false;
    }

    private async Task OnPopupClickAsync(object id)
    {
        _hideConfirmationPopup = true;
        Success = Error = string.Empty;
        if (id != null)
        {
            if (Convert.ToInt64(id) == 1)
            {
                _menuData.Menu.IsActive = false;
                await SaveMenuDataAsync().ConfigureAwait(true);
            }
        }
    }
}