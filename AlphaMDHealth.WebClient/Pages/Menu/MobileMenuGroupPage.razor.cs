using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using AlphaMDHealth.WebClient.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AlphaMDHealth.WebClient;

public partial class MobileMenuGroupPage : BasePage
{
    private readonly MenuGroupDTO _menuGroupData = new MenuGroupDTO { RecordCount = -1 };
    private bool _hideDeletedConfirmationPopup = true;
    private List<ButtonActionModel> _popupActions;
    private bool _isEditable;
    private AmhDropdownControl _multiSelectValue = new AmhDropdownControl();

    /// <summary>
    /// Menu Group ID parameter
    /// </summary>
    [Parameter]
    public long MenuGroupID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _menuGroupData.MenuGroup = new MenuGroupModel
        {
            MenuGroupID = MenuGroupID
        };
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenuGroupsFromServerAsync(_menuGroupData, CancellationToken.None), _menuGroupData).ConfigureAwait(true);
        if (_menuGroupData.ErrCode == ErrorCode.OK)
        {
            foreach (var item in _menuGroupData.Languages)
            {
                var existingItem = _menuGroupData.MenuGroupDetails.FirstOrDefault(x => x.LanguageID == item.LanguageID);
                if (existingItem != null)
                {
                    existingItem.LanguageName = item.LanguageName;
                }
            }
            _isEditable = LibPermissions.HasPermission(_menuGroupData.FeaturePermissions, AppPermissions.MobileMenuGroupAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_menuGroupData.ErrCode.ToString());
        }
    }

    private List<TabDataStructureModel> _dataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading), FieldType= FieldTypes.TextEntryControl, ResourceKey=ResourceConstants.R_SECTION_TITLE_KEY, IsRequired=false },
    };

    private async Task OnSaveButtonClickedAsync(MouseEventArgs e)
    {
        if (IsValid())
        {
            _menuGroupData.IsActive = true;
            await SaveMobileMenuGroupDataAsync().ConfigureAwait(true);
        }
    }

    private async Task OnCancelClickAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
             new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickAsync(object sequenceNo)
    {
        if (sequenceNo != null)
        {
            _hideDeletedConfirmationPopup = false;
            Error = string.Empty;
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _menuGroupData.IsActive = false;
                await SaveMobileMenuGroupDataAsync().ConfigureAwait(true);
            }
            else
            {
                _hideDeletedConfirmationPopup = true;
            }
        }
    }

    private async Task SaveMobileMenuGroupDataAsync()
    {
        if (_menuGroupData.IsActive)
        {
            byte sequenceNo = 1;
            _menuGroupData.MenuGroupLinks = new List<MenuGroupLinkModel>();
            var selectedIds = _multiSelectValue.Value.Split('|');
            foreach (var selectedId in selectedIds)
            {
                if (long.TryParse(selectedId, out var optionId))
                {
                    var demoModel = _menuGroupData.MenuNodes.Find(x => x.OptionID == optionId);
                    _menuGroupData.MenuGroupLinks.Add(new MenuGroupLinkModel
                    {
                        GroupID = MenuGroupID,
                        Heading = demoModel.OptionText,
                        TargetID = demoModel.OptionID,
                        SequenceNo = sequenceNo
                    });
                    sequenceNo++;
                }
            }
        }
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncMobileMenuGroupToServerAsync(_menuGroupData, CancellationToken.None), _menuGroupData).ConfigureAwait(true);
        if (_menuGroupData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_menuGroupData.ErrCode.ToString());
        }
        else
        {
            Error = _menuGroupData.ErrCode.ToString();
        }
    }
}