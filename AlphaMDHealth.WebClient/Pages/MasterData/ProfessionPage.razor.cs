using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;

namespace AlphaMDHealth.WebClient;

public partial class ProfessionPage : BasePage
{
    private readonly ProfessionDTO _professionData = new ProfessionDTO { RecordCount = -1};
    private List<ButtonActionModel> _popupActions;
    private bool _hideDeletedConfirmationPopup = true;
    private bool _isEditable;

    /// <summary>
    /// Profession ID parameter
    /// </summary>
    [Parameter]
    public byte ProfessionID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetDataAsync().ConfigureAwait(true);
    }

    private async Task GetDataAsync()
    {
        _professionData.Profession = new ProfessionModel { ProfessionID = ProfessionID };
        await SendServiceRequestAsync(new ProfessionService(AppState.webEssentials).SyncProfessionsFromServerAsync(_professionData), _professionData).ConfigureAwait(true);
        if (_professionData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_professionData.FeaturePermissions, AppPermissions.ProfessionAddEdit.ToString());
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_professionData.ErrCode.ToString());
        }
    }

    private List<TabDataStructureModel> DataFormatter = new List<TabDataStructureModel>
    {
        new TabDataStructureModel{DataField = nameof(ProfessionModel.Profession),ResourceKey = ResourceConstants.R_PROFESSION_NAME_TEXT_KEY}
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

    private async Task OnDeleteConfirmationPopupClickedAsync(object sequenceNo)
    {
        _hideDeletedConfirmationPopup = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                _professionData.IsActive = false;
                await SaveProfessionAsync().ConfigureAwait(true);
            }
        }
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _professionData.IsActive = true;
            await SaveProfessionAsync().ConfigureAwait(false);
        }
    }

    private async Task SaveProfessionAsync()
    {
        ProfessionDTO professionData = new ProfessionDTO
        {
            Profession = new ProfessionModel { ProfessionID = (byte)ProfessionID, IsActive = _professionData.IsActive },
            Professions = _professionData.Professions,
        };
        await SendServiceRequestAsync(new ProfessionService(AppState.webEssentials).SyncProfessionToServerAsync(professionData), professionData).ConfigureAwait(true);
        if (professionData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(_professionData.ErrCode.ToString());
        }
        else
        {
            Error = professionData.ErrCode.ToString();
        }
    }
}