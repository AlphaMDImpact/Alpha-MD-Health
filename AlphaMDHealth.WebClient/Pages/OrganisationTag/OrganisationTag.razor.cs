using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;
public partial class OrganisationTag : BasePage
{
    private readonly OrganisationTagDTO _organisationTagData = new OrganisationTagDTO { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// OrganisationTag ID Parameter
    /// </summary>
    [Parameter]
    public long OrganisationTagId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _organisationTagData.OrganisationTag = new OrganisationTagModel
        {
            OrganisationTagID = OrganisationTagId
        };
        await SendServiceRequestAsync(new OrganisationTagService(AppState.webEssentials).SyncOrganisationTagsFromServerAsync(_organisationTagData, CancellationToken.None), _organisationTagData).ConfigureAwait(true);
        if (_organisationTagData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_organisationTagData.FeaturePermissions, AppPermissions.OrganisationTagAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_organisationTagData.ErrCode.ToString());
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(OrganisationTagModel.TagText),
            ResourceKey= ResourceConstants.R_ORGANISATION_TAG_TEXT_KEY,
        },
        new TabDataStructureModel
        {
            DataField = nameof(OrganisationTagModel.TagDescription),
            ResourceKey = ResourceConstants.R_ORGANISATION_TAG_DESCRIPTION_KEY,
        },
    };

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _popupActions = new List<ButtonActionModel> {
            new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY },
            new ButtonActionModel { ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey = ResourceConstants.R_CANCEL_ACTION_KEY },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnActionClickAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _organisationTagData.IsActive = false;
                await SaveOrganisationTagAsync(_organisationTagData).ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            OrganisationTagDTO organisationTagData = new OrganisationTagDTO()
            {
                OrganisationTag = new OrganisationTagModel { OrganisationTagID = OrganisationTagId },
                OrganisationTags = _organisationTagData.OrganisationTags
            };
            await SaveOrganisationTagAsync(organisationTagData).ConfigureAwait(true);
        }
    }

    private async Task SaveOrganisationTagAsync(OrganisationTagDTO organisationTagData)
    {
        await SendServiceRequestAsync(new OrganisationTagService(AppState.webEssentials).SyncOrganisationTagToServerAsync(organisationTagData, CancellationToken.None), organisationTagData).ConfigureAwait(true);
        if (organisationTagData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(organisationTagData.ErrCode.ToString());
        }
        else
        {
            Error = organisationTagData.ErrCode.ToString();
        }
    }
}


