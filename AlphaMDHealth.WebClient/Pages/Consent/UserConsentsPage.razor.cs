using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;

namespace AlphaMDHealth.WebClient;

public partial class UserConsentsPage : BasePage
{
    private readonly ConsentDTO _consentData = new() { IsActive = false };
    private List<ButtonActionModel> _actionData;
    private bool _hideConfirmation = true;
    private bool _isEditable;
    private IList<ButtonActionModel> _actionButtons = null;

    protected override async Task OnInitializedAsync()
    {
        await GetConsentsData().ConfigureAwait(true);
    }

    private async Task GetConsentsData()
    {
        _consentData.Consent = new ConsentModel();
        _consentData.RecordCount = -3;
        await SendServiceRequestAsync(new ConsentService(AppState.webEssentials).GetConsentsAsync(_consentData), _consentData).ConfigureAwait(true);
        _isEditable = LibPermissions.HasPermission(_consentData.FeaturePermissions, AppPermissions.UserConsentsView.ToString());
        _isDataFetched = true;
        if (GenericMethods.IsListNotEmpty(_consentData.Consents))
        {
            _consentData.Consents.ForEach(x =>
            {
                x.OrganisationName = AppState.MasterData?.OrganisationName;
            });
        }
        if (IsPatientMobileView)
        {
            if (_isEditable)
            {
                _actionButtons ??= new List<ButtonActionModel>();
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = ResourceConstants.R_NEXT_ACTION_KEY,
                    ButtonAction = () => { OnSendClickAsync(); },
                    ButtonClass = "mobile-view-button",
                });
            }
        }

    }

    private string GetStatus(bool isActiveStatus)
    {
        string currentStatus = LibResources.GetResourceValueByKey(_consentData?.Resources, ResourceConstants.R_ACCEPTED_STATUS_KEY);
        string statusStyle;
        if (isActiveStatus)
        {
            statusStyle = Constants.ACCEPT_COLOR;
        }
        else
        {
            statusStyle = string.Empty;
        }
        return string.IsNullOrEmpty(statusStyle)
            ? string.Empty
            : $"<label style ='{statusStyle}'><b>{currentStatus}</b></label>";

    }

    private List<TableDataStructureModel> GenerateTableStructure()
    {
        return new List<TableDataStructureModel>
        {
            new TableDataStructureModel{ DataField=nameof(ConsentModel.ConsentID), IsKey=true, IsSearchable=false, IsHidden=true, IsSortable=false },
            new TableDataStructureModel{ DataField=nameof(ConsentModel.ConsentName),DataHeader = ResourceConstants.R_CONSENT_PAGE_KEY },
            new TableDataStructureModel{ DataField=nameof(ConsentModel.Accepted), DataHeader=ResourceConstants.R_STATUS_KEY,IsHtmlTag = true }
        };
    }

    private AmhViewCellModel getViewCellModel()
    {
        return new AmhViewCellModel
        {
            ID = nameof(ConsentModel.ConsentID),
            LeftHeader = nameof(ConsentModel.ConsentName),
            LeftHeaderFieldType = FieldTypes.LinkHStartVCenterBoldUnderlineLabelControl,
            LeftDescription = nameof(ConsentModel.OrganisationName),
            LeftDescriptionFieldType = FieldTypes.PrimarySmallHStartVCenterLabelControl,
            RightHeader = nameof(ConsentModel.Accepted),
            RightHeaderFieldType = FieldTypes.HtmlLightCenterLabelControl
        };
    }

    private void OnListItemClicked(ConsentModel consentData)
    {
        if (consentData != null)
        {
            _consentData.Consent = _consentData.Consents.FirstOrDefault(x => x.ConsentID == Convert.ToInt64(consentData.ConsentID));
            ShowDetailPage = true;
        }
    }

    /// <summary>
    /// Save consent data based on action
    /// </summary>
    /// <param name="index">Button index</param>
    private void OnConsentActionClicked(long index)
    {
        AppState.Loader.ShowLoader(true);
        switch (index)
        {
            case 1:
                // On Accepted
                AcceptRejectConsent(true);
                break;
            case 2:
                // On Rejected
                AcceptRejectConsent(false);
                break;
            case 0:
            default:
                // Navigated on back click
                break;
        }
        AppState.Loader.ShowLoader(false);
        ShowDetailPage = false;
        StateHasChanged();
    }

    private void AcceptRejectConsent(bool isAccepted)
    {
        var consent = _consentData.Consents.FirstOrDefault(x => x.ConsentID == _consentData.Consent.ConsentID);
        _consentData.Consents.FirstOrDefault(x => x.ConsentID == _consentData.Consent.ConsentID).Accepted = isAccepted ? GetStatus(true) : string.Empty;
        consent.IsAccepted = isAccepted;
        consent.Accepted = isAccepted ? GetStatus(true) : string.Empty;
        consent.AcceptedOn = DateTimeOffset.UtcNow;
        if (IsPatientMobileView)
        {
            if (_isEditable)
            {
                _actionButtons ??= new List<ButtonActionModel>();
                _actionButtons.Add(new ButtonActionModel
                {
                    ButtonResourceKey = AppState.MasterData.IsConsentAccepted ? ResourceConstants.R_SAVE_ACTION_KEY : ResourceConstants.R_NEXT_ACTION_KEY,
                    ButtonAction = () => { OnSendClickAsync(); StateHasChanged(); },
                    ButtonClass = "mobile-view-button",
                });
            }
        }
        StateHasChanged();
    }

    private async Task OnSendClickAsync()
    {
        if (_consentData.Consents.FirstOrDefault(x => x.IsRequired && !x.IsAccepted) != null)
        {
            _hideConfirmation = false;
            _actionData = new List<ButtonActionModel> {
                new ButtonActionModel { ButtonID = Constants.NUMBER_ONE, ButtonResourceKey = ResourceConstants.R_OK_ACTION_KEY }
            };
        }
        else
        {
            await SaveConsentsAsync(true).ConfigureAwait(true);
        }
    }

    private async Task OnPopupOkClickAsync(object sequenceNo)
    {
        _hideConfirmation = true;
        Success = Error = string.Empty;
        if (sequenceNo != null)
        {
            if (Convert.ToInt64(sequenceNo) == 1)
            {
                await SaveConsentsAsync(false).ConfigureAwait(true);
            }
        }
    }

    private async Task SaveConsentsAsync(bool isMandatoryAccepted)
    {
        var consentData = new ConsentDTO
        {
            Consents = _consentData.Consents
        };
        await SendServiceRequestAsync(new ConsentService(AppState.webEssentials).SyncConsentsToServerAsync(consentData, CancellationToken.None), consentData).ConfigureAwait(true);
        if (consentData.ErrCode == ErrorCode.OK)
        {
            await LoadMasterPageDataAsync(AppState.webEssentials.GetPreferenceValue<long>(StorageConstants.PR_ACCOUNT_ID_KEY, 0));
            if (isMandatoryAccepted)
            {
                await NavigateToAsync(AppPermissions.DashboardView.ToString(), true).ConfigureAwait(false);
            }
            else
            {
                await NavigateToAsync(AppPermissions.NavigationComponent.ToString(), true).ConfigureAwait(false);
            }
        }
        else
        {
            Error = consentData.ErrCode.ToString();
        }
    }
}