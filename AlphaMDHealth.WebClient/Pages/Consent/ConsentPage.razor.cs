using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AlphaMDHealth.WebClient;

public partial class ConsentPage : BasePage
{
    private readonly ConsentDTO _consentData = new ConsentDTO { RecordCount = -1 };
    private List<ButtonActionModel> _messageButtonActions;
    private bool _hideDeletedConfirmationPopup = true;
    private double? _sequenceValue;
    private bool _isEditable;

    /// <summary>
    /// Consent ID parameter
    /// </summary>
    [Parameter]
    public long ConsentID { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _consentData.Consent = new ConsentModel
        {
            ConsentID = ConsentID
        };
        await SendServiceRequestAsync(new ConsentService(AppState.webEssentials).GetConsentsAsync(_consentData), _consentData).ConfigureAwait(true);
        if (_consentData.ErrCode == ErrorCode.OK)
        {
            _isEditable = LibPermissions.HasPermission(_consentData.FeaturePermissions, AppPermissions.ConsentAddEdit.ToString());
            if (_consentData.Consent?.ConsentID != 0)
            {
                _sequenceValue = _consentData.Consent.SequenceNo;
            }
            if (_consentData.Roles.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_consentData.Consent.RoleID)) != null)
            {
                _consentData.Roles.FirstOrDefault(x => x.OptionID == Convert.ToInt64(_consentData.Consent.RoleID)).IsSelected = true;
            }
            _isDataFetched = true;
        }
        else
        {
            await OnClose.InvokeAsync(_consentData.ErrCode.ToString());
        }
    }

    private List<OptionModel> GetRememberMeOption()
    {
        return new List<OptionModel> {
            new OptionModel { OptionID = 1, OptionText = LibResources.GetResourceValueByKey(_consentData.Resources, ResourceConstants.R_IS_REQUIRED_KEY) }
        };
    }

    private void OnRememberMeClicked(string val)
    {
        _consentData.Consent.IsRequired = !string.IsNullOrWhiteSpace(val) && Convert.ToInt32(val) == 1;
    }

    private async Task OnSaveButtonClickedAsync()
    {
        Success = Error = string.Empty;
        if (IsValid())
        {
            _consentData.IsActive = true;
            _consentData.Consent.SequenceNo = Convert.ToByte(_sequenceValue ?? 0, CultureInfo.InvariantCulture);
            _consentData.Consent.PageID = _consentData.Pages?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0;
            _consentData.Consent.RoleID = (byte)(_consentData.Roles?.FirstOrDefault(x => x.IsSelected)?.OptionID ?? 0);           
            await SaveConsentDataAsync().ConfigureAwait(true);
        }
    }

    private async Task OnCancelClickedAsync()
    {
        await OnClose.InvokeAsync(string.Empty);
    }

    private void OnRemoveClick()
    {
        _messageButtonActions = new List<ButtonActionModel>
        {
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ONE, ButtonResourceKey =ResourceConstants.R_OK_ACTION_KEY  },
            new ButtonActionModel{ ButtonID = Constants.NUMBER_ZERO, ButtonResourceKey =ResourceConstants.R_CANCEL_ACTION_KEY  },
        };
        _hideDeletedConfirmationPopup = false;
    }

    private async Task OnConsentActionClickAsync(object sequenceNo)
    {
        Success = Error = string.Empty;
        _hideDeletedConfirmationPopup = true;
        if (Convert.ToInt64(sequenceNo) == 1)
        {
            _consentData.IsActive = false;
            await SaveConsentDataAsync().ConfigureAwait(true);
        }
    }

    private async Task SaveConsentDataAsync()
    {
        ConsentDTO consentData = new ConsentDTO
        {
            IsActive = _consentData.IsActive,
            Consent = _consentData.Consent,
            RecordCount = -1
        };
        await SendServiceRequestAsync(new ConsentService(AppState.webEssentials).SyncConsentsToServerAsync(consentData, CancellationToken.None), consentData).ConfigureAwait(true);
        if (consentData.ErrCode == ErrorCode.OK)
        {
            await OnClose.InvokeAsync(consentData.ErrCode.ToString());
        }
        else
        {
            Error = consentData.ErrCode.ToString();
        }
    }
}
