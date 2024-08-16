using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ReasonPage : BasePage
{
    private readonly ReasonDTO _reasonData = new ReasonDTO { RecordCount = -1 };
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Reason ID Parameter
    /// </summary>
    [Parameter]
    public byte ReasonID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _reasonData.Reason = new ReasonModel
        {
            ReasonID = ReasonID
        };
        await SendServiceRequestAsync(new ReasonService(AppState.webEssentials).SyncReasonsFromServerAsync(_reasonData, CancellationToken.None), _reasonData).ConfigureAwait(true);
        if (_reasonData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_reasonData.FeaturePermissions, AppPermissions.ReasonAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_reasonData.ErrCode.ToString());
        }
    }

    private readonly List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel
        {
            DataField=nameof(ReasonModel.Reason),
            ResourceKey= ResourceConstants.R_REASON_NAME_TEXT_KEY,
        },
        new TabDataStructureModel
        {
            DataField = nameof(ReasonModel.ReasonDescription),
            ResourceKey = ResourceConstants.R_DESCRIPTION_NAME_TEXT_KEY,
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
        if(sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _reasonData.IsActive = _reasonData.Reason.IsActive = false;
                await SaveReasonAsync(_reasonData).ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            ReasonDTO reasonData = new ReasonDTO
            {
                Reason = new ReasonModel { ReasonID = ReasonID },
                Reasons = _reasonData.Reasons
            };
            await SaveReasonAsync(reasonData).ConfigureAwait(true);
        }
    }

    private async Task SaveReasonAsync(ReasonDTO reasonData)
    {
        await SendServiceRequestAsync(new ReasonService(AppState.webEssentials).SyncReasonToServerAsync(reasonData, CancellationToken.None), reasonData).ConfigureAwait(true);
        if (reasonData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(reasonData.ErrCode.ToString());
        }
        else
        {
            Error = reasonData.ErrCode.ToString();
        }
    }
}