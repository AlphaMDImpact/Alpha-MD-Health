using AlphaMDHealth.ClientBusinessLayer;
using AlphaMDHealth.Model;
using AlphaMDHealth.Utility;
using DevExpress.Internal;
using System.Globalization;

namespace AlphaMDHealth.MobileClient;

public class UserConsentsView : ViewManager
{
    private readonly AmhLabelControl _instructionsLabel;
    private readonly AmhButtonControl _actionButton;
    private ConsentDTO _consentData;
    private readonly AmhLabelControl _headerLabel;
    private readonly AmhListViewControl<ConsentModel> _consentsListView;

    public UserConsentsView(BasePage page, object parameters) : base(page, parameters)
    {
        ParentPage.PageService = new ConsentService(App._essentials);
        _consentsListView = new AmhListViewControl<ConsentModel>(FieldTypes.OneRowListViewControl)
        {
            SourceFields = new AmhViewCellModel
            {
                ID = nameof(ConsentModel.ConsentID),
                LeftHeader = nameof(ConsentModel.ConsentName),
                RightIcon = nameof(ConsentModel.Description),
            },
            ShowSearchBar = false
        };
        //var customCellModel = new CustomCellModel
        //{
        //    CellHeader = nameof(ConsentModel.ConsentName),
        //    IsLinkHeader = true,
        //    RemoveLeftMargin = true,
        //    CellDescription = nameof(ConsentModel.Description),
        //    CellRightContentHeader = nameof(ConsentModel.Accepted),
        //    IsRightHeaderHtml = true,
        //    ArrangeHorizontal = false,
        //    //RowHeight = (double)App.Current.Resources[StyleConstants.ST_DOUBLE_ROW_HEIGHT] / 2
        //};
        _actionButton = new AmhButtonControl(FieldTypes.PrimaryButtonControl)
        {
            Margin = new Thickness(0, (double)Application.Current.Resources[StyleConstants.ST_APP_PADDING]),
            IsEnabled = false,
        };
        //_actionButton.SetIsDisableButton(true);
        _instructionsLabel = new AmhLabelControl(FieldTypes.SecondarySmallHStartVCenterLabelControl)
        {
            //LineBreakMode = LineBreakMode.WordWrap
            IsEnabled = true,
        };
        _headerLabel = new AmhLabelControl(FieldTypes.LightMediumHEndVCenterBoldLabelControl)
        {
            IsVisible = false
        };
        ParentPage.SetPageLayoutOption(new OnIdiom<LayoutOptions> { Phone = LayoutOptions.FillAndExpand, Tablet = LayoutOptions.CenterAndExpand }, false);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 2, true);
        ParentPage.AddRowColumnDefinition(GridLength.Star, 1, true);
        ParentPage.AddRowColumnDefinition(GridLength.Auto, 1, true);
        ParentPage.PageLayout.Add(_headerLabel, 0, 0);
        ParentPage.PageLayout.Add(_instructionsLabel, 0, 1);
        ParentPage.PageLayout.Add(_consentsListView, 0, 2);
        //AddCollectionView(ParentPage.PageLayout, customCellModel, 0, 2);
        ParentPage.PageLayout.Add(_actionButton, 0, 3);
        SetPageContent(ParentPage.PageLayout);
    }

    public override async Task LoadUIAsync(bool isRefreshRequest)
    {
        _consentData = new ConsentDTO
        {
            Consents = new List<ConsentModel>(),
            // Note : IsActive is used to store IsBeforeLogin flag data
            IsActive = GenericMethods.MapValueType<bool>(GetParameterValue(nameof(BaseDTO.IsActive)))
        };

        _consentData.Resources = ParentPage.PageData.Resources;
        _consentData.RecordCount = -3;
        await (ParentPage.PageService as ConsentService).GetConsentsAsync(_consentData).ConfigureAwait(true);
        _consentsListView.PageResources = ParentPage.PageData = _consentData;
        ParentPage.ApplyPageResources();
        _consentsListView.ErrorCode = _consentData.ErrCode;
        ParentPage.PageData = ParentPage.PageService.PageData;
        if (_consentData.IsActive)
        {
            _headerLabel.IsVisible = true;
            _headerLabel.Value = LibPermissions.GetFeatureText(_consentData.FeaturePermissions, AppPermissions.UserConsentsView.ToString());
            //_headerLabel.Value = ParentPage.GetFeatureValueByCode(AppPermissions.UserConsentsView.ToString());
        }
        if (_consentData.ErrCode == ErrorCode.OK && GenericMethods.IsListNotEmpty(_consentData.Consents))
        {
            _actionButton.Value = LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, _consentData.IsActive ? ResourceConstants.R_NEXT_ACTION_KEY : ResourceConstants.R_SAVE_ACTION_KEY);
            //_actionButton.Value = ParentPage.GetResourceValueByKey(_consentData.IsActive ? ResourceConstants.R_NEXT_ACTION_KEY : ResourceConstants.R_SAVE_ACTION_KEY);
            _instructionsLabel.Value = LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, ResourceConstants.R_INFORMED_CONSENT_INSTRUCTIONS_LABEL_KEY);
            //_instructionsLabel.Value = ParentPage.GetResourceValueByKey(ResourceConstants.R_INFORMED_CONSENT_INSTRUCTIONS_LABEL_KEY);
            _consentsListView.DataSource = _consentData.Consents;
            //_consentsListView.OnValueChanged += OnSelectionChanged;
            //OnListItemSelection(OnSelectionChanged, true);
        }
        else
        {
            _actionButton.Value = LibResources.GetResourceValueByKey(ParentPage.PageData?.Resources, ResourceConstants.R_OK_ACTION_KEY);
            //_actionButton.Value = ParentPage.GetResourceValueByKey(ResourceConstants.R_OK_ACTION_KEY);
            //RenderErrorView(ParentPage.PageLayout, _consentData.ErrCode == ErrorCode.OK ? ResourceConstants.R_NO_DATA_FOUND_KEY : _consentData.ErrCode.ToString(), false, 0, false, true);
        }
        _actionButton.IsEnabled = true;
        _actionButton.OnValueChanged += OnNextButtonClicked;
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _consentData.Consent = e.CurrentSelection[0] as ConsentModel;
        if (_consentData.IsActive)
        {
            await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(_consentData.Consent.PageID.ToString(CultureInfo.InvariantCulture)
                , string.Empty, string.Empty, _consentData.Consent.IsAccepted ? "2" : "1", PageType.ConsentPage.ToString())
            ).ConfigureAwait(false);
        }
        else
        {
            //Detail page after login navigation
            await ParentPage.PushPageByTargetAsync(Constants.STATIC_MESSAGE_PAGE_IDENTIFIER,
                GenericMethods.GenerateParamsWithPlaceholder(Param.id, Param.type, Param.name, Param.identifier),
                _consentData.Consent.PageID.ToString(CultureInfo.InvariantCulture), PageType.ConsentPage.ToString(), string.Empty, _consentData.Consent.IsAccepted ? "2" : "1"
            ).ConfigureAwait(true);
        }
    }

    public override async Task UnloadUIAsync()
    {
        _actionButton.OnValueChanged -= OnNextButtonClicked;
    }

    private async void OnNextButtonClicked(object sender, EventArgs e)
    {
        AppHelper.ShowBusyIndicator = true;
        _actionButton.OnValueChanged -= OnNextButtonClicked;
        if (_consentData.Consents.Any(x => x.IsRequired && !x.IsAccepted)
            && !await ParentPage.DisplayMessagePopupAsync(ResourceConstants.R_ACCEPT_REQUIRED_CONSENT_ERROR_MSG_KEY, false, true, false))
        {
            CompleteSave();
            return;
        }
        _consentData.Consents.ForEach(x =>
        {
            x.IsSynced = false;
            if (x.AcceptedOn == default)
            {
                x.AcceptedOn = GenericMethods.GetUtcDateTime;
            }
        });
        await new ConsentService(App._essentials).SaveConsentAsync(_consentData).ConfigureAwait(true);
        if (_consentData.ErrCode != ErrorCode.OK)
        {
            //ParentPage.DisplayOperationStatus(ParentPage.GetResourceValueByKey(ErrorCode.ErrorWhileSavingRecords.ToString()));
            CompleteSave();
            return;
        }
        if (_consentData.Consents.Any(x => x.IsRequired && !x.IsAccepted))
        {
            await ParentPage.SyncDataWithServerAsync(Pages.UserConsentsPage, _consentData.IsActive, default).ConfigureAwait(true);
            //await ShellMasterPage.CurrentShell.PushMainPageAsync(new StaticMessagePage(ErrorCode.ConsentRequired.ToString())).ConfigureAwait(false);
        }
        else if (_consentData.IsActive)
        {
            await ParentPage.NavigateOnNextPageAsync(false, _consentData.IsActive, LoginFlow.UserConsentsPage).ConfigureAwait(false);
        }
        else
        {
            await ParentPage.SyncDataWithServerAsync(Pages.UserConsentsPage, _consentData.IsActive, default).ConfigureAwait(true);
            await ShellMasterPage.CurrentShell.RenderPageAsync().ConfigureAwait(false);
        }
        _actionButton.OnValueChanged += OnNextButtonClicked;
    }

    private void CompleteSave()
    {
        AppHelper.ShowBusyIndicator = false;
        _actionButton.OnValueChanged += OnNextButtonClicked;
    }
}