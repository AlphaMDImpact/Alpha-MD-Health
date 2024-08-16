using System.Globalization;
using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class WebMenuGroup : BasePage
{
    private readonly MenuGroupDTO _menuGroupData = new MenuGroupDTO { RecordCount = -1 };
    private IList<TabDataStructureModel> _dataFormatter;
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Web menu group id parameter
    /// </summary>
    [Parameter]
    public long MenuGroupId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _menuGroupData.MenuGroup = new MenuGroupModel { MenuGroupID = MenuGroupId };
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncWebMenuGroupsFromServerAsync(_menuGroupData, CancellationToken.None), _menuGroupData).ConfigureAwait(true);
        var resource = _menuGroupData.Resources.Find(x => x.ResourceKey == "IdentifierKey");
        if (_menuGroupData.ErrCode == ErrorCode.OK)
        {
            OnPageTypeChanged(MenuGroupId == 0
                ? _menuGroupData.PageTypes.FirstOrDefault().OptionID.ToString(CultureInfo.InvariantCulture)
                : ((long)_menuGroupData.MenuGroup.PageType).ToString(CultureInfo.InvariantCulture));
            _isEditable = LibPermissions.HasPermission(_menuGroupData.FeaturePermissions, AppPermissions.WebGroupContentAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_menuGroupData.ErrCode.ToString());
        }
    }

    private void OnPageTypeChanged(string value)
    {
        _menuGroupData.MenuGroup.PageType = value.ToEnum<ContentType>();
        RemoveControlContainsKey(ResourceConstants.R_SECTION_TITLE_KEY);
        RemoveControlContainsKey(ResourceConstants.R_EDITOR_KEY);
        switch (_menuGroupData.MenuGroup.PageType)
        {
            case ContentType.Link:                
                _dataFormatter = GetLinksDataFormatter();
                foreach (var data in _menuGroupData.MenuGroupDetails)
                {
                    data.PageData = string.Empty;
                }
                break;
            case ContentType.Both:
                _dataFormatter = GetContentDataFormatter();
                break;
            case ContentType.Content:
            default:
                _dataFormatter = GetContentDataFormatter();
                foreach (var item in _menuGroupData.MenuNodes)
                {
                    item.IsSelected = false;
                }
                break;
        }
    }

    private List<TabDataStructureModel> GetContentDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageHeading), ResourceKey=ResourceConstants.R_SECTION_TITLE_KEY, IsRequired = false },
            new TabDataStructureModel{DataField=nameof(ContentDetailModel.PageData),  ResourceKey=ResourceConstants.R_EDITOR_KEY,
                //MaxFileDimensionSize = LibSettings.GetSettingValueByKey(_menuGroupData.Settings, SettingsConstants.S_SMALL_IMAGE_RESOLUTION_KEY),
                //MaxFileUploadSize = LibSettings.GetSettingValueByKey(_menuGroupData.Settings, SettingsConstants.S_MAX_FILE_UPLOAD_KEYS)
            },
        };
    }

    private List<TabDataStructureModel> GetLinksDataFormatter()
    {
        return new List<TabDataStructureModel>
        {
            new TabDataStructureModel{ DataField=nameof(ContentDetailModel.PageHeading), ResourceKey=ResourceConstants.R_SECTION_TITLE_KEY, IsRequired = false},
        };
    }

    private void OnDeleteClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_TWO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnDeleteConfirmationPopUpClickedAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _menuGroupData.IsActive = false;
                _menuGroupData.ErrCode = ErrorCode.OK;
                await SyncWebMenuGroupToServerAsync(_menuGroupData).ConfigureAwait(false);
            }
        }
    }

    private async Task OnCanceledClickAsync()
    {
        await OnClose.InvokeAsync(null);
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            MenuGroupDTO menuGroupData = new MenuGroupDTO
            {
                MenuGroup = _menuGroupData.MenuGroup,
                MenuGroupDetails = _menuGroupData.MenuGroupDetails
            };
            menuGroupData.IsActive = true;
            byte sequenceNo = 1;
            if (_menuGroupData.MenuGroup.PageType != ContentType.Content)
            {
                menuGroupData.MenuGroupLinks = new List<MenuGroupLinkModel>();
                var selectedId = _menuGroupData.MenuNodes.FindAll(x => x.IsSelected == true);
                foreach (var optionItem in selectedId)
                {
                    var demoModel = _menuGroupData.MenuNodes.Find(x => x.OptionID == optionItem.OptionID);
                    menuGroupData.MenuGroupLinks.Add(
                    new MenuGroupLinkModel
                    {
                        GroupID = MenuGroupId,
                        PageTypeID = (MenuType)demoModel.ParentOptionID,
                        TargetID = demoModel.OptionID,
                        SequenceNo = sequenceNo
                    });
                    sequenceNo++;
                }
            }
            await SyncWebMenuGroupToServerAsync(menuGroupData);
        }
    }

    private async Task SyncWebMenuGroupToServerAsync(MenuGroupDTO menuGroupData)
    {
        await SendServiceRequestAsync(new MenuService(AppState.webEssentials).SyncWebMenuGroupsToServerAsync(menuGroupData, CancellationToken.None), menuGroupData).ConfigureAwait(true);
        _menuGroupData.ErrCode = menuGroupData.ErrCode;
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